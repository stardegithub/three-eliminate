using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 公共常量
/// </summary>
public class Util
{

    //目录
    public const string ResourcesPrefab = "Prefabs/";
    //文件名称
    public const string Block = "Block";

    //动画参数名称
    public const string Pressed = "Pressed";
    public const string Exit = "Exit";

    //参数
    public static float BlockMoveTime = 0.2f;
    public static float BlockDropTime = 0.2f;

    public static Dictionary<EBlockType, Sprite> randomSprites = new Dictionary<EBlockType, Sprite>();
    public static Sprite GetSpriteAssetsByType(EBlockType type)
    {
        Sprite sprite = null;
        if (!randomSprites.ContainsKey(type))
        {
            switch (type)
            {
                case EBlockType.Apple:
                    sprite = Resources.Load<Sprite>("Texture/Gift");
                    break;
                case EBlockType.Banana:
                    sprite = Resources.Load<Sprite>("Texture/Health");
                    break;
                case EBlockType.Grape:
                    sprite = Resources.Load<Sprite>("Texture/LifePreserver");
                    break;
                case EBlockType.Lemon:
                    sprite = Resources.Load<Sprite>("Texture/Shield");
                    break;
                case EBlockType.Pear:
                    sprite = Resources.Load<Sprite>("Texture/Strawberry");
                    break;
            }
            randomSprites[type] = sprite;
        }

        return randomSprites[type];
    }
    //block类型
    public enum EBlockType
    {
		//None = -1,
        //普通可消除类型
        Apple = 0,
        Banana,
        Pear,
        Grape,
        Lemon,
		Num,
    }
    public enum EBlockAttribute
    {
        None = 0,
        Ice,

    }
    public enum EEliminateType
    {
        Default = -1,
        Ttype = 0,
        Ltype,
        Itype,
        Xtype,
    }

}
