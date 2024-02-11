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

namespace ert_for_host
{
    /// <summary>
    /// warning.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Warning : Window
    {
        public bool cancelled;
        public Warning(string title, string description, int type=1)
        {
            InitializeComponent();
            titleLabel.Content = title;
            descriptionTextblock.Text = description;
            if (type == 0)
            {
                cancelBtn.Visibility = Visibility.Visible;
            } else
            {
                cancelBtn.Visibility = Visibility.Hidden;
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.cancelled = false;
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.cancelled = true;
            this.Close();
        }
    }
}
