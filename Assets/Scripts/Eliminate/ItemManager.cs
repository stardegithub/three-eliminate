using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eliminate
{
    public class ItemManager : MonoSingletion<ItemManager>
    {


        //随机图案
        public Sprite[] randomSprites;
        public Transform itemParent;
        //行列
        public int tableRow = 5;
        public int tableColumn = 5;
        //偏移量
        public Vector2 offset = new Vector2(0, 0);
        //所有的Item
        public Item[,] allItems;
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
        private float itemSize = 0;


        void Awake()
        {
            allItems = new Item[tableRow, tableColumn];
            allPos = new Vector3[tableRow, tableColumn];
            // sameItemsList = new List<Item>();
            //boomList = new List<Item>();
            var canvas = Resources.Load<GameObject>("Prefabs/ItemCanvas");
            itemParent = GameObject.Instantiate(canvas).transform.GetChild(0).transform;
        }

        /// <summary>
        /// 获取Item边长
        /// </summary>
        /// <returns>The item size.</returns>
        private float GetItemSize()
        {
            return Resources.Load<GameObject>(Util.ResourcesPrefab + Util.Item).
                GetComponent<RectTransform>().rect.height;
        }

        public void LoadResource()
        {
            randomSprites = new Sprite[4];
            randomSprites[0] = Resources.Load<Sprite>("Texture/Gift");
            randomSprites[1] = Resources.Load<Sprite>("Texture/Health");
            randomSprites[2] = Resources.Load<Sprite>("Texture/LifePreserver");
            randomSprites[3] = Resources.Load<Sprite>("Texture/Strawberry");
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitGame()
        {
            LoadResource();
            //获取Item边长
            itemSize = GetItemSize();
            //生成ITEM
            for (int i = 0; i < tableRow; i++)
            {
                for (int j = 0; j < tableColumn; j++)
                {
                    //生成
                    GameObject currentItem =
                        ObjectPool.instance.GetGameObject(Util.Item, itemParent);
                    //设置坐标
                    currentItem.transform.localPosition =
                        new Vector3(j * itemSize, i * itemSize, 0) + new Vector3(offset.x, offset.y, 0);
                    //随机图案编号
                    int random = Random.Range(0, randomSprites.Length);
                    //获取Item组件
                    Item current = currentItem.GetComponent<Item>();
                    current.Init(i, j, randomSprites[random], Util.EItemType.Default);

                    //保存到数组
                    allItems[i, j] = current;
                    //记录世界坐标
                    allPos[i, j] = currentItem.transform.position;
                }
            }
            AllBoom();
        }
        public void AllBoom()
        {
            EliminateFunc func = new EliminateFunc();
            List<Item> checkItemList = new List<Item>();
            foreach (var item in allItems)
            {
                checkItemList.Add(item);

            }

            var eliminatelist = func.SelectEliminateItemList(checkItemList, allItems);
            Eliminate(eliminatelist);
        }

        public void Eliminate(List<Item> eliminateList)
        {
            //有消除
            bool hasBoom = false;
            if (eliminateList.Count > 0)
            {
                //创	建临时的BoomList
                List<Item> tempBoomList = new List<Item>();
                //转移到临时列表
                tempBoomList.AddRange(eliminateList);
                //开启处理BoomList的协程
                StartCoroutine(ManipulateBoomList(tempBoomList));
                hasBoom = true;
                isOperation = true;
            }

            if (!hasBoom)
            {
                EliminateFunc func = new EliminateFunc();
                if (!func.IsNextCanEliminate(allItems))
                {
                    UpsetItem();
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
        IEnumerator ManipulateBoomList(List<Item> tempBoomList)
        {
            foreach (var item in tempBoomList)
            {
                item.hasCheck = true;
                item.GetComponent<Image>().color = randomColor * 2;
                //离开动画
                //		item.GetComponent<AnimatedButton> ().Exit ();
                //Boom声音
                //			AudioManager.instance.PlayMagicalAudio();
                //将被消除的Item在全局列表中移除
                allItems[item.itemRow, item.itemColumn] = null;
            }
            //检测Item是否已经开发播放离开动画
            // while (!tempBoomList [0].GetComponent<AnimatedButton> ().CheckPlayExit ()) {
            // 	yield return 0;
            // }


            //延迟0.2秒(这里在播放移动动画)
            yield return new WaitForSeconds(0.2f);

            //回收Item
            foreach (var item in tempBoomList)
            {
				item.EmilinateSelf();
                //item.hasCheck = false;
                //ObjectPool.instance.ResetGameObject(item.gameObject);
            }
            //开启下落
            yield return StartCoroutine(ItemsDrop());
            //延迟0.38秒（这里应该是在播放下落）
            //yield return new WaitForSeconds (0.38f);


        }

        /// <summary>
        /// Items下落
        /// </summary>
        /// <returns>The drop.</returns>
        IEnumerator ItemsDrop()
        {
            isOperation = true;
            //逐列检测
            for (int i = 0; i < tableColumn; i++)
            {
                //计数器
                int count = 0;
                //下落队列
                Queue<Item> dropQueue = new Queue<Item>();
                //逐行检测
                for (int j = 0; j < tableRow; j++)
                {
                    if (allItems[j, i] != null)
                    {
                        //计数
                        count++;
                        //放入队列
                        dropQueue.Enqueue(allItems[j, i]);
                    }
                }
                //下落
                for (int k = 0; k < count; k++)
                {
                    //获取要下落的Item
                    Item current = dropQueue.Dequeue();
                    //修改全局数组(原位置情况)
                    allItems[current.itemRow, current.itemColumn] = null;
                    //修改Item的行数
                    current.itemRow = k;
                    //修改全局数组(填充新位置)
                    allItems[current.itemRow, current.itemColumn] = current;
                    //下落
                    current.CurrentItemDrop(allPos[current.itemRow, current.itemColumn]);
                }
            }

            //yield return new WaitForSeconds (0.2f);

            CreateNewItem();
            AllBoom();
            yield return new WaitForSeconds(0.2f);

            //yield break;

        }
        /// <summary>
        /// 生成新的Item
        /// </summary>
        /// <returns>The new item.</returns>
        public void CreateNewItem()
        {
            isOperation = true;
            for (int i = 0; i < tableColumn; i++)
            {
                int count = 0;
                Queue<GameObject> newItemQueue = new Queue<GameObject>();
                for (int j = 0; j < tableRow; j++)
                {
                    if (allItems[j, i] == null)
                    {
                        //生成一个Item
                        GameObject current = ObjectPool.instance.GetGameObject(Util.Item, itemParent);
                        current.transform.position = allPos[tableRow - 1, i];
                        newItemQueue.Enqueue(current);
                        count++;
                    }
                }
                for (int k = 0; k < count; k++)
                {
                    //获取Item组件
                    Item currentItem = newItemQueue.Dequeue().GetComponent<Item>();
                    //随机数
                    int random = Random.Range(0, randomSprites.Length);
                    //修改脚本中的图片
                    currentItem.curSpr = randomSprites[random];
                    //修改真实图片
                    currentItem.curtImg.sprite = randomSprites[random];
                    //获取要移动的行数
                    int r = tableRow - count + k;
                    //移动
                    currentItem.ItemMove(r, i, allPos[r, i]);
                }
            }
        }


        // /// <summary>
        // /// 检测行列是否合法
        // /// </summary>
        // /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        // /// <param name="itemRow">Item row.</param>
        // /// <param name="itemColumn">Item column.</param>
        public bool CheckRCLegal(int itemRow, int itemColumn)
        {
            if (itemRow >= 0 && itemRow < tableRow && itemColumn >= 0 && itemColumn < tableColumn)
                return true;
            return false;
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        public void UpsetItem()
        {
            foreach (var item in allItems)
            {
                if (item != null)
                {
                    //随机图案编号
                    int random = Random.Range(0, randomSprites.Length);
                    //设置图案
                    item.curSpr = randomSprites[random];
                    //设置图片
                    item.curtImg.sprite = randomSprites[random];
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