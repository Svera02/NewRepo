using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace итер4
{
    public class GameEngine : IDisposable
    {

        public string PlayerName { get; }
        public List<List<Point>> MyShips { get; }
        public string CurrentTurn { get; private set; }
        public bool IsGameOver { get; private set; }
        public string Winner { get; private set; }

        public int MyHits { get; private set; }
        public int MyMisses { get; private set; }
        public int EnemyHits { get; private set; }
        public int EnemyMisses { get; private set; }

        public event Action<string> TurnChanged;
        public event Action<int, int, CellState, bool> CellUpdated;
        public event Action<string> GameOver;
        public event Action<GameStatistics> StatisticsUpdated;

        private CellState[,] _myBoard = new CellState[10, 10];
        private CellState[,] _enemyBoard = new CellState[10, 10];
        private bool _disposed = false;

        public enum CellState
        {
            Empty,     
            Ship,      
            Hit,        
            Miss,       
            Sunk       
        }

        public struct GameStatistics
        {
            public int TotalShots;
            public int AccuracyPercent;
            public int RemainingShips;
            public int EnemyRemainingShips;
        }

        public GameEngine(string playerName, List<List<Point>> myShips)
        {
            PlayerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
            MyShips = myShips ?? throw new ArgumentNullException(nameof(myShips));

            InitializeBoards();
        }

        private void InitializeBoards()
        {

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    _myBoard[x, y] = CellState.Empty;
                    _enemyBoard[x, y] = CellState.Empty;
                }
            }


            foreach (var ship in MyShips)
            {
                foreach (var point in ship)
                {
                    if (IsValidCoordinate(point.X, point.Y))
                    {
                        _myBoard[point.X, point.Y] = CellState.Ship;
                    }
                }
            }
        }

        public void UpdateFromServer(GameState serverState)
        {
            if (serverState == null || IsGameOver) return;

            if (CurrentTurn != serverState.current_turn)
            {
                CurrentTurn = serverState.current_turn;
                TurnChanged?.Invoke(CurrentTurn);
            }

            ProcessEnemyShots(serverState);

            ProcessMyShots(serverState);

            UpdateStatistics();

            CheckGameOver();
        }

        private void ProcessEnemyShots(GameState state)
        {
            if (state.shots == null || !state.shots.ContainsKey(PlayerName))
                return;

            foreach (var coord in state.shots[PlayerName])
            {
                int x = coord[0];
                int y = coord[1];

                if (!IsValidCoordinate(x, y)) continue;

                var currentState = _myBoard[x, y];

                if (currentState == CellState.Hit || currentState == CellState.Miss || currentState == CellState.Sunk)
                    continue;

                if (currentState == CellState.Ship)
                {

                    _myBoard[x, y] = CellState.Hit;
                    EnemyHits++;
                    CellUpdated?.Invoke(x, y, CellState.Hit, true);

                    if (IsShipDestroyed(x, y, true))
                    {
                        MarkShipAsDestroyed(x, y, true);
                    }
                }
                else if (currentState == CellState.Empty)
                {

                    _myBoard[x, y] = CellState.Miss;
                    EnemyMisses++;
                    CellUpdated?.Invoke(x, y, CellState.Miss, true);
                }
            }
        }

        private void ProcessMyShots(GameState state)
        {

            string enemy = state.players?.FirstOrDefault(p => p != PlayerName);
            if (enemy == null || state.shots == null || !state.shots.ContainsKey(enemy))
                return;

            foreach (var coord in state.shots[enemy])
            {
                int x = coord[0];
                int y = coord[1];

                if (!IsValidCoordinate(x, y)) continue;

                var currentState = _enemyBoard[x, y];

                if (currentState != CellState.Empty) continue;

                _enemyBoard[x, y] = CellState.Miss;
                CellUpdated?.Invoke(x, y, CellState.Miss, false);
            }
        }

        public bool CanShootAt(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (IsGameOver) return false;
            if (CurrentTurn != PlayerName) return false;
            if (_enemyBoard[x, y] != CellState.Empty) return false;

            return true;
        }

        public ShootResult PrepareShoot(int x, int y)
        {
            if (!CanShootAt(x, y))
            {
                return new ShootResult
                {
                    Success = false,
                    Message = "Нельзя выстрелить в эту клетку"
                };
            }

            _enemyBoard[x, y] = CellState.Miss;
            CellUpdated?.Invoke(x, y, CellState.Miss, false);

            return new ShootResult
            {
                Success = true,
                X = x,
                Y = y
            };
        }

        public void ProcessServerShootResult(int x, int y, string serverResult)
        {
            if (!IsValidCoordinate(x, y)) return;

            if (serverResult == "hit" || serverResult == "win")
            {
                _enemyBoard[x, y] = CellState.Hit;
                MyHits++;
                CellUpdated?.Invoke(x, y, CellState.Hit, false);
            }
            else if (serverResult == "miss")
            {
                _enemyBoard[x, y] = CellState.Miss;
                MyMisses++;
                CellUpdated?.Invoke(x, y, CellState.Miss, false);
            }

            UpdateStatistics();
        }

        public CellState GetMyCellState(int x, int y)
        {
            return IsValidCoordinate(x, y) ? _myBoard[x, y] : CellState.Empty;
        }

        public CellState GetEnemyCellState(int x, int y)
        {
            return IsValidCoordinate(x, y) ? _enemyBoard[x, y] : CellState.Empty;
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < 10 && y >= 0 && y < 10;
        }

        private bool IsShipDestroyed(int x, int y, bool myBoard)
        {
            var board = myBoard ? _myBoard : _enemyBoard;

            var shipCells = FindShipCells(x, y, board);

            return shipCells.All(cell => board[cell.X, cell.Y] == CellState.Hit);
        }

        private List<Point> FindShipCells(int startX, int startY, CellState[,] board)
        {
            var result = new List<Point>();
            var visited = new bool[10, 10];
            var queue = new Queue<Point>();

            queue.Enqueue(new Point(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {

                        if (Math.Abs(dx) + Math.Abs(dy) != 1) continue;

                        int nx = current.X + dx;
                        int ny = current.Y + dy;

                        if (IsValidCoordinate(nx, ny) && !visited[nx, ny])
                        {
                            if (board[nx, ny] == CellState.Ship || board[nx, ny] == CellState.Hit)
                            {
                                queue.Enqueue(new Point(nx, ny));
                                visited[nx, ny] = true;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void MarkShipAsDestroyed(int x, int y, bool myBoard)
        {
            var board = myBoard ? _myBoard : _enemyBoard;
            var shipCells = FindShipCells(x, y, board);

            foreach (var cell in shipCells)
            {
                board[cell.X, cell.Y] = CellState.Sunk;
                CellUpdated?.Invoke(cell.X, cell.Y, CellState.Sunk, myBoard);
            }
        }

        private void UpdateStatistics()
        {
            var stats = new GameStatistics
            {
                TotalShots = MyHits + MyMisses + EnemyHits + EnemyMisses,
                AccuracyPercent = MyHits + MyMisses > 0 ? (MyHits * 100) / (MyHits + MyMisses) : 0,
                RemainingShips = CountRemainingShips(true),
                EnemyRemainingShips = CountRemainingShips(false)
            };

            StatisticsUpdated?.Invoke(stats);
        }

        private int CountRemainingShips(bool myBoard)
        {
            var board = myBoard ? _myBoard : _enemyBoard;
            int count = 0;
            var visited = new bool[10, 10];

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (!visited[x, y] && (board[x, y] == CellState.Ship || board[x, y] == CellState.Hit))
                    {

                        count++;

                        var shipCells = FindShipCells(x, y, board);
                        foreach (var cell in shipCells)
                        {
                            visited[cell.X, cell.Y] = true;
                        }
                    }
                }
            }

            return count;
        }

        private void CheckGameOver()
        {
            if (IsGameOver) return;

            bool myShipsDestroyed = CountRemainingShips(true) == 0;

            bool enemyShipsDestroyed = CountRemainingShips(false) == 0;

            if (myShipsDestroyed || enemyShipsDestroyed)
            {
                IsGameOver = true;
                Winner = myShipsDestroyed ? "Противник" : PlayerName;
                GameOver?.Invoke(Winner);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    TurnChanged = null;
                    CellUpdated = null;
                    GameOver = null;
                    StatisticsUpdated = null;
                }
                _disposed = true;
            }
        }

        ~GameEngine()
        {
            Dispose(false);
        }
    }

    public class ShootResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}

