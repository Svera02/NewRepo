using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace итер4
{



    public partial class GameForm : Form
    {
        private IGameService _service;
        private string _playerName;
        private List<List<Point>> _myShips;
        private System.Windows.Forms.Timer _updateTimer;
        private Label _statusLabel; // Добавляем поле для статуса

        private Button[,] _myGrid = new Button[10, 10];
        private Button[,] _enemyGrid = new Button[10, 10];

        public GameForm(string playerName, IGameService service, List<List<Point>> myShips)
        {
            try
            {
                if (string.IsNullOrEmpty(playerName))
                    throw new ArgumentException("Имя игрока не может быть пустым", nameof(playerName));

                if (service == null)
                    throw new ArgumentNullException(nameof(service), "Сервис не может быть null");

                if (myShips == null)
                    throw new ArgumentNullException(nameof(myShips), "Корабли не могут быть null");

                _playerName = playerName;
                _service = service;
                _myShips = myShips;

                // Настройка формы
                this.Text = $"Морской бой - Игрок: {playerName}";
                this.Size = new Size(800, 600);
                this.StartPosition = FormStartPosition.CenterScreen;

                InitializeComponent();
                CreateGameGrids();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartGame();
        }

        private void StartGame()
        {
            try
            {
                if (_service == null || _myShips == null)
                {
                    MessageBox.Show("Ошибка инициализации игры", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                PaintMyShips();
                _service.StateUpdated += Service_StateUpdated;

                _updateTimer = new System.Windows.Forms.Timer();
                _updateTimer.Interval = 2000;
                _updateTimer.Tick += async (s, e) => await UpdateGameState();
                _updateTimer.Start();

                _ = UpdateGameState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска игры: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void PaintMyShips()
        {
            if (_myShips == null || _myGrid == null) return;

            int shipNumber = 1;
            foreach (var ship in _myShips)
            {
                foreach (var p in ship)
                {
                    if (p.X >= 0 && p.X < 10 && p.Y >= 0 && p.Y < 10 && _myGrid[p.X, p.Y] != null)
                    {
                        _myGrid[p.X, p.Y].BackColor = Color.Blue;
                        _myGrid[p.X, p.Y].Text = shipNumber.ToString();
                        _myGrid[p.X, p.Y].ForeColor = Color.White;
                    }
                }
                shipNumber++;
            }
        }

        private void Service_StateUpdated(object sender, GameStateEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateUI(e.State)));
            }
            else
            {
                UpdateUI(e.State);
            }
        }

        private async Task UpdateGameState()
        {
            try
            {
                if (_service == null) return;

                var state = await _service.GetStateAsync();
                if (state != null)
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() => UpdateUI(state)));
                    }
                    else
                    {
                        UpdateUI(state);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateUI(GameState state)
        {
            if (state == null) return;

            if (state.player_status != null && state.player_status.ContainsKey(_playerName))
            {
                var playerStatus = state.player_status[_playerName];

                if (playerStatus == "win")
                {
                    MessageBox.Show("ПОБЕДА! Вы уничтожили все корабли противника!", "Игра окончена",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }
                else if (playerStatus == "lose")
                {
                    MessageBox.Show("ПОРАЖЕНИЕ! Все ваши корабли уничтожены.", "Игра окончена",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }
            }

            if (state.current_turn == _playerName)
            {
                SetEnemyGridEnabled(true);
                UpdateStatusLabel("ВАШ ХОД! Стреляйте по полю противника", Color.Green);
            }
            else if (!string.IsNullOrEmpty(state.current_turn))
            {
                SetEnemyGridEnabled(false);
                UpdateStatusLabel($"Ход противника. Ожидание...", Color.Red);
            }
            else
            {
                SetEnemyGridEnabled(false);
                UpdateStatusLabel("Ожидание начала игры...", Color.Black);
            }

            UpdateShots(state);
        }

        private void UpdateStatusLabel(string text, Color color)
        {
            if (_statusLabel == null)
            {
                _statusLabel = new Label
                {
                    Name = "lblStatus",
                    Text = text,
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    ForeColor = color,
                    Location = new Point(20, 50),
                    Size = new Size(400, 30)
                };
                this.Controls.Add(_statusLabel);
            }
            else
            {
                _statusLabel.Text = text;
                _statusLabel.ForeColor = color;
            }
        }

        private void UpdateShots(GameState state)
        {
            if (state.shots != null && state.shots.ContainsKey(_playerName))
            {
                foreach (var coord in state.shots[_playerName])
                {
                    int x = coord[0];
                    int y = coord[1];

                    bool hit = _myShips.Any(ship => ship.Any(p => p.X == x && p.Y == y));

                    if (hit)
                    {
                        UpdateMyCell(x, y, Color.Red, "X");
                    }
                    else
                    {
                        UpdateMyCell(x, y, Color.Gray, "•");
                    }
                }
            }
        }

        private void CreateGameGrids()
        {

            AddFieldLabels();
            CreateMyGrid();
            CreateEnemyGrid();
            AddGridCoordinates();
        }

        private void AddFieldLabels()
        {
            var myFieldLabel = new Label
            {
                Text = "Мое поле",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 100),
                Size = new Size(300, 30)
            };
            this.Controls.Add(myFieldLabel);

            var enemyFieldLabel = new Label
            {
                Text = "Поле противника",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(470, 100),
                Size = new Size(300, 30)
            };
            this.Controls.Add(enemyFieldLabel);
        }

        private void CreateMyGrid()
        {
            int cellSize = 30;
            int offsetX = 20;
            int offsetY = 140;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = offsetX + x * cellSize,
                        Top = offsetY + y * cellSize,
                        Tag = new Point(x, y),
                        BackColor = Color.LightBlue,
                        FlatStyle = FlatStyle.Flat,
                        Enabled = false
                    };

                    btn.FlatAppearance.BorderColor = Color.DarkGray;
                    btn.FlatAppearance.BorderSize = 1;

                    this.Controls.Add(btn);
                    _myGrid[x, y] = btn;
                }
            }
        }

        private void CreateEnemyGrid()
        {
            int cellSize = 30;
            int offsetX = 470;
            int offsetY = 140;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = offsetX + x * cellSize,
                        Top = offsetY + y * cellSize,
                        Tag = new Point(x, y),
                        BackColor = Color.LightGray,
                        FlatStyle = FlatStyle.Flat,
                        Enabled = false
                    };

                    btn.FlatAppearance.BorderColor = Color.DarkGray;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Click += EnemyCell_Click;

                    this.Controls.Add(btn);
                    _enemyGrid[x, y] = btn;
                }
            }
        }

        private void AddGridCoordinates()
        {
            AddCoordinatesForMyGrid();
            AddCoordinatesForEnemyGrid();
        }

        private void AddCoordinatesForMyGrid()
        {
            int cellSize = 30;
            int offsetX = 20;
            int offsetY = 140;
            Color textColor = Color.DarkBlue;

            for (int i = 0; i < 10; i++)
            {
                var lbl = new Label
                {
                    Text = ((char)('А' + i)).ToString(),
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    ForeColor = textColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = cellSize,
                    Height = 20,
                    Left = offsetX + i * cellSize,
                    Top = offsetY - 25
                };
                this.Controls.Add(lbl);
            }

            for (int i = 0; i < 10; i++)
            {
                var lbl = new Label
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    ForeColor = textColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = 20,
                    Height = cellSize,
                    Left = offsetX - 25,
                    Top = offsetY + i * cellSize
                };
                this.Controls.Add(lbl);
            }
        }

        private void AddCoordinatesForEnemyGrid()
        {
            int cellSize = 30;
            int offsetX = 470;
            int offsetY = 140;
            Color textColor = Color.DarkRed;

            for (int i = 0; i < 10; i++)
            {
                var lbl = new Label
                {
                    Text = ((char)('А' + i)).ToString(),
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    ForeColor = textColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = cellSize,
                    Height = 20,
                    Left = offsetX + i * cellSize,
                    Top = offsetY - 25
                };
                this.Controls.Add(lbl);
            }

            for (int i = 0; i < 10; i++)
            {
                var lbl = new Label
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Arial", 9, FontStyle.Bold),
                    ForeColor = textColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = 20,
                    Height = cellSize,
                    Left = offsetX - 25,
                    Top = offsetY + i * cellSize
                };
                this.Controls.Add(lbl);
            }
        }

        private async void EnemyCell_Click(object sender, EventArgs e)
        {
            if (_service == null) return;

            var btn = sender as Button;
            if (btn != null)
            {
                var point = (Point)btn.Tag;
                btn.Enabled = false;

                try
                {
                    var result = await _service.ShootAsync(_playerName, point.X, point.Y);

                    if (result.success)
                    {
                        if (result.result == "hit" || result.result == "win")
                        {
                            UpdateEnemyCell(point.X, point.Y, Color.Orange, "X");
                        }
                        else if (result.result == "miss")
                        {
                            UpdateEnemyCell(point.X, point.Y, Color.DarkGray, "•");
                        }

                    }
                    else
                    {
                        MessageBox.Show(result.message, "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btn.Enabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn.Enabled = true;
                }
            }
        }

        public void UpdateMyCell(int x, int y, Color color, string text = "")
        {
            if (x >= 0 && x < 10 && y >= 0 && y < 10 && _myGrid[x, y] != null)
            {
                _myGrid[x, y].BackColor = color;
                _myGrid[x, y].Text = text;
                _myGrid[x, y].ForeColor = Color.White;
            }
        }

        public void UpdateEnemyCell(int x, int y, Color color, string text = "")
        {
            if (x >= 0 && x < 10 && y >= 0 && y < 10 && _enemyGrid[x, y] != null)
            {
                _enemyGrid[x, y].BackColor = color;
                _enemyGrid[x, y].Text = text;
                _enemyGrid[x, y].ForeColor = Color.White;
                _enemyGrid[x, y].Enabled = false;
            }
        }

        public void SetEnemyGridEnabled(bool enabled)
        {
            if (_enemyGrid == null) return;

            foreach (var btn in _enemyGrid)
            {
                if (btn != null && btn.BackColor == Color.LightGray)
                {
                    btn.Enabled = enabled;
                }
            }
        }

        public void SetEnemyCellEnabled(int x, int y, bool enabled)
        {
            if (x >= 0 && x < 10 && y >= 0 && y < 10 && _enemyGrid[x, y] != null)
            {
                _enemyGrid[x, y].Enabled = enabled;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            if (_service != null)
            {
                _service.StateUpdated -= Service_StateUpdated;
            }

            base.OnFormClosed(e);
        }
    }
}










