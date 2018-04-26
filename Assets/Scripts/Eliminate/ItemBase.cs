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
	public Sprite currentSpr;
	public Image currentImg;
	public Util.ItemType currentType;	

	public void Awake()
	{
		currentImg = transform.GetChild (0).GetComponent<Image> ();
	}
	public void Init(int row,int column,Sprite spr)
	{
		itemRow = row;
		itemColumn = column;
		currentImg.sprite = spr;
		currentSpr = currentImg.sprite;

	}

	public virtual void CheckAroundBoom()
	{

	}

	public  virtual bool IsMoveAroundCanEliminate()
	{
		return false;
	}

}


