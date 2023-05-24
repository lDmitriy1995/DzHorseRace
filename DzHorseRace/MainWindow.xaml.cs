using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HorseRaceSimulation
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<Horse> horses;
        private CancellationTokenSource cancellationTokenSource;
        private Random random;
        private DispatcherTimer timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Horse> Horses
        {
            get { return horses; }
            set
            {
                horses = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Horses)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            random = new Random();
            Horses = new List<Horse>()
            {
                new Horse("Horse 1"),
                new Horse("Horse 2"),
                new Horse("Horse 3"),
                new Horse("Horse 4"),
                new Horse("Horse 5")
            };
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            ResetButton.IsEnabled = false;

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += async (s, args) =>
                {
                    await UpdateProgressAsync(cancellationToken);
                };

                timer.Start();
            }
            catch (TaskCanceledException)
            {
                // Ignore task cancellation
            }
            finally
            {
                timer.Stop();
                timer.Tick -= async (s, args) =>
                {
                    await UpdateProgressAsync(cancellationToken);
                };

                StartButton.IsEnabled = true;
                ResetButton.IsEnabled = true;
            }
        }

        private async Task UpdateProgressAsync(CancellationToken cancellationToken)
        {
            foreach (var horse in Horses)
            {
                if (horse.Progress < 100)
                {
                    horse.Progress += random.Next(1, 5);
                }

                await Task.Delay(10);
            }

            if (Horses.TrueForAll(h => h.Progress >= 100))
            {
                DisplayResults();
                cancellationTokenSource.Cancel();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsGrid.ItemsSource = null;
            foreach (var horse in Horses)
            {
                horse.Progress = 0;
            }
        }

        private void DisplayResults()
        {
            List<Result> results = new List<Result>();
            foreach (var horse in Horses)
            {
                results.Add(new Result() { Horse = horse.Name, Time = DateTime.Now.ToString("HH:mm:ss") });
            }
            ResultsGrid.ItemsSource = results;
        }
    }

    public class Horse : INotifyPropertyChanged
    {
        private string name;
        private int progress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public Horse(string name)
        {
            Name = name;
            Progress = 0;
        }
    }

    public class Result
    {
        public string Horse { get; set; }
        public string Time { get; set; }
    }
}
