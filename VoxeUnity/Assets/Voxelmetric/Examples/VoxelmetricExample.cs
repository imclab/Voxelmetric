﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Voxelmetric.Code.Core;
using Voxelmetric.Code.Core.Serialization;
using Voxelmetric.Code.Data_types;
using Voxelmetric.Code.Load_Resources.Blocks;
using Voxelmetric.Code.Utilities;

namespace Voxelmetric.Examples
{
    public class VoxelmetricExample : MonoBehaviour
    {
        public World world;
        public Camera cam;
        private Vector2 rot;

        public string blockToPlace = "air";
        public Text selectedBlockText;
        public Text saveProgressText;

        private Vector3Int pfStart;
        private Vector3Int pfStop;
        public PathFinder pf;

        private SaveProgress saveProgress;
        private EventSystem eventSystem;

        public void SetType(string newType)
        {
            blockToPlace = newType;
        }

        void Start()
        {
            rot.y = 360f - cam.transform.localEulerAngles.x;
            rot.x = cam.transform.localEulerAngles.y;
            eventSystem = FindObjectOfType<EventSystem>();
        }

        void Update()
        {
            // Roatation
            if (Input.GetMouseButton(1))
            {
                rot = new Vector2(
                    rot.x+Input.GetAxis("Mouse X")*3,
                    rot.y+Input.GetAxis("Mouse Y")*3
                    );

                cam.transform.localRotation = Quaternion.AngleAxis(rot.x, Vector3.up);
                cam.transform.localRotation *= Quaternion.AngleAxis(rot.y, Vector3.left);
            }

            // Movement
            bool turbo = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            cam.transform.position += cam.transform.forward*40*(turbo ? 3 : 1)*Input.GetAxis("Vertical")*Time.deltaTime;
            cam.transform.position += cam.transform.right*40*(turbo ? 3 : 1)*Input.GetAxis("Horizontal")*Time.deltaTime;

            // Screenspace mouse cursor coordinates
            var mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));

            if (world!=null)
            {
                Block block = world.blockProvider.GetBlock(blockToPlace);
                VmRaycastHit hit = Code.Voxelmetric.Raycast(
                    new Ray(cam.transform.position, mousePos-cam.transform.position),
                    world, 100, block.Type==BlockProvider.AirType
                    );

                // Display the type of the selected block
                if (selectedBlockText!=null)
                    selectedBlockText.text = Code.Voxelmetric.GetBlock(world, hit.vector3Int).DisplayName;

                // Save current world status
                if (saveProgressText != null)
                    saveProgressText.text = saveProgress != null ? SaveStatus() : "Save";

                // Clicking voxel blocks
                if (Input.GetMouseButtonDown(0) && eventSystem!=null && !eventSystem.IsPointerOverGameObject())
                {
                    if (hit.block.Type!=BlockProvider.AirType)
                    {
                        bool adjacent = block.Type!=BlockProvider.AirType;
                        Code.Voxelmetric.SetBlock(world, adjacent ? hit.adjacentPos : hit.vector3Int, new BlockData(block.Type, block.Solid));
                    }
                }

                // Pathfinding
                if (Input.GetKeyDown(KeyCode.I))
                {
                    pfStart = hit.vector3Int;
                }

                if (Input.GetKeyDown(KeyCode.O))
                {
                    pfStop = hit.vector3Int;
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    pf = new PathFinder(pfStart, pfStop, world, 2);
                    Debug.Log(pf.path.Count);
                }

                if (pf!=null && pf.path.Count!=0)
                {
                    for (int i = 0; i<pf.path.Count-1; i++)
                        Debug.DrawLine(pf.path[i].Add(0, 1, 0), pf.path[i+1].Add(0, 1, 0));
                }

                // Test of ranged block setting
                if (Input.GetKeyDown(KeyCode.T))
                {
                    Code.Voxelmetric.SetBlockRange(world, new Vector3Int(-44, -44, -44), new Vector3Int(44, 44, 44), BlockProvider.AirBlock);
                }
            }
        }

        public void SaveAll()
        {
            var chunksToSave = Code.Voxelmetric.SaveAll(world);
            saveProgress = new SaveProgress(chunksToSave);
        }

        public string SaveStatus()
        {
            if (saveProgress == null)
                return "";

            return saveProgress.GetProgress() + "%";
        }

    }
}
