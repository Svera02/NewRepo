using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace итер4
{

    public partial class WaitingForm : Form
    {
        private readonly string _playerName;
        private readonly IGameService _service;
        private bool _openedShipForm = false;

        public WaitingForm(string playerName, IGameService service)
        {
            InitializeComponent();
            _playerName = playerName;
            _service = service ?? throw new ArgumentNullException(nameof(service));

            statusLabel.Text = "Ожидание других игроков...";
            progressBar1.Style = ProgressBarStyle.Marquee;

            this.Load += WaitingForm_Load;
            this.FormClosed += WaitingForm_FormClosed;

            _service.StateUpdated += Service_StateUpdated;
        }

        private void WaitingForm_Load(object sender, EventArgs e)
        {
            _service.StartPolling(2000); 
        }

        private void WaitingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _service.StateUpdated -= Service_StateUpdated;
            _service.StopPolling();
        }

        private void Service_StateUpdated(object sender, GameStateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ProcessState(e.State)));
            }
            else
            {
                ProcessState(e.State);
            }
        }

        private void ProcessState(GameState state)
        {
            if (state == null) return;

            statusLabel.Text = $"Ожидание игроков... ({state.connected}/2)";

            if (state.game_ready && !_openedShipForm)
            {
                _openedShipForm = true;
                _service.StopPolling();
                progressBar1.Style = ProgressBarStyle.Blocks;
                statusLabel.Text = "Игрок найден! Игра начинается...";

                var shipForm = new ShipPlacementForm(_playerName, _service);
                shipForm.FormClosed += (s, a) => this.Close();
                shipForm.Show();
                this.Hide();
            }
        }

        private void cancelbutton_Click_1(object sender, EventArgs e)
        {
            _service.StopPolling();
            this.Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }



}






