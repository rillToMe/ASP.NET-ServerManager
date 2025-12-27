using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ServerManager
{
    public partial class AboutWindow : Window
    {
        private bool _licenseOpen = false;

        public AboutWindow()
        {
            InitializeComponent();

            CopyrightText.Text =
                $"© {DateTime.Now.Year} DitDev. All rights reserved.";

            LoadLicense();
        }

        private void OpenPortfolio(object sender, RoutedEventArgs e)
            => OpenUrl("https://ditdev.vercel.app/");

        private void OpenGitHub(object sender, RoutedEventArgs e)
            => OpenUrl("https://github.com/rillToMe");

        private void OpenInstagram(object sender, RoutedEventArgs e)
            => OpenUrl("https://www.instagram.com/rill_lyrics/");

        private void OpenTikTok(object sender, RoutedEventArgs e)
            => OpenUrl("https://www.tiktok.com/@goodvibes_music28");

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening link: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLicense()
        {
            try
            {
                var path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets",
                    "LICENSE.txt"
                );

                LicenseText.Text = File.Exists(path)
                    ? File.ReadAllText(path)
                    : "LICENSE file not found.";
            }
            catch (Exception ex)
            {
                LicenseText.Text = $"Failed to load license: {ex.Message}";
            }
        }

        private void ToggleLicense(object sender, RoutedEventArgs e)
        {
            if (_licenseOpen)
                CloseLicense();
            else
                OpenLicense();

            _licenseOpen = !_licenseOpen;
        }

        private void OpenLicense()
        {
            LicenseContainer.Visibility = Visibility.Visible;

            var sb = new Storyboard();

            var fadeIn = new DoubleAnimation(0, 1,
                TimeSpan.FromMilliseconds(220));

            Storyboard.SetTargetProperty(
                fadeIn, new PropertyPath(Border.OpacityProperty));

            var scaleAnim = new DoubleAnimation
            {
                From = 0.95,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            Storyboard.SetTargetProperty(
                scaleAnim,
                new PropertyPath("RenderTransform.ScaleY"));

            sb.Children.Add(fadeIn);
            sb.Children.Add(scaleAnim);

            Storyboard.SetTarget(sb, LicenseContainer);
            sb.Begin();
        }

        private void CloseLicense()
        {
            var sb = new Storyboard();

            var fadeOut = new DoubleAnimation(1, 0,
                TimeSpan.FromMilliseconds(180));

            Storyboard.SetTargetProperty(
                fadeOut, new PropertyPath(Border.OpacityProperty));

            sb.Children.Add(fadeOut);

            sb.Completed += (_, __) =>
            {
                LicenseContainer.Visibility = Visibility.Collapsed;
            };

            Storyboard.SetTarget(sb, LicenseContainer);
            sb.Begin();
        }
    }
}
