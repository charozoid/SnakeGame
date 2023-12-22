using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;

class SnakeGame
{
    public const int WIDTH = 800;
    public const int HEIGHT = 608;

    public const int GRID_SIZE_X = WIDTH / 16;
    public const int GRID_SIZE_Y = HEIGHT / 16;
    public static ICollidable[,] grid = new ICollidable[GRID_SIZE_X, GRID_SIZE_Y];
    public static bool gameOver = false;
    public static void Main(string[] args)
    {

        RenderWindow window = new RenderWindow(new SFML.Window.VideoMode(WIDTH, HEIGHT), "Snake");

        window.SetVerticalSyncEnabled(true);
        window.Closed += (sender, e) => { window.Close(); };
        window.SetFramerateLimit(120);
        Snake snake = new Snake(Color.Green);
        snake.GridPos = new Vector2f(1, 1);
        window.KeyPressed += (sender, e) => OnKeyPressed(sender, e, snake);
        Stopwatch stopwatch = new Stopwatch();

        if (!gameOver)
        {
            stopwatch.Restart();
        }

        while (window.IsOpen)
        {
            //OnKeyPressed(snake);
            window.DispatchEvents();
            window.Clear(Color.Black);
            if (stopwatch.ElapsedMilliseconds > 200)
            {
                stopwatch.Restart();
                snake.Move();
            }
            foreach (Block block in snake.blocks)
            {
                window.Draw(block.block);
            }
            window.Draw(snake.head);
            window.Display();

        }
    }
    public static void OnKeyPressed(object sender, KeyEventArgs e, Snake snake)
    {
        if (gameOver)
        {
            return;
        }
        if (e.Code == Keyboard.Key.Up)
        {
            snake.direction = Snake.Direction.Up;
        }
        else if (e.Code == Keyboard.Key.Down)
        {
            snake.direction = Snake.Direction.Down;
        }
        else if (e.Code == Keyboard.Key.Left)
        {
            snake.direction = Snake.Direction.Left;
        }
        else if (e.Code == Keyboard.Key.Right)
        {
            snake.direction = Snake.Direction.Right;
        }
    }
    /*public static void GetInput(Snake snake)
    {
        if (gameOver)
        {
            return;
        }
        if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
        {
            snake.direction = Snake.Direction.Up;
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
        {
            snake.direction = Snake.Direction.Down;
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
        {
            snake.direction = Snake.Direction.Left;
        }
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
        {
            snake.direction = Snake.Direction.Right;
        }
    }*/
    public static Vector2f GridToVector2f(Vector2f gridPos)
    {
        return new Vector2f(gridPos.X * 16, gridPos.Y * 16);
    }
}
class Snake : ICollidable
{
    private const int HEAD_SIZE = 16;
    public event EventHandler<KeyEventArgs> KeyPressed = null;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public List<Block> blocks = new List<Block>();
    public Vector2f gridPos = new Vector2f();
    public Vector2f GridPos { get { return gridPos; } set { gridPos = value; head.Position = SnakeGame.GridToVector2f(gridPos); } }
    public RectangleShape head = new RectangleShape(new Vector2f(HEAD_SIZE, HEAD_SIZE));
    public Direction direction;
    public Vector2f Position { get { return head.Position; } set { head.Position = value; } }
    public Snake(Color color)
    {
        head.FillColor = color;
        direction = Direction.Right;
    }
    public void Move()
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        switch (direction)
        {          
            case Direction.Up:
                if (y - 1 < 0 || grid[x, y - 1] != null)
                {
                    SnakeGame.gameOver = true;
                    return;
                }
                GridPos = new Vector2f(x, y - 1);
                break;
            case Direction.Down:
                if (y + 1 > SnakeGame.GRID_SIZE_Y - 1 || grid[x, y + 1] != null)
                {
                    SnakeGame.gameOver = true;
                    return;
                }
                GridPos = new Vector2f(x, y + 1);
                break;
            case Direction.Left:         
                if (x - 1 < 0 || grid[x - 1, y] != null)
                {
                    SnakeGame.gameOver = true;
                    return;
                }
                GridPos = new Vector2f(x - 1, y);
                break;
            case Direction.Right:
                if (x + 1 > SnakeGame.GRID_SIZE_X - 1 || grid[x + 1, y] != null)
                {
                    SnakeGame.gameOver = true;
                    return;
                }
                GridPos = new Vector2f(x + 1, y);
                break;
        }
        Block block = new Block(GridPos, head.FillColor);
        if (SnakeGame.grid[(int)gridPos.X, (int)gridPos.Y] == null)
        {
            SnakeGame.grid[(int)gridPos.X, (int)gridPos.Y] = block;
        }
        blocks.Add(block);
    }
}
class Block : ICollidable
{
    public Vector2f gridPos = new Vector2f();

    public Vector2f GridPos { get { return gridPos; } set { gridPos = value; block.Position = SnakeGame.GridToVector2f(gridPos); } }
    public RectangleShape block = new RectangleShape(new Vector2f(16, 16));
    public Block(Vector2f gridPos ,Color color)
    {
        block.FillColor = color;
        GridPos = gridPos;
    }
}

interface ICollidable
{
}
