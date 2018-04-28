using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eliminate
{
    ///<summary>
    /// 消除算法接口
    ///</summary>
    public class EliminateFunc : ICheckEliminateAlgorithm
    {
        public List<Item> SelectEliminateItemList(List<Item> checkItemList, Item[,] allItems)
        {
            List<Item> ret = new List<Item>();

            foreach (var item in checkItemList)
            {
                //指定位置的Item存在，且没有被检测过
                if (item && !item.hasCheck)
                {
                    //检测周围的消除
                    List<Item> sameItemsList = new List<Item>();
                    FillSameItemsList(ref sameItemsList, item, allItems);
                    var eliminateList = GetEliminateList(item, sameItemsList, allItems);
                    // //避免重复加入列表
                    for (int i = 0; i < eliminateList.Count; i++)
                    {
                        if (!ret.Contains(eliminateList[i]))
                        {
                            ret.Add(eliminateList[i]);
                        }
                    }
					//ret = eliminateList;
                }
            }
            return ret;
        }
        public bool IsNextCanEliminate(Item[,] allItems)
        {
            foreach (var item in allItems)
            {
                if (IsMoveCanEliminate(item, Vector2.up, allItems)
                || IsMoveCanEliminate(item, Vector2.down, allItems)
                || IsMoveCanEliminate(item, Vector2.left, allItems)
                || IsMoveCanEliminate(item, Vector2.right, allItems))
                {
                    return true;
                }
            }
            return false;

        }
        /// <summary>
        /// 填充相同Item列表  
        /// </summary>
        private void FillSameItemsList(ref List<Item> sameItemsList, Item current, Item[,] allItems)
        {
            //如果已存在，跳过
            if (sameItemsList.Contains(current))
                return;
            //添加到列表
            sameItemsList.Add(current);
            //上下左右的Item
            List<Item> tempItemList = GetAroundItem(current, allItems);

            for (int i = 0; i < tempItemList.Count; i++)
            {
                //如果Item不合法，跳过
                if (tempItemList[i] == null)
                    continue;
                if (current.curSpr == tempItemList[i].curSpr)
                {
                    FillSameItemsList(ref sameItemsList, tempItemList[i], allItems);
                }
            }
        }

        private List<Item> GetEliminateList(Item current, List<Item> sameItemsList, Item[,] allItems)
        {
            List<Item> eliminateList = new List<Item>();
            //计数器
            int rowCount = 0;
            int columnCount = 0;
            //临时列表
            List<Item> rowTempList = new List<Item>();
            List<Item> columnTempList = new List<Item>();
            ///横向纵向检测
            foreach (var item in sameItemsList)
            {
                //如果在同一行
                if (item.itemRow == current.itemRow)
                {
                    //判断该点与Curren中间有无间隙
                    bool rowCanBoom = CheckItemsInterval(true, current, item, allItems);
                    if (rowCanBoom)
                    {
                        //计数
                        rowCount++;
                        //添加到行临时列表
                        rowTempList.Add(item);
                    }
                }
                //如果在同一列
                if (item.itemColumn == current.itemColumn)
                {
                    //判断该点与Curren中间有无间隙
                    bool columnCanBoom = CheckItemsInterval(false, current, item, allItems);
                    if (columnCanBoom)
                    {
                        //计数
                        columnCount++;
                        //添加到列临时列表
                        columnTempList.Add(item);
                    }
                }
            }
            //横向消除
            bool horizontalBoom = false;
            //如果横向三个以上
            if (rowCount > 2)
            {
                //将临时列表中的Item全部放入BoomList
                eliminateList.AddRange(rowTempList);
                //横向消除
                horizontalBoom = true;
            }
            //如果纵向三个以上
            if (columnCount > 2)
            {
                if (horizontalBoom)
                {
                    //剔除自己
                    eliminateList.Remove(current);
                }
                //将临时列表中的Item全部放入BoomList
                eliminateList.AddRange(columnTempList);
            }
            // for (int i = 0; i < eliminateList.Count && eliminateList.Count > 2; i++)
            // {
            //     eliminateList[i].hasCheck = true;
            // }
            return eliminateList;
        }

        /// <summary>
        /// 检测item向dir方向移动一格是否可消除
        /// </summary>
        /// <returns><c>true</c>, if can boom, <c>false</c> cant boom.</returns>
        /// <param name="item">Item.</param>
        /// <param name="dir">vector2.</param>
        private bool IsMoveCanEliminate(Item item, Vector2 dir, Item[,] allItems)
        {
            if (item == null)
            {
                Debug.Log("dsfsd");
            }
            int tableRow = allItems.GetLength(0);
            int tableColumn = allItems.GetLength(1);
            //获取目标行列
            int targetRow = item.itemRow + System.Convert.ToInt32(dir.y);
            int targetColumn = item.itemColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = CheckRCLegal(targetRow, targetColumn, tableRow, tableColumn);
            if (!isLagal)
            {
                return false;
            }
            //获取目标
            Item target = allItems[targetRow, targetColumn];
            //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
            Item myItem = allItems[item.itemRow, item.itemColumn];
            if (!target || !myItem)
            {
                return false;
            }
            //相互移动
            target. ItemMove(item.itemRow, item.itemColumn, Vector3.zero, false);
            item. ItemMove(targetRow, targetColumn, Vector3.zero, false);

            //返回值
            bool isok = true;
            //消除检测	

            List<Item> sameItemsList = new List<Item>();
            FillSameItemsList(ref sameItemsList, item, allItems);
            var eliminateList = GetEliminateList(item, sameItemsList, allItems);
            isok = eliminateList.Count > 0 ? true : false;

            //还原	
            //临时行列
            int tempRow, tempColumn;
            tempRow = myItem.itemRow;
            tempColumn = myItem.itemColumn;
            //移动
            item. ItemMove(target.itemRow, target.itemColumn, Vector3.zero, false);
            target. ItemMove(tempRow, tempColumn, Vector3.zero, false);

            return isok;
        }

        /// <summary>
        /// 检测两个Item之间是否有间隙（图案不一致）
        /// </summary>
        /// <param name="isHorizontal">是否是横向.</param>
        /// <param name="begin">检测起点.</param>
        /// <param name="end">检测终点.</param>
        private bool CheckItemsInterval(bool isHorizontal, Item begin, Item end, Item[,] allItems)
        {
            //获取图案
            Sprite spr = begin.curSpr;
            //如果是横向
            if (isHorizontal)
            {
                //起点终点列号
                int beginIndex = begin.itemColumn;
                int endIndex = end.itemColumn;
                //如果起点在右，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.itemColumn;
                    endIndex = begin.itemColumn;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //异常处理（中间未生成，标识为不合法）
                    if (allItems[begin.itemRow, i] == null)
                        return false;
                    //如果中间有间隙（有图案不一致的）
                    if (allItems[begin.itemRow, i].curSpr != spr)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //起点终点行号
                int beginIndex = begin.itemRow;
                int endIndex = end.itemRow;
                //如果起点在上，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.itemRow;
                    endIndex = begin.itemRow;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //如果中间有间隙（有图案不一致的）
                    if (allItems[i, begin.itemColumn].curSpr != spr)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private List<Item> GetAroundItem(Item current, Item[,] allItems)
        {
            List<Item> items = new List<Item>();
            int tableRow = allItems.GetLength(0);
            int tableColumn = allItems.GetLength(1);
            //up
            int row = current.itemRow + 1;
            int column = current.itemColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                items.Add(allItems[row, column]);
            }
            //down
            row = current.itemRow - 1;
            column = current.itemColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                items.Add(allItems[row, column]);
            }
            //left
            row = current.itemRow;
            column = current.itemColumn - 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                items.Add(allItems[row, column]);
            }
            //right
            row = current.itemRow;
            column = current.itemColumn + 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                items.Add(allItems[row, column]);
            }

            return items;
        }

        /// <summary>
        /// 检测行列是否合法
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        /// <param name="itemRow">Item row.</param>
        /// <param name="itemColumn">Item column.</param>
        public bool CheckRCLegal(int itemRow, int itemColumn, int tableRow, int tableColumn)
        {
            if (itemRow >= 0 && itemRow < tableRow && itemColumn >= 0 && itemColumn < tableColumn)
                return true;
            return false;
        }


    }


	public class EliminateTypeFunc : ICheckEliminateType
	{
		public Util.EEliminateType CheckEliminateType(List<Eliminate.Item> checkItemList,Eliminate.Item[,] allItems)
		{
			return Util.EEliminateType.Ltype;
		}
	}

}
