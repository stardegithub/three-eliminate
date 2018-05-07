using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eliminate;
public class GameController : MonoBehaviour {

	//单例
	public static GameController instance;
	private BlockManager blockManager; 

	void Awake()
	{
		instance = this;
		blockManager = BlockManager.Instance;
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
		blockManager.InitGame();
	}
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (0);
		}
	}
}