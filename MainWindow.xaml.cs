/* 
 * 作者：Arendelle
 * Mail：arendelle@outlook.my
 * 仓库地址：https://github.com/Arendelle/QueueSimulation
 */
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
        private Thread CalculationThread;
        MatlabClass QueueCalc;

        public MainWindow()
        {
            InitializeComponent();
        }
        // 启动程序时开启DLL加载进程
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
            Console.WriteLine("DLL Loaded.");
        }
        // 单次仿真数据显示到输出窗口
        private void Display_output(double[,] data)
        {
            TextBox_Ws.Text = data[0, 0].ToString();
            TextBox_Wq.Text = data[0, 1].ToString();
            TextBox_Wb.Text = data[0, 2].ToString();
            TextBox_Ls.Text = data[0, 3].ToString();
            TextBox_Lq.Text = data[0, 4].ToString();
            ListBox_Length.Items.Clear();
            for (int i = 5; i < data.GetLength(1); i++)
            {
                ListBoxItem newItem = new ListBoxItem();
                newItem.Content = (i - 5).ToString() + ": " + data[0, i].ToString();
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
            MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, 1 / lambda, mu, t);
            double[,] ans = (double[,])result.ToArray();
            Display_output(ans);
            Label_OutputMode.Content = "模型：M/M/m";
        }

        private void Button_sim2_Click(object sender, RoutedEventArgs e)
        {
            int s = Convert.ToInt32(TextBox_s2.Text);
            int m = Convert.ToInt32(TextBox_m2.Text);
            double lambda = Convert.ToDouble(TextBox_lambda2.Text);
            double d = Convert.ToDouble(TextBox_mu2.Text);
            int t = Convert.ToInt32(TextBox_simtime2.Text);
            MWNumericArray result = (MWNumericArray)QueueCalc.M_D_m((MWArray)s, m, 1 / lambda, d, t);
            double[,] ans = (double[,])result.ToArray();
            Display_output(ans);
            Label_OutputMode.Content = "模型：M/D/m";
        }
        private void Button_sim3_Click(object sender, RoutedEventArgs e)
        {
            int s = Convert.ToInt32(TextBox_s2.Text);
            int m = Convert.ToInt32(TextBox_m2.Text);
            double lambda = Convert.ToDouble(TextBox_lambda2.Text);
            double a = Convert.ToDouble(TextBox_a3.Text);
            double b = Convert.ToDouble(TextBox_b3.Text);
            int t = Convert.ToInt32(TextBox_simtime2.Text);
            MWNumericArray result = (MWNumericArray)QueueCalc.M_G_m((MWArray)s, m, 1 / lambda, a, b, t);
            double[,] ans = (double[,])result.ToArray();
            Display_output(ans);
            Label_OutputMode.Content = "模型：M/G/m";
        }

        private void RadioButton_Page2_0_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page2_s1.IsEnabled = true;
            TextBox_Page2_m1.IsEnabled = false;
        }

        private void RadioButton_Page2_1_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page2_s1.IsEnabled = false;
            TextBox_Page2_m1.IsEnabled = true;
        }

        // Page2计算并绘图方法
        private void Page2CalcMethod()
        {

            // 分析模式， mode == 0 服务台模式。 mode == 1 顾客模式
            bool mode = false;
            // 参考指标 0 - 4
            int paramMode = 0;
            double lambda = 0, mu = 0;
            double[] dataX, dataY;
            int s = 0, m = 0, left = 0, right = 0, t = 0;
            RadioButton_Page2_1.Dispatcher.Invoke(new Action(() =>
            {
                mode = (bool)RadioButton_Page2_1.IsChecked;
                paramMode = ComboBox_Page2.SelectedIndex;
                lambda = Convert.ToDouble(TextBox_Page2_lambda.Text);
                mu = Convert.ToDouble(TextBox_Page2_mu.Text);
                t = Convert.ToInt32(TextBox_Page2_time.Text);
            }));
            if (mode)
            {
                // 顾客模式
                TextBox_Page2_m0.Dispatcher.Invoke(new Action(() =>
                {
                    left = Convert.ToInt32(TextBox_Page2_m0.Text);
                    right = Convert.ToInt32(TextBox_Page2_m1.Text);
                    s = Convert.ToInt32(TextBox_Page2_s0.Text);
                    // 初始化进度条
                    ProgressBar_Page2.Maximum = right - left + 1;
                    ProgressBar_Page2.Value = 0;
                }));
                dataX = new double[right - left + 1];
                dataY = new double[right - left + 1];

                for (int i = left; i <= right; i++)
                {
                    dataX[i - left] = i;
                    MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, i, 1 / lambda, mu, t);
                    double[,] ans = (double[,])result.ToArray();
                    dataY[i - left] = ans[0, paramMode];
                    // 更新进度条
                    ProgressBar_Page2.Dispatcher.Invoke(new Action(() => { ProgressBar_Page2.Value++; }));
                }
            }
            else
            {
                // 服务台模式
                TextBox_Page2_m0.Dispatcher.Invoke(new Action(() =>
                {
                    left = Convert.ToInt32(TextBox_Page2_s0.Text);
                    right = Convert.ToInt32(TextBox_Page2_s1.Text);
                    m = Convert.ToInt32(TextBox_Page2_m0.Text);
                    // 初始化进度条
                    ProgressBar_Page2.Maximum = right - left + 1;
                    ProgressBar_Page2.Value = 0;
                }));
                dataX = new double[right - left + 1];
                dataY = new double[right - left + 1];

                for (int i = left; i <= right; i++)
                {
                    dataX[i - left] = i;
                    MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)i, m, 1 / lambda, mu, t);
                    double[,] ans = (double[,])result.ToArray();
                    dataY[i - left] = ans[0, paramMode];
                    // 更新进度条
                    ProgressBar_Page2.Dispatcher.Invoke(new Action(() => { ProgressBar_Page2.Value++; }));
                }
            }
            string[] legend = { "Ws", "Wq", "Wb", "Ls", "Lq" };
            Plot1.Dispatcher.Invoke(new Action(() =>
            {
                Plot1.Plot.AddScatter(dataX, dataY, label: legend[paramMode]);
                Plot1.Plot.Legend();
                Plot1.Refresh();
            }));
        }
        private void Button_Page2_run_Click(object sender, RoutedEventArgs e)
        {
            // 开新线程独立计算，防止阻塞UI
            CalculationThread = new Thread(new ThreadStart(Page2CalcMethod));
            CalculationThread.Start();
        }

        private void Button_Page2_Reset_Click(object sender, RoutedEventArgs e)
        {
            Plot1.Plot.Clear();
            Plot1.Refresh();
        }

        // Page2计算并绘图方法
        private void Page3CalcMethod()
        {

            // 分析模式， mode == 0 lambda模式。 mode == 1 mu模式
            bool mode = false;
            // 参考指标 0 - 4
            int paramMode = 0;
            double lambda = 0, mu = 0, left = 0, right = 0, step = 0;
            double[] dataX, dataY;
            int s = 0, m = 0, t = 0;
            RadioButton_Page3_1.Dispatcher.Invoke(new Action(() =>
            {
                s = Convert.ToInt32(TextBox_Page3_s.Text);
                m = Convert.ToInt32(TextBox_Page3_m.Text);
                mode = (bool)RadioButton_Page3_1.IsChecked;
                paramMode = ComboBox_Page3.SelectedIndex;
                t = Convert.ToInt32(TextBox_Page3_time.Text);
            }));
            if (!mode)
            {
                // lambda模式
                TextBox_Page3_lambda0.Dispatcher.Invoke(new Action(() =>
                {
                    mu = Convert.ToDouble(TextBox_Page3_mu0.Text);
                    left = Convert.ToDouble(TextBox_Page3_lambda0.Text);
                    right = Convert.ToDouble(TextBox_Page3_lambda1.Text);
                    step = Convert.ToDouble(TextBox_Page3_step0.Text);
                    // 初始化进度条
                    ProgressBar_Page3.Maximum = (right - left) / step;
                    ProgressBar_Page3.Value = 0;
                }));
                dataX = new double[(int)((right - left) / step) + 1];
                dataY = new double[(int)((right - left) / step) + 1];

                double xtemp = left;
                for (int i = 0; i <= (int)((right - left) / step); i++)
                {
                    dataX[i] = xtemp;
                    MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, 1 / xtemp, mu, t);
                    double[,] ans = (double[,])result.ToArray();
                    dataY[i] = ans[0, paramMode];
                    xtemp += step;
                    // 更新进度条
                    ProgressBar_Page3.Dispatcher.Invoke(new Action(() => { ProgressBar_Page3.Value++; }));
                }
            }
            else
            {
                // mu模式
                TextBox_Page3_mu0.Dispatcher.Invoke((new Action(() =>
                {
                    lambda = Convert.ToDouble(TextBox_Page3_lambda0.Text);
                    left = Convert.ToDouble(TextBox_Page3_mu0.Text);
                    right = Convert.ToDouble(TextBox_Page3_mu1.Text);
                    step = Convert.ToDouble(TextBox_Page3_step1.Text);
                    // 初始化进度条
                    ProgressBar_Page3.Maximum = (right - left) / step;
                    ProgressBar_Page3.Value = 0;
                })));
                dataX = new double[(int)((right - left) / step) + 1];
                dataY = new double[(int)((right - left) / step) + 1];

                double xtemp = left;
                for (int i = 0; i <= (int)((right - left) / step); i++)
                {
                    dataX[i] = i;
                    MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, 1 / lambda, xtemp, t);
                    double[,] ans = (double[,])result.ToArray();
                    dataY[i] = ans[0, paramMode];
                    xtemp += step;
                    // 更新进度条
                    ProgressBar_Page3.Dispatcher.Invoke(new Action(() => { ProgressBar_Page3.Value++; }));
                }
            }
            string[] legend = { "Ws", "Wq", "Wb", "Ls", "Lq" };
            Plot2.Dispatcher.Invoke((new Action(() =>
            {
                Plot2.Plot.AddScatter(dataX, dataY, label: legend[paramMode]);
                Plot2.Plot.Legend();
                Plot2.Refresh();
            })));
        }

        private void RadioButton_Page3_0_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page3_lambda1.IsEnabled = true;
            TextBox_Page3_mu1.IsEnabled = false;
            TextBox_Page3_step0.IsEnabled = true;
            TextBox_Page3_step1.IsEnabled = false;
        }

        private void RadioButton_Page3_1_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page3_mu1.IsEnabled = true;
            TextBox_Page3_lambda1.IsEnabled = false;
            TextBox_Page3_step1.IsEnabled = true;
            TextBox_Page3_step0.IsEnabled = false;
        }

        private void Button_Page3_run_Click(object sender, RoutedEventArgs e)
        {
            // 开新线程独立计算，防止阻塞UI
            CalculationThread = new Thread(new ThreadStart(Page3CalcMethod));
            CalculationThread.Start();
        }

        private void Button_Page3_Reset_Click(object sender, RoutedEventArgs e)
        {
            Plot2.Plot.Clear();
            Plot2.Refresh();
        }

        private void RadioButton_Page4_0_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page4_lambda1.IsEnabled = true;
            TextBox_Page4_mu1.IsEnabled = false;
            TextBox_Page4_step0.IsEnabled = false;
            TextBox_Page4_step1.IsEnabled = false;
        }

        private void RadioButton_Page4_1_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page4_s1.IsEnabled = false;
            TextBox_Page4_mu1.IsEnabled = false;
            TextBox_Page4_step0.IsEnabled = true;
            TextBox_Page4_step1.IsEnabled = false;
        }

        private void RadioButton_Page4_2_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_Page4_s1.IsEnabled = false;
            TextBox_Page4_mu1.IsEnabled = true;
            TextBox_Page4_step0.IsEnabled = false;
            TextBox_Page4_step1.IsEnabled = true;
        }

        // mode == 0: 服务台模式，mode == 1: lambda模式，mode == 2: mu模式
        private void Page4CalcMethod(int mode)
        {
            int s = 0, m = 0, t = 0;
            double lambda = 0, mu = 0;
            Window1.Dispatcher.Invoke(new Action(() =>
            {
                s = Convert.ToInt32(TextBox_Page4_s0.Text);
                m = Convert.ToInt32(TextBox_Page4_m.Text);
                t = Convert.ToInt32(TextBox_Page4_time.Text);

                lambda = Convert.ToDouble(TextBox_Page4_lambda0.Text);
                mu = Convert.ToDouble(TextBox_Page4_mu0.Text);
            }));
            switch (mode)
            {
                // 服务台模式
                case 0:
                    {
                        int left = s, right = 0;
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            right = Convert.ToInt32(TextBox_Page4_s1.Text);
                            // 初始化进度条
                            ProgressBar_Page4.Maximum = (right - left) * (m + 1);
                            ProgressBar_Page4.Value = 0;
                        }));
                        double[] dataX = new double[right - left + 1];
                        double[] dataY = new double[right - left + 1];
                        double[][] buffer = new double[m + 1][];
                        for (int i = 0; i <= m; i++) buffer[i] = new double[right - left + 1];
                        for (int i = 0; i <= right - left; i++) dataX[i] = i + left;
                        // 计算各点数值，存入缓冲区
                        for (int i = 0; i <= m; i++)
                        {
                            for (int j = left; j <= right; j++)
                            {
                                MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)j, m, 1 / lambda, mu, t);
                                double[,] ans = (double[,])result.ToArray();
                                for (int k = 0; k <= m; k++)
                                {
                                    buffer[k][j - left] = ans[0, k + 5];
                                }
                                Window1.Dispatcher.Invoke(new Action(() => { ProgressBar_Page4.Value++; }));
                            }
                        }
                        // 绘图
                        for (int i = 0; i <= m; i++)
                        {
                            Window1.Dispatcher.Invoke(new Action(() =>
                            {
                                Plot3.Plot.AddScatter(dataX, buffer[i], label: "队长为" + i.ToString() + "的概率");
                                Plot3.Plot.Legend();
                            }));
                        }
                        for (int i = 0; i <= right - left; i++) dataY[i] = 1 - buffer[0][i]; 
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            Plot3.Plot.AddScatter(dataX, dataY, label: "顾客不能立刻得到服务的概率");
                            Plot3.Plot.Legend();
                            Plot3.Refresh();
                        }));
                        break;
                    }
                // lambda模式
                case 1:
                    {
                        double left = lambda, right = 0, step = 0;
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            right = Convert.ToDouble(TextBox_Page4_lambda1.Text);
                            step = Convert.ToDouble(TextBox_Page4_step0.Text);
                            // 初始化进度条
                            ProgressBar_Page4.Maximum = (right - left) / step * (m + 1);
                            ProgressBar_Page4.Value = 0;
                        }));
                        double[] dataX = new double[(int)((right - left) / step) + 1];
                        double[] dataY = new double[(int)((right - left) / step) + 1];
                        double[][] buffer = new double[m + 1][];
                        for (int i = 0; i <= m; i++) buffer[i] = new double[(int)((right - left) / step) + 1];
                        for (int i = 0; i <= (int)((right - left) / step); i++) dataX[i] = i * step + left;
                        // 计算各点数值，存入缓冲区
                        for (int i = 0; i <= m; i++)
                        {
                            for (int j = 0; j <= (int)((right - left) / step); j++)
                            {
                                MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, 1 / (j * step + left), mu, t);
                                double[,] ans = (double[,])result.ToArray();
                                for (int k = 0; k <= m; k++)
                                {
                                    buffer[k][j] = ans[0, k + 5];
                                }
                                Window1.Dispatcher.Invoke(new Action(() => { ProgressBar_Page4.Value++; }));
                            }
                        }
                        // 绘图
                        for (int i = 0; i <= m; i++)
                        {
                            Window1.Dispatcher.Invoke(new Action(() =>
                            {
                                Plot3.Plot.AddScatter(dataX, buffer[i], label: "队长为" + i.ToString() + "的概率");
                                Plot3.Plot.Legend();
                            }));
                        }
                        for (int i = 0; i <= (int)((right - left) / step); i++) dataY[i] = 1 - buffer[0][i];
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            Plot3.Plot.AddScatter(dataX, dataY, label: "顾客不能立刻得到服务的概率");
                            Plot3.Plot.Legend();
                            Plot3.Refresh();
                        }));
                        break;
                    }
                // mu模式
                case 2:
                    {
                        double left = mu, right = 0, step = 0;
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            right = Convert.ToDouble(TextBox_Page4_mu1.Text);
                            step = Convert.ToDouble(TextBox_Page4_step1.Text);
                            // 初始化进度条
                            ProgressBar_Page4.Maximum = (right - left) / step * (m + 1);
                            ProgressBar_Page4.Value = 0;
                        }));
                        double[] dataX = new double[(int)((right - left) / step) + 1];
                        double[] dataY = new double[(int)((right - left) / step) + 1];
                        double[][] buffer = new double[m + 1][];
                        for (int i = 0; i <= m; i++) buffer[i] = new double[(int)((right - left) / step) + 1];
                        for (int i = 0; i <= (int)((right - left) / step); i++) dataX[i] = i * step + left;
                        // 计算各点数值，存入缓冲区
                        for (int i = 0; i <= m; i++)
                        {
                            for (int j = 0; j <= (int)((right - left) / step); j++)
                            {
                                MWNumericArray result = (MWNumericArray)QueueCalc.M_M_m((MWArray)s, m, 1 / lambda, (j * step + left), t);
                                double[,] ans = (double[,])result.ToArray();
                                for (int k = 0; k <= m; k++)
                                {
                                    buffer[k][j] = ans[0, k + 5];
                                }
                                Window1.Dispatcher.Invoke(new Action(() => { ProgressBar_Page4.Value++; }));
                            }
                        }
                        // 绘图
                        for (int i = 0; i <= m; i++)
                        {
                            Window1.Dispatcher.Invoke(new Action(() =>
                            {
                                Plot3.Plot.AddScatter(dataX, buffer[i], label: "队长为" + i.ToString() + "的概率");
                                Plot3.Plot.Legend();
                            }));
                        }
                        for (int i = 0; i <= (int)((right - left) / step); i++) dataY[i] = 1 - buffer[0][i];
                        Window1.Dispatcher.Invoke(new Action(() =>
                        {
                            Plot3.Plot.AddScatter(dataX, dataY, label: "顾客不能立刻得到服务的概率");
                            Plot3.Plot.Legend();
                            Plot3.Refresh();
                        }));
                        break;
                    }

            }

        }
        private void Button_Page4_run_Click(object sender, RoutedEventArgs e)
        {
            int mode = 0;
            if ((bool)RadioButton_Page4_0.IsChecked) mode = 0;
            else if ((bool)RadioButton_Page4_1.IsChecked) mode = 1;
            else if ((bool)RadioButton_Page4_2.IsChecked) mode = 2;
            Plot3.Reset();
            // 开新线程独立计算，防止阻塞UI
            CalculationThread = new Thread(() => Page4CalcMethod(mode));
            CalculationThread.Start();
        }

        private void Button_Page4_Reset_Click(object sender, RoutedEventArgs e)
        {
            Plot2.Plot.Clear();
            Plot2.Refresh();
        }
    }
}
