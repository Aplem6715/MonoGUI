using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using ReversiGUI.Games;

namespace ReversiGUI.Boards
{
    class StoneModel:Button
    {
        private Ellipse image;
        private SolidColorBrush stoneBrush = new SolidColorBrush();
        private SolidColorBrush strokeBrush = new SolidColorBrush();
        private int x, y;
        private OnClickPos onClickPos;

        //コンストラクタ サイズは35x35
        public StoneModel(int initY, int initX, ControlTemplate resource, OnClickPos onClick)
        {
            image = new Ellipse();

            x = initX;
            y = initY;

            stoneBrush.Color = Color.FromArgb(0, 0, 0, 0);
            image.Fill = stoneBrush;
            image.StrokeThickness = 1;
            strokeBrush.Color = Color.FromArgb(255, 0, 0, 0);
            image.Stroke = strokeBrush;
            image.Visibility = Visibility.Hidden;

            image.Width = 35;
            image.Height = 35;

            this.AddChild(image);

            this.SetValue(Grid.RowProperty, initY+1);
            this.SetValue(Grid.ColumnProperty, initX+1);

            this.Width = 40;
            this.Height = 40;

            this.Template = resource;

            this.onClickPos = onClick;
        }

        //白にする
        public void SetWhite()
        {
            image.Visibility = Visibility.Visible;
            strokeBrush.Color = Color.FromArgb(255, 0, 0, 0);
            stoneBrush.Color = Color.FromArgb(255, 255, 255, 255);
        }

        //黒にする
        public void SetBlack()
        {
            image.Visibility = Visibility.Visible;
            strokeBrush.Color = Color.FromArgb(255, 0, 0, 0);
            stoneBrush.Color = Color.FromArgb(255, 0, 0, 0);
        }

        public void SetMobilityColor(int color)
        {
            image.Visibility = Visibility.Visible;
            if (color == Const.BLACK)
            {
                stoneBrush.Color = Color.FromArgb(48, 0, 0, 0);
            }
            else {
                stoneBrush.Color = Color.FromArgb(48, 255, 255, 255);
            }
            strokeBrush.Color = Color.FromArgb(48, 0, 0, 0);
        }

        //非表示
        public void SetEmpty()
        {
            image.Visibility = Visibility.Hidden;
        }

        public Ellipse GetEllipse()
        {
            return image;
        }

        protected override void OnClick()
        {
            onClickPos(x, y);
            base.OnClick();
        }
    }
}
