﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace H170C_Tester
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {

        DispatcherTimer timerTextInput;

        Uri uriTestPage = new Uri("Page/Test/Test.xaml", UriKind.Relative);
        Uri uriConfPage = new Uri("Page/Config/Conf.xaml", UriKind.Relative);
        Uri uriHelpPage = new Uri("Page/Help/Help.xaml", UriKind.Relative);

        public MainWindow()
        {
            InitializeComponent();
            App._naviTest = FrameTest.NavigationService;
            App._naviConf = FrameConf.NavigationService;
            App._naviHelp = FrameHelp.NavigationService;
            App._naviInfo = FrameInfo.NavigationService;

            this.MouseLeftButtonDown += (sender, e) => this.DragMove();//ウィンドウ全体でドラッグ可能にする

            this.DataContext = State.VmMainWindow;



            //タイマーの設定
            timerTextInput = new DispatcherTimer(DispatcherPriority.Normal);
            timerTextInput.Interval = TimeSpan.FromMilliseconds(1000);
            timerTextInput.Tick += timerTextInput_Tick;
            timerTextInput.Start();

            GetInfo();

            //カレントディレクトリの取得
            State.CurrDir = Directory.GetCurrentDirectory();

            //試験用パラメータのロード
            State.LoadConfigData();

            General.Init周辺機器();//非同期処理です

            InitMainForm();//メインフォーム初期

            //this.WindowState = WindowState.Maximized;

            Flags.PressOpenCheckBeforeTest = true;//アプリ立ち上げ時はtrueにしておく

        }



        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                while (Flags.Initializing周辺機器) ;

                if (Flags.State1768)
                {
                    General.ResetIo();
                    LPC1768.ClosePort();//IO閉じる
                }

                if (!State.Save個別データ())
                {
                    MessageBox.Show("個別データの保存に失敗しました");
                }
                if (!General.SaveRetryLog())
                {
                    MessageBox.Show("リトライログの保存に失敗しました");
                }

            }
            catch
            {
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Flags.Testing)
            {
                e.Cancel = true;
            }
            else
            {
                Flags.StopInit周辺機器 = true;
            }
        }



        void timerTextInput_Tick(object sender, EventArgs e)
        {
            timerTextInput.Stop();
            if (!Flags.SetOpecode)
            {
                State.VmMainWindow.Opecode = "";
            }
        }

        private void cbOperator_DropDownClosed(object sender, EventArgs e)
        {
            if (cbOperator.SelectedIndex == -1)
                return;
            Flags.SetOperator = true;

            if (Flags.SetOpecode)
            {
                return;
            }

            State.VmMainWindow.ReadOnlyOpecode = false;
            Flags.SetOpecode = false;
            tbOpecode.Focus();

        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            if (Flags.Testing) return;

            if (!Flags.SetOperator)
            {
                cbOperator.Focus();
            }
            else
            {
                Flags.SetOpecode = false;
                tbOpecode.Focus();
            }

        }

        private void tbOpecode_TextChanged(object sender, TextChangedEventArgs e)
        {
            //１文字入力されるごとに、タイマーを初期化する
            timerTextInput.Stop();
            timerTextInput.Start();

            if (State.VmMainWindow.Opecode.Length != 13) return;
            //以降は工番が正しく入力されているかどうかの判定
            if (System.Text.RegularExpressions.Regex.IsMatch(
                State.VmMainWindow.Opecode, @"^\d-\d\d-\d\d\d\d-\d\d\d$",
                System.Text.RegularExpressions.RegexOptions.ECMAScript))
            {
                timerTextInput.Stop();
                Flags.SetOpecode = true;
            }
        }


        //アセンブリ情報の取得
        private void GetInfo()
        {
            //アセンブリバージョンの取得
            var asm = Assembly.GetExecutingAssembly();
            var M = asm.GetName().Version.Major.ToString();
            var N = asm.GetName().Version.Minor.ToString();
            var B = asm.GetName().Version.Build.ToString();
            State.AssemblyInfo = M + "." + N + "." + B;

        }

        //フォームのイニシャライズ
        private void InitMainForm()
        {
            TabInfo.Header = "";//実行時はエラーインフォタブのヘッダを空白にして作業差に見えないようにする
            TabInfo.IsEnabled = false; //作業差がTABを選択できないようにします

            State.VmMainWindow.ReadOnlyOpecode = true;
            State.VmMainWindow.EnableOtherButton = true;

            State.VmTestStatus.UnitTestEnable = false;
            State.VmMainWindow.OperatorEnable = true;

        }

        //フォーカスのセット
        public void SetFocus()
        {
            if (!Flags.SetOperator)
            {

                if (!cbOperator.IsFocused)
                    cbOperator.Focus();
                return;
            }


            if (!Flags.SetOpecode)
            {
                //if (!tbOpecode.IsFocused)
                tbOpecode.Focus();
                return;
            }


        }


        private async void TabMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = TabMenu.SelectedIndex;
            if (index == 0)
            {
                Flags.OtherPage = false;//フラグを初期化しておく

                App._naviConf.Refresh();
                App._naviHelp.Refresh();
                App._naviTest.Navigate(uriTestPage);
                SetFocus();//テスト画面に移行する際にフォーカスを必須項目入力欄にあてる
                State.VmMainWindow.EnableOtherButton = false;
                await Task.Delay(500);
                State.VmMainWindow.EnableOtherButton = true;
            }
            else if (index == 1)
            {
                Flags.OtherPage = true;
                App._naviConf.Navigate(uriConfPage);
                App._naviHelp.Refresh();
                State.VmMainWindow.EnableOtherButton = false;
                await Task.Delay(500);
                State.VmMainWindow.EnableOtherButton = true;
            }
            else if (index == 2)
            {
                Flags.OtherPage = true;
                App._naviHelp.Navigate(uriHelpPage);
                App._naviConf.Refresh();
                State.VmMainWindow.EnableOtherButton = false;
                await Task.Delay(500);
                State.VmMainWindow.EnableOtherButton = true;

            }
            else if (index == 3)//Infoタブ 作業者がこのタブを選択することはない。 TEST画面のエラー詳細ボタンを押した時にこのタブが選択されるようコードビハインドで記述
            {
                //Flags.OtherPage = true;
                App._naviInfo.Navigate(State.uriOtherInfoPage);
                App._naviConf.Refresh();
                App._naviHelp.Refresh();
            }


        }




    }
}
