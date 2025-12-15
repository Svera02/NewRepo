using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using итер4;

namespace итер4
{


    public class GameState
    {
        public bool success { get; set; }
        public List<string> players { get; set; }
        public int connected { get; set; }
        public bool game_ready { get; set; }
        public bool game_started { get; set; } 
        public Dictionary<string, string> player_status { get; set; }
        public Dictionary<string, List<int[]>> shots { get; set; }
        public string current_turn { get; set; }

        public bool CanShoot(string playerName)
        {
            if (!game_ready || player_status == null || current_turn == null)
                return false;

            return player_status.ContainsKey(playerName) &&
                   player_status[playerName] == "battle" &&
                   current_turn == playerName;
        }
    }


    public class GameStateEventArgs : EventArgs
    {
        public GameState State { get; }
        public GameStateEventArgs(GameState s) => State = s;
    }

    public interface IGameService : IDisposable
    {
        event EventHandler<GameStateEventArgs> StateUpdated;
        Task<string> ConnectAsync(string playerName);
        Task SendShipsAsync(string playerName, List<List<int[]>> ships);
        Task<GameState> GetStateAsync();
        Task<ShotResult> ShootAsync(string playerName, int x, int y);
        void StartPolling(int intervalMs = 2000);
        void StopPolling();
    }
    public class ShotResult
    {
        public bool success { get; set; }
        public string result { get; set; } 
        public string message { get; set; }
    }

}


