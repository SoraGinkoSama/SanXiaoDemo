using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 根据元素数量 动态调整布局组件的间隔
/// </summary>
public class AdjustSpacing : MonoBehaviour
{
    [Header("不同元素数量时的间隔")]
    public List<float> Spacing;
    private HorizontalLayoutGroup m_HorizontalLayoutGroup;
    private VerticalLayoutGroup m_VerticalLayoutGroup;
    private int m_OldChildCount;
    private void Start()
    {
        m_HorizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        m_VerticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        m_OldChildCount = transform.childCount;
    }
    private void Update()
    {
        if (m_HorizontalLayoutGroup != null && m_OldChildCount != transform.childCount)
        {
            m_OldChildCount = transform.childCount;
            m_HorizontalLayoutGroup.spacing = Spacing[Mathf.Clamp(transform.childCount, 0, Spacing.Count - 1)];
        }
        if (m_VerticalLayoutGroup != null && m_OldChildCount != transform.childCount)
        {
            m_OldChildCount = transform.childCount;
            m_VerticalLayoutGroup.spacing = Spacing[Mathf.Clamp(transform.childCount, 0, Spacing.Count - 1)];
        }
    }

}
