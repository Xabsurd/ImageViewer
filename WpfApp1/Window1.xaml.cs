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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        public Window1(MainWindow mwa)
        {
            this.Owner = mwa;
            InitializeComponent();

            // 设置Binding对象
            Binding b = new Binding();
            b.Path = new PropertyPath(TextBox.TextProperty);
            b.Source = text;
            b.Mode = BindingMode.OneWay;
            b.StringFormat = "您选择的交通工具为：{0}";

            MainWindow mw = Owner as MainWindow;
            // 使属性与Binding关联
            mw?.tbtxt.SetBinding(TextBox.TextProperty, b);
        }
    }
}
