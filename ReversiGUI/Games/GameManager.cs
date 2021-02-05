using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReversiGUI.Boards;
using ReversiGUI.AI;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Runtime.InteropServices;
using System.Windows.Shapes;

namespace ReversiGUI.Games
{

    delegate void OnClickPos(int x, int y);

    enum DIFFICULTY
    {
        EASY,
        NORMAL,
        HARD,
        PRO,
        CUSTOM
    }

    enum GAMEMODE
    {
        NOTSET,
        PVP,
        PVC
    }

    enum COM_VALUE_MODE
    {
        V_POSITION,
        V_PATTERNLINER,
        V_STONEDIFF,
    }

    enum Com_Search_Mode
    {
        S_MINIMAX,
        S_ALPHABETA,
        S_NEGASCOUT,
        S_IDDFS,
        S_EZ,
        S_MTDF
    }

    class GameManager
    {
        private GAMEMODE Gamemode = GAMEMODE.NOTSET;
        private Board MainBoard;
        private Com com;
        private ScrollingLog LogWindow;
        private ProgressRing comProgRing;
        private Rectangle putCursor;
        private ToggleSwitch humanColorSwitch;
        private ToggleSwitch comInfoSwitch;
        private bool canInput = false;
        private int color;
        private int cpuColor = Const.WHITE;

        public bool showMob = true;

        public GameManager(Grid grid, ControlTemplate resource, RichTextBox textBox, Label wText, Label bText, ProgressRing ring, Rectangle cursor, ToggleSwitch humanColorToggle, ToggleSwitch comInfo)
        {
            //ログは最優先で生成
            LogWindow = new ScrollingLog(textBox);
            MainBoard = new Board(grid, resource, PosInput, wText, bText);
            com = new Com();
            comProgRing = ring;
            comProgRing.IsActive = false;
            putCursor = cursor;
            humanColorSwitch = humanColorToggle;
            comInfoSwitch = comInfo;
        }
        
        public void Start(GAMEMODE StartMode, int mid, int end, DIFFICULTY difficulty = DIFFICULTY.NORMAL)
        {
            Gamemode = StartMode;
            LogWindow.Clear();
            LogWindow.LogAlart("Game Start:モード"+Gamemode.ToString()+"で新しいマッチを開始します");
            if(Gamemode == GAMEMODE.PVC)
            {
                LogWindow.LogWarning("難易度は"+difficulty.ToString()+"です(歯車アイコンから変更可能)");
                LogWindow.LogWarning("中盤先読み:"+mid+" 読み切り:"+end);
                switch (difficulty)
                {
                    case DIFFICULTY.EASY:
                        com.SetParam(mid, end, 4, 2, 1, (int)COM_VALUE_MODE.V_POSITION, (int)Com_Search_Mode.S_ALPHABETA);
                        break;
                    case DIFFICULTY.NORMAL:
                        com.SetParam(mid, end, 8, 2, 1, (int)COM_VALUE_MODE.V_POSITION, (int)Com_Search_Mode.S_IDDFS);
                        break;
                    case DIFFICULTY.HARD:
                        com.SetParam(mid, end, 14, 2, 1, (int)COM_VALUE_MODE.V_PATTERNLINER, (int)Com_Search_Mode.S_IDDFS);
                        break;
                    case DIFFICULTY.PRO:
                        com.SetParam(mid, end, 0, 4, 1, (int)COM_VALUE_MODE.V_PATTERNLINER, (int)Com_Search_Mode.S_IDDFS);
                        break;
                    case DIFFICULTY.CUSTOM:
                        com.SetParam(mid, end, 0, 4, 1, (int)COM_VALUE_MODE.V_PATTERNLINER, (int)Com_Search_Mode.S_IDDFS);
                        break;
                }
            }
            MainBoard.Reset();
            ResetGame();
        }

        private int OppColor(int color)
        {
            return 1-color;
        }

        private string GetColorName(int color)
        {
            if (color==Const.BLACK) {
                return "黒";
            }
            else
            {
                return "白";
            }
        }

        public void PosInput(int x, int y)
        {
            switch (Gamemode)
            {
                case GAMEMODE.PVP:
                    PVP_Update(x, y);
                    break;
                case GAMEMODE.PVC:
                    PVC_Update(x, y);
                    break;
            }
        }

        public void OnMobShowChange(bool IsChecked)
        {
            showMob = IsChecked;
            MainBoard.ReDraw(showMob, color);
        }

