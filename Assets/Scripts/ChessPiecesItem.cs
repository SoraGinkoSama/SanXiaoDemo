using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
/// <summary>
/// 棋子元素
/// </summary>
public class ChessPiecesItem : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    [HideInInspector]public Vector2Int Index;//在数组里的位置
    private Tween m_Tween;//缩放用的tween

    //#设置类型，元素形象的多样化可以在这个属性内实现
    private int m_Type;
    public int Type
    {
        get { return m_Type; }
        set
        {
            m_Type = value;
            //设置一下样式
            //目前仅修改颜色暂用  可以根据需求替换成创建某个预制体到当前物体下(那个预制体取消射线检测，只负责展示形象)
            GetComponent<Image>().color = GameManager.Instance.Colors[value];
        }
    }

    //封装个属性 方便外部使用
    private RectTransform m_RectTrans;
    public RectTransform RectTransform
    {
        get
        {
            if (m_RectTrans == null)
            {
                m_RectTrans = GetComponent<RectTransform>();
            }
            return m_RectTrans;
        }
    }

   

    /// <summary>
    /// 进入元素时调用。如果是第一个元素就选中，如果不是第一个元素，但跟之前元素是同一类型也选中
    /// </summary>
    public void Link()
    {
        //如果没有按下 就无视 主要针对鼠标的情况
        if (!GameManager.Instance.Draging) return;
        if (GameManager.Instance.StepIndex != 1) return;
        //如果已经在数组里 那么就移除掉 并且取消连线 
        if (GameManager.Instance.Links.Contains(this))
        {
            //找到当前元素的索引
            int index = 0;
            foreach (var item in GameManager.Instance.Links)
            {
                if (item == this)
                {
                    break;
                }
                index++;
            }

            //将当前元素后面的都移除出去 以及连线也清除掉
            int count = GameManager.Instance.Links.Count;
            List<Transform> del = new List<Transform>();
            for (int i = index + 1; i < count; i++)
            {
                GameManager.Instance.Links[GameManager.Instance.Links.Count - 1].ResetSelected();
                GameManager.Instance.Links.RemoveAt(GameManager.Instance.Links.Count - 1);
                del.Add(GameManager.Instance.LineItemParent.GetChild(i-1));
            }
            del.ForEach(a => 
            {
                Destroy(a.gameObject);
            });
            GameManager.Instance.CurPos = Index;
            return;
        }

        //如果当前没有选中元素 
        if (GameManager.Instance.CurPos.magnitude>99)
        {
            //将这个元素设置成当前元素
            GameManager.Instance.CurPos = Index;
            //情况数组以及将当前元素加入
            GameManager.Instance.Links.Clear();
            GameManager.Instance.Links.Add(this);
            //调用选中函数
            Selected();
            //#播放音效 之后可以替换成音频管理器对应的播放音效函数
            GameManager.Instance.ShortClipAudio.clip = GameManager.Instance.LinkClip;
            GameManager.Instance.ShortClipAudio.Play();
        }
        else
        {
            //如果当前元素跟之前的元素相邻 且同类型 则连接
            if (Vector2.Distance(Index,GameManager.Instance.CurPos)<2&&GameManager.Instance.EqualType(Index))
            {
                //创建连接线
                GameManager.Instance.CreateLine(Index);
                //设置索引 以及将其加入数组
                GameManager.Instance.CurPos = Index;
                GameManager.Instance.Links.Add(this);
                //调用选中函数
                Selected();
                //#播放音效 之后可以替换成音频管理器对应的播放音效函数
                GameManager.Instance.ShortClipAudio.clip = GameManager.Instance.LinkClip;
                GameManager.Instance.ShortClipAudio.Play();
            }
         
        }
    }

    /// <summary>
    /// 选中时的表现在这里细化
    /// </summary>
    public void Selected()
    {
        m_Tween= transform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 取消选中的效果在这里细化
    /// </summary>
    public void ResetSelected()
    {
        m_Tween.Kill();
        m_Tween = transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
    }
     
    public void OnBeginDrag(PointerEventData eventData)
    {
        //开始拖拽时设置一下标识 以及选中一下元素
        GameManager.Instance.Draging = true;
        Link();
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //拖拽结束 设置一下标识  以及调用清除函数，判断是否能清除
        GameManager.Instance.Draging = false;
        GameManager.Instance.Eliminate();
    }

    /// <summary>
    /// 销毁时调用的函数 可以在这里加销毁相关的效果
    /// </summary>
    public void Destroy()
    {
        //目标数量--
        GameManager.Instance.CurTargetCount[Type]--;
        Destroy(gameObject);
        //如果目标数量已经凑够就胜利
        bool finish = true;
        GameManager.Instance.CurTargetCount.ForEach(a => 
        {
            if (a>0)
            {
                finish = false;
            }
        });
        if (finish)
        {
            GameManager.Instance.Victory();
        }

    }

    private void OnDestroy()
    {
        m_Tween.Kill();
    }

}
