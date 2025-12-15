using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace итер4
{

    public class HttpGameService : IGameService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private System.Timers.Timer _pollTimer;

        public event EventHandler<GameStateEventArgs> StateUpdated;

        public HttpGameService(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL не может быть пустым", nameof(baseUrl));

            _baseUrl = baseUrl.TrimEnd('/');
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<string> ConnectAsync(string playerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    throw new ArgumentException("Имя игрока не может быть пустым", nameof(playerName));

                var data = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("name", playerName)
            });

                var response = await _client.PostAsync("/connect", data);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[CONNECT] Ответ: {responseText}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage;

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        errorMessage = "Имя игрока не передано";
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        errorMessage = "Игра заполнена (2 игрока подключены).";
                    else
                        errorMessage = $"Ошибка сервера: {response.StatusCode}";

                    throw new InvalidOperationException(errorMessage);
                }

                return responseText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONNECT] Ошибка: {ex.Message}");
                throw;
            }
        }

        public async Task SendShipsAsync(string playerName, List<List<int[]>> ships)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(playerName))
                    throw new ArgumentException("Имя игрока не может быть пустым", nameof(playerName));

                if (ships == null)
                    throw new ArgumentNullException(nameof(ships), "Корабли не могут быть null");

                if (ships.Count == 0)
                    throw new ArgumentException("Список кораблей не может быть пустым", nameof(ships));

                Console.WriteLine($"[SHIPS] Отправка кораблей для: {playerName}");
                Console.WriteLine($"[SHIPS] Количество кораблей: {ships.Count}");

                var requestData = new
                {
                    name = playerName,
                    ships = ships
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"[SHIPS] JSON данные: {jsonData}");

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                string responseText = "";

                try
                {
                    response = await _client.PostAsync("/ships", content);
                    responseText = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"[SHIPS] Статус ответа: {response.StatusCode}");
                    Console.WriteLine($"[SHIPS] Текст ответа: {responseText}");
                }
                catch (HttpRequestException httpEx)
                {
                    throw new InvalidOperationException($"Ошибка сети: {httpEx.Message}", httpEx);
                }
                catch (TaskCanceledException)
                {
                    throw new InvalidOperationException("Таймаут запроса. Сервер не отвечает.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Ошибка сервера при отправке кораблей: {response.StatusCode}\n{responseText}");
                }

                try
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseText);

                    if (result == null)
                        throw new InvalidOperationException("Пустой ответ от сервера");

                    if (result.ContainsKey("success"))
                    {
                        var successObj = result["success"];
                        if (successObj is bool && (bool)successObj)
                        {

                            return;
                        }
                    }

                    if (result.ContainsKey("message"))
                    {
                        var message = result["message"];
                        throw new InvalidOperationException(message?.ToString() ?? "Неизвестная ошибка");
                    }

                    throw new InvalidOperationException("Неизвестный формат ответа от сервера");
                }
                catch (JsonException jsonEx)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[SHIPS] JSON парсинг ошибка, но статус OK: {jsonEx.Message}");
                        return;
                    }
                    throw new InvalidOperationException($"Ошибка формата ответа: {jsonEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SHIPS] Критическая ошибка: {ex.Message}");
                Console.WriteLine($"[SHIPS] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<GameState> GetStateAsync()
        {
            try
            {
                var response = await _client.GetAsync("/state");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[STATE] Ответ: {json}");

                var state = JsonConvert.DeserializeObject<GameState>(json);

                if (state == null)
                    throw new InvalidOperationException("Не удалось десериализовать состояние игры");

                if (state.players == null)
                    state.players = new List<string>();

                if (state.player_status == null)
                    state.player_status = new Dictionary<string, string>();

                if (state.shots == null)
                    state.shots = new Dictionary<string, List<int[]>>();

                return state;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STATE] Ошибка: {ex.Message}");
                throw;
            }
        }

        public async Task<ShotResult> ShootAsync(string playerName, int x, int y)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    throw new ArgumentException("Имя игрока не может быть пустым", nameof(playerName));

                if (x < 0 || x >= 10 || y < 0 || y >= 10)
                    throw new ArgumentException("Координаты должны быть в диапазоне 0-9");

                Console.WriteLine($"[SHOOT] Выстрел от {playerName} в ({x}, {y})");

                var requestData = new
                {
                    name = playerName,
                    x = x,
                    y = y
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync("/shoot", content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[SHOOT] Ответ: {responseText}");

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Ошибка сервера: {response.StatusCode}\n{responseText}");

                var result = JsonConvert.DeserializeObject<ShotResult>(responseText);

                if (result == null)
                    throw new InvalidOperationException("Не удалось десериализовать результат выстрела");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SHOOT] Ошибка: {ex.Message}");
                throw;
            }
        }

        public void StartPolling(int intervalMs = 2000)
        {
            if (_pollTimer != null)
                return;

            _pollTimer = new System.Timers.Timer(intervalMs);
            _pollTimer.Elapsed += async (s, e) => await PollOnceAsync();
            _pollTimer.AutoReset = true;
            _pollTimer.Start();

            Console.WriteLine($"[POLLING] Запущен с интервалом {intervalMs}мс");
        }

        public void StopPolling()
        {
            if (_pollTimer == null)
                return;

            _pollTimer.Stop();
            _pollTimer.Dispose();
            _pollTimer = null;

            Console.WriteLine("[POLLING] Остановлен");
        }

        private async Task PollOnceAsync()
        {
            try
            {
                var state = await GetStateAsync();
                if (state != null)
                {
                    StateUpdated?.Invoke(this, new GameStateEventArgs(state));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POLLING] Ошибка: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopPolling();
            _client?.Dispose();
            Console.WriteLine("[SERVICE] Сервис остановлен");
        }
    }
}
