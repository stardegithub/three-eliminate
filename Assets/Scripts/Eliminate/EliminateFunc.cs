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
        public List<Block> SelectEliminateBlockList(List<Block> checkBlockList, Block[,] allBlocks)
        {
            List<Block> ret = new List<Block>();
            EliminateTypeFunc func = new EliminateTypeFunc();


            foreach (var block in checkBlockList)
            {
                //指定位置的Item存在，且没有被检测过
                if (block && !block.hasCheck)
                {
                    //检测周围的消除
                    List<Block> sameBlocksList = new List<Block>();
                    FillSameBlocksList(ref sameBlocksList, block, allBlocks);
                    var eliminateList = GetEliminateList(block, sameBlocksList, allBlocks);
                    // //避免重复加入列表
                    for (int i = 0; i < eliminateList.Count; i++)
                    {
                        if (!ret.Contains(eliminateList[i]))
                        {
                            ret.Add(eliminateList[i]);
                            block.curEliminateType = func.CheckEliminateType(eliminateList[i], eliminateList);
                        }
                    }
                    //ret = eliminateList;
                }
            }
            return ret;
        }
        public bool IsNextCanEliminate(Block[,] allBlocks)
        {
            foreach (var block in allBlocks)
            {
                if (IsMoveCanEliminate(block, Vector2.up, allBlocks)
                || IsMoveCanEliminate(block, Vector2.down, allBlocks)
                || IsMoveCanEliminate(block, Vector2.left, allBlocks)
                || IsMoveCanEliminate(block, Vector2.right, allBlocks))
                {
                    return true;
                }
            }
            return false;

        }
        /// <summary>
        /// 填充相同Item列表  
        /// </summary>
        private void FillSameBlocksList(ref List<Block> sameBlocksList, Block current, Block[,] allBlocks)
        {
            //如果已存在，跳过
            if (sameBlocksList.Contains(current))
                return;
            //添加到列表
            sameBlocksList.Add(current);
            //上下左右的Item
            List<Block> tempBlockList = GetAroundBlock(current, allBlocks);

            for (int i = 0; i < tempBlockList.Count; i++)
            {
                //如果Item不合法，跳过
                if (tempBlockList[i] == null)
                    continue;
                //if (current.curSpr == tempItemList[i].curSpr)
                if (current.curType == tempBlockList[i].curType)
                {
                    FillSameBlocksList(ref sameBlocksList, tempBlockList[i], allBlocks);
                }
            }
        }

        private List<Block> GetEliminateList(Block current, List<Block> sameBlocksList, Block[,] allBlocks)
        {
            List<Block> eliminateList = new List<Block>();
            //计数器
            int rowCount = 0;
            int columnCount = 0;
            //临时列表
            List<Block> rowTempList = new List<Block>();
            List<Block> columnTempList = new List<Block>();
            ///横向纵向检测
            foreach (var block in sameBlocksList)
            {
                //如果在同一行
                if (block.blockRow == current.blockRow)
                {
                    //判断该点与Curren中间有无间隙
                    bool rowCanBoom = CheckBlocksInterval(true, current, block, allBlocks);
                    if (rowCanBoom)
                    {
                        //计数
                        rowCount++;
                        //添加到行临时列表
                        rowTempList.Add(block);
                    }
                }
                //如果在同一列
                if (block.blockColumn == current.blockColumn)
                {
                    //判断该点与Curren中间有无间隙
                    bool columnCanBoom = CheckBlocksInterval(false, current, block, allBlocks);
                    if (columnCanBoom)
                    {
                        //计数
                        columnCount++;
                        //添加到列临时列表
                        columnTempList.Add(block);
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
        /// <param name="block">Item.</param>
        /// <param name="dir">vector2.</param>
        private bool IsMoveCanEliminate(Block block, Vector2 dir, Block[,] allBlocks)
        {

            int tableRow = allBlocks.GetLength(0);
            int tableColumn = allBlocks.GetLength(1);
            //获取目标行列
            int targetRow = block.blockRow + System.Convert.ToInt32(dir.y);
            int targetColumn = block.blockColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = CheckRCLegal(targetRow, targetColumn, tableRow, tableColumn);
            if (!isLagal)
            {
                return false;
            }
            //获取目标
            Block target = allBlocks[targetRow, targetColumn];
            //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
            Block myBlock = allBlocks[block.blockRow, block.blockColumn];
            if (!target || !myBlock)
            {
                return false;
            }
            //相互移动
            // target.BlockMove(block.blockRow, block.blockColumn, Vector3.zero, false);
            // block.BlockMove(targetRow, targetColumn, Vector3.zero, false);
            target.BlockChange(block.blockRow, block.blockColumn);
            block.BlockChange(targetRow, targetColumn);

            //返回值
            bool isok = true;
            //消除检测	

            List<Block> sameBlocksList = new List<Block>();
            FillSameBlocksList(ref sameBlocksList, block, allBlocks);
            var eliminateList = GetEliminateList(block, sameBlocksList, allBlocks);
            isok = eliminateList.Count > 0 ? true : false;

            //还原	
            //临时行列
            int tempRow, tempColumn;
            tempRow = myBlock.blockRow;
            tempColumn = myBlock.blockColumn;
            //移动
            // block.BlockMove(target.blockRow, target.blockColumn, Vector3.zero, false);
            // target.BlockMove(tempRow, tempColumn, Vector3.zero, false);
            block.BlockChange(target.blockRow, target.blockColumn);
            target.BlockChange(tempRow, tempColumn);

            return isok;
        }

        /// <summary>
        /// 检测两个Item之间是否有间隙（图案不一致）
        /// </summary>
        /// <param name="isHorizontal">是否是横向.</param>
        /// <param name="begin">检测起点.</param>
        /// <param name="end">检测终点.</param>
        private bool CheckBlocksInterval(bool isHorizontal, Block begin, Block end, Block[,] allBlocks)
        {
            //获取图案
            Util.EBlockType type = begin.curType;
            //如果是横向
            if (isHorizontal)
            {
                //起点终点列号
                int beginIndex = begin.blockColumn;
                int endIndex = end.blockColumn;
                //如果起点在右，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.blockColumn;
                    endIndex = begin.blockColumn;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //异常处理（中间未生成，标识为不合法）
                    if (allBlocks[begin.blockRow, i] == null)
                        return false;
                    //如果中间有间隙（有图案不一致的）
                    if (allBlocks[begin.blockRow, i].curType != type)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //起点终点行号
                int beginIndex = begin.blockRow;
                int endIndex = end.blockRow;
                //如果起点在上，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.blockRow;
                    endIndex = begin.blockRow;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //如果中间有间隙（有图案不一致的）
                    if (allBlocks[i, begin.blockColumn].curType != type)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private List<Block> GetAroundBlock(Block current, Block[,] allBlocks)
        {
            List<Block> blocks = new List<Block>();
            int tableRow = allBlocks.GetLength(0);
            int tableColumn = allBlocks.GetLength(1);
            //up
            int row = current.blockRow + 1;
            int column = current.blockColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //down
            row = current.blockRow - 1;
            column = current.blockColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //left
            row = current.blockRow;
            column = current.blockColumn - 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //right
            row = current.blockRow;
            column = current.blockColumn + 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }

            return blocks;
        }

        /// <summary>
        /// 检测行列是否合法
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        /// <param name="row">Item row.</param>
        /// <param name="column">Item column.</param>
        public bool CheckRCLegal(int row, int column, int tableRow, int tableColumn)
        {
            if (row >= 0 && row < tableRow && column >= 0 && column < tableColumn)
                return true;
            return false;
        }


    }


    public class EliminateTypeFunc : ICheckEliminateType
    {
        public Util.EEliminateType CheckEliminateType(Block curBlock, List<Block> checkBlockList)
        {
            // List<Item> leftList = new List<Item>();
            // List<Item> rightList = new List<Item>();
            // List<Item> topList = new List<Item>();
            // List<Item> bottomList = new List<Item>();
            int leftNum = 0;
            int rightNum = 0;
            int topNum = 0;
            int bottomNum = 0;

            var curRow = curBlock.blockRow;
            var curCloumn = curBlock.blockColumn;

            for (int i = 0; i < checkBlockList.Count; i++)
            {
                if (checkBlockList[i].blockRow == curRow)//同列
                {
                    if (checkBlockList[i].blockColumn > curCloumn)//右边
                    {
                        rightNum++;
                    }
                    else if (checkBlockList[i].blockColumn < curCloumn)//左边
                    {
                        leftNum++;
                    }
                }
                if (checkBlockList[i].blockColumn == curCloumn)//同行
                {
                    if (checkBlockList[i].blockRow > curRow)//上边
                    {
                        topNum++;
                    }
                    else if (checkBlockList[i].blockRow < curRow)//下边
                    {
                        bottomNum++;
                    }
                }
            }
            Util.EEliminateType ret = Util.EEliminateType.Default;

            if (rightNum + leftNum >= 3 || topNum + bottomNum >= 3)//直线型
            {

                Debug.Log("直线");
                ret = Util.EEliminateType.Itype;
            }

            if (rightNum >= 1 && leftNum >= 1 && topNum >= 1 && bottomNum >= 1)//十字
            {
                Debug.Log("十字");
                ret = Util.EEliminateType.Xtype;

            }

            if ((rightNum * leftNum * topNum * bottomNum == 0) &&
            ((rightNum >= 1 && leftNum >= 1 && topNum + bottomNum >= 2) || topNum >= 1 && bottomNum >= 1 && rightNum + leftNum >= 2))//T字
            {
                Debug.Log("t字");
                ret = Util.EEliminateType.Ttype;

            }

            if ((rightNum >= 2 && leftNum == 0 && topNum >= 2 && bottomNum == 0) ||
            (rightNum >= 2 && leftNum == 0 && topNum == 0 && bottomNum >= 2) ||
            (rightNum == 0 && leftNum >= 2 && topNum == 0 && bottomNum >= 2) ||
            (rightNum == 0 && leftNum >= 2 && topNum >= 2 && bottomNum == 0))//L字
            {
                Debug.Log("l字");
                ret = Util.EEliminateType.Ltype;

            }

            return Util.EEliminateType.Default;
        }
    }

    public class EliminateFunction : IEliminate
    {
        public Eliminate.BlockManagerInfo Init(int row, int colunm, List<Util.EBlockType> blockTypes, float size)
        {
            Eliminate.BlockManagerInfo blockManagerInfo = new BlockManagerInfo();

            var canvas = Resources.Load<GameObject>("Prefabs/BlockCanvas");
            blockManagerInfo.blockParent = GameObject.Instantiate(canvas).transform.GetChild(0).transform;


            blockManagerInfo.allBlocks = new Block[row, colunm];
            blockManagerInfo.allPos = new Vector3[row, colunm];
            Vector2 offset = new Vector2(0, 0);
            //生成ITEM
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < colunm; j++)
                {
                    //生成
                    GameObject currentBlock =
                        ObjectPool.instance.GetGameObject(Util.Block, blockManagerInfo.blockParent);
                    //设置坐标
                    currentBlock.transform.localPosition =
                        new Vector3(j * size, i * size, 0) + new Vector3(offset.x, offset.y, 0);
                    //随机图案编号
                    int random = Random.Range(0, (int)Util.EBlockType.Num);
                    //获取Item组件
                    Block current = currentBlock.GetComponent<Block>();
                    current.Init(i, j, (Util.EBlockType)random);

                    //保存到数组
                    blockManagerInfo.allBlocks[i, j] = current;
                    //记录世界坐标
                    blockManagerInfo.allPos[i, j] = currentBlock.transform.position;
                }
            }
            return blockManagerInfo;

        }

        public List<Eliminate.Block> CheckEliminate(List<Eliminate.Block> checkBlockList, Eliminate.Block[,] allBlocks)
        {

            List<Block> ret = new List<Block>();
            EliminateTypeFunc func = new EliminateTypeFunc();

            foreach (var block in checkBlockList)
            {
                //指定位置的Item存在，且没有被检测过
                if (block && !block.hasCheck)
                {
                    //检测周围的消除
                    List<Block> sameBlocksList = new List<Block>();
                    FillSameBlocksList(ref sameBlocksList, block, allBlocks);
                    var eliminateList = GetEliminateList(block, sameBlocksList, allBlocks);
                    // //避免重复加入列表
                    for (int i = 0; i < eliminateList.Count; i++)
                    {
                        if (!ret.Contains(eliminateList[i]))
                        {
                            ret.Add(eliminateList[i]);
                            block.curEliminateType = func.CheckEliminateType(eliminateList[i], eliminateList);
                        }
                        eliminateList[i].hasCheck = true;
                    }
                    //ret = eliminateList;
                }
            }
            return ret;
        }

        public void DoEliminate(List<Eliminate.Block> eliminateList, Eliminate.Block[,] allBlocks)
        {
            foreach (var block in eliminateList)
            {
                allBlocks[block.blockRow, block.blockColumn] = null;
                block.EmilinateSelf();
            }
            //           yield return new WaitForSeconds(Util.BlockMoveTime);

            //ManipulateEliminateList(eliminateList,allBlocks);
        }
        // IEnumerator ManipulateEliminateList(List<Eliminate.Block> eliminateList, Eliminate.Block[,] allBlocks)
        // {
        //     foreach (var block in eliminateList)
        //     {
        //         allBlocks[block.blockRow, block.blockColumn] = null;
        //         block.EmilinateSelf();
        //     }
        // }

        public void GenerateBlocks(BlockManagerInfo blockManagerInfo)
        {
            int tableRow = blockManagerInfo.allBlocks.GetLength(0);
            int tableColumn = blockManagerInfo.allBlocks.GetLength(1);
            //isOperation = true;
            //逐列检测
            for (int i = 0; i < tableColumn; i++)
            {
                //计数器
                int count = 0;
                //下落队列
                Queue<Block> dropQueue = new Queue<Block>();
                //逐行检测
                for (int j = 0; j < tableRow; j++)
                {
                    if (blockManagerInfo.allBlocks[j, i] != null)
                    {
                        //计数
                        count++;
                        //放入队列
                        dropQueue.Enqueue(blockManagerInfo.allBlocks[j, i]);
                    }
                }
                //下落
                for (int k = 0; k < count; k++)
                {
                    //获取要下落的Item
                    Block current = dropQueue.Dequeue();
                    //修改全局数组(原位置情况)
                    blockManagerInfo.allBlocks[current.blockRow, current.blockColumn] = null;
                    //修改Item的行数
                    current.blockRow = k;
                    //修改全局数组(填充新位置)
                    blockManagerInfo.allBlocks[current.blockRow, current.blockColumn] = current;
                    //下落
                    current.CurrentBlockDrop(blockManagerInfo.allPos[current.blockRow, current.blockColumn]);
                }
            }

            //yield return new WaitForSeconds (0.2f);

            //CreateNewBlock();
            //isOperation = true;
            for (int i = 0; i < tableColumn; i++)
            {
                int count = 0;
                Queue<GameObject> newBlockQueue = new Queue<GameObject>();
                for (int j = 0; j < tableRow; j++)
                {
                    if (blockManagerInfo.allBlocks[j, i] == null)
                    {
                        //生成一个Item
                        GameObject current = ObjectPool.instance.GetGameObject(Util.Block, blockManagerInfo.blockParent);
                        current.transform.position = blockManagerInfo.allPos[tableRow - 1, i];
                        //随机数
                        int random = Random.Range(0, (int)Util.EBlockType.Num);
                        current.GetComponent<Block>().Init(tableRow - 1, i, (Util.EBlockType)random);

                        // //修改脚本中的图片
                        // //                    currentItem.curSpr = randomSprites[random];
                        // //修改真实图片
                        // currentItem.curtImg.sprite = Util.GetSpriteAssetsByType(currentItem.curType);
                        newBlockQueue.Enqueue(current);
                        count++;
                    }
                }
                for (int k = 0; k < count; k++)
                {
                    //获取Item组件
                    Block currentBlock = newBlockQueue.Dequeue().GetComponent<Block>();

                    //获取要移动的行数
                    int r = tableRow - count + k;
                    //移动
                    currentBlock.BlockMove(r, i, blockManagerInfo.allPos[r, i]);
                }
            }

        }

        public bool CheckImpasse(Eliminate.Block[,] allBlocks)
        {
            foreach (var block in allBlocks)
            {
                if (IsMoveCanEliminate(block, Vector2.up, allBlocks)
                || IsMoveCanEliminate(block, Vector2.down, allBlocks)
                || IsMoveCanEliminate(block, Vector2.left, allBlocks)
                || IsMoveCanEliminate(block, Vector2.right, allBlocks))
                {

                    return false;
                }
            }
            UpsetBlock(allBlocks);
            return true;
        }

        public void Idle()
        {

        }

        public void Operation(Eliminate.Block curBlock, Eliminate.Block targetBlock)
        {
            //临时行列
            int tempRow, tempColumn;
            tempRow = curBlock.blockRow;
            tempColumn = curBlock.blockColumn;
            //相互移动
            curBlock.BlockMove(targetBlock.blockRow, targetBlock.blockColumn, targetBlock.transform.position);
            targetBlock.BlockMove(tempRow, tempColumn, curBlock.transform.position);


            //开启协程
            //BlockExchange(curBlock, target);
        }


        // void BlockExchange(Eliminate.Block curBlock, Eliminate.Block targetBlock)
        // {

        //     //还原标志位
        //     bool reduction = false;

        //     //消除处理
        //     EliminateFunc func = new EliminateFunc();
        //     List<Block> checkBlockList = new List<Block>();
        //     checkBlockList.Add(curBlock);
        //     checkBlockList.Add(targetBlock);

        //     var eliminateList = func.SelectEliminateBlockList(checkBlockList, BlockManager.Instance.BlockManagerInfo.allBlocks);

        //     if (eliminateList.Count > 0)
        //     {
        //         BlockManager.Instance.EliminateBlock(eliminateList);
        //         reduction = false;
        //     }
        //     else
        //     {
        //         reduction = true;
        //     }
        //     //还原
        //     if (reduction)
        //     {
        //         //延迟
        //         //yield return new WaitForSeconds(Util.BlockMoveTime);
        //         //临时行列
        //         int tempRow, tempColumn;
        //         tempRow = curBlock.blockRow;
        //         tempColumn = curBlock.blockColumn;
        //         //移动
        //         curBlock.BlockMove(targetBlock.blockRow, targetBlock.blockColumn, targetBlock.transform.position);
        //         targetBlock.BlockMove(tempRow, tempColumn, curBlock.transform.position);
        //         //延迟
        //        // yield return new WaitForSeconds(Util.BlockMoveTime);
        //         //操作完毕
        //         BlockManager.Instance.isOperation = false;
        //     }
        // }


        /// <summary>
        /// 填充相同Item列表  
        /// </summary>
        private void FillSameBlocksList(ref List<Block> sameBlocksList, Block current, Block[,] allBlocks)
        {
            //如果已存在，跳过
            if (sameBlocksList.Contains(current))
                return;
            //添加到列表
            sameBlocksList.Add(current);
            //上下左右的Item
            List<Block> tempBlockList = GetAroundBlock(current, allBlocks);

            for (int i = 0; i < tempBlockList.Count; i++)
            {
                //如果Item不合法，跳过
                if (tempBlockList[i] == null)
                    continue;
                //if (current.curSpr == tempItemList[i].curSpr)
                if (current.curType == tempBlockList[i].curType)
                {
                    FillSameBlocksList(ref sameBlocksList, tempBlockList[i], allBlocks);
                }
            }
        }

        private List<Block> GetEliminateList(Block current, List<Block> sameBlocksList, Block[,] allBlocks)
        {
            List<Block> eliminateList = new List<Block>();
            //计数器
            int rowCount = 0;
            int columnCount = 0;
            //临时列表
            List<Block> rowTempList = new List<Block>();
            List<Block> columnTempList = new List<Block>();
            ///横向纵向检测
            foreach (var block in sameBlocksList)
            {
                //如果在同一行
                if (block.blockRow == current.blockRow)
                {
                    //判断该点与Curren中间有无间隙
                    bool rowCanBoom = CheckBlocksInterval(true, current, block, allBlocks);
                    if (rowCanBoom)
                    {
                        //计数
                        rowCount++;
                        //添加到行临时列表
                        rowTempList.Add(block);
                    }
                }
                //如果在同一列
                if (block.blockColumn == current.blockColumn)
                {
                    //判断该点与Curren中间有无间隙
                    bool columnCanBoom = CheckBlocksInterval(false, current, block, allBlocks);
                    if (columnCanBoom)
                    {
                        //计数
                        columnCount++;
                        //添加到列临时列表
                        columnTempList.Add(block);
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
        /// <param name="block">Item.</param>
        /// <param name="dir">vector2.</param>
        private bool IsMoveCanEliminate(Block block, Vector2 dir, Block[,] allBlocks)
        {

            int tableRow = allBlocks.GetLength(0);
            int tableColumn = allBlocks.GetLength(1);
            //获取目标行列
            int targetRow = block.blockRow + System.Convert.ToInt32(dir.y);
            int targetColumn = block.blockColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = CheckRCLegal(targetRow, targetColumn, tableRow, tableColumn);
            if (!isLagal)
            {
                return false;
            }
            //获取目标
            Block target = allBlocks[targetRow, targetColumn];
            //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
            Block myBlock = allBlocks[block.blockRow, block.blockColumn];
            if (!target || !myBlock)
            {
                return false;
            }
            //相互移动
            // target.BlockMove(block.blockRow, block.blockColumn, Vector3.zero, false);
            // block.BlockMove(targetRow, targetColumn, Vector3.zero, false);
            target.BlockChange(block.blockRow, block.blockColumn);
            block.BlockChange(targetRow, targetColumn);


            //返回值
            bool isok = true;
            //消除检测	

            List<Block> sameBlocksList = new List<Block>();
            FillSameBlocksList(ref sameBlocksList, block, allBlocks);
            var eliminateList = GetEliminateList(block, sameBlocksList, allBlocks);
            isok = eliminateList.Count > 0 ? true : false;

            //还原	
            //临时行列
            int tempRow, tempColumn;
            tempRow = myBlock.blockRow;
            tempColumn = myBlock.blockColumn;
            //移动
            // block.BlockMove(target.blockRow, target.blockColumn, Vector3.zero, false);
            // target.BlockMove(tempRow, tempColumn, Vector3.zero, false);
            block.BlockChange(target.blockRow, target.blockColumn);
            target.BlockChange(tempRow, tempColumn);

            return isok;
        }

        /// <summary>
        /// 检测两个Item之间是否有间隙（图案不一致）
        /// </summary>
        /// <param name="isHorizontal">是否是横向.</param>
        /// <param name="begin">检测起点.</param>
        /// <param name="end">检测终点.</param>
        private bool CheckBlocksInterval(bool isHorizontal, Block begin, Block end, Block[,] allBlocks)
        {
            //获取图案
            Util.EBlockType type = begin.curType;
            //如果是横向
            if (isHorizontal)
            {
                //起点终点列号
                int beginIndex = begin.blockColumn;
                int endIndex = end.blockColumn;
                //如果起点在右，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.blockColumn;
                    endIndex = begin.blockColumn;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //异常处理（中间未生成，标识为不合法）
                    if (allBlocks[begin.blockRow, i] == null)
                        return false;
                    //如果中间有间隙（有图案不一致的）
                    if (allBlocks[begin.blockRow, i].curType != type)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //起点终点行号
                int beginIndex = begin.blockRow;
                int endIndex = end.blockRow;
                //如果起点在上，交换起点终点列号
                if (beginIndex > endIndex)
                {
                    beginIndex = end.blockRow;
                    endIndex = begin.blockRow;
                }
                //遍历中间的Item
                for (int i = beginIndex + 1; i < endIndex; i++)
                {
                    //如果中间有间隙（有图案不一致的）
                    if (allBlocks[i, begin.blockColumn].curType != type)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private List<Block> GetAroundBlock(Block current, Block[,] allBlocks)
        {
            List<Block> blocks = new List<Block>();
            int tableRow = allBlocks.GetLength(0);
            int tableColumn = allBlocks.GetLength(1);
            //up
            int row = current.blockRow + 1;
            int column = current.blockColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //down
            row = current.blockRow - 1;
            column = current.blockColumn;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //left
            row = current.blockRow;
            column = current.blockColumn - 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }
            //right
            row = current.blockRow;
            column = current.blockColumn + 1;
            if (CheckRCLegal(row, column, tableRow, tableColumn))
            {
                blocks.Add(allBlocks[row, column]);
            }

            return blocks;
        }

        /// <summary>
        /// 检测行列是否合法
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        /// <param name="row">Item row.</param>
        /// <param name="column">Item column.</param>
        public bool CheckRCLegal(int row, int column, int tableRow, int tableColumn)
        {
            if (row >= 0 && row < tableRow && column >= 0 && column < tableColumn)
                return true;
            return false;
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        public void UpsetBlock(Block[,] allBlocks)
        {
            foreach (var block in allBlocks)
            {
                if (block != null)
                {
                    //随机数
                    int random = Random.Range(0, (int)Util.EBlockType.Num);
                    block.Init(block.blockRow, block.blockColumn, (Util.EBlockType)random);
                }
            }
        }

    }


}
