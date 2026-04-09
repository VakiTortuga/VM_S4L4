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
        private List<CustomPolynomial> splineSegments = new List<CustomPolynomial>();

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
                Dock = DockStyle.Left,
                Width = 300,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
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

            SplitContainer splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            splitContainer.Panel1.Controls.Add(dataGridView);
            splitContainer.Panel2.Controls.Add(chart);
            splitContainer.SplitterDistance = 300;

            this.Controls.Add(splitContainer);
            this.Controls.Add(topPanel);
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

            // получаем значения ширины каждого интервала
            double[] h = new double[n];
            for (int i = 1; i < n; i++)
                h[i] = xNodes[i] - xNodes[i - 1];

            double[] c = new double[n];
            c[0] = 0;
            c[n - 1] = 0;

            // частный случай решения для двух точек
            if (n == 2)
            {
                double a = yNodes[0];
                double b = (yNodes[1] - yNodes[0]) / h[1];
                double d = 0;
                double c_val = 0;
                splineSegments.Clear();
                splineSegments.Add(new CustomPolynomial(a, b, c_val, d, xNodes[0], xNodes[1]));
                return;
            }

            int m = n - 2;

            float[,] matrix = new float[m, m];
            float[] vector = new float[m];

            for (int i = 0; i < m; i++)
            {
                int idx = i + 1;

                double hi = h[idx];
                double hi1 = h[idx + 1];
                double left = (yNodes[idx] - yNodes[idx - 1]) / hi;
                double right = (yNodes[idx + 1] - yNodes[idx]) / hi1;

                if (i > 0)
                    matrix[i, i - 1] = (float)hi;

                matrix[i, i] = (float)(2 * (hi + hi1));

                if (i < m - 1)
                    matrix[i, i + 1] = (float)hi1;

                vector[i] = (float)(3 * (right - left));
            }

            // Решаем СЛАУ методом Гаусса с выбором главного элемента
            float[]? cInternal = gaussWithMaxElementByMatrix(matrix, vector, m);

            if (cInternal == null)
            {
                throw new Exception("Не удалось решить систему уравнений для сплайна");
            }

            // Заполняем коэффициенты c (c[0] и c[n-1] уже равны 0)
            for (int i = 1; i < n - 1; i++)
                c[i] = cInternal[i - 1];

            // Строим сегменты сплайна
            splineSegments.Clear();
            for (int i = 1; i < n; i++)
            {
                double hi = h[i];
                double a = yNodes[i - 1];
                double b = (yNodes[i] - yNodes[i - 1]) / hi - hi * (2 * c[i - 1] + c[i]) / 3;
                double d = (c[i] - c[i - 1]) / (3 * hi);

                splineSegments.Add(new CustomPolynomial(a, b, c[i - 1], d, xNodes[i - 1], xNodes[i]));
            }
        }

        private float[]? gaussWithMaxElementByMatrix(float[,] A, float[] Y, int N)
        {
            float[] X = new float[N];
            float[,] A_temp = CopyMatrix(A, N);
            float[] Y_temp = CopyVector(Y, N);
            int[] columnOrder = new int[N];
            for (int i = 0; i < N; i++) columnOrder[i] = i;

            const float eps = 0.0000001f;

            for (int k = 0; k < N; k++)
            {
                int maxRow = k;
                int maxCol = k;
                float maxVal = Math.Abs(A_temp[k, k]);

                // Поиск максимального элемента
                for (int i = k; i < N; i++)
                {
                    for (int j = k; j < N; j++)
                    {
                        if (Math.Abs(A_temp[i, j]) > maxVal)
                        {
                            maxVal = Math.Abs(A_temp[i, j]);
                            maxRow = i;
                            maxCol = j;
                        }
                    }
                }

                if (maxVal < eps)
                {
                    MessageBox.Show("Решение получить невозможно из-за вырожденности матрицы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Перестановка строк
                if (maxRow != k)
                {
                    for (int j = 0; j < N; j++)
                    {
                        float temp = A_temp[k, j];
                        A_temp[k, j] = A_temp[maxRow, j];
                        A_temp[maxRow, j] = temp;
                    }

                    float tempY = Y_temp[k];
                    Y_temp[k] = Y_temp[maxRow];
                    Y_temp[maxRow] = tempY;
                }

                // Перестановка столбцов
                if (maxCol != k)
                {
                    for (int i = 0; i < N; i++)
                    {
                        float temp = A_temp[i, k];
                        A_temp[i, k] = A_temp[i, maxCol];
                        A_temp[i, maxCol] = temp;
                    }

                    int tempCol = columnOrder[k];
                    columnOrder[k] = columnOrder[maxCol];
                    columnOrder[maxCol] = tempCol;
                }

                // Прямой ход метода Гаусса
                for (int i = k + 1; i < N; i++)
                {
                    float factor = A_temp[i, k] / A_temp[k, k];

                    for (int j = k; j < N; j++)
                    {
                        A_temp[i, j] -= factor * A_temp[k, j];
                    }
                    Y_temp[i] -= factor * Y_temp[k];
                }
            }

            // Обратный ход
            float[] X_temp = new float[N];
            for (int i = N - 1; i >= 0; i--)
            {
                X_temp[i] = Y_temp[i];
                for (int j = i + 1; j < N; j++)
                {
                    X_temp[i] -= A_temp[i, j] * X_temp[j];
                }

                if (Math.Abs(A_temp[i, i]) < 0.0000001f)
                {
                    MessageBox.Show("Деление на ноль при обратном ходе", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                X_temp[i] /= A_temp[i, i];
            }

            // Восстанавливаем порядок переменных
            for (int i = 0; i < N; i++)
            {
                X[columnOrder[i]] = X_temp[i];
            }

            return X;
        }

        // Вспомогательные методы для копирования
        private float[,] CopyMatrix(float[,] source, int N)
        {
            float[,] result = new float[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    result[i, j] = source[i, j];
                }
            }
            return result;
        }

        private float[] CopyVector(float[] source, int N)
        {
            float[] result = new float[N];
            for (int i = 0; i < N; i++)
            {
                result[i] = source[i];
            }
            return result;
        }

        private void DrawChart()
        {
            chart.Series.Clear();

            // Рисуем сплайн
            if (chkShowSpline.Checked && splineSegments.Count > 0)
            {
                Series seriesSpline = new Series("Кубический сплайн");
                seriesSpline.ChartType = SeriesChartType.Line;
                seriesSpline.BorderWidth = 4;
                seriesSpline.Color = Color.Red;
                seriesSpline.Legend = "Legend";

                foreach (var segment in splineSegments)
                {
                    int steps = 100;
                    for (int j = 0; j <= steps; j++)
                    {
                        double x = segment.Xmin + (segment.Xmax - segment.Xmin) * j / steps;
                        double dx = x - segment.Xmin;
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
                seriesPoly.Color = Color.FromArgb(255 , 0, 255, 0);
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

            // Настройка осей с целыми значениями
            ChartArea chartArea = chart.ChartAreas[0];

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
                    minY = Math.Min(minY, Math.Min(poly.A * poly.Xmin * poly.Xmin * poly.Xmin +
                                                   poly.B * poly.Xmin * poly.Xmin +
                                                   poly.C * poly.Xmin + poly.D,
                                                   poly.A * poly.Xmax * poly.Xmax * poly.Xmax +
                                                   poly.B * poly.Xmax * poly.Xmax +
                                                   poly.C * poly.Xmax + poly.D));
                    maxY = Math.Max(maxY, Math.Max(poly.A * poly.Xmin * poly.Xmin * poly.Xmin +
                                                   poly.B * poly.Xmin * poly.Xmin +
                                                   poly.C * poly.Xmin + poly.D,
                                                   poly.A * poly.Xmax * poly.Xmax * poly.Xmax +
                                                   poly.B * poly.Xmax * poly.Xmax +
                                                   poly.C * poly.Xmax + poly.D));
                }

                // Округляем границы до целых чисел
                minX = Math.Floor(minX);
                maxX = Math.Ceiling(maxX);
                minY = Math.Floor(minY);
                maxY = Math.Ceiling(maxY);

                chartArea.AxisX.Minimum = minX;
                chartArea.AxisX.Maximum = maxX;
                chartArea.AxisY.Minimum = minY;
                chartArea.AxisY.Maximum = maxY;

                // Настройка интервалов для целых значений
                // Определяем оптимальный интервал для оси X
                double xRange = maxX - minX;
                if (xRange <= 10)
                    chartArea.AxisX.Interval = 1;
                else if (xRange <= 20)
                    chartArea.AxisX.Interval = 2;
                else if (xRange <= 50)
                    chartArea.AxisX.Interval = 5;
                else
                    chartArea.AxisX.Interval = 10;

                // Определяем оптимальный интервал для оси Y
                double yRange = maxY - minY;
                if (yRange <= 10)
                    chartArea.AxisY.Interval = 1;
                else if (yRange <= 20)
                    chartArea.AxisY.Interval = 2;
                else if (yRange <= 50)
                    chartArea.AxisY.Interval = 5;
                else
                    chartArea.AxisY.Interval = 10;

                // Настройка форматирования подписей (без десятичных знаков)
                chartArea.AxisX.LabelStyle.Format = "0";
                chartArea.AxisY.LabelStyle.Format = "0";

                // Настройка основных делений сетки
                chartArea.AxisX.MajorGrid.Interval = chartArea.AxisX.Interval;
                chartArea.AxisY.MajorGrid.Interval = chartArea.AxisY.Interval;

                // Настройка вспомогательных делений сетки
                chartArea.AxisX.MinorGrid.Enabled = false;
                chartArea.AxisY.MinorGrid.Enabled = false;

                // Принудительное отображение целых значений
                chartArea.AxisX.IntervalOffset = 0;
                chartArea.AxisY.IntervalOffset = 0;

                // Настройка максимального количества делений
                chartArea.AxisX.MaximumAutoSize = 100;
                chartArea.AxisY.MaximumAutoSize = 100;

                // Включение автоматической подстройки интервалов
                chartArea.AxisX.IsLabelAutoFit = true;
                chartArea.AxisY.IsLabelAutoFit = true;

                // Настройка угла поворота подписей для X оси (если нужно)
                if (xRange > 15)
                    chartArea.AxisX.LabelStyle.Angle = -45;
                else
                    chartArea.AxisX.LabelStyle.Angle = 0;
            }
            else
            {
                // Настройка осей по умолчанию
                chartArea.AxisX.Interval = 1;
                chartArea.AxisY.Interval = 1;
                chartArea.AxisX.LabelStyle.Format = "0";
                chartArea.AxisY.LabelStyle.Format = "0";
                chartArea.AxisX.IntervalOffset = 0;
                chartArea.AxisY.IntervalOffset = 0;
            }

            // Дополнительные настройки для улучшения отображения
            chartArea.AxisX.MajorTickMark.Interval = chartArea.AxisX.Interval;
            chartArea.AxisY.MajorTickMark.Interval = chartArea.AxisY.Interval;
            chartArea.AxisX.LabelAutoFitMinFontSize = 8;
            chartArea.AxisY.LabelAutoFitMinFontSize = 8;

            // Перерисовываем график
            chart.Invalidate();
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