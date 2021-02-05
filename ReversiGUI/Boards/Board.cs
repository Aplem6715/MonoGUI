using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using ReversiGUI.Games;

namespace ReversiGUI.Boards
{

    public static class Define
    {
        public const int BLACK = 0;
        public const int WHITE = 1;

        public const int NOMOVE = 64;
        public const int PASS = 64;
        public const int UNDO = 65;

        public const int FAIL = 66;
    }

    static class Const
    {
        public const int BLACK = 0;
        public const int WHITE = 1;
        public const int NOCOLOR = -1;
        public const int BOARD_SIZE = 8;
    }

    class Board
    {
        private UInt64[] stone = new UInt64[2];
        private UInt64[] mobility = new UInt64[2];
        private StoneModel[,] StoneList = new StoneModel[8, 8];
        private Label[] CountText = new Label[2];
        private Label[,] PositionLabel = new Label[2, 16];
        public int[] Count { get; } = new int[2];

        #region DLLs


        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void DllInit();

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void DllBoardReset();

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllPut(int pos);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllStoneCount(int color);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllUndo();

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllIsGameEnd();

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllIsLegal(int pos);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static UInt64 DllGetMobility();

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static UInt64 DllGetMobilityC(int color);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static UInt64 DllGetStones(int color);
        
        
        #endregion

        public Board(Grid grid, ControlTemplate resource, OnClickPos onClick, Label wText, Label bText)
        {
            DllInit();
            CountText[Const.BLACK] = bText;
            CountText[Const.WHITE] = wText;
            InitStones(grid, resource, onClick);
        }

        
        public void InitStones(Grid grid, ControlTemplate resource, OnClickPos onClick)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    StoneList[y, x] = new StoneModel(y, x, resource, onClick);
                    grid.Children.Add(StoneList[y, x]);
                }
            }
            for(int i=0; i<8; i++)
            {
                //横軸の表示A~H
                PositionLabel[0, i] = new Label();
                PositionLabel[0, i+1] = new Label();
                //縦軸の表示1~8
                PositionLabel[1, i] = new Label();
                PositionLabel[1, i + 1] = new Label();

                //横軸の文字A~Z
                PositionLabel[0, i].Content = (char)('A' + i);
                PositionLabel[0, i+1].Content = (char)('A' + i);
                //縦軸の文字1~8
                PositionLabel[1, i].Content = i+1;
                PositionLabel[1, i + 1].Content = i+1;

                //横軸の位置
                PositionLabel[0, i].SetValue(Grid.ColumnProperty, i+1);
                PositionLabel[0, i+1].SetValue(Grid.ColumnProperty, i + 1);
                PositionLabel[0, i].SetValue(Grid.RowProperty, 0);
                PositionLabel[0, i + 1].SetValue(Grid.RowProperty, 9);
                //縦軸の位置
                PositionLabel[1, i].SetValue(Grid.ColumnProperty, 0);
                PositionLabel[1, i+1].SetValue(Grid.ColumnProperty, 9);
                PositionLabel[1, i].SetValue(Grid.RowProperty, i+1);
                PositionLabel[1, i + 1].SetValue(Grid.RowProperty, i+1);

                //幅
                PositionLabel[0, i].Width = 40;
                PositionLabel[0, i+1].Width = 40;
                PositionLabel[1, i].Width = 40;
                PositionLabel[1, i+1].Width = 40;
                //高さ
                PositionLabel[0, i].Height = 40;
                PositionLabel[0, i + 1].Height = 40;
                PositionLabel[1, i].Height = 40;
                PositionLabel[1, i + 1].Height = 40;

                //フォントサイズ
                PositionLabel[0, i].FontSize = 16;
                PositionLabel[0, i + 1].FontSize = 16;
                PositionLabel[1, i].FontSize = 16;
                PositionLabel[1, i + 1].FontSize = 16;

                //各軸の水平方向の寄り
                PositionLabel[0, i].HorizontalContentAlignment = HorizontalAlignment.Center;
                PositionLabel[0, i + 1].HorizontalContentAlignment = HorizontalAlignment.Center;
                PositionLabel[1, i].HorizontalContentAlignment = HorizontalAlignment.Right;
                PositionLabel[1, i + 1].HorizontalContentAlignment = HorizontalAlignment.Left;

                //各軸の垂直方向の寄り
                PositionLabel[0, i].VerticalContentAlignment = VerticalAlignment.Bottom;
                PositionLabel[0, i + 1].VerticalContentAlignment = VerticalAlignment.Top;
                PositionLabel[1, i].VerticalContentAlignment = VerticalAlignment.Center;
                PositionLabel[1, i + 1].VerticalContentAlignment = VerticalAlignment.Center;

                grid.Children.Add(PositionLabel[0, i]);
                grid.Children.Add(PositionLabel[0, i+1]);
                grid.Children.Add(PositionLabel[1, i]);
                grid.Children.Add(PositionLabel[1, i+1]);

            }
        }

        public void ReDraw(bool showMobility, int color)
        {
            UInt64 cursor;

            stone[Const.BLACK] = DllGetStones(Const.BLACK);
            stone[Const.WHITE] = DllGetStones(Const.WHITE);

            if (showMobility)
            {
                mobility[Const.BLACK] = GetMobility(Const.BLACK);
                mobility[Const.WHITE] = GetMobility(Const.WHITE);
            }

            int x, y;
            for (y = 0; y < Const.BOARD_SIZE; y++)
            {
                for (x = 0; x < Const.BOARD_SIZE; x++)
                {
                    cursor = (ulong)0x8000000000000000 >> (Const.BOARD_SIZE * (7-y) + (7-x));
                    if ((stone[Const.BLACK] & cursor) != 0)
                    {
                        StoneList[y, x].SetBlack();
                    }
                    else if ((stone[Const.WHITE] & cursor) != 0)
                    {
                        StoneList[y, x].SetWhite();
                    }
                    else if (showMobility && (mobility[color] & cursor) != 0)
                    {
                        StoneList[y, x].SetMobilityColor(color);
                    }
                    else
                    {
                        StoneList[y, x].SetEmpty();
                    }
                }
            }
            Count[Const.BLACK] = DllStoneCount(Const.BLACK);
            Count[Const.WHITE] = DllStoneCount(Const.WHITE);
            CountText[Const.BLACK].Content = "黒:" + Count[Const.BLACK];
            CountText[Const.WHITE].Content = "白:" + Count[Const.WHITE];
        }

        public UInt64 GetMobility(int color)
        {
            return DllGetMobilityC(color);
        }

        public bool Put(int pos)
        {
            if(DllPut(pos) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Undo()
        {
            return DllUndo();
        }

        public bool IsGameEnd()
        {
            if (DllIsGameEnd() == 1) return true;
            return false;
        }

        public bool IsLegal(int pos)
        {
            if (DllIsLegal(pos) == 1) return true;
            return false;
        }

        public void Reset()
        {
            DllBoardReset();
        }
    }
}
