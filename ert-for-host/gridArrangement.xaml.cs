using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using static ert_for_host.GridArrangement;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using Microsoft.Office.Interop.Excel;

namespace ert_for_host
{
    /// <summary>
    /// gridArrangement.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GridArrangement : UserControl
    {
        public readonly static List<string> carData = new() { "Porsche 991 GT3 R", "Mercedes-AMG GT3", "Ferrari 488 GT3", "Audi R8 LMS", "Lamborghini Huracan GT3", "McLaren 650S GT3", "Nissan GT-R Nismo GT3 (2018)", "BMW M6 GT3", "Bentley Continental GT3 (2018)", "Porsche 991II GT3 Cup", "Nissan GT-R Nismo GT3 (2017)", "Bentley Continental GT3 (2016)", "Aston Martin V12 Vantage GT3", "Lamborghini Gallardo R-EX", "Jaguar G3", "Lexus RC F GT3", "Lamborghini Huracan Evo (2019)", "Honda NSX GT3", "Lamborghini Huracan SuperTrofeo", "Audi R8 LMS Evo (2019)", "AMR V8 Vantage (2019)", "Honda NSX Evo (2019)", "McLaren 720S GT3 (2019)", "Porsche 911II GT3 R (2019)", "Ferrari 488 GT3 Evo (2020)", "Mercedes-AMG GT3 (2020)", "Ferrari 488 Challenge Evo", "BMW M2 CS Racing", "Porsche 911 GT3 Cup (Type 992)", "Lamborghini Huracán Super Trofeo Evo2", "BMW M4 GT3", "Audi R8 LMS GT3 evo II", "Ferrari 296 GT3", "Lamborghini Huracan Evo2", "Porsche 992 GT3 R", "McLaren 720S GT3 Evo (2023)", "Alpine A110 GT4", "AMR V8 Vantage GT4", "Audi R8 LMS GT4", "BMW M4 GT4", "Chevrolet Camaro GT4", "Ginetta G55 GT4", "KTM X-Bow GT4", "Maserati MC GT4", "McLaren 570S GT4", "Mercedes-AMG GT4", "Porsche 718 Cayman GT4", "Audi R8 LMS GT2", "KTM XBOW GT2", "Maserati MC20 GT2", "Mercedes AMG GT2", "Porsche 911 GT2 RS CS Evo", "Porsche 935" };
        public readonly static List<string> classData = new() { "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GTC", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GTC", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GTC", "TCX", "GTC", "GTC", "GT3", "GT3", "GT3", "GT3", "GT3", "GT3", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT4", "GT2", "GT2", "GT2", "GT2", "GT2", "GT2" };
        public readonly static Dictionary<string, int> classIdData = new() {
            {"GT2", 0},
            {"GT3", 1},
            {"GT4", 2},
            {"GTC", 3},
            {"TCX", 4}
        };
        public readonly Dictionary<int, List<Lap>> recordDataByClass = new() {
            {0, new() },
            {1, new() },
            {2, new() },
            {3, new() },
            {4, new() }
        };
        public List<Lap> displayLaps = new();
        public bool ready = false;
        public GridArrangement()
        {
            InitializeComponent();
            carSort.ItemsSource = carData;
            recordDataGrid.ItemsSource = displayLaps;
        }
        public void RefreshRecordData()
        {
            displayLaps.Clear();
            // filter records by class
            bool separateByClass = separateClassCheck.IsChecked ?? true;
            List<bool> classFilter = new()
            {
                gt2Check.IsChecked ?? true,
                gt3Check.IsChecked ?? true,
                gt4Check.IsChecked ?? true,
                gtcCheck.IsChecked ?? true,
                tcxCheck.IsChecked ?? true
            };
            List<List<Lap>> filteredData = new();
            for (int i = 0; i < classFilter.Count; i++)
            {
                if (classFilter[i])
                    filteredData.Add(recordDataByClass[i]);
            }

            // filter record with different car in same class
            if (hideSameClassCheck.IsChecked ?? true)
            {
                for (int i = 0; i < filteredData.Count; i++) {
                    Dictionary<string, Lap> filterDuplication = new();
                    foreach (Lap lap in filteredData[i])
                    {
                        if (!filterDuplication.ContainsKey(lap.SteamId))
                        {
                            filterDuplication.Add(lap.SteamId, lap);
                        } else
                        {
                            if (filterDuplication[lap.SteamId].Laptime >= lap.Laptime)
                            {
                                filterDuplication[lap.SteamId] = lap;
                            }
                        }
                    }
                    filteredData[i] = filterDuplication.Values.ToList<Lap>();
                }
            }

            // sort
            if (separateByClass)
            {
                foreach (List<Lap> records in filteredData)
                {
                    displayLaps.AddRange(records);
                }
            } else
            {
                foreach(List<Lap> records in filteredData)
                {
                    displayLaps.AddRange(records);
                }
                displayLaps = displayLaps.OrderBy(x => x.Laptime).ToList<Lap>();
            }

            // change displayed items
            recordDataGrid.Items.Refresh();
        }
        private static string AddZeroLeft(string value, int length)
        {
            while (length - value.Length > 0)
            {
                value = "0" + value;
                length--;
            }
            return value;
        }
        private static string AddZeroRight(string value, int length)
        {
            while (length - value.Length > 0)
            {
                value += "0";
                length--;
            }
            return value;
        }

        private static string FormatTime(int time, string format= "%m:%s.%ms")
        {
            int ms = time % 1000;
            time -= ms;
            time /= 1000;
            int s = time % 60;
            time -= s;
            time /= 60;
            int m = time;
            return format.Replace("%ms", AddZeroRight(ms.ToString(), 3)).Replace("%s", AddZeroLeft(s.ToString(), 2)).Replace("%m", m.ToString());
        }



        private void ExportClick(object sender, RoutedEventArgs e)
        {
            /*Microsoft.Office.Interop.Excel.Application excel = new();
            Workbook workbook = excel.Workbooks.Add();
            excel.DisplayAlerts = false;
            Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets["Sheet1"];
            List<string> headers = new List<string>() {
                "SteamID", "FirstName", "LastName", "Laptime", "Car", "CarID", "Class"
            };
            for (int headerCol = 0; headerCol < headers.Count; headerCol++)
            {
                worksheet.Cells[1, headerCol + 1] = headers[headerCol];
            }

            List<Lap> records = recordDataGrid.Items.Cast<Lap>().ToList();
            for (int row = 0; row < records.Count; row ++)
            {

                worksheet.Cells[row + 2, 1] = records[row].SteamId;
                worksheet.Cells[row + 2, 2] = records[row].FirstName;
                worksheet.Cells[row + 2, 3] = records[row].LastName;
                worksheet.Cells[row + 2, 4] = records[row].LaptimeString;
                worksheet.Cells[row + 2, 5] = records[row].Car;
                worksheet.Cells[row + 2, 6] = records[row].CarId; 
                worksheet.Cells[row + 2, 7] = records[row].ClassType;
            }

            SaveFileDialog excelSave = new();
            excelSave.Filter = "Excel worksheet (*.xls)|*.xlsx";
            excelSave.FileName = "result.xlsx";
            if (excelSave.ShowDialog() == true)
            {
                worksheet.SaveAs(excelSave.FileName);
                workbook.Close();
                excel.Quit();
            }*/
            
        }
        public class Lap
        {
            public int Laptime { get; set; }
            public string SteamId { get; set; }
            public string Car { get; set; }
            public string ClassType { get; set; }
            public int ClassId { get; set; }
            public string LaptimeString { get; set; }
            public int CarId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public static bool Equals(Lap lap1, Lap lap2)
            {
                return lap1.SteamId == lap2.SteamId && lap1.CarId == lap2.CarId;
            }
        }
        public void LoadFile(string filePath)

        {
            string resultData = File.ReadAllText(filePath);
            JObject parsedData = JObject.Parse(resultData);
            if (parsedData["sessionType"].ToObject<string>() == "R") return;
            foreach(JToken lap in parsedData["sessionResult"]["leaderBoardLines"])
            {
                Lap data = new();
                data.Laptime = lap["timing"]["bestLap"].ToObject<int>();
                data.FirstName = lap["currentDriver"]["firstName"].ToObject<string>();
                data.LastName = lap["currentDriver"]["lastName"].ToObject<string>();
                data.SteamId = lap["currentDriver"]["playerId"].ToObject<string>().Substring(0);
                int car = lap["car"]["carModel"].ToObject<int>();
                data.ClassType = classData[car];
                data.ClassId = classIdData[data.ClassType];
                data.Car = carData[car];
                data.CarId = car;
                data.LaptimeString = FormatTime(data.Laptime);
                recordDataByClass[data.ClassId].Add(data);
            }
            for (int i =0; i < recordDataByClass.Count;i++)
            {
                recordDataByClass[i] = recordDataByClass[i].OrderBy(x => x.Laptime).ToList<Lap>();
            }
        }
        private void ImportClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Multiselect = true;
            ofd.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            if (ofd.ShowDialog() ?? false)
            {
                foreach (string filepath in ofd.FileNames)
                {
                    try
                    {
                        LoadFile(filepath);
                    } catch { }
                }
                RefreshRecordData();
            }
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            if (displayLaps.Count > 0)
            {
                Warning clearWarning = new("Clear all the records", displayLaps.Count.ToString() + " records will be cleared and this cannot be undone.\nAre you sure?", 0);
                clearWarning.Owner = System.Windows.Window.GetWindow(this);
                if (clearWarning.ShowDialog() ?? false)
                {
                    if (clearWarning.cancelled) return;
                }
            }
            for (int i=0; i < recordDataByClass.Count; i++)
            {
                recordDataByClass[i] = new();
            }
            displayLaps.Clear();
            recordDataGrid.ItemsSource = null;
            recordDataGrid.ItemsSource = displayLaps;
            recordDataGrid.Items.Refresh();
        }

        [GeneratedRegex("[1-9][0-9]*:[0-9]{1,2}[.][0-9]{1,3}")]
        private static partial Regex laptimeRegex();

        private bool CheckLaptime(string laptime)
        {

            Match laptimeMatch = laptimeRegex().Match(laptime);
            if (!laptimeMatch.Success || int.Parse(laptime.Split(":")[1].Split(".")[0]) >= 60)
            {
                Warning LaptimeWarning = new("Invalid format", "Laptime must be written in the format of minute:second.millisecond\nex)2:05.218");
                LaptimeWarning.Owner = System.Windows.Window.GetWindow(this);
                LaptimeWarning.ShowDialog();
                return false;
            }
            return true;
        }

        [GeneratedRegex("[0-9]{17}")]
        private static partial Regex steamIdRegex();
        private bool CheckSteamId(string steamId)
        {

            Match steamIdMatch = steamIdRegex().Match(steamId);
            if (!steamIdMatch.Success)
            {
                Warning SteamIdWarning = new("Invalid format", "SteamID must consist of only 17 digits");
                SteamIdWarning.Owner = System.Windows.Window.GetWindow(this);
                SteamIdWarning.ShowDialog();
                return false;
            }
            return true;
        }

        private bool CheckName(string firstName, string lastName)
        {
            if (firstName.Length == 0 || lastName.Length == 0)
            {
                Warning NameWarning = new("Invalid name", "First name and last name can't be empty");
                NameWarning.Owner = System.Windows.Window.GetWindow(this);
                NameWarning.ShowDialog();
                return false;
            }
            return true;
        }
        private void RecordDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Lap lap = (Lap)e.Row.Item;
            if (!CheckSteamId(lap.SteamId)) return;
            if (!CheckLaptime(lap.LaptimeString)) return;
            if (!CheckName(lap.FirstName, lap.LastName)) return;

            int minute = int.Parse(lap.LaptimeString.Split(":")[0]);
            int second = int.Parse(lap.LaptimeString.Split(":")[1].Split(".")[0]);
            int millisecond = int.Parse(lap.LaptimeString.Split(":")[1].Split(".")[1]);
            lap.Laptime = minute * 60 * 1000 + second * 1000 + millisecond;
            lap.LaptimeString = FormatTime(lap.Laptime);
            lap.CarId = carData.FindIndex(x => x.Equals(lap.Car));
            lap.ClassType = classData[lap.CarId];
            lap.ClassId = classIdData[lap.ClassType];
            (sender as DataGrid).RowEditEnding -= RecordDataGrid_RowEditEnding;
            (sender as DataGrid).CommitEdit();
            RefreshRecordData();
            (sender as DataGrid).RowEditEnding += RecordDataGrid_RowEditEnding;
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            GridAdd gridInput = new();
            gridInput.Owner = System.Windows.Window.GetWindow(this);
            gridInput.ShowDialog();
            if (gridInput.success)
            {
                Lap lap = new()
                {
                    Laptime = gridInput.laptime,
                    SteamId = gridInput.steamid,
                    CarId = gridInput.carId,
                    Car = carData[gridInput.carId],
                    ClassId = gridInput.classId,
                    ClassType = classData[gridInput.carId],
                    FirstName = gridInput.firstName,
                    LastName = gridInput.lastName,
                    LaptimeString = FormatTime(gridInput.laptime)
                };
                foreach (Lap item in recordDataByClass[lap.ClassId])
                {
                    if (item.CarId == lap.CarId)
                    {
                        Warning duplicateWarning = new("Duplicate Record", "Record with same car already exists");
                        duplicateWarning.Owner = System.Windows.Window.GetWindow(this);
                        duplicateWarning.ShowDialog();
                        return;
                    }
                }
                recordDataByClass[lap.ClassId].Add(lap);
                RefreshRecordData();
            }
        }
        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            System.Collections.IList temp = (System.Collections.IList)recordDataGrid.SelectedItems;
            List<Lap> selected = temp.Cast<Lap>().ToList();
            foreach (Lap lap in selected)
            {
                for (int i = 0; i < recordDataByClass[lap.ClassId].Count; i++)
                {
                    if (Lap.Equals(lap, recordDataByClass[lap.ClassId][i]))
                    {
                        recordDataByClass[lap.ClassId].RemoveAt(i);
                        break;
                    }
                }
            }
            foreach (Lap lap in selected)
            {
                for (int i = 0; i < displayLaps.Count; i++)
                {
                    if (Lap.Equals(lap, displayLaps[i]))
                    {
                        displayLaps.RemoveAt(i);
                        break;
                    }
                }
            }
            recordDataGrid.ItemsSource = displayLaps;
            recordDataGrid.Items.Refresh();

        }

        private void SeparateClassCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (this.ready) RefreshRecordData();
        }

        private void SeparateClassCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.ready) RefreshRecordData();
        }

        private void HideSameClassCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (this.ready) RefreshRecordData();
        }

        private void HideSameClassCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.ready) RefreshRecordData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ready = true;

        }


        private void RecordDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                recordDataGrid.SelectedIndex = -1;
                recordDataGrid.SelectedCells.Clear();
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                this.RemoveClick(new(), new());
            }

        }
    }
}
