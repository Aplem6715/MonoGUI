using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace ReversiGUI.Games
{
    class ScrollingLog
    {
        public delegate void MessageCallback(int color, string str);

        private MessageCallback callBackHolder;

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void SetCallBack(MessageCallback callback);

        public RichTextBox RichTextBox { set; get; }

        // コンストラクタ
        public ScrollingLog(RichTextBox richTextBox)
        {
            RichTextBox = richTextBox;
            //プログラム終了までデリゲートを保持する
            callBackHolder = LogC;
            SetCallBack(callBackHolder);
        }

        // クリア
        public void Clear()
        {
            var paragraph = RichTextBox.Document.Blocks.LastBlock as Paragraph;
            paragraph.Inlines.Clear();
        }

        //区切り線を引く
        public void WriteHorizon()
        {
            LogWarning("-----------------------------------------------------------");
        }

        // テキスト追加
        public virtual void WriteLine(string text, Brush brush = null)
        {
            var paragraph = RichTextBox.Document.Blocks.LastBlock as Paragraph;

            // 書き込み
            if (brush != null)
            {
                // 色付きでテキスト追加
                var span = new Span { Foreground = brush };
                span.Inlines.Add(text + "\n");
                paragraph.Inlines.Add(span);
            }
            else
            {
                // そのままテキスト追加
                paragraph.Inlines.Add(text + "\n");
            }

            // 最終行にスクロール
            RichTextBox.ScrollToEnd();
        }

        // 通常ログ
        public void Log(object message)
        {
            WriteLine(message.ToString());
        }

        public void LogBlack(object message)
        {
            WriteLine(message.ToString(), Brushes.Black);
        }

        public void LogWhite(object message)
        {
            WriteLine(message.ToString(), Brushes.WhiteSmoke);
        }

        public void LogAlart(object message)
        {
            WriteLine(message.ToString(), Brushes.Lime);
        }

        // 警告ログ
        public void LogWarning(object message)
        {
            WriteLine(message.ToString(), Brushes.Orange);
        }

        // エラーログ
        public void LogError(object message)
        {
            WriteLine(message.ToString(), Brushes.Tomato);
        }

        //Cから呼ばれるコールバックメソッド
        public void LogC(int color, string str)
        {
            switch (color)
            {
                case 0:
                    LogBlack((object)str);
                    break;
                case 1:
                    LogWhite((object)str);
                    break;
                case 2:
                    LogAlart((object)str);
                    break;
                case 3:
                    LogWarning((object)str);
                    break;
                case 4:
                    LogError((object)str);
                    break;
                default:
                    Log((object)str);
                    break;
            }
        }
    }
}
