using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace CsvViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnPickFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select CSV File",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                string filePath = dlg.FileName;
                DataTable dt = LoadCsv(filePath);
                dataGrid.ItemsSource = dt.DefaultView;

                try
                {
                    ShowYearCounts(dt);
                }
                catch(InvalidDataException ex)
                {
                    MessageBox.Show( ex.Message,"CSV Format Error",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
            }
        }

        private DataTable LoadCsv(string filePath)
        {
            DataTable dt = new DataTable();

            using(TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                //Column Names
                if(!parser.EndOfData)
                {
                    string[] columnNames = parser.ReadFields();
                    foreach (var header in columnNames)
                        dt.Columns.Add(header);
                }

                //Rows
                while(!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    dt.Rows.Add(fields);
                }
            }

            return dt;
        }

        private void ShowYearCounts(DataTable dataTable)
        {
            yearCountsList.Items.Clear();

            if (!dataTable.Columns.Contains("Year"))
                throw new InvalidDataException("CSV requires column: 'Year'.");

            Dictionary<string, int> countPerYear = GetMovieCountsPerYear(dataTable);

            foreach(KeyValuePair<string,int> kv in countPerYear.OrderBy(k => k.Key))
                yearCountsList.Items.Add($"{kv.Key}: {kv.Value}");
        }

        private Dictionary<string, int> GetMovieCountsPerYear(DataTable dataTable)
        {
            Dictionary<string, int> countPerYear = new Dictionary<string, int>();

            foreach (DataRow row in dataTable.Rows)
            {
                string releaseYear = row["Year"].ToString();
                if(string.IsNullOrWhiteSpace(releaseYear)) 
                    continue;

                if(countPerYear.ContainsKey(releaseYear))
                    countPerYear[releaseYear]++;
                else
                    countPerYear[releaseYear] = 1;
            }

            return countPerYear;
        }
    }
}
