using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eliminate
{

    public class BlockManagerInfo
    {
        //全局所有block二位数组
        public Block[,] allBlocks;
        //全局block坐标二维数组
        public Vector3[,] allPos;
        //block的父节点
        public Transform blockParent;

    }
    public class BlockManager : MonoSingletion<BlockManager>
    {
        //manager存储的信息
        public BlockManagerInfo BlockManagerInfo;
        //是否有操作在进行
        public bool isOperation = false;
        //ITEM的边长
        private float BlockSize = 0;
        //实现接口的实例
        private IEliminate eliminateFunc;
        //本次游戏的block类型列表
        private List<Util.EBlockType> blockTypes = new List<Util.EBlockType>();
        //行列
        private int tableRow = 5;
        private int tableColumn = 5;

        void Awake()
        {
            eliminateFunc = new EliminateFunction();

            //下列之后应读配置数据
            for (int i = 0; i < (int)Util.EBlockType.Num; i++)
            {
                blockTypes.Add((Util.EBlockType)i);
            }
            BlockSize = GetBlockSize();
        }

        /// <summary>
        /// 获取Item边长
        /// </summary>
        /// <returns>The item size.</returns>
        private float GetBlockSize()
        {
            return Resources.Load<GameObject>(Util.ResourcesPrefab + Util.Block).
                GetComponent<RectTransform>().rect.height;
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitGame()
        {
            BlockManagerInfo = eliminateFunc.Init(tableRow, tableColumn, blockTypes, BlockSize);
            CheckAllBlockEliminate();
        }
        public void CheckAllBlockEliminate()
        {
            List<Block> checkBlockList = new List<Block>();
            foreach (var block in BlockManagerInfo.allBlocks)
            {
                checkBlockList.Add(block);

            }

            var eliminatelist = eliminateFunc.CheckEliminate(checkBlockList, BlockManagerInfo.allBlocks);
            EliminateBlock(eliminatelist);
        }

        public void EliminateBlock(List<Block> eliminateList)
        {
            //有消除
            if (eliminateList.Count > 0)
            {
                StartCoroutine(ManipulateBoomList(eliminateList));
                isOperation = true;
            }
            else
            {
                if (eliminateFunc.CheckImpasse(BlockManagerInfo.allBlocks))
                {
                    CheckAllBlockEliminate();
                    Debug.Log("here  cant eliminate!");
                }
                //操作结束
                isOperation = false;
            }
        }


        /// 处理BoomList
        IEnumerator ManipulateBoomList(List<Block> tempBoomList)
        {
            eliminateFunc.DoEliminate(tempBoomList, BlockManagerInfo.allBlocks);
            //开启下落
            yield return StartCoroutine(Generate());
        }

        /// Items下落
        IEnumerator Generate()
        {
            isOperation = true;

            eliminateFunc.GenerateBlocks(BlockManagerInfo);

            bool isDropOver = false;
            while (!isDropOver)
            {
                foreach (var block in BlockManagerInfo.allBlocks)
                {
                    if (block.isMoving)
                    {
                        yield return null;
                        break;
                    }
                    isDropOver = true;
                }
            }
            CheckAllBlockEliminate();
        }
        public void OperateBlock(Block curBlock, Vector2 dir)
        {
            //点击异常处理
            if (dir.magnitude != 1)
            {
                isOperation = false;
                return;
            }
            //获取目标行列
            int targetRow = curBlock.blockRow + System.Convert.ToInt32(dir.y);
            int targetColumn = curBlock.blockColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = CheckRCLegal(targetRow, targetColumn);
            if (!isLagal)
            {
                isOperation = false;
                //不合法跳出
                return;
            }
            //获取目标
            Block targetBlock = BlockManagerInfo.allBlocks[targetRow, targetColumn];
            if (!targetBlock || !curBlock)
            {
                isOperation = false;
                //Item已经被消除
                return;
            }
            StartCoroutine(BlockExchange(curBlock, targetBlock));

        }
        IEnumerator BlockExchange(Block curBlock, Block targetBlock)
        {
            eliminateFunc.Operation(curBlock, targetBlock);
            while (curBlock.isMoving || curBlock.isMoving)
            {
                yield return null;
            }
            //还原标志位
            bool reduction = false;

            //消除处理
            List<Block> checkBlockList = new List<Block>();
            checkBlockList.Add(curBlock);
            checkBlockList.Add(targetBlock);

            var eliminateList = eliminateFunc.CheckEliminate(checkBlockList, BlockManagerInfo.allBlocks);

            if (eliminateList.Count > 0)
            {
                EliminateBlock(eliminateList);
                reduction = false;
            }
            else
            {
                reduction = true;
            }
            //还原
            if (reduction)
            {

                //临时行列
                int tempRow, tempColumn;
                tempRow = curBlock.blockRow;
                tempColumn = curBlock.blockColumn;
                //移动
                eliminateFunc.Operation(targetBlock, curBlock);

                while (curBlock.isMoving || curBlock.isMoving)
                {
                    yield return null;
                }
                //操作完毕
                isOperation = false;
            }
        }

        /// <summary>
        /// 检测行列是否合法
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        /// <param name="itemRow">Item row.</param>
        /// <param name="itemColumn">Item column.</param>
        public bool CheckRCLegal(int row, int column)
        {
            if (row >= 0 && row < tableRow && column >= 0 && column < tableColumn)
                return true;
            return false;
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }
}