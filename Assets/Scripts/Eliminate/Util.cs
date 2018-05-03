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
    public const string Item = "Item";

    //动画参数名称
    public const string Pressed = "Pressed";
    public const string Exit = "Exit";

    //参数
    public static float ItemMoveTime = 0.2f;
    public static float ItemDropTime = 0.2f;

    public static Dictionary<EItemType, Sprite> randomSprites = new Dictionary<EItemType, Sprite>();
    public static Sprite GetSpriteAssetsByType(EItemType type)
    {
        Sprite sprite = null;
        if (!randomSprites.ContainsKey(type))
        {
            switch (type)
            {
                case EItemType.Apple:
                    sprite = Resources.Load<Sprite>("Texture/Gift");
                    break;
                case EItemType.Banana:
                    sprite = Resources.Load<Sprite>("Texture/Health");
                    break;
                case EItemType.Grape:
                    sprite = Resources.Load<Sprite>("Texture/LifePreserver");
                    break;
                case EItemType.Lemon:
                    sprite = Resources.Load<Sprite>("Texture/Shield");
                    break;
                case EItemType.Pear:
                    sprite = Resources.Load<Sprite>("Texture/Strawberry");
                    break;
            }
            randomSprites[type] = sprite;
        }

        return randomSprites[type];
    }
    //item类型
    public enum EItemType
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
    public enum EItemAttribute
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
