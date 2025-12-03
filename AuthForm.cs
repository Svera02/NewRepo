using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace итер4
{
        public partial class AuthForm : Form
        {
            public AuthForm()
            {
                InitializeComponent();
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

           
                using (HttpClient client = new HttpClient())
                {
                    var data = new FormUrlEncodedContent(new[]
                    {
                      new KeyValuePair<string, string>("name", playerName)
                    });

                    var response = await client.PostAsync("http://127.0.0.1:5000/connect", data);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (result.Contains("уже подключён") || result.Contains("already connected"))
                        {
                            MessageBox.Show($"Игрок с именем '{playerName}' уже подключён!\nВыберите другое имя.",
                                          "Имя занято", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            MessageBox.Show("Игра уже заполнена (2 игрока подключены).",
                                          "Игра заполнена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else
                        {
                            MessageBox.Show(result, "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            WaitingForm waiting = new WaitingForm(playerName);
                            waiting.Show();
                            this.Hide();
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        MessageBox.Show("Ошибка: имя игрока не передано",
                                      "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        MessageBox.Show("Игра уже заполнена (2 игрока подключены).",
                                      "Игра заполнена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка сервера: {response.StatusCode}\n{result}",
                                      "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения:\n" + ex.Message,
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }

}


