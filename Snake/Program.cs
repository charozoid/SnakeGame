using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

class SnakeGame
{
    public const int WIDTH = 800;
    public const int HEIGHT = 608;
    public const int BLOCK_SIZE = 32;
    public const int GRID_SIZE_X = WIDTH / BLOCK_SIZE;
    public const int GRID_SIZE_Y = HEIGHT / BLOCK_SIZE;

    public enum Gamemode
    {
        Score,
        Versus
    }

    public static Random random = new Random();

    public static ICollidable[,] grid = new ICollidable[GRID_SIZE_X, GRID_SIZE_Y];

    public static bool gameOver = false;
    public static int score = 0;
    public static Snake loser;
    public static void Main(string[] args)
    {
        Gamemode gamemode = Gamemode.Versus;
        RenderWindow window = new RenderWindow(new VideoMode(WIDTH, HEIGHT), "Snake");

        window.SetVerticalSyncEnabled(true);
        window.Closed += (sender, e) => { window.Close(); };
        window.SetFramerateLimit(120);

        Snake snake = new Snake(Color.Red);
        AISnake aiSnake = new AISnake(Color.Red);
        switch (gamemode)
        {
            case Gamemode.Score:
                snake = new Snake(Color.Green);
                snake.GridPos = new Vector2f(5, 5);
                break;
            case Gamemode.Versus:
                snake = new Snake(Color.Green);
                snake.GridPos = new Vector2f(1, 5);
                aiSnake = new AISnake(Color.Blue);
                aiSnake.GridPos = new Vector2f(20, 5);
                break;
        }

        Stopwatch stopwatch = new Stopwatch();
        Stopwatch animationsStopwatch = new Stopwatch();
        animationsStopwatch.Restart();
        stopwatch.Restart();

        Text text = new Text("", new Font("arial.ttf"));
        text.Position = new Vector2f(WIDTH / 2 - 150, HEIGHT / 2 - 50);
        text.FillColor = Color.Red;

        window.KeyPressed += (sender, e) => OnKeyPressed(sender, e, snake);
        while (window.IsOpen)
        {
            window.DispatchEvents();
            window.Clear(Color.Black);

            switch (gamemode)
            {
                case Gamemode.Score:
                    if (!gameOver && stopwatch.ElapsedMilliseconds > 200)
                    {
                        stopwatch.Restart();
                        snake.Move();
                    }
                    if (gameOver && animationsStopwatch.ElapsedMilliseconds > 50)
                    {
                        text.DisplayedString = "Game Over! Score: " + score;
                        snake.RemoveBody();
                        snake.head.FillColor = Color.Red;
                        animationsStopwatch.Restart();
                    }
                    snake.DrawBody(window);
                    window.Draw(snake.head);

                    window.Draw(snake.head);
                    break;
                case Gamemode.Versus:
                    if (gameOver && animationsStopwatch.ElapsedMilliseconds > 50)
                    {
                        Snake winner = snake;
                        if (loser == snake)
                        {
                            winner = aiSnake;
                        }
                        text.DisplayedString = "Game Over! Winner: " + winner.name;
                        loser.RemoveBody();
                        loser.head.FillColor = Color.Red;
                        animationsStopwatch.Restart();
                    }
                    if (!gameOver && stopwatch.ElapsedMilliseconds > 200)
                    {
                        stopwatch.Restart();
                        aiSnake.Move();
                        snake.Move();
                    }
                    snake.DrawBody(window);
                    aiSnake.DrawBody(window);
                    window.Draw(snake.head);
                    window.Draw(aiSnake.head);
                    break;
            }
            window.Draw(text);
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
    public static Vector2f GridToVector2f(Vector2f gridPos)
    {
        return new Vector2f(gridPos.X * BLOCK_SIZE, gridPos.Y * BLOCK_SIZE);
    }
}
class AISnake : Snake
{
    public enum AITypes
    {
        Random,
        Smart
    }
    public AITypes aitype = AITypes.Smart;
    public AISnake(Color color) : base(color)
    {
        name = "AI";
    }
    public override void Move()
    {

        switch (aitype)
        {
            case AITypes.Random:
                RandomMove();
                break;
            case AITypes.Smart:
                SmartMove();
                break;
        }
        if (IsCollisionImminent(gridPos))
        {
            SnakeGame.loser = this;
            SnakeGame.gameOver = true;
            return;
        }
    }
    private bool IsDirectionSafe(Direction targetDirection)
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        switch (targetDirection)
        {
            case Direction.Up:
                if (y - 1 < 0 || grid[x, y - 1] != null)
                {
                    return false;
                }
                break;
            case Direction.Down:
                if (y + 1 > SnakeGame.GRID_SIZE_Y - 1 || grid[x, y + 1] != null)
                {
                    return false;
                }
                break;
            case Direction.Left:
                if (x - 1 < 0 || grid[x - 1, y] != null)
                {
                    return false;
                }
                break;
            case Direction.Right:
                if (x + 1 > SnakeGame.GRID_SIZE_X - 1 || grid[x + 1, y] != null)
                {
                    return false;
                }
                break;
        }
        return true;
    }
    private void SmartMove()
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        List<Direction> directions = new List<Direction>() {Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        if (y > 0 && direction != Direction.Down && IsDirectionSafe(Direction.Up))
        {
            directions.Add(Direction.Up);
        }
        if (y < SnakeGame.GRID_SIZE_Y && direction != Direction.Up && grid[x, y + 1] == null && IsDirectionSafe(Direction.Down))
        {
            directions.Add(Direction.Down);
        }
        if (x >= 0 && direction != Direction.Right && grid[x - 1, y] == null && IsDirectionSafe(Direction.Left))
        {
            directions.Add(Direction.Left);
        }
        if (x < SnakeGame.GRID_SIZE_X && direction != Direction.Left && IsDirectionSafe(Direction.Right))
        {
            directions.Add(Direction.Right);
        }
        direction = directions[SnakeGame.random.Next(directions.Count)];
        base.Move();
    }
    private void RandomMove()
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        List<Direction> directions = new List<Direction>();
        if (y - 1 >= 0 && direction != Direction.Down)
        {
            directions.Add(Direction.Up);
        }
        if (y + 1 < SnakeGame.GRID_SIZE_Y && direction != Direction.Up)
        {
            directions.Add(Direction.Down);
        }
        if (x - 1 >= 0 && direction != Direction.Right)
        {
            directions.Add(Direction.Left);
        }
        if (x + 1 < SnakeGame.GRID_SIZE_X && direction != Direction.Left)
        {
            directions.Add(Direction.Right);
        }
        direction = directions[SnakeGame.random.Next(directions.Count)];
        base.Move();
    }

}
class Snake : ICollidable
{
    private const int HEAD_SIZE = SnakeGame.BLOCK_SIZE;
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
    private Stopwatch stopwatch = new Stopwatch();
    public string name;


