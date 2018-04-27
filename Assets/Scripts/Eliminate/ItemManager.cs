using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eliminate
{
    public class ItemManager : MonoSingletion<ItemManager>
    {

        // public static ItemManager Instance
        // {
        // 	get {
        // 		if(instance == null)
        // 		{
        // 			instance = new ItemManager();
        // 		}
        // 		return instance;}
        // }
        // //单例
        // private static ItemManager instance;
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
        public List<Item> sameItemsList;
        //要消除的Item列表
        public List<Item> boomList;
        //随机颜色
        public Color randomColor;
        //正在操作
        public bool isOperation = false;
        //是否正在执行AllBoom
        public bool allBoom = false;

        //ITEM的边长
        private float itemSize = 0;

        // public ItemManager()
        // {	
        // 	//instance = this;
        // 	allItems = new Item[tableRow,tableColumn];
        // 	allPos = new Vector3[tableRow,tableColumn];
        // 	sameItemsList = new List<Item> ();
        // 	boomList = new List<Item> ();

        // 	itemParent = Resources.Load<GameObject>("Prefabs/ItemCanvas").transform;
        //  }
        void Awake()
        {
            allItems = new Item[tableRow, tableColumn];
            allPos = new Vector3[tableRow, tableColumn];
            sameItemsList = new List<Item>();
            boomList = new List<Item>();
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
                    current.Init(i, j, randomSprites[random], Util.ItemType.Default);
                    // //设置行列
                    // current.itemRow = i;
                    // current.itemColumn = j;
                    // //设置图案
                    // current.curSpr = randomSprites[random];
                    // //设置图片
                    // current.curtImg.sprite = randomSprites [random];
                    //保存到数组
                    allItems[i, j] = current;
                    //记录世界坐标
                    allPos[i, j] = currentItem.transform.position;
                }
            }
            AllBoom();
        }

        // void Start()
        // {
        // 	//初始化游戏
        // 	InitGame ();
        // 	AllBoom ();
        // }

        public void AllBoom()
        {
            //有消除
            bool hasBoom = false;
            foreach (var item in allItems)
            {
                //指定位置的Item存在，且没有被检测过
                if (item && !item.hasCheck)
                {
                    //检测周围的消除
                    item.CheckAroundBoom();
				    if (boomList.Count > 0)
                    {
				
                        hasBoom = true;
                        isOperation = true;
                    }
                }
            }
			// EliminateFunc func = new EliminateFunc();
			// boomList = func.SelectEliminateItem(allItems);
		
			// if (boomList.Count > 0)
			// {
			// 	//创建临时的BoomList
			// 	List<Item> tempBoomList = new List<Item>();
			// 	//转移到临时列表
			// 	tempBoomList.AddRange(boomList);
			// 	//开启处理BoomList的协程
			// 	StartCoroutine(ManipulateBoomList(tempBoomList));
			// 	hasBoom = true;
			// 	isOperation = true;
			// }

            if (!hasBoom)
            {
                if (!IsNextCanEliminate())
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
        /// 填充相同Item列表  
        /// </summary>
        public void FillSameItemsList(Item current)
        {
            //如果已存在，跳过
            if (sameItemsList.Contains(current))
                return;
            //添加到列表
            sameItemsList.Add(current);
            //上下左右的Item
            Item[] tempItemList = new Item[]{
            GetUpItem(current),GetDownItem(current),
            GetLeftItem(current),GetRightItem(current)};
            for (int i = 0; i < tempItemList.Length; i++)
            {
                //如果Item不合法，跳过
                if (tempItemList[i] == null)
                    continue;
                if (current.curSpr == tempItemList[i].curSpr)
                {
                    FillSameItemsList(tempItemList[i]);
                }
            }
        }

        /// <summary>
        /// 检测牌面是否有可消除
        /// </summary>
        /// <returns><c>true</c>, if can boom, <c>false</c> cant boom.</returns>
        public bool IsNextCanEliminate()
        {
            foreach (var item in allItems)
            {
                if (IsMoveCanEliminate(item, Vector2.up)
                || IsMoveCanEliminate(item, Vector2.down)
                || IsMoveCanEliminate(item, Vector2.left)
                || IsMoveCanEliminate(item, Vector2.right))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测item向dir方向移动一格是否可消除
        /// </summary>
        /// <returns><c>true</c>, if can boom, <c>false</c> cant boom.</returns>
        /// <param name="item">Item.</param>
        /// <param name="dir">vector2.</param>
        public bool IsMoveCanEliminate(Item item, Vector2 dir)
        {
            if (item == null)
            {
                Debug.Log("dsfsd");
            }
            //获取目标行列
            int targetRow = item.itemRow + System.Convert.ToInt32(dir.y);
            int targetColumn = item.itemColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = CheckRCLegal(targetRow, targetColumn);
            if (!isLagal)
            {
                isOperation = false;
                //不合法跳出
                return false;
            }
            //获取目标
            Item target = allItems[targetRow, targetColumn];
            //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
            Item myItem = allItems[item.itemRow, item.itemColumn];
            if (!target || !myItem)
            {
                isOperation = false;
                //Item已经被消除
                return false;
            }
            //相互移动
            target.GetComponent<ItemOperation>().ItemMove(item.itemRow, item.itemColumn, Vector3.zero, false);
            item.GetComponent<ItemOperation>().ItemMove(targetRow, targetColumn, Vector3.zero, false);

            //返回值
            bool isok = true;
            //消除检测	
            isok = item.IsMoveAroundCanEliminate();
            //还原	
            //临时行列
            int tempRow, tempColumn;
            tempRow = myItem.itemRow;
            tempColumn = myItem.itemColumn;
            //移动
            item.GetComponent<ItemOperation>().ItemMove(target.itemRow, target.itemColumn, Vector3.zero, false);
            target.GetComponent<ItemOperation>().ItemMove(tempRow, tempColumn, Vector3.zero, false);

            return isok;
        }

        /// <summary>
        /// 判断待消除列表是否可消除
        /// </summary>
        /// <param name="current">Current item.</param>
        public bool IsBoomListCanEliminate(Item current)
        {
            bool isok = true;
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
                    bool rowCanBoom = CheckItemsInterval(true, current, item);
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
                    bool columnCanBoom = CheckItemsInterval(false, current, item);
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
                boomList.AddRange(rowTempList);
                //横向消除
                horizontalBoom = true;
            }
            //如果纵向三个以上
            if (columnCount > 2)
            {
                if (horizontalBoom)
                {
                    //剔除自己
                    boomList.Remove(current);
                }
                //将临时列表中的Item全部放入BoomList
                boomList.AddRange(columnTempList);
            }

            //如果没有消除对象，返回false
            if (boomList.Count == 0)
            {
                isok = false;
            }
            boomList.Clear();
            return isok;
        }

        /// <summary>
        /// 填充待消除列表
        /// </summary>
        /// <param name="current">Current.</param>

        public void FillBoomList(Item current)
        {
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
                    bool rowCanBoom = CheckItemsInterval(true, current, item);
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
                    bool columnCanBoom = CheckItemsInterval(false, current, item);
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
                boomList.AddRange(rowTempList);
                //横向消除
                horizontalBoom = true;
            }
            //如果纵向三个以上
            if (columnCount > 2)
            {
                if (horizontalBoom)
                {
                    //剔除自己
                    boomList.Remove(current);
                }
                //将临时列表中的Item全部放入BoomList
                boomList.AddRange(columnTempList);
            }
            //如果没有消除对象，返回
            if (boomList.Count == 0)
                return;
            //创建临时的BoomList
            List<Item> tempBoomList = new List<Item>();
            //转移到临时列表
            tempBoomList.AddRange(boomList);
            //开启处理BoomList的协程
            StartCoroutine(ManipulateBoomList(tempBoomList));
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
				item.hasCheck = false;
                ObjectPool.instance.ResetGameObject(item.gameObject);
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
                    current.GetComponent<ItemOperation>().
                        CurrentItemDrop(allPos[current.itemRow, current.itemColumn]);
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
                        GameObject current = //(GameObject)Instantiate(Resources.
                                             //Load<GameObject> (Util.ResourcesPrefab + Util.Item));
                            ObjectPool.instance.GetGameObject(Util.Item, itemParent);
                        //current.SetActive(true);
                        //current.transform.parent = transform;
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
                    currentItem.GetComponent<ItemOperation>().ItemMove(r, i, allPos[r, i]);
                }
            }
        }

        /// <summary>
        /// 检测两个Item之间是否有间隙（图案不一致）
        /// </summary>
        /// <param name="isHorizontal">是否是横向.</param>
        /// <param name="begin">检测起点.</param>
        /// <param name="end">检测终点.</param>
        private bool CheckItemsInterval(bool isHorizontal, Item begin, Item end)
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

        /// <summary>
        /// 获取上方Item
        /// </summary>
        /// <returns>The up item.</returns>
        /// <param name="current">Current.</param>
        private Item GetUpItem(Item current)
        {
            int row = current.itemRow + 1;
            int column = current.itemColumn;
            if (!CheckRCLegal(row, column))
                return null;
            return allItems[row, column];
        }
        /// <summary>
        /// 获取下方Item
        /// </summary>
        /// <returns>The down item.</returns>
        /// <param name="current">Current.</param>
        private Item GetDownItem(Item current)
        {
            int row = current.itemRow - 1;
            int column = current.itemColumn;
            if (!CheckRCLegal(row, column))
                return null;
            return allItems[row, column];
        }
        /// <summary>
        /// 获取左方Item
        /// </summary>
        /// <returns>The left item.</returns>
        /// <param name="current">Current.</param>
        private Item GetLeftItem(Item current)
        {
            int row = current.itemRow;
            int column = current.itemColumn - 1;
            if (!CheckRCLegal(row, column))
                return null;
            return allItems[row, column];
        }
        /// <summary>
        /// 获取右方Item
        /// </summary>
        /// <returns>The right item.</returns>
        /// <param name="current">Current.</param>
        private Item GetRightItem(Item current)
        {
            int row = current.itemRow;
            int column = current.itemColumn + 1;
            if (!CheckRCLegal(row, column))
                return null;
            return allItems[row, column];
        }
        /// <summary>
        /// 检测行列是否合法
        /// </summary>
        /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
        /// <param name="itemRow">Item row.</param>
        /// <param name="itemColumn">Item column.</param>
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