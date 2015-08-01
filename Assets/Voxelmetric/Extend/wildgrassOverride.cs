﻿using UnityEngine;
using System.Collections;

public class wildgrassOverride : BlockOverride
{

    // On create set the height to 10 and schedule and update in 1 second
    public override Block OnCreate(Chunk chunk, BlockPos pos, Block block)
    {
        block.data2 = 100;
        return block;
    }

    //On random update add 100 to the height
    public override void RandomUpdate(Chunk chunk, BlockPos pos, Block block)
    {
        block.data2 += 100;
        chunk.SetBlock(pos, block);
    }
}