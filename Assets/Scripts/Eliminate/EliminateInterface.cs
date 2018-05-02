using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
/// 消除算法接口
///</summary>
public interface ICheckEliminateAlgorithm
{
    //输入要检查的列表和全部数组，输出不重复的可消除列表
    List<Eliminate.Item> SelectEliminateItemList(List<Eliminate.Item> checkItemList, Eliminate.Item[,] allItems);

    //检测当前是否有解
    bool IsNextCanEliminate(Eliminate.Item[,] allItems);
}


public interface ICheckEliminateType
{

    /// <summary>
    /// 检测以item为中心的消除类型
    /// </summary>
    /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
    /// <param name="curItem">基点item.</param>
    /// <param name="checkItemList">待检查列表，可消除的同色相邻方块.</param>
    Util.EEliminateType CheckEliminateType(Eliminate.Item curItem, List<Eliminate.Item> checkItemList);
}
