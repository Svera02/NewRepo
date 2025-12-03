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
        private string playerName;
        private Timer checkTimer;
        private bool openedShipForm = false;
        private HttpClient client;

        public WaitingForm(string name)
        {
            InitializeComponent();
            playerName = name;
            client = new HttpClient();

            statusLabel.Text = "Ожидание других игроков...";
            progressBar1.Style = ProgressBarStyle.Marquee;

            checkTimer = new Timer();
            checkTimer.Interval = 2000;
            checkTimer.Tick += CheckTimer_Tick;

            this.Load += (s, e) => checkTimer.Start();
        }

        private async void CheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var response = await client.GetAsync("http://127.0.0.1:5000/state");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var state = Newtonsoft.Json.JsonConvert.DeserializeObject<GameState>(json);

                    if (state != null && state.game_ready && !openedShipForm)
                    {
                        openedShipForm = true;
                        checkTimer.Stop();
                        progressBar1.Style = ProgressBarStyle.Blocks;
                        statusLabel.Text = "Игрок найден! Игра начинается...";

                        ShipPlacementForm shipForm = new ShipPlacementForm(playerName);
                        shipForm.FormClosed += (s, args) => this.Close();
                        shipForm.Show();

                        this.Hide();
                    }
                    else if (state != null)
                    {
                        statusLabel.Text = $"Ожидание игроков... ({state.connected}/2)";
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка связи с сервером...";
                Console.WriteLine(ex.Message);
            }
        }

        private void cancelbutton_Click_1(object sender, EventArgs e)
        {
            checkTimer.Stop();
            client?.Dispose();
            this.Close();
        }

        private class GameState
        {
            public List<string> players { get; set; }
            public int connected { get; set; }
            public bool game_ready { get; set; }
        }

       
    }


}






