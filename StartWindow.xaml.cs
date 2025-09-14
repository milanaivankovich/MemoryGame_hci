using System;
using System.Windows;
using System.Windows.Controls;

namespace MemoryGame
{
    public partial class StartWindow : Window
    {
        public int SelectedDifficulty { get; private set; }
        public string SelectedTheme { get; private set; } = string.Empty;

        public StartWindow()
        {
            InitializeComponent();

            SelectedDifficulty = 4;
            SelectedTheme = "flags";

            UpdateStartButtonState();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStartButtonState();
        }

        private void UpdateStartButtonState()
        {
            if (StartGameButton == null) return; 

            bool difficultySelected = DifficultyComboBox?.SelectedItem != null;
            bool themeSelected = ThemeComboBox?.SelectedItem != null;

            StartGameButton.IsEnabled = difficultySelected && themeSelected;
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DifficultyComboBox.SelectedItem is ComboBoxItem difficultyItem && difficultyItem.Tag != null)
                {
                    SelectedDifficulty = int.Parse(difficultyItem.Tag.ToString()!);
                }
                else
                {
                    MessageBox.Show("Molimo odaberite težinu igre!", "Upozorenje",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ThemeComboBox.SelectedItem is ComboBoxItem themeItem && themeItem.Tag != null)
                {
                    SelectedTheme = themeItem.Tag.ToString()!;
                }
                else
                {
                    MessageBox.Show("Molimo odaberite temu igre!", "Upozorenje",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}