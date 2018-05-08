using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Eliminate
{
    public class Block : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        private BlockManager blockManager;
        //按下的鼠标坐标
        private Vector3 downPos;
        //抬起的鼠标坐标
        private Vector3 upPos;
        //被检测
        public bool hasCheck = false;
        public int blockRow;//行
        public int blockColumn;//列
        //public Sprite curSpr;
        public Image curtImg;
        public Util.EBlockType curType;
        public Util.EEliminateType curEliminateType;
        public bool isMoving = false;

        public void Awake()
        {
            curtImg = transform.GetChild(0).GetComponent<Image>();
        }
        public void Init(int row, int column, Util.EBlockType type)
        {
            var spr = Util.GetSpriteAssetsByType(type);
            blockRow = row;
            blockColumn = column;
            curtImg.sprite = spr;
            //            curSpr = curtImg.sprite;
            curType = type;
            curEliminateType = Util.EEliminateType.Default;
        }
        void OnEnable()
        {
            blockManager = BlockManager.Instance;
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
            if (BlockManager.Instance.isOperation || isMoving)
                return;//返回
                       //正在操作
            BlockManager.Instance.isOperation = true;
            upPos = Input.mousePosition;

            //获取方向
            Vector2 dir = GetDirection();
            BlockManager.Instance.OperateBlock(this, dir);
        }

        /// <summary>
        /// Item的移动
        /// </summary>
        public void BlockMove(int targetRow, int targetColumn, Vector3 pos)
        {
            isMoving = true;
            BlockChange(targetRow, targetColumn);
            transform.DOMove(pos, Util.BlockMoveTime).OnComplete(delegate(){
                isMoving = false;
            });
        }

        public void BlockChange(int targetRow, int targetColumn)
        {
            //改行列
            blockRow = targetRow;
            blockColumn = targetColumn;
            BlockManager.Instance.BlockManagerInfo.allBlocks[targetRow, targetColumn] = this;
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
        public void CurrentBlockDrop(Vector3 pos)
        {
            isMoving = true;
            //下落
            transform.DOMove(pos, Util.BlockDropTime).OnComplete(delegate(){
                isMoving = false;
            });
        }

        //  /// <summary>
        // /// Bolck交换
        // /// </summary>
        // /// <returns>The exchange.</returns>
        // /// <param name="dir">Dir.</param>
        // IEnumerator BlockExchange(Vector2 dir)
        // {
        //     //获取目标行列
        //     int targetRow = blockRow + System.Convert.ToInt32(dir.y);
        //     int targetColumn = blockColumn + System.Convert.ToInt32(dir.x);
        //     //检测合法
        //     bool isLagal = BlockManager.Instance.CheckRCLegal(targetRow, targetColumn);
        //     if (!isLagal)
        //     {
        //         BlockManager.Instance.isOperation = false;
        //         //不合法跳出
        //         yield break;
        //     }
        //     //获取目标
        //     Block target = BlockManager.Instance.BlockManagerInfo.allBlocks[targetRow, targetColumn];
        //     //从全局列表中获取当前item，查看是否已经被消除，被消除后不能再交换
        //     Block myBlock = BlockManager.Instance.BlockManagerInfo.allBlocks[blockRow, blockColumn];
        //     if (!target || !myBlock)
        //     {
        //         BlockManager.Instance.isOperation = false;
        //         //Item已经被消除
        //         yield break;
        //     }
        //     //相互移动
        //     target.BlockMove(blockRow, blockColumn, transform.position);
        //     BlockMove(targetRow, targetColumn, target.transform.position);
        //     //还原标志位
        //     bool reduction = false;

        //     //消除处理
        //     EliminateFunc func = new EliminateFunc();
        //     List<Block> checkBlockList = new List<Block>();
        //     checkBlockList.Add(this);
        //     checkBlockList.Add(target);
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
        //         yield return new WaitForSeconds(Util.BlockMoveTime);
        //         //临时行列
        //         int tempRow, tempColumn;
        //         tempRow = myBlock.blockRow;
        //         tempColumn = myBlock.blockColumn;
        //         //移动
        //         myBlock.BlockMove(target.blockRow, target.blockColumn, target.transform.position);
        //         target.BlockMove(tempRow, tempColumn, myBlock.transform.position);
        //         //延迟
        //         yield return new WaitForSeconds(Util.BlockMoveTime);
        //         //操作完毕
        //         BlockManager.Instance.isOperation = false;
        //     }
        // }
    }
}