        public void Undo()
        {
            if (canInput)
            {
                int tmpColor = OppColor(color);
                //手番が変わるまで戻す
                while (tmpColor == OppColor(color))
                {
                    tmpColor = MainBoard.Undo();
                }
                if (tmpColor == -1)
                {
                    LogWindow.LogWarning("Undo:これ以上戻せません");
                }
                else
                {
                    color = tmpColor;
                    LogWindow.LogAlart("Undo:戻しました");
                    MainBoard.ReDraw(showMob, color);
                }
                //カーソル非表示
                putCursor.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void GameEnd()
        {
            //盤面表示更新
            MainBoard.ReDraw(showMob, color);
            LogWindow.LogError("###############試合終了!!################");
            LogWindow.Log("黒:"+MainBoard.Count[Const.BLACK]+" 対 白:"+MainBoard.Count[Const.WHITE]);
            if (cpuColor == Const.NOCOLOR)
            {
                if (MainBoard.Count[Const.BLACK] > MainBoard.Count[Const.WHITE])
                {
                    LogWindow.LogAlart("黒の勝ち!!");
                }
                else if (MainBoard.Count[Const.BLACK] < MainBoard.Count[Const.WHITE])
                {
                    LogWindow.LogAlart("白の勝ち!!");
                }
                else
                {
                    LogWindow.LogAlart("引き分け!!");
                }
            }
            else
            {
                if (MainBoard.Count[cpuColor] > MainBoard.Count[OppColor(cpuColor)])
                {
                    LogWindow.LogAlart("CPUの勝ちです!!");
                }
                else if (MainBoard.Count[cpuColor] < MainBoard.Count[OppColor(cpuColor)])
                {
                    LogWindow.LogAlart("あなたの勝ちです!!");
                }
                else
                {
                    LogWindow.LogAlart("引き分け!!");
                }
            }
        }

        private void ResetGame()
        {
            canInput = true;
            color = Const.BLACK;
            MainBoard.ReDraw(showMob, color);
            putCursor.Visibility = System.Windows.Visibility.Hidden;
            if (Gamemode == GAMEMODE.PVC) {
                if ((bool)humanColorSwitch.IsChecked)
                {
                    cpuColor = Const.WHITE;
                }
                else
                {
                    cpuColor = Const.BLACK;
                    PVC_Update();
                }
            }
            else
            {
                cpuColor = Const.NOCOLOR;
            }
        }

        private void PVP_Update(int x, int y)
        {
            int pos = 8 * y + x;

            if (canInput)
            {
                //着手可能なら
                if (MainBoard.IsLegal(pos))
                {
                    //着手
                    if (MainBoard.Put(pos))
                    {
                        putCursor.Visibility = System.Windows.Visibility.Visible;
                        putCursor.SetValue(Grid.RowProperty, y + 1);
                        putCursor.SetValue(Grid.ColumnProperty, x + 1);
                        if (color == Const.BLACK)
                        {
                            LogWindow.LogBlack("Put:" + GetColorName(color) + "が着手しました(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                        }
                        else
                        {
                            LogWindow.LogWhite("Put:" + GetColorName(color) + "が着手しました(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                        }
                    }
                    else
                    {
                        LogWindow.LogWarning("Put:" + GetColorName(color) + "が着手できませんでした(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                    }
                    //色反転
                    color = OppColor(color);

                    //次の人が置ける場所がない!!
                    if (MainBoard.GetMobility(color) == 0)
                    {
                        //互いにないならゲーム終了
                        if (MainBoard.GetMobility(OppColor(color)) == 0)
                        {
                            GameEnd();
                            return;
                        }
                        if (color == Const.BLACK)
                        {
                            LogWindow.LogBlack("黒:置ける場所がないorz...パスです");
                        }
                        else
                        {
                            LogWindow.LogWhite("白:置ける場所がないorz...パスです");
                        }
                        //色反転
                        color = OppColor(color);
                    }
                    //盤面表示更新
                    MainBoard.ReDraw(showMob, color);
                }
            }
        }

        private void PVC_Update(int x=0, int y=0)
        {
            if (canInput)
            {
                LogWindow.WriteHorizon();
                if (color == cpuColor)
                {
                    PVC_Computer();
                }
                else
                {
                    PVC_Human(x, y);
                }
            }
        }

        private void PVC_Human(int x, int y)
        {

            int pos = 8 * y + x;

            //着手可能なら
            if (MainBoard.IsLegal(pos))
            {
                //着手
                if (MainBoard.Put(pos))
                {
                    putCursor.Visibility = System.Windows.Visibility.Visible;
                    putCursor.SetValue(Grid.RowProperty, y + 1);
                    putCursor.SetValue(Grid.ColumnProperty, x + 1);
                    if (color == Const.BLACK)
                    {
                        LogWindow.LogBlack("Put:" + GetColorName(color) + "が着手しました(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                    }
                    else
                    {
                        LogWindow.LogWhite("Put:" + GetColorName(color) + "が着手しました(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                    }
                }
                else
                {
                    LogWindow.LogWarning("Put:" + GetColorName(color) + "が着手できませんでした(" + (char)(x + 'A') + ", " + (y + 1) + ")");
                }
                //色反転
                color = OppColor(color);

                //次の人が置ける場所がない!!
                if (MainBoard.GetMobility(color) == 0)
                {
                    //互いにないならゲーム終了
                    if (MainBoard.GetMobility(OppColor(color)) == 0)
                    {
                        GameEnd();
                        return;
                    }
                    if (color == Const.BLACK)
                    {
                        LogWindow.LogBlack("CPU黒:置ける場所がないorz...パスです");
                    }
                    else
                    {
                        LogWindow.LogWhite("CPU白:置ける場所がないorz...パスです");
                    }
                    //色反転
                    color = OppColor(color);
                }
                else
                {
                    //CPUがパスでないならもう一度更新
                    PVC_Update();
                }
                //盤面表示更新
                MainBoard.ReDraw(showMob, color);
            }
            else
            {
                LogWindow.LogError("(" + (char)(x + 'A') + ", " + (y + 1) + ")には置けません");
            }
        }

        private async void PVC_Computer()
        {
            int pos;
            int x, y;
            double value = -1;
            var sw = new System.Diagnostics.Stopwatch();
            
            comProgRing.IsActive = true;
            canInput = false;
            sw.Start();
            pos = await Task<UInt64>.Run(() =>
            {
                return com.GetNextMoveAsync(color, out value);
            });
            sw.Stop();
            comProgRing.IsActive = false;

            TimeSpan time = sw.Elapsed;

            if (pos == Define.FAIL)
            {
                LogWindow.LogError("CPUエラー終了:CPUの設定ができていないかもしれません");
            }

            x = pos % 8+1;
            y = pos / 8+1;
            //着手
            if (MainBoard.Put(pos))
            {
                putCursor.Visibility = System.Windows.Visibility.Visible;
                putCursor.SetValue(Grid.ColumnProperty, x);
                putCursor.SetValue(Grid.RowProperty, y);
                com.ShowMassage();
                if (color == Const.BLACK)
                {
                    LogWindow.LogBlack("Put:CPUが着手しました(" + (char)(x + 'A' - 1) + ", " + (y) + ")");
                    if ((bool)comInfoSwitch.IsChecked)
                    {
                        LogWindow.LogBlack("予想石差:" + value + "    思考時間:" + String.Format("{0:f}",time.TotalSeconds) + "[秒]");
                    }
                }
                else
                {
                    if ((bool)comInfoSwitch.IsChecked)
                    {
                        LogWindow.LogWhite("予想石差:" + value + "    思考時間:" + String.Format("{0:f}", time.TotalSeconds) + "[秒]");
                    }
                    LogWindow.LogWhite("Put:CPUが着手しました(" + (char)(x + 'A' - 1) + ", " + (y) + ")");
                }
            }
            else
            {
                LogWindow.LogWarning("Put:" + GetColorName(color) + "が着手できませんでした(" + (char)(x + 'A') + ", " + (y) + ")");
            }
            //色反転
            color = OppColor(color);

            //次の人が置ける場所がない!!
            if (MainBoard.GetMobility(color) == 0)
            {
                //互いにないならゲーム終了
                if (MainBoard.GetMobility(OppColor(color)) == 0)
                {
                    GameEnd();
                    return;
                }
                if (color == Const.BLACK)
                {
                    LogWindow.LogBlack("黒:置ける場所がないorz...パスです");
                }
                else
                {
                    LogWindow.LogWhite("白:置ける場所がないorz...パスです");
                }
                //色反転
                color = OppColor(color);
                canInput = true;
                //もっかいCPUの番
                PVC_Update();
            }
            else
            {
                //ボタンを有効に.
                canInput = true;
            }
            //盤面表示更新
            MainBoard.ReDraw(showMob, color);
        }
    }
}