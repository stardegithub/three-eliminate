using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
/// 消除算法接口
///</summary>
public interface ICheckEliminateAlgorithm
{
	//输入要检查的列表和全部数组，输出不重复的可消除列表
	List<Eliminate.Item> SelectEliminateItemList(List<Eliminate.Item> checkItemList,Eliminate.Item[,] allItems);

	//检测当前是否有解
	bool IsNextCanEliminate(Eliminate.Item[,] allItems);
}  


public interface ICheckEliminateType
{
	Util.EEliminateType CheckEliminateType(List<Eliminate.Item> checkItemList,Eliminate.Item[,] allItems);
}
