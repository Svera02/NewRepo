using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace итер4
{


    public class ShipPlacementManager
    {
        public int[] Ships { get; } = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        public int ShipIndex { get; private set; } = 0;
        public bool Horizontal { get; set; } = true;

        public List<List<Point>> PlacedShips { get; } = new List<List<Point>>();

        public bool AllShipsPlaced => ShipIndex >= Ships.Length;

        public event Action StateChanged;

        public void Clear()
        {
            PlacedShips.Clear();
            ShipIndex = 0;
            Horizontal = true;
            StateChanged?.Invoke();
        }

        public List<Point> GetPreview(Point start)
        {
            int size = Ships[ShipIndex];
            var preview = new List<Point>();

            for (int i = 0; i < size; i++)
            {
                int x = Horizontal ? start.X + i : start.X;
                int y = Horizontal ? start.Y : start.Y + i;

                if (x >= 10 || y >= 10)
                    return new List<Point>(); 

                preview.Add(new Point(x, y));
            }
            return preview;
        }

        public bool CanPlaceShipAt(int startX, int startY)
        {
            int size = Ships[ShipIndex];

            for (int i = 0; i < size; i++)
            {
                int x = Horizontal ? startX + i : startX;
                int y = Horizontal ? startY : startY + i;

                if (x >= 10 || y >= 10)
                    return false;

                if (PlacedShips.Any(s => s.Contains(new Point(x, y))))
                    return false;

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx < 0 || nx >= 10 || ny < 0 || ny >= 10)
                            continue;

                        bool isPartOfCurrentShip = false;

                        for (int j = 0; j < size; j++)
                        {
                            int sx = Horizontal ? startX + j : startX;
                            int sy = Horizontal ? startY : startY + j;

                            if (nx == sx && ny == sy)
                            {
                                isPartOfCurrentShip = true;
                                break;
                            }
                        }

                        if (!isPartOfCurrentShip &&
                            PlacedShips.Any(s => s.Contains(new Point(nx, ny))))
                            return false;
                    }
            }
            return true;
        }

        public bool PlaceShip(Point start)
        {
            if (!CanPlaceShipAt(start.X, start.Y))
                return false;

            PlacedShips.Add(GetPreview(start));
            ShipIndex++;
            StateChanged?.Invoke();
            return true;
        }

        public void AutoPlace()
        {
            Clear();
            Random r = new Random();

            for (int i = 0; i < Ships.Length; i++)
            {
                ShipIndex = i;
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 1000)
                {
                    attempts++;
                    int x = r.Next(10);
                    int y = r.Next(10);
                    Horizontal = r.Next(2) == 0;

                    if (CanPlaceShipAt(x, y))
                    {
                        PlacedShips.Add(GetPreview(new Point(x, y)));
                        placed = true;
                    }
                }

                if (!placed)
                {
                    Clear();
                    throw new Exception("Не удалось автоматически расставить корабли");
                }
            }

            ShipIndex = Ships.Length;
            StateChanged?.Invoke();
        }
    }



}
