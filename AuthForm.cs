using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace итер4
{
    public partial class AuthForm : Form
    {
        private readonly IGameService _service;

        public AuthForm(IGameService service)
        {
            InitializeComponent();
            _service = service ?? throw new ArgumentNullException(nameof(service));

        }

        private async void connectBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string playerName = playerBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    MessageBox.Show("Введите имя игрока!");
                    return;
                }

                string result = await _service.ConnectAsync(playerName);

                if (result.Contains("уже подключён") || result.Contains("already connected"))
                {
                    MessageBox.Show($"Игрок с именем '{playerName}' уже подключён!\nВыберите другое имя.",
                                  "Имя занято", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(result, "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);

                WaitingForm waiting = new WaitingForm(playerName, _service);
                waiting.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения:\n" + ex.Message,
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }


}


