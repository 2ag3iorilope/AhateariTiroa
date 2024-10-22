using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio; // Importar el espacio de nombres necesario

namespace AhateariTiroa;

public partial class MainPage : ContentPage
{
    private Random random = new Random();
    private List<Image> ducks = new List<Image>();
    private bool isGameRunning = false;
    private int score = 0;
    private const int FAST_DUCK_POINTS = 100;
    private const int SLOW_DUCK_POINTS = 50;
    private IDispatcherTimer gameTimer;
    private const int DUCK_SPAWN_INTERVAL = 2000; // ms

    public MainPage()
    {
        InitializeComponent();
        SetupGame();
    }

    private void SetupGame()
    {
        // Configurar timer del juego
        gameTimer = Application.Current.Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(DUCK_SPAWN_INTERVAL);
        gameTimer.Tick += GameTimer_Tick;

        UpdateScoreDisplay();
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
        if (isGameRunning)
        {
            CreateDuck();
        }
    }

    private void StartGame_Clicked(object sender, EventArgs e)
    {
        if (!isGameRunning)
        {
            StartGame();
        }
        else
        {
            EndGame();
        }
    }

    private void StartGame()
    {
        isGameRunning = true;
        score = 0;
        ducks.Clear();
        UpdateScoreDisplay();
        StartButton.Text = "Terminar Juego";
        gameTimer.Start();
        CreateDuck(); // Crear el primer pato inmediatamente
    }

    private void EndGame()
    {
        isGameRunning = false;
        gameTimer.Stop();
        StartButton.Text = "Iniciar Juego";

        foreach (var duck in ducks.ToList())
        {
            GameArea.Children.Remove(duck);
        }
        ducks.Clear();

        DisplayGameOverAlert();
    }

    private async void DisplayGameOverAlert()
    {
        await DisplayAlert("¡Juego Terminado!", $"Puntuación final: {score}", "OK");
    }

    private async void CreateDuck()
    {
        var duck = new Image
        {
            Source = random.Next(2) == 0 ? "patoazkarra.png" : "patomotela.png", // Cambia esto por tus imágenes
            WidthRequest = 50,
            HeightRequest = 50,
            VerticalOptions = LayoutOptions.Start
        };

        var startY = random.Next(50, (int)GameArea.Height - 100);
        AbsoluteLayout.SetLayoutBounds(duck, new Rect(-50, startY, 50, 50));
        GameArea.Children.Add(duck);
        ducks.Add(duck);

        _ = MoveDuck(duck);
    }

    private async Task MoveDuck(Image duck)
    {
        double speed = duck.Source.ToString().Contains("patoazkarra") ? 7.0 : 4.0;

        while (isGameRunning)
        {
            var bounds = AbsoluteLayout.GetLayoutBounds(duck);
            bounds.X += speed;

            if (bounds.X > GameArea.Width)
            {
                GameArea.Children.Remove(duck);
                ducks.Remove(duck);
                break;
            }

            AbsoluteLayout.SetLayoutBounds(duck, bounds);
            await Task.Delay(16); // ~60 FPS
        }
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        if (!isGameRunning) return;

        var position = e.GetPosition(GameArea).GetValueOrDefault();

        // Reproducir sonido de disparo
       

        var hitDuck = FindHitDuck(position);
        if (hitDuck != null)
        {
            GameArea.Children.Remove(hitDuck);
            ducks.Remove(hitDuck);
            score += hitDuck.Source.ToString().Contains("patoazkarra.png") ? FAST_DUCK_POINTS : SLOW_DUCK_POINTS;
            UpdateScoreDisplay();
        }
    }

  

    private void UpdateScoreDisplay()
    {
        ScoreLabel.Text = $"Puntuación: {score}";
    }

    private Image FindHitDuck(Point position)
    {
        return ducks.FirstOrDefault(duck =>
        {
            var bounds = AbsoluteLayout.GetLayoutBounds(duck);
            return position.X >= bounds.X &&
                   position.X <= bounds.X + bounds.Width &&
                   position.Y >= bounds.Y &&
                   position.Y <= bounds.Y + bounds.Height;
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        gameTimer?.Stop();
        isGameRunning = false;
    }
}
