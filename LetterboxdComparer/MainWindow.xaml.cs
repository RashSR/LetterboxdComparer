using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                    Dictionary<string, int> countPerYear = GetMovieCountsPerYear(dt);

                    // Draw bar chart
                    DrawBarChart(countPerYear);
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(ex.Message, "CSV Format Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    barChartCanvas.Children.Clear();
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

        private void DrawBarChart(Dictionary<string, int> counts)
        {
            barChartCanvas.Children.Clear();
            if (counts.Count == 0) return;

            // Sort by numeric year
            var sorted = counts
                .Where(k => int.TryParse(k.Key, out _)) // only numeric keys
                .OrderBy(k => int.Parse(k.Key))
                .ToList();

            double canvasHeight = barChartCanvas.Height;
            double barWidth = 40;
            double canvasWidth = Math.Max(sorted.Count * barWidth, 500);
            barChartCanvas.Width = canvasWidth;

            int maxCount = sorted.Max(k => k.Value);
            if(maxCount == 0) 
                maxCount = 1; // avoid divide by zero
            
            double scale = (canvasHeight - 20) / maxCount; // leave padding on top

            for (int i = 0; i < sorted.Count; i++)
            {
                var year = sorted[i].Key;
                var count = sorted[i].Value;

                double barHeight = count * scale;
                double x = i * barWidth;
                double y = canvasHeight - barHeight;

                // Bar rectangle
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth - 10, // spacing
                    Height = barHeight,
                    Fill = System.Windows.Media.Brushes.Gray
                };
                Canvas.SetLeft(rect, x + 5);
                Canvas.SetTop(rect, y);
                barChartCanvas.Children.Add(rect);

                // Add year label BELOW the bar
                var label = new TextBlock
                {
                    Text = year,
                    Width = barWidth,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(label, x);
                Canvas.SetTop(label, canvasHeight + 2); // just below the canvas bottom
                barChartCanvas.Children.Add(label);

                // Adjust the canvas height if needed to fit labels
                barChartCanvas.Height = canvasHeight + 20; // extra space for year labels

                // Count label
                var countLabel = new TextBlock
                {
                    Text = count.ToString(),
                    Width = barWidth,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(countLabel, x);
                Canvas.SetTop(countLabel, y - 15);
                barChartCanvas.Children.Add(countLabel);
            }
        }

    }
}
