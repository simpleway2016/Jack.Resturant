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

namespace RePushOrder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Model _model;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = _model = new Model();
            _model.Url = "http://";
            _model.Channel = "Meituan";
        }

        private async void btnPush_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            this.Cursor = Cursors.Wait;
            try
            {
                await Task.Run(() => {
                    var type = typeof(MainWindow).Assembly.GetType($"RePushOrder.Impls.{_model.Channel}");
                    IPush push = (IPush)Activator.CreateInstance(type);
                    push.Push(_model.Content, _model.Url);

                });
                MessageBox.Show(this,"推送成功！");
            }
            catch (Exception ex)
            {
                var err = ex;
                while (err.InnerException != null)
                    err = err.InnerException;
                MessageBox.Show(this, err.Message);
            }
            finally
            {
                this.Cursor = null;
                ((Button)sender).IsEnabled = true;
            }
            
        }
    }

    class Model:Way.Lib.DataModel
    {
        public string[] Channels
        {
            get
            {
                return new string[] { "Meituan", "Ele","Baidu" };
            }
        }

        string _Channel;
        public string Channel
        {
            get
            {
                return _Channel;
            }
            set
            {
                if (_Channel != value)
                {
                    _Channel = value;
                    this.OnPropertyChanged("Channel", null, value);
                }
            }
        }


        string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                if (_Url != value)
                {
                    _Url = value;
                    this.OnPropertyChanged("Url", null, value);
                }
            }
        }


        string _Content;
        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                if (_Content != value)
                {
                    _Content = value;
                    this.OnPropertyChanged("Content", null, value);
                }
            }
        }
    }
}
