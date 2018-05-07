using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eliminate
{
    public class BlockManager : MonoSingletion<BlockManager>
    {
        
        IEliminate eliminateFunc;
        //随机图案
        //public Sprite[] randomSprites;
        public Transform blockParent;
        //行列
        public int tableRow = 5;
        public int tableColumn = 5;
        //偏移量
        public Vector2 offset = new Vector2(0, 0);
        //所有的Item
        public Block[,] allBlocks;
        //所有Item的坐标
        public Vector3[,] allPos;
        //相同Item列表
        // public List<Item> sameItemsList;
        //要消除的Item列表
        //public List<Item> boomList;
        //随机颜色
        public Color randomColor;
        //正在操作
        public bool isOperation = false;
        //是否正在执行AllBoom
        public bool allBoom = false;

        //ITEM的边长
        private float BlockSize = 0;


        void Awake()
        {
            allBlocks = new Block[tableRow, tableColumn];
            allPos = new Vector3[tableRow, tableColumn];
            // sameItemsList = new List<Item>();
            //boomList = new List<Item>();
            var canvas = Resources.Load<GameObject>("Prefabs/BlockCanvas");
            blockParent = GameObject.Instantiate(canvas).transform.GetChild(0).transform;

            eliminateFunc = new EliminateFunction();
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

        // public void LoadResource()
        // {
        //     randomSprites = new Sprite[4];
        //     randomSprites[0] = Resources.Load<Sprite>("Texture/Gift");
        //     randomSprites[1] = Resources.Load<Sprite>("Texture/Health");
        //     randomSprites[2] = Resources.Load<Sprite>("Texture/LifePreserver");
        //     randomSprites[3] = Resources.Load<Sprite>("Texture/Strawberry");
        // }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitGame()
        {
            //LoadResource();
            //获取Item边长
            BlockSize = GetBlockSize();
            //生成ITEM
            for (int i = 0; i < tableRow; i++)
            {
                for (int j = 0; j < tableColumn; j++)
                {
                    //生成
                    GameObject currentBlock =
                        ObjectPool.instance.GetGameObject(Util.Block, blockParent);
                    //设置坐标
                    currentBlock.transform.localPosition =
                        new Vector3(j * BlockSize, i * BlockSize, 0) + new Vector3(offset.x, offset.y, 0);
                    //随机图案编号
                    int random = Random.Range(0, (int)Util.EBlockType.Num);
                    //获取Item组件
                    Block current = currentBlock.GetComponent<Block>();
                    current.Init(i, j, (Util.EBlockType)random);

                    //保存到数组
                    allBlocks[i, j] = current;
                    //记录世界坐标
                    allPos[i, j] = currentBlock.transform.position;
                }
            }
            AllBoom();
        }
        public void AllBoom()
        {
            //EliminateFunc func = new EliminateFunc();
            List<Block> checkBlockList = new List<Block>();
            foreach (var block in allBlocks)
            {
                checkBlockList.Add(block);

            }

            var eliminatelist = eliminateFunc.CheckEliminate(checkBlockList, allBlocks);
            Eliminate(eliminatelist);
        }

        public void Eliminate(List<Block> eliminateList)
        {
            //有消除
            bool hasBoom = false;
            if (eliminateList.Count > 0)
            {
                //创	建临时的BoomList
                List<Block> tempBoomList = new List<Block>();
                //转移到临时列表
                tempBoomList.AddRange(eliminateList);
                //开启处理BoomList的协程
                StartCoroutine(ManipulateBoomList(tempBoomList));
                hasBoom = true;
                isOperation = true;
            }

            if (!hasBoom)
            {
                //EliminateFunc func = new EliminateFunc();
                if (!eliminateFunc.CheckImpasse(allBlocks))
                {
                    UpsetBlock();
                    AllBoom();
                    Debug.Log("here  cant eliminate!");
                }
                //操作结束
                isOperation = false;
            }
        }



        /// <summary>
        /// 处理BoomList
        /// </summary>
        /// <returns>The boom list.</returns>
        IEnumerator ManipulateBoomList(List<Block> tempBoomList)
        {
            foreach (var block in tempBoomList)
            {
                block.hasCheck = true;
                block.GetComponent<Image>().color = randomColor * 2;
                //离开动画
                //		item.GetComponent<AnimatedButton> ().Exit ();
                //Boom声音
                //			AudioManager.instance.PlayMagicalAudio();
                //将被消除的Item在全局列表中移除
                allBlocks[block.blockRow, block.blockColumn] = null;
            }
            //检测Item是否已经开发播放离开动画
            // while (!tempBoomList [0].GetComponent<AnimatedButton> ().CheckPlayExit ()) {
            // 	yield return 0;
            // }


            //延迟0.2秒(这里在播放移动动画)
            yield return new WaitForSeconds(0.2f);

            //回收Item
            foreach (var block in tempBoomList)
            {
                block.EmilinateSelf();
                //item.hasCheck = false;
                //ObjectPool.instance.ResetGameObject(item.gameObject);
            }
            //开启下落
            yield return StartCoroutine(BlocksDrop());
            //延迟0.38秒（这里应该是在播放下落）
            //yield return new WaitForSeconds (0.38f);


        }

        /// <summary>
        /// Items下落
        /// </summary>
        /// <returns>The drop.</returns>
        IEnumerator BlocksDrop()
        {
            isOperation = true;
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
                    if (allBlocks[j, i] != null)
                    {
                        //计数
                        count++;
                        //放入队列
                        dropQueue.Enqueue(allBlocks[j, i]);
                    }
                }
                //下落
                for (int k = 0; k < count; k++)
                {
                    //获取要下落的Item
                    Block current = dropQueue.Dequeue();
                    //修改全局数组(原位置情况)
                    allBlocks[current.blockRow, current.blockColumn] = null;
                    //修改Item的行数
                    current.blockRow = k;
                    //修改全局数组(填充新位置)
                    allBlocks[current.blockRow, current.blockColumn] = current;
                    //下落
                    current.CurrentBlockDrop(allPos[current.blockRow, current.blockColumn]);
                }
            }

            //yield return new WaitForSeconds (0.2f);

            CreateNewBlock();
            AllBoom();
            yield return new WaitForSeconds(0.2f);

            //yield break;

        }
        /// <summary>
        /// 生成新的Item
        /// </summary>
        /// <returns>The new item.</returns>
        public void CreateNewBlock()
        {
            isOperation = true;
            for (int i = 0; i < tableColumn; i++)
            {
                int count = 0;
                Queue<GameObject> newBlockQueue = new Queue<GameObject>();
                for (int j = 0; j < tableRow; j++)
                {
                    if (allBlocks[j, i] == null)
                    {
                        //生成一个Item
                        GameObject current = ObjectPool.instance.GetGameObject(Util.Block, blockParent);
                        current.transform.position = allPos[tableRow - 1, i];
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
                    currentBlock.BlockMove(r, i, allPos[r, i]);
                }
            }
        }


        // /// <summary>
        // /// 检测行列是否合法
        // /// </summary>
        // /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        // /// <param name="itemRow">Item row.</param>
        // /// <param name="itemColumn">Item column.</param>
        public bool CheckRCLegal(int row, int column)
        {
            if (row >= 0 && row < tableRow && column >= 0 && column < tableColumn)
                return true;
            return false;
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        public void UpsetBlock()
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }
}