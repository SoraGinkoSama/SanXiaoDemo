using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Ŀ������Ԥ����ű�
/// </summary>
public class TargetCountItem : MonoBehaviour
{
    public int Type;
    public Text Num;
    private void Start()
    {
        //todo:���ݲ�ͬ���� ����ʼ��
        switch (Type)
        {            
            default:
                break;
        }
        //��ʱֻ�޸�һ��Ԫ����ɫ
        transform.Find("ͼƬ").GetComponent<Image>().color = GameManager.Instance.Colors[Type];
    }

    private void Update()
    {
        //ʱʱ��������
        Num.text = GameManager.Instance.CurTargetCount[Type].ToString();        
        if (GameManager.Instance.CurTargetCount[Type]<=0)
        {
            Destroy(gameObject);
        }
    }

}
