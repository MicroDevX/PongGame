using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

using System;

using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PongGame;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    #region GameConstants
    private readonly double GCWidth;
    private readonly double GCHeight;
    private readonly double Ball_Size     = 18;
    private readonly double Paddle_Width  = 16;
    private readonly double Paddle_Height =100;
    private readonly double PaddleSpeed   = 6; // Machine speed
    private readonly double  Radius = 10;
    private double LeftPaddleY;
    private double RightPaddleY;
    private double BallX;
    private double BallY;
    private double BallSpeedX    = 5;
    private double BallSpeedY    = 3;
    private bool IsGameRunning   = false;
    private double _MeScore      = 0;
    private double _MachineScore = 0;
    private readonly DispatcherTimer timer;
    static SolidColorBrush Fill_Color(Color color) => new(color);
    #endregion

    private void MachineMove()
    {
        var TargetY = BallY + Ball_Size /2-Paddle_Height/2;

        if (RightPaddleY + Paddle_Height / 2 < TargetY)
        {
            RightPaddleY += PaddleSpeed;
        } else if (RightPaddleY + Paddle_Height / 2 > TargetY)
        {
            RightPaddleY -= PaddleSpeed;
        }

        // Clamp Machine paddle
        RightPaddleY = Math.Max(0, Math.Min(GCHeight - Paddle_Height, RightPaddleY));
    }
    private void Update()
    {
        // Move ball
        BallX += BallSpeedX;
        BallY += BallSpeedY;

        // Top/Bottom wall collision
        if (BallY <= 0 || BallY + Ball_Size >= GCHeight)
        {
            BallSpeedY = -BallSpeedY;
            BallY = Math.Max(0, Math.Min(GCHeight - Ball_Size, BallY));
        }

        // Left paddle collision
        if (
            BallX <= Paddle_Width &&
            BallY + Ball_Size > LeftPaddleY &&
            BallY < LeftPaddleY + Paddle_Height
          )
        {
            BallSpeedX = Math.Abs(BallSpeedX);

            // Add some "spin" effect based on where it hits the paddle
            BallSpeedY += ((BallY + Ball_Size / 2) - (LeftPaddleY + Paddle_Height / 2)) * 0.08;
        }

        // Right paddle collision
        if (
            BallX + Ball_Size >= GCWidth - Paddle_Width &&
            BallY + Ball_Size > RightPaddleY &&
            BallY < RightPaddleY + Paddle_Height
          )
        {
            BallSpeedX = -Math.Abs(BallSpeedX);
            BallSpeedY += ((BallY + Ball_Size / 2) - (RightPaddleY + Paddle_Height / 2)) * 0.08;
        }

        // Score
        if (BallX < 0)
        {
            _MachineScore++;
            MachineScore.Text = $"{_MachineScore}  :Machine";
            ResetBall();
        } else if (BallX + Ball_Size > GCWidth)
        {
            _MeScore++;
            MeScore.Text = $"Me:  {_MeScore}";
            ResetBall();
        }
        // Move Machine paddle
        MachineMove();

    }
    private void Draw()
    {
        // Clean
        GameCanvas.Children.Clear();

        // Draw paddles
        var PaddleMe = new Rectangle
        {
            Fill = Fill_Color(Windows.UI.Color.FromArgb(255,0,255,0)),
            Width = Paddle_Width,
            Height = Paddle_Height,
            RadiusX = Radius,
            RadiusY = Radius
        };
        Canvas.SetLeft(PaddleMe, 0);
        Canvas.SetTop(PaddleMe, LeftPaddleY);
        GameCanvas.Children.Add(PaddleMe);

        var PaddleMachine = new Rectangle
        {
            Fill = Fill_Color(Windows.UI.Color.FromArgb(255,255,0,0)),
            Width = Paddle_Width,
            Height = Paddle_Height,
            RadiusX = Radius,
            RadiusY = Radius
        };
        Canvas.SetLeft(PaddleMachine, GCWidth - Paddle_Width);
        Canvas.SetTop(PaddleMachine, RightPaddleY);
        GameCanvas.Children.Add(PaddleMachine);

        // Draw ball
        var Ball = new Ellipse
        {
            Stroke = Fill_Color(Windows.UI.Color.FromArgb(255,255,255,255)),
            StrokeThickness = 4,
            Fill = Fill_Color(Windows.UI.Color.FromArgb(255,0,0,255)),
            Width = Ball_Size,
            Height = Ball_Size
        };
        Canvas.SetLeft(Ball, BallX);
        Canvas.SetTop(Ball, BallY);
        GameCanvas.Children.Add(Ball);

        // Draw center line
        var CenterLine = new Line
        {
            X1 = GameCanvas.Width / 2,
            Y1 = 0,
            X2 = GameCanvas.Width / 2,
            Y2 = GameCanvas.Height,
            Stroke = Fill_Color(Windows.UI.Color.FromArgb(155,255,255,255)),
            StrokeThickness = 2,
            StrokeDashArray = [8, 8]
        };
        GameCanvas.Children.Add(CenterLine);
        // Draw scores (optional, since HTML shows scores)
    }
    private void ResetBall()
    {
        BallX = GCWidth / 2 - Ball_Size / 2;
        BallY = GCHeight / 2 - Ball_Size / 2;
        // Randomize direction
        var rand = new Random();
        BallSpeedX = (rand.NextDouble() > 0.5 ? 1 : -1) * 5;
        BallSpeedY = (rand.NextDouble() > 0.5 ? 1 : -1) * (2 + rand.NextDouble() * 3);
    }

    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
        GCWidth = GameCanvas.Width;
        GCHeight = GameCanvas.Height;
        BallX = (GCWidth / 2) - (Ball_Size / 2);
        BallY = (GCHeight / 2) - (Ball_Size / 2);

        LeftPaddleY = (GCHeight / 2) - (Paddle_Height / 2);
        RightPaddleY = (GCWidth / 2) - (Paddle_Height / 2);
        ResetBall();
        timer = new()
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        timer.Tick += (s, e) =>
        {
            Update();
            Draw();
        };
        // timer.Start(); // Start the game loop
    }


    private void GameCanvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var position = e.GetCurrentPoint(GameCanvas).Position;

        LeftPaddleY = position.Y - Paddle_Height / 2;

        // Clamp to canvas bounds
        LeftPaddleY = Math.Max(0, Math.Min(GCHeight - Paddle_Height, LeftPaddleY));
    }

    private void PlayBTN_Click(object sender, RoutedEventArgs e)
    {
        if (!IsGameRunning)
        {
            IsGameRunning = true;
            timer.Start();
            PlayBTN.Content = "\uE769";
        } else
        {
            IsGameRunning = false;
            timer.Stop();
            PlayBTN.Content = "\uE768";
        }
    }

    private void ResetBTN_Click(object sender, RoutedEventArgs e)
    {
        IsGameRunning = false;
        timer.Stop();
        PlayBTN.Content = "\uE768";
        _MeScore = 0;
        _MachineScore = 0;
        MeScore.Text = $"Me:  {_MeScore}";
        MachineScore.Text = $"{_MachineScore}  :Machine";
    }
}

