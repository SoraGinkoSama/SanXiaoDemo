using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 简单UI管理器
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {        
        Instance = this;
    }
    [Tooltip("剩余步数")]
    public Text StepText;
    private int m_Step;
    public int Step
    {
        get => m_Step;
        set
        {
            m_Step = value;
            StepText.text = value.ToString();
        }
    }
    [Tooltip("目标数量父物体")]
    public Transform TargetCountTipParent;
    [Tooltip("测试用的提示")]
    public Text Tip;

    private void Start()
    {
        Tip.transform.parent.gameObject.SetActive(false);
    }

}
