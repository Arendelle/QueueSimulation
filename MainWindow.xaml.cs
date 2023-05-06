using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using Hybrid;
using MathWorks.MATLAB.NET.Arrays;

namespace QueueSimulation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread LoadMatlabThread;
        MatlabClass QueueCalc;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void test(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine("aaa");
        }
        private void Slider1_update(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBox_s1.Text = Slider1.Value.ToString();
        }

        private void Slider2_update(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBox_s2.Text = Slider2.Value.ToString();
        }

        private void Slider3_update(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBox_s3.Text = Slider3.Value.ToString();
        }

        private void Button_sim1_Click(object sender, RoutedEventArgs e)
        {
            MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)2, 5, 1, 0.25, 1000);
            double[,] ans = (double[,])result.ToArray();
            Console.WriteLine(ans);
        }

        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            //LoadMatlabMethod();
            LoadMatlabThread = new Thread(new ThreadStart(LoadMatlabMethod));
            LoadMatlabThread.Start();
        }

        // 启动时加载Matlab组件
        private void LoadMatlabMethod()
        {
            Dispatcher.Invoke((new Action(() => { QueueCalc = new MatlabClass(); })));
            Thread.Sleep(5000);
            Console.WriteLine("DLL Loaded.");
        }
    }
}
