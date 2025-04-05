using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pacman
{
    public partial class Game : Form
    {
        public const int DIRECTION_RIGHT = 4;
        public const int DIRECTION_UP = 3;
        public const int DIRECTION_LEFT = 2;
        public const int DIRECTION_BOTTOM = 1;

        private int lives = 3;
        private int ghostCount = 4;
        private int fps = 30;
        public int oneBlockSize = 20;
        public int score = 0;
        private float wallSpaceWidth;
        private float wallOffset;
        private Color wallInnerColor = Color.Black;

        public Pacman pacman;
        public List<Ghost> ghosts = new List<Ghost>();
        private Timer gameTimer;

        public int[,] map = new int[,]
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            {1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1},
            {1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1},
            {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            {1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1},
            {1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1},
            {1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1},
            {0, 0, 0, 0, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 0, 0, 0, 0},
            {1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 2, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1},
            {2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2},
            {1, 1, 1, 1, 1, 2, 1, 2, 1, 2, 2, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1},
            {0, 0, 0, 0, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 0, 0, 0, 0},
            {1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1},
            {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            {1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1},
            {1, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1},
            {1, 1, 2, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 2, 2, 1, 1},
            {1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1},
            {1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1},
            {1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };

        public Point[] randomTargetsForGhosts;

        public Point[] ghostImageLocations = new Point[]
        {
            new Point(0, 0),
            new Point(176, 0),
            new Point(0, 121),
            new Point(176, 121)
        };

        public Game()
        {
            InitializeComponent();
            this.Width = 21 * oneBlockSize + 20; // Adjust for map width
            this.Height = 23 * oneBlockSize + 60; // Adjust for map height + UI
            this.DoubleBuffered = true; // Reduce flicker
            this.KeyDown += Game_KeyDown;

            // Initialize wallSpaceWidth and wallOffset
            wallSpaceWidth = oneBlockSize / 1.6f;
            wallOffset = (oneBlockSize - (oneBlockSize / 1.6f)) / 2;

            // Initialize randomTargetsForGhosts here
            randomTargetsForGhosts = new Point[]
            {
                new Point(1 * oneBlockSize, 1 * oneBlockSize),
                new Point(1 * oneBlockSize, (23 - 2) * oneBlockSize),
                new Point((21 - 2) * oneBlockSize, oneBlockSize),
                new Point((21 - 2) * oneBlockSize, (23 - 2) * oneBlockSize)
            };

            CreateNewPacman();
            CreateGhosts();

            gameTimer = new Timer();
            gameTimer.Interval = 1000 / fps;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            // Update wallSpaceWidth and wallOffset after oneBlockSize is set
            wallSpaceWidth = oneBlockSize / 1.6f;
            wallOffset = (oneBlockSize - (oneBlockSize / 1.6f)) / 2;
        }

        private void CreateRect(Graphics g, int x, int y, int width, int height, Color color)
        {
            g.FillRectangle(new SolidBrush(color), x, y, width, height);
        }

        private void CreateNewPacman()
        {
            pacman = new Pacman(oneBlockSize, oneBlockSize, oneBlockSize, oneBlockSize, oneBlockSize / 5f, this);
        }

        private void CreateGhosts()
        {
            ghosts.Clear();
            for (int i = 0; i < ghostCount * 2; i++)
            {
                var newGhost = new Ghost(
                    9 * oneBlockSize + (i % 2 == 0 ? 0 : 1) * oneBlockSize,
                    10 * oneBlockSize + (i % 2 == 0 ? 0 : 1) * oneBlockSize,
                    oneBlockSize,
                    oneBlockSize,
                    pacman.Speed / 2,
                    ghostImageLocations[i % 4].X,
                    ghostImageLocations[i % 4].Y,
                    124,
                    116,
                    6 + i,
                    this
                );
                ghosts.Add(newGhost);
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            UpdateGame();
            Invalidate(); // Trigger repaint
        }

        private void UpdateGame()
        {
            pacman.MoveProcess();
            pacman.Eat();
            UpdateGhosts();
            if (pacman.CheckGhostCollision(ghosts))
            {
                OnGhostCollision();
            }
        }

        private void OnGhostCollision()
        {
            lives--;
            RestartPacmanAndGhosts();
            if (lives == 0)
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over!");
            }
        }

        private void RestartPacmanAndGhosts()
        {
            CreateNewPacman();
            CreateGhosts();
        }

        private void UpdateGhosts()
        {
            foreach (var ghost in ghosts)
            {
                ghost.MoveProcess(); // Assuming Ghost has a Move method
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.Black);
            DrawWalls(g);
            DrawFoods(g);
            DrawGhosts(g);
            pacman.Draw(g);
            DrawScore(g);
            DrawRemainingLives(g);
        }

        private void DrawWalls(Graphics g)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == 1)
                    {
                        CreateRect(g, j * oneBlockSize, i * oneBlockSize, oneBlockSize, oneBlockSize, Color.FromArgb(52, 45, 202));
                        if (j > 0 && map[i, j - 1] == 1)
                            CreateRect(g, j * oneBlockSize, i * oneBlockSize + (int)wallOffset, (int)(wallSpaceWidth + wallOffset), (int)wallSpaceWidth, wallInnerColor);
                        if (j < map.GetLength(1) - 1 && map[i, j + 1] == 1)
                            CreateRect(g, j * oneBlockSize + (int)wallOffset, i * oneBlockSize + (int)wallOffset, (int)(wallSpaceWidth + wallOffset), (int)wallSpaceWidth, wallInnerColor);
                        if (i < map.GetLength(0) - 1 && map[i + 1, j] == 1)
                            CreateRect(g, j * oneBlockSize + (int)wallOffset, i * oneBlockSize + (int)wallOffset, (int)wallSpaceWidth, (int)(wallSpaceWidth + wallOffset), wallInnerColor);
                        if (i > 0 && map[i - 1, j] == 1)
                            CreateRect(g, j * oneBlockSize + (int)wallOffset, i * oneBlockSize, (int)wallSpaceWidth, (int)(wallSpaceWidth + wallOffset), wallInnerColor);
                    }
                }
            }
        }

        private void DrawFoods(Graphics g)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == 2)
                    {
                        CreateRect(g, j * oneBlockSize + oneBlockSize / 3, i * oneBlockSize + oneBlockSize / 3, oneBlockSize / 3, oneBlockSize / 3, Color.FromArgb(254, 184, 151));
                    }
                }
            }
        }

        private void DrawGhosts(Graphics g)
        {
            foreach (var ghost in ghosts)
            {
                ghost.Draw(g); // Assuming Ghost has a Draw method
            }
        }

        private void DrawScore(Graphics g)
        {
            try { g.DrawString($"Score: {score}", new Font("Emulogic", 12), Brushes.White, 0, oneBlockSize * map.GetLength(0)); }
            catch { g.DrawString($"Score: {score}", new Font("Arial", 12), Brushes.White, 0, oneBlockSize * map.GetLength(0)); }
        }

        private void DrawRemainingLives(Graphics g)
        {
            g.DrawString("Lives: ", new Font("Arial", 12), Brushes.White, 220, oneBlockSize * map.GetLength(0));
            for (int i = 0; i < lives; i++)
            {
                // Use pacman's sprite for lives (frame 1)
                int frameWidth = pacman.pacmanFrames.Width / 7; // Assuming 7 frames
                g.DrawImage(
                    pacman.pacmanFrames,
                    new Rectangle(350 + i * oneBlockSize, oneBlockSize * map.GetLength(0) + 2, oneBlockSize, oneBlockSize),
                    new Rectangle(0, 0, frameWidth, pacman.pacmanFrames.Height),
                    GraphicsUnit.Pixel
                );
            }
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    pacman.NextDirection = DIRECTION_LEFT;
                    break;
                case Keys.Up:
                case Keys.W:
                    pacman.NextDirection = DIRECTION_UP;
                    break;
                case Keys.Right:
                case Keys.D:
                    pacman.NextDirection = DIRECTION_RIGHT;
                    break;
                case Keys.Down:
                case Keys.S:
                    pacman.NextDirection = DIRECTION_BOTTOM;
                    break;
            }
        }
    }
}