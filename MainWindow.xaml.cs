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

        private void Display_output(double[,] data)
        {
            Label_OutputMode.Content = "模型：M/M/m";
            TextBox_Ws.Text = data[0, 0].ToString();
            TextBox_Wq.Text = data[0, 1].ToString();
            TextBox_Wb.Text = data[0, 2].ToString();
            TextBox_Ls.Text = data[0, 3].ToString();
            TextBox_Lq.Text = data[0, 4].ToString();
            for (int i = 5; i < data.GetLength(1); i++) {
                //ListBox_Length.Items.Add((i - 5).ToString() + data[0, i].ToString());
                ListBoxItem newItem = new ListBoxItem();
                newItem.Content = (i - 5).ToString() + " : " + data[0, i].ToString();
                ListBox_Length.Items.Add(newItem);
            }
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
            int s = Convert.ToInt32(TextBox_s1.Text);
            int m = Convert.ToInt32(TextBox_m1.Text);
            double lambda = Convert.ToDouble(TextBox_lambda1.Text);
            double mu = Convert.ToDouble(TextBox_mu1.Text);
            int t = Convert.ToInt32(TextBox_simtime1.Text);
            MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, lambda, mu, t);
            double[,] ans = (double[,])result.ToArray();
            Display_output(ans);
            //Console.WriteLine(ans);
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
            //Thread.Sleep(5000);
            Console.WriteLine("DLL Loaded.");
        }
    }
}
