using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MemoryGame
{
    public partial class MainWindow : Window
    {
        private int Rows;
        private int Cols;
        private string selectedTheme;
        private List<Card> cards = new List<Card>();
        private Card? firstFlippedCard;
        private Card? secondFlippedCard;
        private DispatcherTimer timer = new DispatcherTimer();
        private int elapsedTime;
        private bool isPaused = false;
        private int moveCount = 0;
        private int matchedPairs = 0;
        private readonly string resultsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MemoryGame-main","game_results.txt");

        public MainWindow(int difficulty, string theme)
{
    try
    {
        InitializeComponent();
        
        Rows = difficulty;
        Cols = difficulty;
        selectedTheme = theme;

        this.Title = $"Memory Card Game - {GetThemeDisplayName(theme)} - {difficulty}x{difficulty}";

        foreach (ComboBoxItem item in DifficultyComboBox.Items)
        {
            if (item.Tag != null && int.Parse(item.Tag.ToString()!) == difficulty)
            {
                DifficultyComboBox.SelectedItem = item;
                break;
            }
        }

        InitializeTimer();
        InitializeGame();
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error in MainWindow constructor: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        
        this.Loaded += (s, e) =>
        {
            MessageBox.Show($"Error initializing game: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        };
    }
}

        private void InitializeGame()
        {
            try
            {
                cards = new List<Card>();
                firstFlippedCard = null;
                secondFlippedCard = null;
                moveCount = 0;
                matchedPairs = 0;
                MoveCounterText.Text = "Potezi: 0";

                int totalCards = Rows * Cols;
                int pairsNeeded = totalCards / 2;

                List<string> imagePaths = GenerateImagePaths(pairsNeeded, selectedTheme);

                List<string> allCards = new List<string>(imagePaths);
                allCards.AddRange(imagePaths);

                allCards = allCards.OrderBy(x => Guid.NewGuid()).ToList();

                CardGrid.Children.Clear();
                CardGrid.RowDefinitions.Clear();
                CardGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < Rows; i++)
                    CardGrid.RowDefinitions.Add(new RowDefinition());
                for (int j = 0; j < Cols; j++)
                    CardGrid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        Button cardButton = new Button
                        {
                            Content = new Image
                            {
                                Source = new BitmapImage(new Uri("Images/card_back.png", UriKind.Relative)),
                                Stretch = Stretch.Fill
                            },
                            FontSize = 24,
                            Background = Brushes.LightGray,
                            Tag = allCards[i * Cols + j],
                            Margin = new Thickness(2)
                        };
                        cardButton.Click += CardButton_Click;

                        Grid.SetRow(cardButton, i);
                        Grid.SetColumn(cardButton, j);
                        CardGrid.Children.Add(cardButton);

                        cards.Add(new Card
                        {
                            Button = cardButton,
                            ImagePath = allCards[i * Cols + j],
                            IsFlipped = false,
                            IsMatched = false
                        });
                    }
                }

                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in InitializeGame: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "InitializeGame Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private string GetThemeDisplayName(string theme)
        {
            return theme switch
            {
                "flags" => "Zastave",
                "food" => "Hrana",
                "cars" => "Automobili",
                _ => "Default"
            };
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            elapsedTime = 0;
            TimerText.Text = "Vrijeme: 0s";
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            elapsedTime++;
            TimerText.Text = $"Vrijeme: {elapsedTime}s";
        }

        

        private List<string> GenerateImagePaths(int count, string theme)
        {
            List<string> paths = new List<string>();
            string folderName = GetThemeFolderName(theme);

            for (int i = 1; i <= count; i++)
            {
                paths.Add($"Images/{folderName}/image{i}.png");
            }
            return paths;
        }

        private string GetThemeFolderName(string theme)
        {
            return theme switch
            {
                "flags" => "zastave",
                "food" => "hrana",
                "cars" => "automobili",
                _ => "default"
            };
        }

        private async void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedButton) return;
            Card? clickedCard = cards.FirstOrDefault(c => c.Button == clickedButton);

            if (clickedCard == null || clickedCard.IsFlipped || clickedCard.IsMatched || secondFlippedCard != null || isPaused)
                return;

            clickedCard.IsFlipped = true;
            clickedButton.Content = new Image
            {
                Source = new BitmapImage(new Uri(clickedCard.ImagePath, UriKind.Relative)),
                Stretch = Stretch.Fill
            };

            if (firstFlippedCard == null)
            {
                firstFlippedCard = clickedCard;
            }
            else
            {
                secondFlippedCard = clickedCard;
                moveCount++;
                MoveCounterText.Text = $"Potezi: {moveCount}";

                EnableCards(false);

                await CheckForMatch();
            }
        }

        private async Task CheckForMatch()
        {
            if (firstFlippedCard == null || secondFlippedCard == null) return;

            await Task.Delay(500); 

            if (firstFlippedCard.ImagePath == secondFlippedCard.ImagePath)
            {
                firstFlippedCard.IsMatched = true;
                secondFlippedCard.IsMatched = true;
                firstFlippedCard.Button.IsEnabled = false;
                secondFlippedCard.Button.IsEnabled = false;

                firstFlippedCard.Button.Background = Brushes.LightGreen;
                secondFlippedCard.Button.Background = Brushes.LightGreen;

                matchedPairs++;

                if (matchedPairs == (Rows * Cols) / 2)
                {
                    timer.Stop();
                    await GameCompleted();
                }
            }
            else
            {
                await Task.Delay(1000);

                firstFlippedCard.Button.Content = new Image
                {
                    Source = new BitmapImage(new Uri("Images/card_back.png", UriKind.Relative)),
                    Stretch = Stretch.Fill
                };

                secondFlippedCard.Button.Content = new Image
                {
                    Source = new BitmapImage(new Uri("Images/card_back.png", UriKind.Relative)),
                    Stretch = Stretch.Fill
                };

                firstFlippedCard.IsFlipped = false;
                secondFlippedCard.IsFlipped = false;
            }

            firstFlippedCard = null;
            secondFlippedCard = null;

            EnableCards(true);
        }

        private async Task GameCompleted()
        {
            string difficultyText = Rows switch
            {
                4 => "Lako (4x4)",
                6 => "Srednje (6x6)",
                8 => "Teško (8x8)",
                _ => $"{Rows}x{Cols}"
            };

            await SaveGameResult(difficultyText, GetThemeDisplayName(selectedTheme), elapsedTime, moveCount);

            MessageBoxResult result = MessageBox.Show(
                $"Čestitamo! Završili ste!\n\n" +
                $"Težina: {difficultyText}\n" +
                $"Tema: {GetThemeDisplayName(selectedTheme)}\n" +
                $"Vrijeme: {elapsedTime} sekundi\n" +
                $"Potezi: {moveCount}\n\n" +
                $"Želite li da vidite tabelu rezultata?",
                "Kraj igre",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                ShowRankingWindow();
            }
        }

        private async Task SaveGameResult(string difficulty, string theme, int time, int moves)
        {
            try
            {
                string directory = Path.GetDirectoryName(resultsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string resultLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{difficulty}|{theme}|{time}|{moves}";
                await File.AppendAllTextAsync(resultsFilePath, resultLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri čuvanju rezultata: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowRankingWindow()
        {
            try
            {
                RankingWindow rankingWindow = new RankingWindow();
                rankingWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri prikazivanju tabele: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RankingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRankingWindow();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Želite li da restartujete igru?", "Restart",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                timer.Stop();
                elapsedTime = 0;
                TimerText.Text = "Vreme: 0s";
                InitializeGame();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                timer.Start();
                PauseButton.Content = "Pauza";
                EnableCards(true);
            }
            else
            {
                timer.Stop();
                PauseButton.Content = "Nastavi";
                EnableCards(false);
            }

            isPaused = !isPaused;
        }

        private void EnableCards(bool isEnabled)
        {
            foreach (var card in cards.Where(c => !c.IsMatched))
            {
                card.Button.IsEnabled = isEnabled;
            }
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DifficultyComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                int size = int.Parse(selectedItem.Tag.ToString()!);

                if (size != Rows) 
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Promjena težine će restartovati igru. Želite li da nastavite?",
                        "Promjena težine",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Rows = size;
                        Cols = size;
                        this.Title = $"Memory Card Game - {GetThemeDisplayName(selectedTheme)} - {size}x{size}";
                        timer.Stop();
                        elapsedTime = 0;
                        TimerText.Text = "Vreme: 0s";
                        InitializeGame();
                    }
                    else
                    {
                        foreach (ComboBoxItem item in DifficultyComboBox.Items)
                        {
                            if (item.Tag != null && int.Parse(item.Tag.ToString()!) == Rows)
                            {
                                DifficultyComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public class Card
    {
        public required Button Button { get; set; }
        public required string ImagePath { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
    }
}