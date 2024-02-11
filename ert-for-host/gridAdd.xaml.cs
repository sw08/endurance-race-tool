using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// gridAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GridAdd : Window
    {
        public bool success;
        public string steamid;
        public int laptime;
        public int carId;
        public int classId;
        public string firstName;
        public string lastName;
        public GridAdd()
        {
            InitializeComponent();
            carSelect.ItemsSource = GridArrangement.carData;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.success = false;
            this.Close();
        }

        [GeneratedRegex("[1-9][0-9]*:[0-9]{1,2}[.][0-9]{1,3}")]
        private static partial Regex laptimeRegex();
        [GeneratedRegex("[0-9]{17}")]
        private static partial Regex steamIdRegex();
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Match laptimeMatch = laptimeRegex().Match(laptimeInput.Text);
            if (!laptimeMatch.Success || int.Parse(laptimeInput.Text.Split(":")[1].Split(".")[0]) >= 60)
            {
                Warning LaptimeWarning = new("Invalid format", "Laptime must be written in the format of minute:second.millisecond\nex)2:05.218");
                LaptimeWarning.Owner = this;
                LaptimeWarning.ShowDialog();
                return;
            }
            Match steamIdMatch = steamIdRegex().Match(steamIdInput.Text);
            if (!steamIdMatch.Success)
            {
                Warning SteamIdWarning = new("Invalid format", "SteamID must consist of only 17 digits");
                SteamIdWarning.Owner = this;
                SteamIdWarning.ShowDialog();
                return;
            }
            if (firstNameInput.Text.Length == 0 || lastNameInput.Text.Length == 0)
            {
                Warning NameWarning = new("Invalid name", "First name and last name can't be empty");
                NameWarning.Owner = this;
                NameWarning.ShowDialog();
                return;
            }
            int minute = int.Parse(laptimeInput.Text.Split(":")[0]);
            int second = int.Parse(laptimeInput.Text.Split(":")[1].Split(".")[0]);
            int millisecond = int.Parse(laptimeInput.Text.Split(":")[1].Split(".")[1]);
            this.laptime = minute * 60 * 1000 + second * 1000 + millisecond;
            this.firstName = firstNameInput.Text;
            this.lastName = lastNameInput.Text;
            this.steamid = steamIdInput.Text;
            this.carId = carSelect.SelectedIndex;
            this.classId = GridArrangement.classIdData[GridArrangement.classData[0]];
            this.success = true;
            this.Close();
        }

    }
}
