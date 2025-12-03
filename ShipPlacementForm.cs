using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace итер4
{

    public partial class ShipPlacementForm : Form
    {
        private string playerName;
        private Button[,] grid = new Button[10, 10];
        private List<List<Point>> placedShips = new List<List<Point>>();
        private int[] ships = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 }; 
        private int shipIndex = 0;
        private bool horizontal = true;
        private bool isDragging = false;
        private Point dragStart;
        private List<Point> currentShipPreview = new List<Point>();

        public ShipPlacementForm(string name)
        {
            InitializeComponent();
            playerName = name;

            btnClear.Click += (s, e) => ClearAll();
            btnRotate.Click += (s, e) =>
            {
                horizontal = !horizontal;

            };
            btnAuto.Click += (s, e) => AutoPlaceShips();
            btnReady.Click += async (s, e) => await SendShipsToServer();
            btnrules.Click += (s, e) => ShowRules();

            InitGrid();
            UpdateInfo();

        }

        private void InitGrid()
        {
            panelField.Controls.Clear();

            int cellSize = 35;

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    Button btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = col * cellSize,
                        Top = row * cellSize,
                        Tag = new Point(col, row),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.LightGray,
                        Margin = new Padding(1)
                    };

                    btn.MouseDown += Cell_MouseDown;
                    btn.MouseEnter += Cell_MouseEnter;
                    btn.MouseLeave += Cell_MouseLeave;
                    btn.Click += Cell_Click;

                    btn.FlatAppearance.BorderColor = Color.DarkGray;
                    btn.FlatAppearance.BorderSize = 1;

                    grid[col, row] = btn;
                    panelField.Controls.Add(btn);
                }
            }

            AddGridLabels(cellSize);
        }

        private void AddGridLabels(int cellSize)
        {
            for (int col = 0; col < 10; col++)
            {
                Label lbl = new Label
                {
                    Text = ((char)('A' + col)).ToString(),
                    Width = cellSize,
                    Height = 20,
                    Left = col * cellSize,
                    Top = -25,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                panelField.Controls.Add(lbl);
            }

            for (int row = 0; row < 10; row++)
            {
                Label lbl = new Label
                {
                    Text = (row + 1).ToString(),
                    Width = 25,
                    Height = cellSize,
                    Left = -30,
                    Top = row * cellSize,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                panelField.Controls.Add(lbl);
            }
        }

        private void ShowRules()
        {
            string rules = "ПРАВИЛА РАССТАНОВКИ КОРАБЛЕЙ:\n\n" +
                          "1. Размеры кораблей:\n" +
                          "   • 1 корабль — 4 клетки\n" +
                          "   • 2 корабля — 3 клетки\n" +
                          "   • 3 корабля — 2 клетки\n" +
                          "   • 4 корабля — 1 клетка\n\n" +
                          "2. Корабли НЕ МОГУТ:\n" +
                          "   • Касаться друг друга (даже углом)\n" +
                          "   • Выходить за границы поля\n" +
                          "   • Пересекаться\n\n" +
                          "3. Расстояние между кораблями:\n" +
                          "   • Минимум 1 клетка свободного пространства\n\n" +
                          "4. Ориентация:\n" +
                          "   • Горизонтальная или вертикальная\n" +
                          "   • Нажмите 'Повернуть' для смены ориентации";

            MessageBox.Show(rules, "Правила расстановки", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateInfo()
        {
            string shipTypes = GetRemainingShipsInfo();

            if (shipIndex < ships.Length)
            {
                lblInfo.Text = $"Текущий корабль: {ships[shipIndex]}-палубный\n" +
                              $"Осталось разместить:\n{shipTypes}";
            }
            else
            {
                lblInfo.Text = "Все корабли размещены!\nНажмите 'Готов'";
            }

        }

        private string GetRemainingShipsInfo()
        {
            var remaining = ships.Skip(shipIndex).GroupBy(s => s)
                .Select(g => $"{g.Count()} x {g.Key}-палубный")
                .ToArray();

            return string.Join("\n", remaining);
        }

        private void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            if (shipIndex >= ships.Length) return;

            if (e.Button == MouseButtons.Right)
            {
                horizontal = !horizontal;
                return;
            }
        }

        private void Cell_MouseEnter(object sender, EventArgs e)
        {
            if (shipIndex >= ships.Length) return;

            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            if (grid[p.X, p.Y].BackColor == Color.Blue)
                return;

            ClearPreview();

            int size = ships[shipIndex];
            currentShipPreview.Clear();
            bool isValid = true;

            for (int i = 0; i < size; i++)
            {
                int x = horizontal ? p.X + i : p.X;
                int y = horizontal ? p.Y : p.Y + i;

                if (x >= 10 || y >= 10)
                {
                    isValid = false;
                    break;
                }

                currentShipPreview.Add(new Point(x, y));
            }

            if (isValid)
            {
                isValid = CanPlaceShipAt(p.X, p.Y, size);
            }

            foreach (var point in currentShipPreview)
            {
                if (point.X < 10 && point.Y < 10)
                {

                    if (grid[point.X, point.Y].BackColor != Color.Blue)
                    {
                        grid[point.X, point.Y].BackColor = isValid ? Color.LightBlue : Color.LightPink;
                        grid[point.X, point.Y].FlatAppearance.BorderColor = isValid ? Color.Blue : Color.Red;
                    }
                }
            }
        }

        private void Cell_MouseLeave(object sender, EventArgs e)
        {
            ClearPreview();
        }

        private void ClearPreview()
        {
            foreach (var point in currentShipPreview)
            {
                if (point.X < 10 && point.Y < 10)
                {
                    Button btn = grid[point.X, point.Y];

                    Color currentColor = btn.BackColor;

                    if (currentColor == Color.LightBlue || currentColor == Color.LightPink)
                    {
                        btn.BackColor = Color.LightGray;
                        btn.FlatAppearance.BorderColor = Color.DarkGray;
                    }
                }
            }
            currentShipPreview.Clear();
        }

        private bool CanPlaceShipAt(int startX, int startY, int size)
{
    for (int i = 0; i < size; i++)
    {
        int x = horizontal ? startX + i : startX;
        int y = horizontal ? startY : startY + i;

        if (x >= 10 || y >= 10)
            return false;

        if (grid[x, y].BackColor == Color.Blue)
            return false;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                {

                    bool isShipCell = false;
                    for (int j = 0; j < size; j++)
                    {
                        int sx = horizontal ? startX + j : startX;
                        int sy = horizontal ? startY : startY + j;
                        if (nx == sx && ny == sy)
                        {
                            isShipCell = true;
                            break;
                        }
                    }
                    

                    if (!isShipCell && grid[nx, ny].BackColor == Color.Blue)
                        return false;
                }
            }
        }
    }
    return true;
}

        private void Cell_Click(object sender, EventArgs e)
        {
            if (shipIndex >= ships.Length) return;

            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            int size = ships[shipIndex];
            List<Point> newShip = new List<Point>();

            if (!CanPlaceShipAt(p.X, p.Y, size))
            {
                MessageBox.Show("Невозможно разместить корабль здесь!\n" +
                               "Корабли не могут касаться друг друга.",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int i = 0; i < size; i++)
            {
                int x = horizontal ? p.X + i : p.X;
                int y = horizontal ? p.Y : p.Y + i;

                newShip.Add(new Point(x, y));
            }

            foreach (var point in newShip)
            {
                grid[point.X, point.Y].BackColor = Color.Blue;
                grid[point.X, point.Y].FlatAppearance.BorderColor = Color.DarkBlue;
                grid[point.X, point.Y].Text = (shipIndex + 1).ToString();
                grid[point.X, point.Y].ForeColor = Color.White;
            }

            placedShips.Add(newShip);
            shipIndex++;
            UpdateInfo();

            if (shipIndex == ships.Length)
            {
                btnReady.Enabled = true;
                btnReady.BackColor = Color.LightGreen;
                MessageBox.Show("Все корабли размещены!\nНажмите 'Готов' для продолжения.",
                              "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearAll()
        {
            foreach (var btn in grid)
            {
                if (btn != null)
                {
                    btn.BackColor = Color.LightGray;
                    btn.FlatAppearance.BorderColor = Color.DarkGray;
                    btn.Text = "";
                }
            }

            placedShips.Clear();
            shipIndex = 0;
            btnReady.Enabled = false;
            btnReady.BackColor = SystemColors.Control;
            currentShipPreview.Clear();
            UpdateInfo();
        }

        private void AutoPlaceShips()
        {
            ClearAll();
            Random r = new Random();

            var shipsToPlace = ships.ToList();

            shipsToPlace.Sort((a, b) => b.CompareTo(a));

            foreach (int size in shipsToPlace)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 1000)
                {
                    attempts++;

                    int x = r.Next(10);
                    int y = r.Next(10);
                    bool hor = r.Next(2) == 0;

                    bool originalHorizontal = horizontal;
                    horizontal = hor;
                    if (CanPlaceShipAt(x, y, size))
                    {
                        List<Point> ship = new List<Point>();

                        for (int i = 0; i < size; i++)
                        {
                            int xx = hor ? x + i : x;
                            int yy = hor ? y : y + i;

                            ship.Add(new Point(xx, yy));
                        }

                        foreach (var point in ship)
                        {
                            grid[point.X, point.Y].BackColor = Color.Blue;
                            grid[point.X, point.Y].FlatAppearance.BorderColor = Color.DarkBlue;
                            grid[point.X, point.Y].Text = (shipIndex + 1).ToString();
                            grid[point.X, point.Y].ForeColor = Color.White;
                        }

                        placedShips.Add(ship);
                        shipIndex++;
                        placed = true;
                    }

                    horizontal = originalHorizontal;
                }

                if (!placed)
                {
                    MessageBox.Show("Не удалось автоматически разместить корабли.\n" +
                                   "Попробуйте ещё раз или расставьте вручную.",
                                   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearAll();
                    return;
                }
            }

            btnReady.Enabled = true;
            btnReady.BackColor = Color.LightGreen;
            UpdateInfo();

            MessageBox.Show("Корабли успешно расставлены автоматически!",
                           "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task SendShipsToServer()
        {
            try
            {

                if (placedShips.Count != ships.Length)
                {
                    MessageBox.Show("Разместите все корабли перед отправкой!",
                                  "Не готово", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var shipsForServer = new List<List<int[]>>();

                foreach (var ship in placedShips)
                {
                    var shipCoords = ship.Select(p => new int[] { p.X, p.Y }).ToList();
                    shipsForServer.Add(shipCoords);
                }

                using (HttpClient client = new HttpClient())
                {
                    var json = new
                    {
                        name = playerName,
                        ships = shipsForServer
                    };

                    string js = Newtonsoft.Json.JsonConvert.SerializeObject(json);
                    var content = new StringContent(js, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("http://127.0.0.1:5000/ships", content);
                    string result = await response.Content.ReadAsStringAsync();

                    var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);

                    if (responseObj != null && responseObj.success == true)
                    {
                        MessageBox.Show("✅ Корабли успешно отправлены на сервер!\n" +
                                      "Ожидайте начала игры.",
                                      "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при отправке кораблей:\n" +
                                      (responseObj?.message ?? result),
                                      "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения:\n{ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}