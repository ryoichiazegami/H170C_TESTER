﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace H170C_Tester
{
    /// <summary>
    /// Manual.xaml の相互作用ロジック
    /// </summary>
    public partial class Manual
    {
        private Timer TmTimeOut;
        string manualPath = "";

        public Manual()
        {
            InitializeComponent();

            manualPath = Constants.Path_Manual;


            wb.Visibility = Visibility.Hidden;
            TmTimeOut = new Timer();
            TmTimeOut.Tick += async (sender, e) =>
            {
                TmTimeOut.Stop();
                wb.Navigate(new Uri(manualPath + "#toolbar=0&navpanes=0&view=FitH&scrollbar=1&page=1"));
                await Task.Delay(200);
                wb.Visibility = Visibility.Visible;
                Ring.IsActive = false;
            };

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TmTimeOut.Interval = 500;
            TmTimeOut.Start();
            Ring.IsActive = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            wb.Visibility = Visibility.Hidden;

        }
    }
}