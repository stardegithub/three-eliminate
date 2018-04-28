using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	/// <summary>
	/// 公共常量
	/// </summary>
	public class Util {
		
		//目录
		public const string ResourcesPrefab = "Prefabs/";
		//文件名称
		public const string Item = "Item";

		//动画参数名称
		public const string Pressed = "Pressed";
		public const string Exit = "Exit";

		//参数
		public static float ItemMoveTime = 0.2f;
		public static float ItemDropTime = 0.2f;

		//item类型
		public enum EItemType
		{
			Default = 0,
			Block,
		}
		public enum EEliminateType
		{
			Ttype = 0,
			Ltype,
		}

	}
