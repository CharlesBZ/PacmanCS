using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pacman
{
    public class Pacman
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Speed { get; set; }
        public int Direction { get; set; }
        public int NextDirection { get; set; }
        private readonly int frameCount = 7;
        private int currentFrame = 1;
        private readonly Timer animationTimer;
        private readonly Game gameForm; // Reference to Game for map and score access

        public Bitmap pacmanFrames { get; private set; } // Pacman animation

        public Pacman(float x, float y, int width, int height, float speed, Game form)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            Direction = Game.DIRECTION_RIGHT; // Assuming constants are in Game
            NextDirection = Game.DIRECTION_RIGHT;
            gameForm = form;

            // Load Pacman
            try
            {
                pacmanFrames = new Bitmap("animations.gif");
                pacmanFrames.MakeTransparent(pacmanFrames.GetPixel(0, 0)); // Optional
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ghost.png: {ex.Message}");
                pacmanFrames = new Bitmap(Width, Height); // Fallback
            }
            

            animationTimer = new Timer();
            animationTimer.Interval = 100; // 100ms for animation
            animationTimer.Tick += (s, e) => ChangeAnimation();
            animationTimer.Start();
        }

        public void MoveProcess()
        {
            ChangeDirectionIfPossible();
            MoveForwards();
            if (CheckCollisions())
            {
                MoveBackwards();
            }
        }

        public void Eat()
        {
            for (int i = 0; i < gameForm.map.GetLength(0); i++)
            {
                for (int j = 0; j < gameForm.map.GetLength(1); j++)
                {
                    if (gameForm.map[i, j] == 2 && GetMapX() == j && GetMapY() == i)
                    {
                        gameForm.map[i, j] = 3; // Mark as eaten
                        gameForm.score++;       // Increment score
                    }
                }
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

        public bool CheckGhostCollision(List<Ghost> ghosts)
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.GetMapX() == GetMapX() && ghost.GetMapY() == GetMapY())
                {
                    return true;
                }
            }
            return false;
        }

        private void ChangeDirectionIfPossible()
        {
            if (Direction == NextDirection) return;
            int tempDirection = Direction;
            Direction = NextDirection;
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

        private void ChangeAnimation()
        {
            currentFrame = (currentFrame == frameCount) ? 1 : currentFrame + 1;
        }

        public void Draw(Graphics g)
        {
            g.TranslateTransform(X + gameForm.oneBlockSize / 2, Y + gameForm.oneBlockSize / 2);
            g.RotateTransform((Direction - 1) * 90); // Adjust for 1-4 direction values
            g.TranslateTransform(-X - gameForm.oneBlockSize / 2, -Y - gameForm.oneBlockSize / 2);

            // Assuming animations.gif is a horizontal sprite sheet with 7 frames
            int frameWidth = pacmanFrames.Width / frameCount;
            g.DrawImage(
                pacmanFrames,
                new RectangleF(X, Y, Width, Height),
                new RectangleF((currentFrame - 1) * frameWidth, 0, frameWidth, pacmanFrames.Height),
                GraphicsUnit.Pixel
            );

            g.ResetTransform();
        }

        public void Dispose()
        {
            animationTimer.Stop();
            animationTimer.Dispose();
            pacmanFrames.Dispose();
        }
    }
}