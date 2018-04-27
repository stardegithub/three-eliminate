using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eliminate{
	public class Item : ItemBase {

		private ItemManager itemManager;
		//被检测
		public bool hasCheck = false;
		void OnEnable()
		{
			itemManager = ItemManager.Instance;
		}
		/// <summary>
		/// 点击事件
		/// </summary>
		public override void CheckAroundBoom()
		{
			itemManager.sameItemsList.Clear ();
			itemManager.boomList.Clear ();
			itemManager.FillSameItemsList (this);
			itemManager.FillBoomList (this);
		}

		public override bool IsMoveAroundCanEliminate()
		{
			itemManager.sameItemsList.Clear ();
			itemManager.boomList.Clear ();
			itemManager.FillSameItemsList (this);
			return itemManager.IsBoomListCanEliminate (this);
		}

		public override void EmilinateSelf()
		{
			ObjectPool.instance.ResetGameObject(this.gameObject);
			hasCheck = false;
		}
	}
}