using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
/// 消除算法接口
///</summary>
public interface ICheckEliminateAlgorithm
{
    //输入要检查的列表和全部数组，输出不重复的可消除列表
    List<Eliminate.Block> SelectEliminateBlockList(List<Eliminate.Block> checkBlockList, Eliminate.Block[,] allBlocks);

    //检测当前是否有解
    bool IsNextCanEliminate(Eliminate.Block[,] allBlocks);
}


public interface ICheckEliminateType
{

    /// <summary>
    /// 检测以item为中心的消除类型
    /// </summary>
    /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
    /// <param name="curBlock">基点item.</param>
    /// <param name="checkBlockList">待检查列表，可消除的同色相邻方块.</param>
    Util.EEliminateType CheckEliminateType(Eliminate.Block curBlock, List<Eliminate.Block> checkBlockList);
}

public interface IEliminate
{
    Eliminate.BlockManagerInfo Init(int row,int colunm,List<Util.EBlockType> blockTypes,float size);

    List<Eliminate.Block> CheckEliminate(List<Eliminate.Block> checkBlockList, Eliminate.Block[,] allBlocks);

    void DoEliminate(List<Eliminate.Block> eliminateBlockList, Eliminate.Block[,] allBlocks);

    void GenerateBlocks(Eliminate.BlockManagerInfo blockManagerInfo);

    bool CheckImpasse(Eliminate.Block[,] allBlocks);

    void Idle();

    void Operation(Eliminate.Block curBlock,Eliminate.Block targetBlock);

}