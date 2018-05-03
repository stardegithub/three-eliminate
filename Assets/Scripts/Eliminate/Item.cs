using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Eliminate
{
    public class Item : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        private ItemManager itemManager;
        //按下的鼠标坐标
        private Vector3 downPos;
        //抬起的鼠标坐标
        private Vector3 upPos;
        //被检测
        public bool hasCheck = false;
        public int itemRow;//行
        public int itemColumn;//列
        //public Sprite curSpr;
        public Image curtImg;
        public Util.EItemType curType;
        public Util.EEliminateType curEliminateType;

        public void Awake()
        {
            curtImg = transform.GetChild(0).GetComponent<Image>();
        }
        public void Init(int row, int column, Util.EItemType type)
        {
            var spr = Util.GetSpriteAssetsByType(type);
            itemRow = row;
            itemColumn = column;
            curtImg.sprite = spr;
//            curSpr = curtImg.sprite;
            curType = type;
            curEliminateType = Util.EEliminateType.Default;
        }
        void OnEnable()
        {
            itemManager = ItemManager.Instance;
        }
        public void EmilinateSelf()
        {
            ObjectPool.instance.ResetGameObject(this.gameObject);
            hasCheck = false;
          //  curType = Util.EItemType.Default;
            curEliminateType = Util.EEliminateType.Default;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            downPos = Input.mousePosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //如果其他人正在操作
            if (ItemManager.Instance.isOperation)
                return;//返回
                       //正在操作
            ItemManager.Instance.isOperation = true;
            upPos = Input.mousePosition;
            //获取方向
            Vector2 dir = GetDirection();
            //点击异常处理
            if (dir.magnitude != 1)
            {
                ItemManager.Instance.isOperation = false;
                return;
            }
            //开启协程
            StartCoroutine(ItemExchange(dir));
        }

        /// <summary>
        /// Item交换
        /// </summary>
        /// <returns>The exchange.</returns>
        /// <param name="dir">Dir.</param>
        IEnumerator ItemExchange(Vector2 dir)
        {
            //获取目标行列
            int targetRow = itemRow + System.Convert.ToInt32(dir.y);
            int targetColumn = itemColumn + System.Convert.ToInt32(dir.x);
            //检测合法
            bool isLagal = ItemManager.Instance.CheckRCLegal(targetRow, targetColumn);
            if (!isLagal)
            {
                ItemManager.Instance.isOperation = false;
                //不合法跳出
                yield break;
            }
            //获取目标
            Item target = ItemManager.Instance.allItems[targetRow, targetColumn];
            //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
            Item myItem = ItemManager.Instance.allItems[itemRow, itemColumn];
            if (!target || !myItem)
            {
                ItemManager.Instance.isOperation = false;
                //Item已经被消除
                yield break;
            }
            //相互移动
            target.ItemMove(itemRow, itemColumn, transform.position);
            ItemMove(targetRow, targetColumn, target.transform.position);
            //还原标志位
            bool reduction = false;

            //消除处理
            EliminateFunc func = new EliminateFunc();
            List<Item> checkItemList = new List<Item>();
            checkItemList.Add(this);
            checkItemList.Add(target);
            var eliminateList = func.SelectEliminateItemList(checkItemList, ItemManager.Instance.allItems);
            if (eliminateList.Count > 0)
            {
                ItemManager.Instance.Eliminate(eliminateList);
                reduction = false;
            }
            else
            {
                reduction = true;
            }
            //还原
            if (reduction)
            {
                //延迟
                yield return new WaitForSeconds(Util.ItemMoveTime);
                //临时行列
                int tempRow, tempColumn;
                tempRow = myItem.itemRow;
                tempColumn = myItem.itemColumn;
                //移动
                myItem.ItemMove(target.itemRow, target.itemColumn, target.transform.position);
                target.ItemMove(tempRow, tempColumn, myItem.transform.position);
                //延迟
                yield return new WaitForSeconds(Util.ItemMoveTime);
                //操作完毕
                ItemManager.Instance.isOperation = false;
            }
        }
        /// <summary>
        /// Item的移动
        /// </summary>
        public void ItemMove(int targetRow, int targetColumn, Vector3 pos, bool isNeedMove = true)
        {
            //改行列
            itemRow = targetRow;
            itemColumn = targetColumn;
            //改全局列表
            ItemManager.Instance.allItems[targetRow, targetColumn] = this;
            //移动
            if (isNeedMove)
            {
                transform.DOMove(pos, Util.ItemMoveTime);
            }
        }

        /// <summary>
        /// 获取鼠标滑动方向
        /// </summary>
        /// <returns>The direction.</returns>
        public Vector2 GetDirection()
        {
            //方向向量
            Vector3 dir = upPos - downPos;
            //如果是横向滑动
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                //返回横向坐标
                return new Vector2(dir.x / Mathf.Abs(dir.x), 0);
            }
            else
            {
                //返回纵向坐标
                return new Vector2(0, dir.y / Mathf.Abs(dir.y));
            }
        }
        /// <summary>
        /// 下落
        /// </summary>
        /// <param name="pos">Position.</param>
        public void CurrentItemDrop(Vector3 pos)
        {
            //下落
            transform.DOMove(pos, Util.ItemDropTime);
        }

    }
}
