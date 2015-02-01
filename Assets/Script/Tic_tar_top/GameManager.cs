using UnityEngine;
using System.Collections;

public class GameManager: MonoBehaviour {
    public GameObject gamePanel;
    public GameObject _Qizi;
    public GameObject Label;
    public int number_N = 15;
    public int Twoheng = 50;
    private int BoxHeight;
    private int BoxWeight;
    private int[,] BoxMatrix;
    private int totalStep;
    private int FiftyFive;
    public static float Xoffset = 16.27f;
    public static float Yoffset = 16.27f;
    private int WuziqiCount;
    public Vector2[] GameStep;
    private bool[, ,] ptable = new bool[15, 15,572];//j记录玩家点对应的组合
    private bool[, ,] ctable = new bool[15, 15,572];//记录Ai点对应的组合
    private int[,] pgrades = new int[15, 15];//记录玩家每个点的权值
    private int[,] cgrades = new int[15, 15];//记录Ai每个点的权值
    private int[,] board = new int[15, 15];
    private int icount;
    private int[,] ZuHeCountNumber = new int[2, 572];
    private int cgrade;
    private int pgrade;
    private int cMaxPoint_x;
    private int cMaxPoint_y;
    private int pMaxPoint_x;
    private int pMaxPoint_Y;
    public  GameObject RestartButton;
    private Vector3 baseVector3;
    enum GameState
    {
        MyAction,
        AiAction,
        GameOver
    }
    enum PositionState
    {
        empty,
        whiteExist,
        blackExist
    }
    GameState gameState = GameState.MyAction;
	// Use this for initialization
	void Start () {
        InitBoard();
        BoxWeight = number_N;
        BoxHeight = number_N;
        FiftyFive = 15;
        GameStep = new Vector2[225];
        BoxMatrix = new int[BoxHeight, BoxWeight];
        WuziqiCount = 0;
        icount = 1;
        cgrade = 0;
        pgrade = 0;

	}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
	// Update is called once per frame
    /// <summary>
    /// 初始化游戏
    /// </summary>
   public void InitBoard()
    {
        //Xoffset = 15.88167f;
        //Yoffset = 15.88167f;
       //生产棋子
        for (int i = 0; i < 15;i++ )
        {
            for (int j = 0; j < 15;j++ )
            {
                GameObject qizi=(GameObject)GameObject.Instantiate(_Qizi);
                qizi.name = i +"_"+ j;
                qizi.transform.parent = _Qizi.transform.parent;
                qizi.transform.localScale = new Vector3(1, 1, 1);
                qizi.transform.localPosition = new Vector3(_Qizi.transform.localPosition.x + Xoffset * j, _Qizi.transform.localPosition.y - Yoffset * i, 0);
                qizi.transform.localScale = new Vector3(0.08f,0.32f, 0f);
                qizi.transform.FindChild("white").GetComponent<UISprite>().active = false;
                qizi.transform.FindChild("black").GetComponent<UISprite>().active = false;

                //Debug.Log("Xoffset:" +Xoffset);
            }
        }
        _Qizi.SetActive(false);
       //初始化权值
        for (int i = 0; i < 15; i++)
            for (int j = 0; j < 15; j++)
            {
                pgrades[i, j] = 0;
                cgrades[i, j] = 0;
                board[i, j] = (int)PositionState.empty;
            }
       // 产生所以的获胜连子组合。共15*10*2+121*2=542种
      //横向上的所有组合
       for(int i=0;i<15;i++)
           for (int j = 0; j < 11; j++)
           {
               for (int k = 0; k < 5; k++)
               {
                  // Debug.Log("i:" + i + "j:" + j + "icount:" + icount);
                   ptable[i, j + k, icount] = true;
                   ctable[i, j + k, icount] = true;
               }
               icount++;
           }
       //纵向上的所有组合
       for (int i = 0; i < 15; i++)
           for (int j = 0; j < 11; j++)
           {
               for (int k = 0; k < 5; k++)
               {
                   //Debug.Log("i:" + i + "j:" + j + "icount:" + icount);
                   ptable[j+k,i, icount] = true;
                   ctable[j+k,i, icount] = true;
               }
               icount++;
           }
       //x=y上的所有组合
       for (int i = 0; i < 11; i++)
           for (int j = 0; j < 11; j++)
           {
               for (int k = 0; k < 5; k++)
               {
                   ptable[i+k, j + k, icount] = true;
                   ctable[i+k, j + k, icount] = true;
               }
               icount++;
           }
       //x=-y上的所有组合
       for (int i = 0; i < 11; i++)
           for (int j = 14; j >=4; j--)
           {
               for (int k = 0; k < 5; k++)
               {
                   //Debug.Log("Icount:" + icount);
                   ptable[i+k, j -k, icount] = true;
                   ctable[i+k, j -k, icount] = true;
               }
               icount++;
           }
       //初始化每个5连子组合的方案里面的目前的含有子数为0；
       //在这里我用0代表人类，1代表电脑
       for(int i=0;i<2;i++)
           for (int j = 0; j < 542; j++)
           {
               ZuHeCountNumber[i, j] = 0;
           }
       Debug.Log("Icount:"+icount);
    }
   public void ButtonPress(GameObject o)
   {
       #region 玩家指令

       //Debug.Log("be clicked");
       if (gameState == GameState.MyAction)
       {
           string[] _strArr = o.name.Split(new char[] { '_' });
           int _row = System.Convert.ToInt32(_strArr[0]);
           int _column = System.Convert.ToInt32(_strArr[1]);
           if (IsNullStep(BoxMatrix, _row, _column))
           {
               GameStep[totalStep] = new Vector2(_row, _column);
               totalStep++;
               BoxMatrix[_row, _column] = 1;
               board[_row, _column] = (int)PositionState.whiteExist;
               //给包含当前点的所有组合加上一个子的计数
               for (int i = 0; i < 572; i++)
               {
                   if (ptable[_row, _column, i] && ZuHeCountNumber[0, i] != 7)
                   {
                       //Debug.Log("before ZuHeCountNumber[0,i]:" + ZuHeCountNumber[0, i]);
                       ZuHeCountNumber[0, i]++;
                      //Debug.Log("later ZuHeCountNumber[0,i]:" + ZuHeCountNumber[0, i]);
                   }
                   if (ctable[_row, _column, i])
                   {
                       ctable[_row, _column, i] = false;
                       ZuHeCountNumber[1, i] = 7;
                   }
               }
               //Debug.Log("BoxMatrix:" + BoxMatrix[_row, _column]); 
               o.transform.FindChild("white").GetComponent<UISprite>().active = true;
               bool IsGameOver = GameLogicInFiveChess(_row, _column, 1);
               if (IsGameOver)
               {
                   gameState = GameState.GameOver;

                   Debug.Log("比赛结束");

               }
               else
               {
                   gameState = GameState.AiAction;
                   Debug.Log("轮到电脑");
               }
           }
       #endregion
           #region AI

           if (gameState==GameState.AiAction)
        {
            _row = 0;
            _column = 0;
            //开始寻找最佳的（电脑是黑子）黑子位置
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    if (board[i, j] == (int)PositionState.empty)
                    {
                        //先找出玩家的最高权值点
                        #region 玩家的权值计算中
                        pgrades[i, j] = 0;

                        if (board[i, j] == (int)PositionState.empty)
                        {
                            for (int k = 0; k < 572; k++)
                            {
                                if (ptable[i, j, k])
                                {
                                    // Debug.Log("玩家当前空余点"+"i:"+i+"j:"+j+"k:"+k);
                                    switch (ZuHeCountNumber[0, k])
                                    {
                                        case 1:
                                            pgrades[i, j] += 5;
                                            break;
                                        case 2:
                                            pgrades[i, j] += 50;
                                            break;
                                        case 3:
                                            pgrades[i, j] += 180;
                                            break;
                                        case 4:
                                            pgrades[i, j] += 400;
                                            break;

                                    }
                                }
                            }
                        }
                        #endregion
                        #region AI的权值计算中
                        cgrades[i, j] = 0;
                        if (board[i, j] == (int)PositionState.empty)
                        {
                            for (int k = 0; k < 572; k++)
                            {
                                if (ctable[i, j, k])
                                {
                                    switch (ZuHeCountNumber[1, k])
                                    {
                                        case 1:
                                            cgrades[i, j] += 5;
                                            break;
                                        case 2:
                                            cgrades[i, j] += 60;
                                            break;
                                        case 3:
                                            cgrades[i, j] += 200;
                                            break;
                                        case 4:
                                            cgrades[i, j] += 500;
                                            break;

                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            //分别找出玩家与AI中权值最大的点。
            #region 分别找出玩家与AI中权值最大的点。
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    if (board[i, j] == (int)PositionState.empty)
                    {
                        if (cgrades[i, j] >= cgrade)
                        {
                            cgrade = cgrades[i, j];
                            cMaxPoint_x = i;
                            cMaxPoint_y = j;
                        }
                        if (pgrades[i, j] >= pgrade)
                        {
                            pgrade = pgrades[i, j];
                            pMaxPoint_x = i;
                            pMaxPoint_Y = j;
                        }
                    }
                }
            #endregion
                    if (cgrade >= pgrade)
                    {
                        _row = cMaxPoint_x;
                        _column = cMaxPoint_y;
                    }
                    else 
                    {
                        _row = pMaxPoint_x;
                        _column = pMaxPoint_Y;
                    }

                //Debug.Log("Ai Clicked");
            //string[] _strArr = o.name.Split(new char[] { '_' });

            #region 生成棋子
                    Debug.Log("Ai found i:" + _row + " j:" + _column + "board[_row, _column] :" + board[_row, _column]);
                    if (board[_row, _column] == (int)PositionState.empty)
                    {
                        pgrade = 0;
                        cgrade = 0;
                        GameStep[totalStep] = new Vector2(_row, _column);
                        totalStep++;
                        BoxMatrix[_row, _column] = -1;

                        board[_row, _column] = (int)PositionState.blackExist;
                        
                        Debug.Log("board[row,column]:" + board[_row, _column]);
                        for (int i = 0; i < 572; i++)
                        {
                            if (ctable[_row, _column, i] && ZuHeCountNumber[1, i] != 7)
                            {
                                //Debug.Log("before ZuHeCountNumber[0,i]:" + ZuHeCountNumber[0, i]);
                                ZuHeCountNumber[1, i]++;
                                //Debug.Log("later ZuHeCountNumber[0,i]:" + ZuHeCountNumber[0, i]);
                            }
                            if (ptable[_row, _column, i])
                            {
                                ptable[_row, _column, i] = false;
                                ZuHeCountNumber[0, i] = 7;
                            }
                        }
                        string name = _row.ToString() + "_" + _column.ToString();
                        GameObject PcChess = GameObject.Find(name);
                        PcChess.transform.FindChild("black").active = true;
                        Debug.Log(PcChess.name);
                        bool IsGameOver = GameLogicInFiveChess(_row, _column, -1);
                        if (IsGameOver)
                        {
                            gameState = GameState.GameOver;
                            Debug.Log("比赛结束");
                        }
                        else
                        {
                            gameState = GameState.MyAction;
                            Debug.Log("轮到人类");
                        }
                    }
            #endregion
            }
       } 
 
	#endregion



    }
    public bool IsNullStep(int[,] _Matrix,int row,int colum)
    {
        if (_Matrix[row,colum]==0)
        {
            return true;
        }
        return false;
    }

    public bool GameLogicInFiveChess(int x, int y,int chessType)
    {

        #region 检查竖向
        int Point_x = x;
        int Point_y = y;
        bool isstop = false;

        WuziqiCount = chessType;
        while (!isstop)
        {

            Point_x--;
            if (Point_x > -1 && Point_x > x - 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:"+BoxMatrix[++Point_x, y]);
                if (BoxMatrix[Point_x, y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        Point_x = x;

        isstop = false;
        while (!isstop)
        {
            Point_x++;
            if (Point_x < number_N && Point_x < x + 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:" + BoxMatrix[--Point_x, y]);
                if (BoxMatrix[Point_x, y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left+Right:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        if (WuziqiCount >= 5)
        {
            Winner(1);
            return true;
        }
        if (WuziqiCount <= -5)
        {
            Winner(2);
            return true;
        }
        if (totalStep == 225)
        {
            Winner(3);
            return true;
        }
        WuziqiCount = chessType;
        #endregion
        #region 检查横向
        Point_x = x;
       Point_y = y;
        isstop = false;


        while (!isstop)
        {

            Point_y--;
            if (Point_y > -1 && Point_y > y - 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:"+BoxMatrix[++Point_x, y]);
                if (BoxMatrix[x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    Debug.Log("left:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        Point_y = y;

        isstop = false;
        while (!isstop)
        {
            Point_y++;
            if (Point_y < number_N && Point_y < y + 5)
            {
               // Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:" + BoxMatrix[--Point_x, y]);
                if (BoxMatrix[x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left+Right:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        if (WuziqiCount >= 5)
        {
            Winner(1);
            return true;
        }
        if (WuziqiCount <= -5)
        {
            Winner(2);
            return true;
        }
        if (totalStep == 225)
        {
            Winner(3);
            return true;
        }
        WuziqiCount = chessType;
        #endregion
        
        #region 检查y=-x
        Point_x = x;
        Point_y = y;
        isstop = false;

        while (!isstop)
        {

            Point_y--;
            Point_x--;
            if (Point_y > -1 && Point_y > y - 5 && Point_x > -1 && Point_x > x - 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:"+BoxMatrix[++Point_x, y]);
                if (BoxMatrix[Point_x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        Point_y = y;
        Point_x = x;
        isstop = false;
        while (!isstop)
        {
            Point_y++;
            Point_x++;
            if (Point_y < number_N && Point_y < y + 5 && Point_x < number_N && Point_x < x + 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);
                // Debug.Log("BoxMatrix:" + BoxMatrix[--Point_x, y]);
                if (BoxMatrix[Point_x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left+Right:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        if (WuziqiCount >= 5)
        {
            Winner(1);
            return true;
        }
        if (WuziqiCount <= -5)
        {
            Winner(2);
            return true;
        }
        if (totalStep == 225)
        {
            Winner(3);
            return true;
        }
        WuziqiCount = chessType;
        #endregion
        #region 检查y=x
        Point_x = x;
        Point_y = y;
        isstop = false;


        while (!isstop)
        {

            Point_y--;
            Point_x = x + y - Point_y;
            if (Point_y > -1 && Point_y > y - 5 && Point_x <number_N && Point_x < x + 5)
            {
                //Debug.Log("Point_x:" + x + " " + "y:" + y);

                if (BoxMatrix[Point_x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }
        Point_y = y;
        Point_x = x;
        isstop = false;
        while (!isstop)
        {
            Point_y++;
            Point_x = x + y - Point_y;
            if (Point_y < number_N && Point_y < y + 5 && Point_x >-1 && Point_x > x - 5)
            {
               // Debug.Log("Point_x:" + Point_x + " " + "y:" + Point_y);

                if (BoxMatrix[Point_x, Point_y] == chessType)
                {
                    WuziqiCount += chessType;
                    //Debug.Log("left+Right:Count" + WuziqiCount);
                }
                else
                    isstop = true;
            }
            else
                isstop = true;
        }

        #endregion
        if (WuziqiCount>=5)
        {
            Winner(1);
            return true;
        }
        if (WuziqiCount <= -5)
        {
            Winner(2);
            return true;
        }
        if (totalStep == 225)
        {
            Winner(3);
            return true;
        }
        return false;
    }
    public void Winner(int winnertype)
    {
        switch (winnertype)
        {
            case 1:
                Label.GetComponent<UILabel>().text = "玩家胜利";

                break;
            case 2:
                Label.GetComponent<UILabel>().text = "电脑胜利";
                break;
            case 3:
                Label.GetComponent<UILabel>().text = "平局";
                break;
            default:
                Label.GetComponent<UILabel>().text = "比赛结束";
                break;
        }
        RestartButton.SetActive(true);
    }
    public void Restart()
    {
        for (int i = 0; i < 15; i++)
            for (int j = 0; j < 15; j++)
            {
                string name = i.ToString() + "_" + j.ToString();
                Destroy(GameObject.Find(name), 0);
            }
        BoxWeight = number_N;
        BoxHeight = number_N;
        FiftyFive = 15;
        GameStep = new Vector2[225];
        BoxMatrix = new int[BoxHeight, BoxWeight];
        WuziqiCount = 0;
        icount = 0;
        cgrade = 0;
        pgrade = 0;
        gameState = GameState.MyAction;
        _Qizi.SetActive(true);
        InitBoard();
        Label.GetComponent<UILabel>().text = "";
        RestartButton.SetActive(false);
    }
}
