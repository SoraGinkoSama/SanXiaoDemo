using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
/// <summary>
/// 游戏管理器
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 面板字段  后续可以读取表格数据来初始化每一关
    [Header("游戏基础设置")]
    public int Column_X;//列数
    public int Row_Y;//行数
    public int Interval_X;//每列间隔
    public int Interval_Y;//每行间隔  
    public int TypeCount;//种类
    public int MinEliminateCount;//最小消除数量
    public int ExtraEliminateCount;//触发横纵列消除的最小数量
    public int Step;//可用步数
    public List<int> TargetCount;//目标数量 索引跟种类id对应  值为零代表这关对这种类型没有消除数量的要求


    public List<Color> Colors;//颜色预设 不得小于种类

    public Transform CheckerboardItemParent;//棋盘元素父物体
    public Transform ChessPiecesItemParent;//棋子元素父物体
    public Transform LineItemParent;//连线元素父物体
    public GameObject CheckerboardItemPrefab;//棋盘元素预制体
    public GameObject ChessPieceItemPrefab;//棋子元素预制体
    public GameObject LinePrefab;//连线预制体
    public GameObject TargetCountItemPrefab;//目标数量提示预制体

    public AudioClip LinkClip;//连接时的音效
    public AudioClip EliminateClip;//消除时的音效
    public AudioSource ShortClipAudio;//播放短音效的音频组件
    public AudioSource LongClipAudio;//播放长音效的音频组件
    public AudioSource BGMAudio;//播放BGM的音频组件
    #endregion

    #region 其他字段
    [HideInInspector] public int CurColumn_X;//此局游戏列数
    [HideInInspector] public int CurRow_Y;//此局游戏行数
    [HideInInspector] public int CurInterval_X;//此局游戏每列间隔
    [HideInInspector] public int CurInterval_Y;//此局游戏每行间隔
    [HideInInspector] public int CurTypeCount;//此局游戏元素种类
    [HideInInspector] public int CurMinEliminateCount;//此局游戏最小消除数量
    [HideInInspector] public int CurExtraEliminateCount;//此局游戏触发横纵列消除的最小数量
    private int m_CurStep;//此局游戏可用步数
    public int CurStep
    {
        get => m_CurStep;
        set
        {
            m_CurStep = value;
            UIManager.Instance.Step = value;
            if (m_CurStep<=0)
            {
                StepIndex = 2;
                UIManager.Instance.Tip.text = "游戏失败，点击任意地方重开";
                UIManager.Instance.Tip.transform.parent.gameObject.SetActive(true);
            }
        }
    }
    [HideInInspector] public List<int> CurTargetCount;//此局游戏目标数量

    [HideInInspector] public Vector2Int CurPos=new Vector2Int(999,0);//当前选中的元素在二维数组里的位置  999,0 代表当前没有选中任何元素
    public CheckerboardItem[,] CheckerboardItems;//存放格子元素的数组 每个格子会绑定一个消除用的元素，因此用这一个数组就可以完成大部分操作
    [HideInInspector]public List<ChessPiecesItem> Links=new List<ChessPiecesItem>();//存放当前连接着的元素  消除或者还原的时候用到
    [HideInInspector] public int StepIndex;//游戏环节  0未开始  1游戏中  2游戏结束

    [HideInInspector] public bool Draging;//拖拽连接中 拖拽中才会选中元素，移动端可能用不太到，主要是鼠标操作时需要使用
    #endregion

    //简单单例
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    //临时用的游戏入口
    private void Start()
    {
        RestartGame();
    }

    /// <summary>
    /// 初始化一局三消游戏
    /// </summary>
    /// <param name="column">列数</param>
    /// <param name="row">行数</param>
    /// <param name="typeCount">元素种类</param>
    /// <param name="minEliminateCount">最小消除数量</param>
    /// <param name="extraEliminateCount">触发横纵列消除的最小数量</param>
    public async void InitGame1(int column, int row, int interval_X, int interval_y, int typeCount, int minEliminateCount, int extraEliminateCount, int step, List<int> targetCount)
    {
        //1.设置数据
        CurColumn_X = column;
        CurRow_Y = row;
        CurInterval_X = interval_X;
        CurInterval_Y = interval_y;
        CurTypeCount = typeCount;
        CurMinEliminateCount = minEliminateCount;
        CurExtraEliminateCount = extraEliminateCount;
        CurStep = step;
        CurTargetCount = new List<int>(TargetCount);//targetCount;
        CurPos = new Vector2Int(999, 0);

        //2.清除可能存在的旧元素
        DestroyOldItem();

        //3.创建目标数量提示
        CreateTargetCountTipItem();

        //4.根据行列数 将棋盘元素创建出来
        CreateCheckerboard();

        //5.填充棋盘
        FillCheckerboard();

        //6.延迟一秒开始游戏
        await Task.Delay(1000);
        StepIndex = 1;

    }

    /// <summary>
    /// 清除之前的旧元素
    /// </summary>
    public void DestroyOldItem()
    {       
        if (CheckerboardItems!=null)
        {
            //遍历棋盘元素数组 删除棋盘元素 以及上面绑定着的棋子元素
            for (int i = 0; i < CheckerboardItems.GetLength(0); i++)
            {
                for (int j = 0; j < CheckerboardItems.GetLength(1); j++)
                {
                    if (CheckerboardItems[i,j]!=null)
                    {
                        if (CheckerboardItems[i, j].ChessPiecesItem != null)
                        {
                            Destroy(CheckerboardItems[i, j].ChessPiecesItem.gameObject);
                        }
                        Destroy(CheckerboardItems[i, j].gameObject);
                    }
                    CheckerboardItems[i, j] = null;

                }
            }
        }
        //清理一下可能存在的 目标数量提示
        foreach (Transform item in UIManager.Instance.TargetCountTipParent)
        {
            Destroy(item.gameObject);
        }
        
    }

    /// <summary>
    /// 创建棋盘
    /// </summary>
    public void CreateCheckerboard()
    {
        //1.初始化数组      
        CheckerboardItems = new CheckerboardItem[CurColumn_X, CurRow_Y];

        //2.计算一下起始点位  这里设定是左下角为0 0 元素
        float startX = 0;
        if (CurColumn_X % 2 == 1)
        {
            //如果列数是奇数 那么最左侧元素的x值是 负的 列数的一半向下取整 * 元素间隔
            startX = -Mathf.FloorToInt(CurColumn_X / 2) * CurInterval_X;
        }
        else
        {
            //如果列数是偶数 那么最左侧元素的x值是 负的 (列数的一半 * 元素间隔  - 一半的元素间隔)
            startX = -(CurColumn_X / 2 * CurInterval_X - CurInterval_X / 2);
        }
        float startY = 0;
        if (CurRow_Y % 2 == 1)
        {
            startY = -Mathf.FloorToInt(CurRow_Y / 2) * CurInterval_Y;
        }
        else
        {
            startY = -CurRow_Y / 2 * CurInterval_Y - CurInterval_Y / 2;
        }

        //3.从起始点开始 按顺序将元素创建出来并且放入数组里
        for (int i = 0; i < CurColumn_X; i++)
        {
            for (int j = 0; j < CurRow_Y; j++)
            {
                //创建棋盘元素到父物体下
                GameObject go = Instantiate(CheckerboardItemPrefab, CheckerboardItemParent);
                //设置位置
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + Interval_X * i, startY + Interval_Y * j);
                //设置元素数据
                CheckerboardItem item = go.GetComponent<CheckerboardItem>();
                item.Index = new Vector2Int(i, j);
                //放入数组
                CheckerboardItems[i, j] = item;
            }
        }

    }

    /// <summary>
    /// 填充棋盘
    /// </summary>
    public void FillCheckerboard()
    {        
        //每列单独计算 因为有的列缺的多 有的列缺的少 所以各算各的更加自然
        for (int i = 0; i < CurColumn_X; i++)
        {          
            StartCoroutine(FillColumn(i));
        }
    }

    /// <summary>
    /// 填充一列的逻辑  #此游戏最核心的逻辑 #此游戏最核心的逻辑 #此游戏最核心的逻辑
    /// </summary>
    /// <param name="columnIndex">要填充的列的索引</param>
    /// <returns></returns>
    public IEnumerator FillColumn(int columnIndex)
    {
        //如果这一列里面还有空
        while (HaveEmptyItem(columnIndex))
        {
            //从倒数第二行开始 如果下面的格子没有元素 如果当前的格子有元素 就将当前格子的元素下移
            for (int i = 1; i < CurRow_Y; i++)
            {
                //如果下一行元素不为空 或者当前行元素为空 那么说明没有移动的必要 就遍历后面行
                if (CheckerboardItems[columnIndex, i - 1].ChessPiecesItem != null || CheckerboardItems[columnIndex, i].ChessPiecesItem == null)
                {
                    //#如果是最上面那行，且那行为空，就创建一个元素填充
                    if (i == CurRow_Y - 1 && CheckerboardItems[columnIndex, i].ChessPiecesItem == null)
                    {
                        FillItem(columnIndex);
                    }
                    continue;
                }

                //否则就将当前格元素下移
                //1.将当前格的棋子赋值给下面格子 2.将当前格子的棋子置空 3.用tween动画让棋子下移 并且更新元素的索引
                CheckerboardItems[columnIndex, i - 1].ChessPiecesItem = CheckerboardItems[columnIndex, i].ChessPiecesItem;
                CheckerboardItems[columnIndex, i].ChessPiecesItem = null;
                CheckerboardItems[columnIndex, i - 1].ChessPiecesItem.RectTransform.DOAnchorPosY(CheckerboardItems[columnIndex, i - 1].RectTransform.anchoredPosition.y, 0.1f).SetEase(Ease.Linear);//#可以在这里调整动画效果，速度，曲线等
                CheckerboardItems[columnIndex, i - 1].ChessPiecesItem.Index = CheckerboardItems[columnIndex, i - 1].Index;

                //如果是最上一行 就创建一个元素填充 因为元素被填充到下面去了
                if (i == CurRow_Y - 1)
                {
                    FillItem(columnIndex);
                }
            }
            yield return new WaitForSeconds(0.1f);//下落一格用时 跟上面的移动动画要匹配起来
        }
    }

    /// <summary>
    /// 检测是否目标列里元素有空格
    /// </summary>
    /// <param name="columnIndex">要检测的列的索引</param>
    /// <returns></returns>
    public bool HaveEmptyItem(int columnIndex)
    {
        for (int i = 0; i < CurRow_Y; i++)
        {
            if (CheckerboardItems[columnIndex, i].ChessPiecesItem==null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 创建一个棋子  #棋子元素的多态可以在这个函数里实现    #棋子元素的多态可以在这个函数里实现   #棋子元素的多态可以在这个函数里实现   
    /// </summary>
    /// <param name="column">创建在哪一列</param>
    public void FillItem(int column)
    {
        //随机抽取一种类型  如果有需求 可以修改一下随机的逻辑，让他按照一些设定的规则来随机 
        int type = Random.Range(0, CurTypeCount);

        //创建一个棋子元素到对应的父物体下
        GameObject go= Instantiate(ChessPieceItemPrefab, ChessPiecesItemParent);
        ChessPiecesItem item= go.GetComponent<ChessPiecesItem>();
        item.Type = type;//设置类型
        //将对象设置到第一行上面一些的位置 然后用tween动画移动到第一行的位置
        Vector2 pos = CheckerboardItems[column, CurRow_Y - 1].RectTransform.anchoredPosition;
        item.RectTransform.anchoredPosition =pos + new Vector2(0, 100);//#如果有需要，可以调整这里的偏移量以及下面的动画效果
        item.RectTransform.DOAnchorPosY(pos.y, 0.1f).SetEase(Ease.Linear);

        //跟棋盘绑定 并且设置索引
        CheckerboardItems[column, CurRow_Y - 1].ChessPiecesItem = item;
        CheckerboardItems[column, CurRow_Y - 1].ChessPiecesItem.Index = CheckerboardItems[column, CurRow_Y - 1].Index;

    }

    /// <summary>
    /// 判断目标格子的元素与当前格子是否是同类型元素  同类型元素可以连接  不同类型元素会无视
    /// </summary>
    /// <param name="index">目标格子索引</param>
    /// <returns></returns>
    public bool EqualType(Vector2Int index)
    {
        if (CheckerboardItems[index.x,index.y].ChessPiecesItem.Type==CheckerboardItems[CurPos.x,CurPos.y].ChessPiecesItem.Type)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 创建连接线的函数
    /// </summary>
    /// <param name="index">目标格子的索引</param>
    public void CreateLine(Vector2Int index)
    {
        //实例化一个线的预制体到对应父物体下
        GameObject go = Instantiate(LinePrefab, LineItemParent);
        //设置到当前格子的位置 并且让他指向目标格子
        go.GetComponent<RectTransform>().anchoredPosition = CheckerboardItems[CurPos.x, CurPos.y].RectTransform.anchoredPosition;
        go.transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, index - CurPos));
        //设置颜色
        go.GetComponent<Image>().color = Colors[CheckerboardItems[CurPos.x, CurPos.y].ChessPiecesItem.Type];

    }

    /// <summary>
    /// 消除逻辑
    /// </summary>
    public void Eliminate()
    {
        //如果被连接的元素大于等于最小消除数
        if (Links.Count>=CurMinEliminateCount)
        {
            //扣除步数
            CurStep--;

            //如果连接的数量达到给定数量 还会销毁最后连接的元素所在的行和列
            if (Links.Count>=CurExtraEliminateCount)
            {
                Vector2Int index = Links[Links.Count - 1].Index;
                //遍历数组 行或者列匹配上的就加入连接数组 等会儿一起消除
                for (int i = 0; i < CurColumn_X; i++)
                {
                    for (int j = 0; j < CurRow_Y; j++)
                    {
                        if (i==index.x||j==index.y)
                        {
                            if (CheckerboardItems[i, j].ChessPiecesItem!=null&&!Links.Contains(CheckerboardItems[i,j].ChessPiecesItem))
                            {
                                Links.Add(CheckerboardItems[i, j].ChessPiecesItem);
                            }
                        }
                        
                    }
                }
            }

            //#临时用的播放音效的逻辑，之后可以用音效管理器的函数替代
            LongClipAudio.clip = EliminateClip;
            LongClipAudio.Play();

            //销毁被连接的元素
            Links.ForEach(a => 
            {
                //置空引用
                CheckerboardItems[a.Index.x, a.Index.y].ChessPiecesItem = null;
                //销毁元素
                a.Destroy();
            });

            //调用填充元素函数
            FillCheckerboard();
          
        }
        //如果数量不足 则会将元素重置回去 并且清空数组
        else
        {
            Links.ForEach(a => 
            {
                a.ResetSelected();
            });
            Links.Clear();
        }

        //不论是否满足消除条件 都会重置索引 以及销毁全部的连接线
        CurPos = new Vector2Int(999, 0);
        foreach (Transform item in LineItemParent.transform)
        {
            Destroy(item.gameObject);
        }
        
    }

    /// <summary>
    /// 创建任务数量提示
    /// </summary>
    public void CreateTargetCountTipItem()
    {
        int index = 0;
        CurTargetCount.ForEach(a => 
        {
            if (a!=0)
            {
                GameObject go= Instantiate(TargetCountItemPrefab, UIManager.Instance.TargetCountTipParent);
                go.GetComponent<TargetCountItem>().Type = index;
            }
            index++;
        });
    }

    private void Update()
    {
        switch (StepIndex)
        {
            case 0:
                break;
            case 1:
                break;
            //按下任意键 或者点击鼠标左右键就重开游戏
            case 2:
                if (Input.anyKeyDown||Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1))
                {
                    RestartGame();
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 重开游戏
    /// </summary>
    private void RestartGame()
    {
        StepIndex = 0;
        //拿界面上的配置 初始化一局游戏
        InitGame1(Column_X, Row_Y, Interval_X, Interval_Y, TypeCount, MinEliminateCount, ExtraEliminateCount, Step, TargetCount);
        UIManager.Instance.Tip.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 游戏胜利
    /// </summary>
    public void Victory()
    {
        if (StepIndex == 2) return;
        StepIndex = 2;
        UIManager.Instance.Tip.text = "游戏胜利，点击任意地方重开游戏";
        UIManager.Instance.Tip.transform.parent.gameObject.SetActive(true);
    }

}