    public Snake(Color color)
    {
        head.FillColor = color;
        direction = Direction.Right;
        stopwatch.Restart();
        name = "Player";
    }

    public bool IsCollisionImminent(Vector2f vector2F)
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)vector2F.X;
        int y = (int)vector2F.Y;
        switch (direction)
        {
            case Direction.Up:
                if (y - 1 < 0 || grid[x, y - 1] != null)
                {
                    return true;
                }
                break;
            case Direction.Down:
                if (y + 1 > SnakeGame.GRID_SIZE_Y - 1 || grid[x, y + 1] != null)
                {
                    return true;
                }
                break;
            case Direction.Left:
                if (x - 1 < 0 || grid[x - 1, y] != null)
                {
                    return true;
                }
                break;
            case Direction.Right:
                if (x + 1 > SnakeGame.GRID_SIZE_X - 1 || grid[x + 1, y] != null)
                {
                    return true;
                }
                break;
        }
        return false;
    }
    public virtual void Move()
    {
        ICollidable[,] grid = SnakeGame.grid;
        int x = (int)gridPos.X;
        int y = (int)gridPos.Y;
        if (IsCollisionImminent(gridPos))
        {
            SnakeGame.loser = this;
            SnakeGame.gameOver = true;
            return;
        }
        switch (direction)
        {
            case Direction.Up:
                GridPos = new Vector2f(x, y - 1);
                break;
            case Direction.Down:
                GridPos = new Vector2f(x, y + 1);
                break;
            case Direction.Left:
                GridPos = new Vector2f(x - 1, y);
                break;
            case Direction.Right:
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
    public void RemoveBody()
    {
        if (blocks.Count == 0)
        {
            head.Size = new Vector2f(0, 0);
            return;
        }
        blocks.RemoveAt(0);
        SnakeGame.score++;
    }

    public void DrawBody(RenderWindow window)
    {
        foreach (Block block in blocks)
        {
            window.Draw(block.block);
        }
    }
}
class Block : ICollidable
{
    public Vector2f gridPos = new Vector2f();

    public Vector2f GridPos { get { return gridPos; } set { gridPos = value; block.Position = SnakeGame.GridToVector2f(gridPos); } }
    public RectangleShape block = new RectangleShape(new Vector2f(SnakeGame.BLOCK_SIZE, SnakeGame.BLOCK_SIZE));
    public Block(Vector2f gridPos, Color color)
    {
        block.FillColor = color;
        GridPos = gridPos;
    }
}
interface ICollidable
{
}
