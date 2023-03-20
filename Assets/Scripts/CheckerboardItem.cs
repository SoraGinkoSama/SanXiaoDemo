using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 棋盘元素
/// </summary>
public class CheckerboardItem : MonoBehaviour
{
    [HideInInspector] public Vector2Int Index;//在数组里的位置
    [HideInInspector] public ChessPiecesItem ChessPiecesItem;//格子上的棋子元素的引用

    //封装一个属性，方便外部使用
    private RectTransform m_RectTrans;
    public RectTransform RectTransform
    {
        get
        {
            if (m_RectTrans==null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            return m_RectTrans;
        }
    }

}
