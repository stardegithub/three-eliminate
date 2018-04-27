using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// item基类
/// </summary>
public class ItemBase : MonoBehaviour {
	public int itemRow;//行
	public int itemColumn;//列
	public Sprite curSpr;
	public Image curtImg;
	public Util.ItemType curType;	

	public void Awake()
	{
		curtImg = transform.GetChild (0).GetComponent<Image> ();
	}
	public void Init(int row,int column,Sprite spr,Util.ItemType type)
	{
		itemRow = row;
		itemColumn = column;
		curtImg.sprite = spr;
		curSpr = curtImg.sprite;
		curType = type;
	}

	public virtual void CheckAroundBoom()
	{

	}

	public virtual bool IsMoveAroundCanEliminate()
	{
		return false;
	}

	public virtual void EmilinateSelf()
	{

	}

}


