using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 目标数量预制体脚本
/// </summary>
public class TargetCountItem : MonoBehaviour
{
    public int Type;
    public Text Num;
    private void Start()
    {
        //todo:根据不同类型 做初始化
        switch (Type)
        {            
            default:
                break;
        }
        //暂时只修改一下元素颜色
        transform.Find("图片").GetComponent<Image>().color = GameManager.Instance.Colors[Type];
    }

    private void Update()
    {
        //时时更新数量
        Num.text = GameManager.Instance.CurTargetCount[Type].ToString();        
        if (GameManager.Instance.CurTargetCount[Type]<=0)
        {
            Destroy(gameObject);
        }
    }

}
