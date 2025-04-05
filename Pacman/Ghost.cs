using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pacman
{
    public class Ghost
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Speed { get; set; }
        public int Direction { get; set; }
        public int ImageX { get; set; }
        public int ImageY { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int Range { get; set; }
        private int randomTargetIndex;
        private Point target;
        private readonly Random random = new Random();
        private Timer directionTimer;
        private readonly Game gameForm; // Reference to Game for map and pacman access

        private readonly Bitmap ghostFrames;

        public Ghost(float x, float y, int width, int height, float speed, int imageX, int imageY, int imageWidth, int imageHeight, int range, Game form)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            Direction = Game.DIRECTION_RIGHT; // Assuming DIRECTION_RIGHT is defined in Game
            ImageX = imageX;
            ImageY = imageY;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            Range = range;
            gameForm = form;
            randomTargetIndex = random.Next(0, 4);
            target = gameForm.randomTargetsForGhosts[randomTargetIndex];

            // Load ghost.png
            try
            {
                ghostFrames = new Bitmap("ghost.png");
                ghostFrames.MakeTransparent(ghostFrames.GetPixel(0, 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ghost.png: {ex.Message}");
                ghostFrames = new Bitmap(Width, Height); // Fallback
            } // Optional: make background transparent

            directionTimer = new Timer();
            directionTimer.Interval = 10000; // 10 seconds
            directionTimer.Tick += (s, e) => ChangeRandomDirection();
            directionTimer.Start();
        }

        public bool IsInRange()
        {
            float xDistance = Math.Abs(gameForm.pacman.GetMapX() - GetMapX());
            float yDistance = Math.Abs(gameForm.pacman.GetMapY() - GetMapY());
            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance) <= Range;
        }

        private void ChangeRandomDirection()
        {
            randomTargetIndex = (randomTargetIndex + 1) % 4;
            target = gameForm.randomTargetsForGhosts[randomTargetIndex];
        }

        public void MoveProcess()
        {
            if (IsInRange())
            {
                target = new Point((int)gameForm.pacman.X, (int)gameForm.pacman.Y);
            }
            else
            {
                target = gameForm.randomTargetsForGhosts[randomTargetIndex];
            }
            ChangeDirectionIfPossible();
            MoveForwards();
            if (CheckCollisions())
            {
                MoveBackwards();
            }
        }

        private void MoveBackwards()
        {
            switch (Direction)
            {
                case Game.DIRECTION_RIGHT:
                    X -= Speed;
                    break;
                case Game.DIRECTION_UP:
                    Y += Speed;
                    break;
                case Game.DIRECTION_LEFT:
                    X += Speed;
                    break;
                case Game.DIRECTION_BOTTOM:
                    Y -= Speed;
                    break;
            }
        }

        private void MoveForwards()
        {
            switch (Direction)
            {
                case Game.DIRECTION_RIGHT:
                    X += Speed;
                    break;
                case Game.DIRECTION_UP:
                    Y -= Speed;
                    break;
                case Game.DIRECTION_LEFT:
                    X -= Speed;
                    break;
                case Game.DIRECTION_BOTTOM:
                    Y += Speed;
                    break;
            }
        }

        private bool CheckCollisions()
        {
            int mapX = (int)(X / gameForm.oneBlockSize);
            int mapY = (int)(Y / gameForm.oneBlockSize);
            int mapXRight = (int)((X + Width - 1) / gameForm.oneBlockSize);
            int mapYBottom = (int)((Y + Height - 1) / gameForm.oneBlockSize);

            return (mapY >= 0 && mapY < gameForm.map.GetLength(0) && mapX >= 0 && mapX < gameForm.map.GetLength(1) &&
                    gameForm.map[mapY, mapX] == 1) ||
                   (mapYBottom >= 0 && mapYBottom < gameForm.map.GetLength(0) && mapX >= 0 && mapX < gameForm.map.GetLength(1) &&
                    gameForm.map[mapYBottom, mapX] == 1) ||
                   (mapY >= 0 && mapY < gameForm.map.GetLength(0) && mapXRight >= 0 && mapXRight < gameForm.map.GetLength(1) &&
                    gameForm.map[mapY, mapXRight] == 1) ||
                   (mapYBottom >= 0 && mapYBottom < gameForm.map.GetLength(0) && mapXRight >= 0 && mapXRight < gameForm.map.GetLength(1) &&
                    gameForm.map[mapYBottom, mapXRight] == 1);
        }

        private void ChangeDirectionIfPossible()
        {
            int tempDirection = Direction;
            Direction = CalculateNewDirection(gameForm.map, (int)(target.X / gameForm.oneBlockSize), (int)(target.Y / gameForm.oneBlockSize));
            if (Direction == 0) // Undefined in JS becomes 0 in C#
            {
                Direction = tempDirection;
                return;
            }

            if (GetMapY() != GetMapYRightSide() && (Direction == Game.DIRECTION_LEFT || Direction == Game.DIRECTION_RIGHT))
            {
                Direction = Game.DIRECTION_UP;
            }
            if (GetMapX() != GetMapXRightSide() && Direction == Game.DIRECTION_UP)
            {
                Direction = Game.DIRECTION_LEFT;
            }

            MoveForwards();
            if (CheckCollisions())
            {
                MoveBackwards();
                Direction = tempDirection;
            }
            else
            {
                MoveBackwards();
            }
        }

        private int CalculateNewDirection(int[,] map, int destX, int destY)
        {
            int[,] mp = (int[,])map.Clone();
            Queue<(int x, int y, int rightX, int rightY, List<int> moves)> queue = new Queue<(int, int, int, int, List<int>)>();
            queue.Enqueue((GetMapX(), GetMapY(), GetMapXRightSide(), GetMapYRightSide(), new List<int>()));

            while (queue.Count > 0)
            {
                var (x, y, rightX, rightY, moves) = queue.Dequeue();
                if (x == destX && y == destY)
                {
                    return moves.Count > 0 ? moves[0] : Game.DIRECTION_BOTTOM;
                }
                else
                {
                    mp[y, x] = 1;
                    var neighbors = AddNeighbors((x, y, rightX, rightY, moves), mp);
                    foreach (var neighbor in neighbors)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return Game.DIRECTION_BOTTOM; // Default direction
        }

        private List<(int x, int y, int rightX, int rightY, List<int> moves)> AddNeighbors((int x, int y, int rightX, int rightY, List<int> moves) poped, int[,] mp)
        {
            var queue = new List<(int x, int y, int rightX, int rightY, List<int> moves)>();
            int numOfRows = mp.GetLength(1); // Columns in C# 2D array
            int numOfColumns = mp.GetLength(0); // Rows in C# 2D array

            if (poped.x - 1 >= 0 && poped.x - 1 < numOfRows && mp[poped.y, poped.x - 1] != 1)
            {
                var tempMoves = new List<int>(poped.moves) { Game.DIRECTION_LEFT };
                queue.Add((poped.x - 1, poped.y, poped.rightX, poped.rightY, tempMoves));
            }
            if (poped.x + 1 >= 0 && poped.x + 1 < numOfRows && mp[poped.y, poped.x + 1] != 1)
            {
                var tempMoves = new List<int>(poped.moves) { Game.DIRECTION_RIGHT };
                queue.Add((poped.x + 1, poped.y, poped.rightX, poped.rightY, tempMoves));
            }
            if (poped.y - 1 >= 0 && poped.y - 1 < numOfColumns && mp[poped.y - 1, poped.x] != 1)
            {
                var tempMoves = new List<int>(poped.moves) { Game.DIRECTION_UP };
                queue.Add((poped.x, poped.y - 1, poped.rightX, poped.rightY, tempMoves));
            }
            if (poped.y + 1 >= 0 && poped.y + 1 < numOfColumns && mp[poped.y + 1, poped.x] != 1)
            {
                var tempMoves = new List<int>(poped.moves) { Game.DIRECTION_BOTTOM };
                queue.Add((poped.x, poped.y + 1, poped.rightX, poped.rightY, tempMoves));
            }
            return queue;
        }

        public int GetMapX()
        {
            return (int)(X / gameForm.oneBlockSize);
        }

        public int GetMapY()
        {
            return (int)(Y / gameForm.oneBlockSize);
        }

        public int GetMapXRightSide()
        {
            return (int)((X * 0.99 + gameForm.oneBlockSize) / gameForm.oneBlockSize);
        }

        public int GetMapYRightSide()
        {
            return (int)((Y * 0.99 + gameForm.oneBlockSize) / gameForm.oneBlockSize);
        }

        public void Draw(Graphics g)
        {
            g.DrawImage(
                ghostFrames,
                new RectangleF(X, Y, Width, Height),
                new RectangleF(ImageX, ImageY, ImageWidth, ImageHeight),
                GraphicsUnit.Pixel
            );

            // Draw range circle (optional)
            using (Pen pen = new Pen(Color.Red))
            {
                g.DrawEllipse(pen, X + gameForm.oneBlockSize / 2 - Range * gameForm.oneBlockSize,
                    Y + gameForm.oneBlockSize / 2 - Range * gameForm.oneBlockSize,
                    Range * gameForm.oneBlockSize * 2, Range * gameForm.oneBlockSize * 2);
            }
        }

        public void Dispose()
        {
            directionTimer?.Stop();
            directionTimer?.Dispose();
            ghostFrames?.Dispose();
        }
    }
}