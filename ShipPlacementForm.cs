using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace итер4
{
    public partial class ShipPlacementForm : Form
    {
        private readonly IGameService _service;
        private readonly ShipPlacementManager _manager;
        private readonly string _playerName;

        private readonly Button[,] _grid = new Button[10, 10];

        public ShipPlacementForm(string playerName, IGameService service)
        {
            InitializeComponent();

            _playerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            _manager = new ShipPlacementManager();
            _manager.StateChanged += UpdateInfo;

            InitGrid();
            UpdateInfo();

            _service.StateUpdated += Service_StateUpdated;
            _service.StartPolling();

            btnClear.Click += (s, e) =>
            {
                _manager.Clear();
                RepaintGrid();
            };

            btnRotate.Click += (s, e) =>
            {
                _manager.Horizontal = !_manager.Horizontal;
                UpdateInfo();
            };

            btnAuto.Click += (s, e) =>
            {
                try
                {
                    _manager.AutoPlace();
                    RepaintGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            btnReady.Click += async (s, e) => await SendShipsToServer();
        }

        private void InitGrid()
        {
            panelField.Controls.Clear();
            int cellSize = 35;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = x * cellSize,
                        Top = y * cellSize,
                        Tag = new Point(x, y),
                        BackColor = Color.LightGray,
                        FlatStyle = FlatStyle.Flat
                    };

                    btn.FlatAppearance.BorderColor = Color.DarkGray;
                    btn.Click += Cell_Click;
                    btn.MouseEnter += Cell_MouseEnter;
                    btn.MouseLeave += (s, e) => RepaintGrid();

                    panelField.Controls.Add(btn);
                    _grid[x, y] = btn;
                }
            }
        }


        private void UpdateInfo()
        {
            if (_manager.AllShipsPlaced)
            {
                lblInfo.Text = "Все корабли размещены";
                btnReady.Enabled = true;
            }
            else
            {
                int size = _manager.Ships[_manager.ShipIndex];
                lblInfo.Text =
                    $"Корабль: {size}-палубный\n" +
                    $"Ориентация: {(_manager.Horizontal ? "Горизонтально" : "Вертикально")}";
            }
        }

        private void RepaintGrid()
        {
            foreach (var btn in _grid)
            {
                btn.BackColor = Color.LightGray;
                btn.Text = "";
            }

            int index = 1;
            foreach (var ship in _manager.PlacedShips)
            {
                foreach (var p in ship)
                {
                    _grid[p.X, p.Y].BackColor = Color.Blue;
                    _grid[p.X, p.Y].Text = index.ToString();
                    _grid[p.X, p.Y].ForeColor = Color.White;
                }
                index++;
            }
        }


        private void Cell_Click(object sender, EventArgs e)
        {
            if (_manager.AllShipsPlaced) return;

            var btn = (Button)sender;
            var p = (Point)btn.Tag;

            if (_manager.PlaceShip(p))
                RepaintGrid();
            else
                MessageBox.Show("Нельзя разместить корабль здесь");
        }

        private void Cell_MouseEnter(object sender, EventArgs e)
        {
            if (_manager.AllShipsPlaced) return;

            var btn = (Button)sender;
            var start = (Point)btn.Tag;

            var preview = _manager.GetPreview(start);
            bool valid = _manager.CanPlaceShipAt(start.X, start.Y);

            foreach (var p in preview)
            {
                if (p.X < 0 || p.Y < 0 || p.X >= 10 || p.Y >= 10)
                    continue;

                _grid[p.X, p.Y].BackColor =
                    valid ? Color.LightBlue : Color.LightPink;
            }
        }

        private async Task SendShipsToServer()
        {
            if (!_manager.AllShipsPlaced)
            {
                MessageBox.Show("Разместите все корабли");
                return;
            }

            var shipsForServer = _manager.PlacedShips
                .Select(ship => ship.Select(p => new[] { p.X, p.Y }).ToList())
                .ToList();

            try
            {
                await _service.SendShipsAsync(_playerName, shipsForServer);

                btnReady.Enabled = false;
                btnAuto.Enabled = false;
                btnClear.Enabled = false;
                panelField.Enabled = false;

                MessageBox.Show("Корабли отправлены. Ожидание второго игрока...");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки:\n" + ex.Message);
            }
        }

        private void Service_StateUpdated(object sender, GameStateEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Service_StateUpdated(sender, e)));
                return;
            }

            if (e?.State == null) return;

            if (e.State.game_started || !string.IsNullOrEmpty(e.State.current_turn))
            {
                Console.WriteLine($"[ShipPlacementForm] Игра началась, открываю игровую форму...");
                OpenBattleForm();
            }
            else if (e.State.player_status != null &&
                     e.State.player_status.TryGetValue(_playerName, out var status))
            {
                if (status == "ready")
                {
                    lblInfo.Text = "Корабли отправлены. Ожидание второго игрока...";
                    lblInfo.ForeColor = Color.Blue;
                }
            }
        }

        private bool _battleFormOpened = false;

        private void OpenBattleForm()
        {
            if (_battleFormOpened) return;
            _battleFormOpened = true;

            try
            {
                if (_service != null)
                {
                    _service.StateUpdated -= Service_StateUpdated;
                    _service.StopPolling();
                }

                var myShips = _manager.PlacedShips;

                if (myShips == null || myShips.Count == 0)
                {
                    MessageBox.Show("Корабли не размещены", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _battleFormOpened = false;
                    return;
                }

                var gameForm = new GameForm(_playerName, _service, myShips);

                gameForm.FormClosed += (s, e) => this.Close();
                gameForm.Show();

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии игровой формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _service.StateUpdated -= Service_StateUpdated;
            base.OnFormClosed(e);
        }




    }

}