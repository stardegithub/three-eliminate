using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
/// 消除算法接口
///</summary>
public interface ICheckEliminateAlgorithm
{
	//输入全部数组，输出可消除列表
	List<Eliminate.Item> SelectEliminateItem(Eliminate.Item[,] allItems);
}  

