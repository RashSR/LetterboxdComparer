using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LetterboxdComparer
{
    public class BarChartPresenter
    {
        private readonly Canvas _canvas;
        private readonly ListBox _yearCountsList;

        public BarChartPresenter(Canvas canvas, ListBox yearCountsList)
        {
            _canvas = canvas;
            _yearCountsList = yearCountsList;

            PickCsvCommand = new RelayCommand(_ => PickAndLoadCsv());
        }

        // Expose a command to bind to the button in XAML
        public ICommand PickCsvCommand { get; }

        // Main method: pick CSV and process
        private void PickAndLoadCsv()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select CSV File",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true)
                return;

            DataTable dt = LoadCsv(dlg.FileName);

            var counts = GetMovieCountsPerYear(dt);

            // Display counts in ListBox
            _yearCountsList.Items.Clear();
            foreach (var kv in counts.OrderBy(k => int.Parse(k.Key)))
                _yearCountsList.Items.Add($"{kv.Key}: {kv.Value}");

            // Draw bar chart
            DrawBarChart(counts);
        }

        private DataTable LoadCsv(string filePath)
        {
            var dt = new DataTable();

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                // Headers
                if (!parser.EndOfData)
                {
                    var headers = parser.ReadFields();
                    foreach (var h in headers) dt.Columns.Add(h);
                }

                // Rows
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    dt.Rows.Add(fields);
                }
            }

            if (!dt.Columns.Contains("Year"))
                throw new InvalidDataException("CSV requires column: 'Year'.");

            return dt;
        }

        private Dictionary<string, int> GetMovieCountsPerYear(DataTable dt)
        {
            var countPerYear = new Dictionary<string, int>();

            foreach (DataRow row in dt.Rows)
            {
                string year = row["Year"].ToString();
                if (string.IsNullOrWhiteSpace(year)) continue;

                if (countPerYear.ContainsKey(year))
                    countPerYear[year]++;
                else
                    countPerYear[year] = 1;
            }

            return countPerYear;
        }

        private void DrawBarChart(Dictionary<string, int> counts)
        {
            _canvas.Children.Clear();
            if (counts.Count == 0) return;

            var sorted = counts
                .Where(k => int.TryParse(k.Key, out _))
                .OrderBy(k => int.Parse(k.Key))
                .ToList();

            double canvasHeight = _canvas.Height;
            double barWidth = 40;
            _canvas.Width = Math.Max(sorted.Count * barWidth, 500);

            int maxCount = sorted.Max(k => k.Value);
            if (maxCount == 0) maxCount = 1;
            double scale = (canvasHeight - 20) / maxCount;

            for (int i = 0; i < sorted.Count; i++)
            {
                var year = sorted[i].Key;
                var count = sorted[i].Value;

                double barHeight = count * scale;
                double x = i * barWidth;
                double y = canvasHeight - barHeight;

                // Original color
                var barBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E056")); // your hex

                // Bar
                var rect = new Rectangle
                {
                    Width = barWidth - 10,
                    Height = barHeight,
                    Fill = barBrush,
                    ToolTip = $"{year}: {count} movies"
                };
                Canvas.SetLeft(rect, x + 5);
                Canvas.SetTop(rect, y);

                // Hover effect
                rect.MouseEnter += (s, e) => rect.Fill = Brushes.Orange;
                rect.MouseLeave += (s, e) => rect.Fill = barBrush;

                _canvas.Children.Add(rect);

                // Count above bar
                var countLabel = new TextBlock
                {
                    Text = count.ToString(),
                    Width = barWidth,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                Canvas.SetLeft(countLabel, x);
                Canvas.SetTop(countLabel, y - 15);
                _canvas.Children.Add(countLabel);

                // Year below bar
                var yearLabel = new TextBlock
                {
                    Text = year,
                    Width = barWidth,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                Canvas.SetLeft(yearLabel, x);
                Canvas.SetTop(yearLabel, canvasHeight + 2);
                _canvas.Children.Add(yearLabel);
            }

            _canvas.Height += 20; // extra space for labels
        }
    }
}
