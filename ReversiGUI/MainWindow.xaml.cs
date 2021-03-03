using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReversiGUI.Boards;
using ReversiGUI.Games;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ReversiGUI
{
    class SplitTextItem
    {
        public string Text { get; set; }
    }

    class Difficult
    {
        public int mid;
        public int end;
        public Difficult(int imid, int iend)
        {
            mid = imid;
            end = iend;
        }
    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        GameManager gManager;
        private Button[,] buttons = new Button[8, 8];
        private Difficult[] diffs = new Difficult[]
        {
            new Difficult(1,4),
            new Difficult(2,6),
            new Difficult(8,12),
            new Difficult(14,20),
            new Difficult(-1,-1),
        };

        public MainWindow()
        {
            var Easy = new SplitTextItem() { Text = "Easy" };
            var Normal = new SplitTextItem() { Text = "Normal" };
            var Hard = new SplitTextItem() { Text = "Hard" };
            var Pro = new SplitTextItem() { Text = "Pro" };
            var Custom = new SplitTextItem() { Text = "#Custom#" };
            var SplitList = new List<SplitTextItem>()
            {
                Easy,
                Normal,
                Hard,
                Pro,
                Custom,
            };

            InitializeComponent();
            DifficultySplit.ItemsSource = SplitList;
            gManager = new GameManager(BoardGrid, (ControlTemplate)this.Resources["ButtonControlTemplate1"], MessageBlock, WhiteText, BlackText, ComProgressRing, PutCursor, ColorToggle, TimerToggle, MPCToggle, PreSearchToggle);

            MidDepthInput.Value = diffs[DifficultySplit.SelectedIndex].mid;
            EndDepthInput.Value = diffs[DifficultySplit.SelectedIndex].end;
            MidDepthInput.IsReadOnly = true;
            EndDepthInput.IsReadOnly = true;
            //gManager.Start(GAMEMODE.PVC);
        }
        
        private async void Button_Credit(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("Reversi GUI", "\nACEM2 佐藤 大地 / Daichi Sato\n\nCopyright (c) 2021 Daichi Sato");
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            SettingFlyout.IsOpen = true;
        }

        private void Button_Undo(object sender, RoutedEventArgs e)
        {
            gManager.Undo();
        }

        private void ToggleSwitch_IsCheckedChanged(object sender, EventArgs e)
        {
            gManager.OnMobShowChange((bool)((ToggleSwitch)sender).IsChecked);
        }

        private void Button_start_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)PVPToggle.IsChecked)
            {
                gManager.Start(GAMEMODE.PVP, diffs[DifficultySplit.SelectedIndex].mid, diffs[DifficultySplit.SelectedIndex].end, (DIFFICULTY)DifficultySplit.SelectedIndex, (int)TimerInput.Value);
            }
            else
            {
                if ((DIFFICULTY)DifficultySplit.SelectedIndex == DIFFICULTY.CUSTOM)
                {
                    gManager.Start(GAMEMODE.PVC, (int)MidDepthInput.Value, (int)EndDepthInput.Value, (DIFFICULTY)DifficultySplit.SelectedIndex, (int)TimerInput.Value);
                }
                else
                {
                    gManager.Start(GAMEMODE.PVC, diffs[DifficultySplit.SelectedIndex].mid, diffs[DifficultySplit.SelectedIndex].end, (DIFFICULTY)DifficultySplit.SelectedIndex, (int)TimerInput.Value);
                }
            }
        }

        private void DifficultySplit_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((DIFFICULTY)DifficultySplit.SelectedIndex == DIFFICULTY.CUSTOM)
            {
                MidDepthInput.IsReadOnly = false;
                EndDepthInput.IsReadOnly = false;
            }
            else
            {
                MidDepthInput.Value = diffs[DifficultySplit.SelectedIndex].mid;
                EndDepthInput.Value = diffs[DifficultySplit.SelectedIndex].end;
                MidDepthInput.IsReadOnly = true;
                EndDepthInput.IsReadOnly = true;
            }
        }
    }
}
