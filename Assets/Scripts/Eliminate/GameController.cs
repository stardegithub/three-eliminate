using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	//单例
	public static GameController instance;
	private ItemManager itemManager; 

	void Awake()
	{
		instance = this;
		itemManager = ItemManager.Instance;
	}

	/// <summary>
	/// 初始化游戏
	/// </summary>
	void Start()
	{
		//初始化游戏
		InitGame ();
	}
	private void InitGame()
	{
		itemManager.InitGame();
	}
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (0);
		}
	}
}