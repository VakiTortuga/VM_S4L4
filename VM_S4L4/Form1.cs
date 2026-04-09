using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace VM_S4L4
{
    public class Form1 : Form
    {
        private DataGridView dataGridView;
        private Button btnBuild, btnAddRow, btnSetCount;
        private NumericUpDown nudPointsCount;
        private Chart chart;
        private TextBox txtStatus;
        private List<double> xNodes = new List<double>();
        private List<double> yNodes = new List<double>();
        private List<SplineSegment> splineSegments = new List<SplineSegment>();

        private GroupBox groupBoxPolynomial;
        private TextBox txtA, txtB, txtC, txtD;
        private NumericUpDown nudXmin, nudXmax;
        private Button btnAddPolynomial;
        private CheckBox chkShowSpline;
        private List<CustomPolynomial> customPolynomials = new List<CustomPolynomial>();

        public Form1()
        {
            this.Text = "Кубический сплайн (дефект 1)";
            this.Width = 1300;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Верхняя панель
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(10) };

            Label lblCount = new Label { Text = "Количество точек:", Left = 10, Top = 15, Width = 120 };
            nudPointsCount = new NumericUpDown { Left = 140, Top = 12, Width = 60, Minimum = 2, Maximum = 20, Value = 5 };
            btnSetCount = new Button { Text = "Задать сетку", Left = 210, Top = 10, Width = 100 };
            btnSetCount.Click += BtnSetCount_Click;
            btnAddRow = new Button { Text = "Добавить точку", Left = 330, Top = 10, Width = 120 };
            btnAddRow.Click += BtnAddRow_Click;
            btnBuild = new Button { Text = "Построить сплайн", Left = 470, Top = 10, Width = 120 };
            btnBuild.Click += BtnBuild_Click;
            btnBuild.Enabled = false;

            chkShowSpline = new CheckBox { Text = "Показать сплайн", Left = 610, Top = 12, Width = 120, Checked = true };
            chkShowSpline.CheckedChanged += (s, e) => DrawChart();

            txtStatus = new TextBox { Left = 10, Top = 50, Width = 700, Height = 50, Multiline = true, ReadOnly = true, BackColor = Color.LightYellow };

            topPanel.Controls.Add(lblCount);
            topPanel.Controls.Add(nudPointsCount);
            topPanel.Controls.Add(btnSetCount);
            topPanel.Controls.Add(btnAddRow);
            topPanel.Controls.Add(btnBuild);
            topPanel.Controls.Add(chkShowSpline);
            topPanel.Controls.Add(txtStatus);

            // Группа для ввода многочлена
            groupBoxPolynomial = new GroupBox { Text = "Добавить произвольный многочлен 3-й степени: a·x³ + b·x² + c·x + d", Left = 10, Top = 110, Width = 700, Height = 55 };

            Label lblA = new Label { Text = "a:", Left = 10, Top = 25, Width = 20 };
            txtA = new TextBox { Left = 35, Top = 22, Width = 60, Text = "0" };
            Label lblB = new Label { Text = "b:", Left = 105, Top = 25, Width = 20 };
            txtB = new TextBox { Left = 130, Top = 22, Width = 60, Text = "0" };
            Label lblC = new Label { Text = "c:", Left = 200, Top = 25, Width = 20 };
            txtC = new TextBox { Left = 225, Top = 22, Width = 60, Text = "0" };
            Label lblD = new Label { Text = "d:", Left = 295, Top = 25, Width = 20 };
            txtD = new TextBox { Left = 320, Top = 22, Width = 60, Text = "0" };

            Label lblXrange = new Label { Text = "x от:", Left = 400, Top = 25, Width = 35 };
            nudXmin = new NumericUpDown { Left = 440, Top = 22, Width = 60, Minimum = -100, Maximum = 100, Value = 0, DecimalPlaces = 1, Increment = 0.5m };
            Label lblTo = new Label { Text = "до:", Left = 505, Top = 25, Width = 25 };
            nudXmax = new NumericUpDown { Left = 535, Top = 22, Width = 60, Minimum = -100, Maximum = 100, Value = 10, DecimalPlaces = 1, Increment = 0.5m };

            btnAddPolynomial = new Button { Text = "Добавить многочлен", Left = 610, Top = 20, Width = 80 };
            btnAddPolynomial.Click += BtnAddPolynomial_Click;

            groupBoxPolynomial.Controls.Add(lblA);
            groupBoxPolynomial.Controls.Add(txtA);
            groupBoxPolynomial.Controls.Add(lblB);
            groupBoxPolynomial.Controls.Add(txtB);
            groupBoxPolynomial.Controls.Add(lblC);
            groupBoxPolynomial.Controls.Add(txtC);
            groupBoxPolynomial.Controls.Add(lblD);
            groupBoxPolynomial.Controls.Add(txtD);
            groupBoxPolynomial.Controls.Add(lblXrange);
            groupBoxPolynomial.Controls.Add(nudXmin);
            groupBoxPolynomial.Controls.Add(lblTo);
            groupBoxPolynomial.Controls.Add(nudXmax);
            groupBoxPolynomial.Controls.Add(btnAddPolynomial);

            topPanel.Controls.Add(groupBoxPolynomial);

            // Таблица для точек
            dataGridView = new DataGridView
            {
                Width = 250,  // Уменьшили ширину таблицы
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom  // Закрепляем таблицу
            };
            dataGridView.Columns.Add("X", "x");
            dataGridView.Columns.Add("Y", "f(x)");
            dataGridView.Columns[0].ValueType = typeof(double);
            dataGridView.Columns[1].ValueType = typeof(double);

            // График
            chart = new Chart { Dock = DockStyle.Fill };
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "x";
            chartArea.AxisY.Title = "f(x)";
            chartArea.AxisX.MajorGrid.Interval = 1;
            chartArea.AxisY.MajorGrid.Interval = 1;
            chart.ChartAreas.Add(chartArea);
            chart.Legends.Add(new Legend("Legend") { Docking = Docking.Top });

            // Создаем TableLayoutPanel для лучшего контроля размера
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250)); // Фиксированная ширина для таблицы
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // Оставшееся место для графика

            mainTable.Controls.Add(dataGridView, 0, 0);
            mainTable.Controls.Add(chart, 1, 0);

            // Добавляем все в форму
            this.Controls.Add(mainTable);
            this.Controls.Add(topPanel);

            // Убеждаемся, что topPanel всегда сверху
            topPanel.BringToFront();
        }

        private void BtnAddPolynomial_Click(object sender, EventArgs e)
        {
            try
            {
                double a = double.Parse(txtA.Text);
                double b = double.Parse(txtB.Text);
                double c = double.Parse(txtC.Text);
                double d = double.Parse(txtD.Text);
                double xmin = (double)nudXmin.Value;
                double xmax = (double)nudXmax.Value;

                if (xmin >= xmax)
                    throw new Exception("x_min должно быть меньше x_max");

                customPolynomials.Add(new CustomPolynomial(a, b, c, d, xmin, xmax));
                txtStatus.Text = $"Добавлен многочлен: {a}·x³ + {b}·x² + {c}·x + {d}, x∈[{xmin};{xmax}]";
                DrawChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSetCount_Click(object sender, EventArgs e)
        {
            int n = (int)nudPointsCount.Value;
            dataGridView.Rows.Clear();
            for (int i = 0; i < n; i++)
                dataGridView.Rows.Add(0, 0);

            if (n == 5)
            {
                double[] exampleX = { 2, 4, 5, 6, 7 };
                double[] exampleY = { 6, 6, 1, -1, 11 };
                for (int i = 0; i < n; i++)
                {
                    dataGridView.Rows[i].Cells[0].Value = exampleX[i];
                    dataGridView.Rows[i].Cells[1].Value = exampleY[i];
                }
            }
            txtStatus.Text = $"Задана сетка из {n} точек.";
            btnBuild.Enabled = true;
        }

        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Add(0, 0);
            txtStatus.Text = $"Всего точек: {dataGridView.Rows.Count}";
            nudPointsCount.Value = dataGridView.Rows.Count;
        }

        private void BtnBuild_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPoints();
                BuildSpline();
                DrawChart();
                txtStatus.Text = "Сплайн построен успешно!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStatus.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void LoadPoints()
        {
            xNodes.Clear();
            yNodes.Clear();
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                var cellX = dataGridView.Rows[i].Cells[0].Value;
                var cellY = dataGridView.Rows[i].Cells[1].Value;
                if (cellX == null || cellY == null)
                    throw new Exception("Есть пустые ячейки!");
                xNodes.Add(Convert.ToDouble(cellX));
                yNodes.Add(Convert.ToDouble(cellY));
            }

            var points = xNodes.Zip(yNodes, (x, y) => new { x, y }).OrderBy(p => p.x).ToList();
            xNodes = points.Select(p => p.x).ToList();
            yNodes = points.Select(p => p.y).ToList();

            for (int i = 1; i < xNodes.Count; i++)
                if (xNodes[i] <= xNodes[i - 1])
                    throw new Exception("x должны строго возрастать!");
        }

        private void BuildSpline()
        {
            int n = xNodes.Count;
            double[] h = new double[n];
            for (int i = 1; i < n; i++)
                h[i] = xNodes[i] - xNodes[i - 1];

            double[] c = new double[n];
            c[0] = 0;
            c[n - 1] = 0;

            if (n == 2)
            {
                double a = yNodes[0];
                double b = (yNodes[1] - yNodes[0]) / h[1];
                double d = 0;
                double c_val = 0;
                splineSegments.Clear();
                splineSegments.Add(new SplineSegment(xNodes[0], xNodes[1], a, b, c_val, d));
                return;
            }

            double[] A = new double[n - 2];
            double[] B = new double[n - 2];
            double[] C = new double[n - 2];
            double[] D = new double[n - 2];

            for (int i = 1; i < n - 1; i++)
            {
                double hi = h[i];
                double hi1 = h[i + 1];
                double left = (yNodes[i] - yNodes[i - 1]) / hi;
                double right = (yNodes[i + 1] - yNodes[i]) / hi1;

                A[i - 1] = hi;
                B[i - 1] = 2 * (hi + hi1);
                C[i - 1] = hi1;
                D[i - 1] = 3 * (right - left);
            }

            double[] cInternal = ThomasAlgorithm(A, B, C, D);

            for (int i = 1; i < n - 1; i++)
                c[i] = cInternal[i - 1];

            splineSegments.Clear();
            for (int i = 1; i < n; i++)
            {
                double hi = h[i];
                double a = yNodes[i - 1];
                double b = (yNodes[i] - yNodes[i - 1]) / hi - hi * (2 * c[i - 1] + c[i]) / 3;
                double d = (c[i] - c[i - 1]) / (3 * hi);

                splineSegments.Add(new SplineSegment(xNodes[i - 1], xNodes[i], a, b, c[i - 1], d));
            }
        }

        private double[] ThomasAlgorithm(double[] a, double[] b, double[] c, double[] d)
        {
            int n = a.Length;
            double[] cp = new double[n];
            double[] dp = new double[n];

            cp[0] = c[0] / b[0];
            dp[0] = d[0] / b[0];

            for (int i = 1; i < n; i++)
            {
                double m = 1.0 / (b[i] - a[i] * cp[i - 1]);
                cp[i] = c[i] * m;
                dp[i] = (d[i] - a[i] * dp[i - 1]) * m;
            }

            double[] x = new double[n];
            x[n - 1] = dp[n - 1];

            for (int i = n - 2; i >= 0; i--)
                x[i] = dp[i] - cp[i] * x[i + 1];

            return x;
        }

        private void DrawChart()
        {
            chart.Series.Clear();

            // Рисуем сплайн
            if (chkShowSpline.Checked && splineSegments.Count > 0)
            {
                Series seriesSpline = new Series("Кубический сплайн");
                seriesSpline.ChartType = SeriesChartType.Line;
                seriesSpline.BorderWidth = 3;
                seriesSpline.Color = Color.Red;
                seriesSpline.Legend = "Legend";

                foreach (var segment in splineSegments)
                {
                    int steps = 100;
                    for (int j = 0; j <= steps; j++)
                    {
                        double x = segment.X0 + (segment.X1 - segment.X0) * j / steps;
                        double dx = x - segment.X0;
                        double y = segment.A + segment.B * dx + segment.C * dx * dx + segment.D * dx * dx * dx;
                        seriesSpline.Points.AddXY(x, y);
                    }
                }
                chart.Series.Add(seriesSpline);
            }

            // Рисуем произвольные члены
            Random rand = new Random();
            foreach (var poly in customPolynomials)
            {
                string seriesName = $"Многочлен: {poly.A}·x³ + {poly.B}·x² + {poly.C}·x + {poly.D}";
                Series seriesPoly = new Series(seriesName);
                seriesPoly.ChartType = SeriesChartType.Line;
                seriesPoly.BorderWidth = 2;
                seriesPoly.Color = Color.FromArgb(rand.Next(100, 255), rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
                seriesPoly.Legend = "Legend";

                int steps = 200;
                for (int j = 0; j <= steps; j++)
                {
                    double x = poly.Xmin + (poly.Xmax - poly.Xmin) * j / steps;
                    double y = poly.A * x * x * x + poly.B * x * x + poly.C * x + poly.D;
                    seriesPoly.Points.AddXY(x, y);
                }
                chart.Series.Add(seriesPoly);
            }

            // Рисуем исходные точки
            if (xNodes.Count > 0)
            {
                Series seriesPoints = new Series("Исходные точки");
                seriesPoints.ChartType = SeriesChartType.Point;
                seriesPoints.MarkerStyle = MarkerStyle.Circle;
                seriesPoints.MarkerSize = 10;
                seriesPoints.MarkerColor = Color.Blue;
                seriesPoints.Color = Color.Blue;

                for (int i = 0; i < xNodes.Count; i++)
                    seriesPoints.Points.AddXY(xNodes[i], yNodes[i]);

                chart.Series.Add(seriesPoints);
            }

            // Настройка осей
            if (xNodes.Count > 0)
            {
                double minX = xNodes.Min() - 2;
                double maxX = xNodes.Max() + 2;
                double minY = yNodes.Min() - 3;
                double maxY = yNodes.Max() + 3;

                foreach (var poly in customPolynomials)
                {
                    minX = Math.Min(minX, poly.Xmin);
                    maxX = Math.Max(maxX, poly.Xmax);
                }

                chart.ChartAreas[0].AxisX.Minimum = minX;
                chart.ChartAreas[0].AxisX.Maximum = maxX;
                chart.ChartAreas[0].AxisY.Minimum = minY;
                chart.ChartAreas[0].AxisY.Maximum = maxY;
            }
        }
    }

    public class SplineSegment
    {
        public double X0 { get; set; }
        public double X1 { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }

        public SplineSegment(double x0, double x1, double a, double b, double c, double d)
        {
            X0 = x0;
            X1 = x1;
            A = a;
            B = b;
            C = c;
            D = d;
        }
    }

    public class CustomPolynomial
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double Xmin { get; set; }
        public double Xmax { get; set; }

        public CustomPolynomial(double a, double b, double c, double d, double xmin, double xmax)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Xmin = xmin;
            Xmax = xmax;
        }
    }
}