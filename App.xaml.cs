using System;
using System.Windows;

namespace MemoryGame
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                StartWindow startWindow = new StartWindow();
                bool? result = startWindow.ShowDialog();

                if (result == true)
                {
                    try
                    {
                        MainWindow mainWindow = new MainWindow(startWindow.SelectedDifficulty, startWindow.SelectedTheme);
                        mainWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating MainWindow: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        Shutdown();
                    }
                }
                else
                {
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in startup: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}