using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemoryGame
{
    public partial class RankingWindow : Window
    {
        private readonly string resultsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MemoryGame-main", "game_results.txt");
        private List<GameResult> allResults = new List<GameResult>();

        public RankingWindow()
        {
            InitializeComponent();
            LoadResults();
            ApplyFilters();
        }

        private void LoadResults()
        {
            try
            {
                allResults.Clear();

                // Kreiraj direktorij ako ne postoji
                string directory = Path.GetDirectoryName(resultsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(resultsFilePath))
                {
                    // Create empty file if it doesn't exist
                    File.WriteAllText(resultsFilePath, "");
                    return;
                }

                string[] lines = File.ReadAllLines(resultsFilePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length == 5)
                    {
                        try
                        {
                            var result = new GameResult
                            {
                                DateTime = parts[0],
                                Difficulty = parts[1],
                                Theme = parts[2],
                                Time = int.Parse(parts[3]),
                                Moves = int.Parse(parts[4])
                            };
                            result.Score = CalculateScore(result.Time, result.Moves);
                            allResults.Add(result);
                        }
                        catch (Exception ex)
                        {
                            // Skip invalid lines silently or log if needed
                            System.Diagnostics.Debug.WriteLine($"Invalid result line: {line} - {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju rezultata: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CalculateScore(int time, int moves)
        {
            // Simple scoring system: Lower time and fewer moves = higher score
            // Base score of 10000, subtract time and moves*10
            int score = 10000 - time - (moves * 10);
            return Math.Max(score, 0); // Don't allow negative scores
        }

        private void ApplyFilters()
        {
            var filteredResults = allResults.AsEnumerable();

            // Apply difficulty filter
            if (DifficultyFilterComboBox?.SelectedItem is ComboBoxItem difficultyItem &&
                difficultyItem.Tag?.ToString() != "All")
            {
                string selectedDifficulty = difficultyItem.Tag.ToString();
                filteredResults = filteredResults.Where(r => r.Difficulty == selectedDifficulty);
            }

            // Apply theme filter
            if (ThemeFilterComboBox?.SelectedItem is ComboBoxItem themeItem &&
                themeItem.Tag?.ToString() != "All")
            {
                string selectedTheme = themeItem.Tag.ToString();
                filteredResults = filteredResults.Where(r => r.Theme == selectedTheme);
            }

            // Sort by score (descending) then by time (ascending) then by moves (ascending)
            var sortedResults = filteredResults
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Time)
                .ThenBy(r => r.Moves)
                .ToList();

            // Add ranking
            for (int i = 0; i < sortedResults.Count; i++)
            {
                sortedResults[i].Rank = i + 1;
            }

            ResultsDataGrid.ItemsSource = sortedResults;
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only apply filters if the window is fully loaded
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Da li ste sigurni da želite da obrišete sve rezultate?\nOva akcija se ne može poništiti!",
                "Potvrda brisanja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(resultsFilePath))
                    {
                        File.WriteAllText(resultsFilePath, "");
                    }

                    allResults.Clear();
                    ApplyFilters();

                    MessageBox.Show("Svi rezultati su uspešno obrisani.", "Uspeh",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška pri brisanju rezultata: {ex.Message}", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class GameResult
    {
        public int Rank { get; set; }
        public string DateTime { get; set; }
        public string Difficulty { get; set; }
        public string Theme { get; set; }
        public int Time { get; set; }
        public int Moves { get; set; }
        public int Score { get; set; }
    }
}