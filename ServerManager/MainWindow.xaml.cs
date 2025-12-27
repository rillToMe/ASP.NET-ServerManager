using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ServerManager
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ServerProject> projects = new();
        private ServerProject? selectedProject;
        private Process? runningProcess;
        private readonly string configPath;
        private readonly string pidPath;
        private string? serverUrl;

        public MainWindow()
        {
            InitializeComponent();
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ServerManager"
            );
            configPath = Path.Combine(appDataPath, "projects.json");
            pidPath = Path.Combine(appDataPath, "running_processes.json");
            LoadProjects();
            RestoreRunningProcesses();
            ServerListBox.ItemsSource = projects;
        }

        private void LoadProjects()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<ServerProject>>(json);
                    if (loaded != null)
                    {
                        projects = loaded;
                        foreach (var p in projects) p.Status = "Stopped";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProjects()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
                var json = JsonSerializer.Serialize(projects, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving projects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreRunningProcesses()
        {
            try
            {
                if (!File.Exists(pidPath)) return;

                var json = File.ReadAllText(pidPath);
                var runningProcs = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

                if (runningProcs == null) return;

                foreach (var kvp in runningProcs)
                {
                    var project = projects.FirstOrDefault(p => p.Id == kvp.Key);
                    if (project == null) continue;

                    try
                    {
                        var process = Process.GetProcessById(kvp.Value);
                        if (process != null && !process.HasExited)
                        {
                            project.Status = "Running";
                            project.ProcessId = kvp.Value;
                            AppendLog($"[RESTORED] Found running server: {project.Name} (PID: {kvp.Value})");
                        }
                        else
                        {
                            project.Status = "Stopped";
                            project.ProcessId = null;
                        }
                    }
                    catch
                    {
                        // Process not found or already exited
                        project.Status = "Stopped";
                        project.ProcessId = null;
                    }
                }

                ServerListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                AppendLog($"Error restoring processes: {ex.Message}");
            }
        }

        private void SaveRunningProcesses()
        {
            try
            {
                var runningProcs = new Dictionary<string, int>();

                foreach (var project in projects.Where(p => p.Status == "Running"))
                {
                    if (project.ProcessId.HasValue)
                    {
                        runningProcs[project.Id] = project.ProcessId.Value;
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(pidPath)!);
                var json = JsonSerializer.Serialize(runningProcs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(pidPath, json);
            }
            catch (Exception ex)
            {
                AppendLog($"Error saving process info: {ex.Message}");
            }
        }

        private void AddServerBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Project Folder (any file in the folder)",
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var folderPath = Path.GetDirectoryName(dialog.FileName)!;
                var folderName = Path.GetFileName(folderPath);

                var newProject = new ServerProject
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = folderName,
                    Path = folderPath,
                    Status = "Stopped"
                };

                projects.Add(newProject);
                SaveProjects();
                ServerListBox.SelectedItem = newProject;
            }
        }

        private void ServerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerListBox.SelectedItem is ServerProject project)
            {
                selectedProject = project;
                ProjectNameBox.Text = project.Name;
                ProjectPathBox.Text = project.Path;

                DetailsPanel.Visibility = Visibility.Visible;
                EmptyState.Visibility = Visibility.Collapsed;

                // If project was restored and is running, set default URL
                if (project.Status == "Running" && string.IsNullOrEmpty(serverUrl))
                {
                    serverUrl = project.ServerUrl ?? "http://localhost:5000";
                }

                UpdateButtonStates();
            }
            else
            {
                DetailsPanel.Visibility = Visibility.Collapsed;
                EmptyState.Visibility = Visibility.Visible;
            }
        }

        private void SaveNameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject != null)
            {
                selectedProject.Name = ProjectNameBox.Text;
                SaveProjects();
                ServerListBox.Items.Refresh();
                MessageBox.Show("Project name updated!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditPathBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select New Project Folder (any file in the folder)",
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = selectedProject.Path
            };

            if (dialog.ShowDialog() == true)
            {
                var newPath = Path.GetDirectoryName(dialog.FileName)!;
                selectedProject.Path = newPath;
                ProjectPathBox.Text = newPath;
                SaveProjects();
                ServerListBox.Items.Refresh();
                MessageBox.Show("Project path updated!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{selectedProject.Name}' from the list?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    if (selectedProject.Status == "Running")
                    {
                        StopServer();
                    }
                    projects.Remove(selectedProject);
                    SaveProjects();
                    selectedProject = null;
                    ServerListBox.SelectedItem = null;
                }
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject == null) return;

            if (!Directory.Exists(selectedProject.Path))
            {
                MessageBox.Show("Project path does not exist!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                LogsBox.Clear();
                AppendLog($"Starting server for: {selectedProject.Name}");
                AppendLog($"Path: {selectedProject.Path}");
                AppendLog("Running command: dotnet run\n");

                runningProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run",
                        WorkingDirectory = selectedProject.Path,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                runningProcess.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AppendLog(args.Data);
                            // Detect server URL from output
                            if (args.Data.Contains("http://") || args.Data.Contains("https://"))
                            {
                                var urlMatch = System.Text.RegularExpressions.Regex.Match(
                                    args.Data, @"(https?://[^\s]+)");
                                if (urlMatch.Success && string.IsNullOrEmpty(serverUrl))
                                {
                                    serverUrl = urlMatch.Value;
                                    selectedProject.ServerUrl = serverUrl;
                                    SaveProjects();
                                    VisitWebBtn.IsEnabled = true;
                                    AppendLog($"\n✓ Server URL detected: {serverUrl}");
                                }
                            }
                        });
                    }
                };

                runningProcess.ErrorDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        Dispatcher.Invoke(() => AppendLog($"ERROR: {args.Data}"));
                    }
                };

                runningProcess.Start();
                runningProcess.BeginOutputReadLine();
                runningProcess.BeginErrorReadLine();

                selectedProject.Status = "Running";
                selectedProject.ProcessId = runningProcess.Id;
                SaveRunningProcesses();
                UpdateButtonStates();
                ServerListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AppendLog($"\nERROR: {ex.Message}");
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
        }

        private void VisitWebBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(serverUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = serverUrl,
                        UseShellExecute = true
                    });
                    AppendLog($"Opening browser: {serverUrl}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening browser: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Server URL not detected yet. Check the console output.",
                    "No URL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopServer()
        {
            if (selectedProject == null) return;

            try
            {
                // If we have a direct process reference, use it
                if (runningProcess != null && !runningProcess.HasExited)
                {
                    AppendLog("\n=== Stopping server ===");
                    runningProcess.Kill(true);
                    runningProcess.WaitForExit(5000);
                    runningProcess.Dispose();
                    runningProcess = null;
                    AppendLog("Server stopped successfully.\n");
                }
                // If not, try to stop using saved PID (for restored processes)
                else if (selectedProject.ProcessId.HasValue)
                {
                    AppendLog("\n=== Stopping server (using PID) ===");
                    var process = Process.GetProcessById(selectedProject.ProcessId.Value);
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                        process.WaitForExit(5000);
                        process.Dispose();
                        AppendLog("Server stopped successfully.\n");
                    }
                }

                serverUrl = null;
                selectedProject.Status = "Stopped";
                selectedProject.ProcessId = null;
                selectedProject.ServerUrl = null;
                SaveProjects();
                SaveRunningProcesses();
                UpdateButtonStates();
                ServerListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                AppendLog($"Error stopping server: {ex.Message}");
                MessageBox.Show($"Error stopping server: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButtonStates()
        {
            if (selectedProject != null)
            {
                bool isRunning = selectedProject.Status == "Running";
                RunBtn.IsEnabled = !isRunning;
                StopBtn.IsEnabled = isRunning;
                VisitWebBtn.IsEnabled = isRunning && !string.IsNullOrEmpty(serverUrl);
            }
        }

        private void AppendLog(string message)
        {
            LogsBox.AppendText($"{message}\n");
            LogScrollViewer.ScrollToBottom();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var runningProjects = projects.Where(p => p.Status == "Running").ToList();

            if (runningProjects.Any())
            {
                var result = MessageBox.Show(
                    $"There are {runningProjects.Count} server(s) still running.\n\n" +
                    "Choose an option:\n" +
                    "• Yes - Stop all servers and exit\n" +
                    "• No - Leave servers running in background\n" +
                    "• Cancel - Don't exit",
                    "Servers Running",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Stop all running servers
                    foreach (var project in runningProjects)
                    {
                        if (project.ProcessId.HasValue)
                        {
                            try
                            {
                                var proc = Process.GetProcessById(project.ProcessId.Value);
                                if (!proc.HasExited)
                                {
                                    proc.Kill(true);
                                    proc.WaitForExit(3000);
                                }
                            }
                            catch { }
                        }
                        project.Status = "Stopped";
                        project.ProcessId = null;
                    }
                    SaveRunningProcesses();
                }
                else if (result == MessageBoxResult.No)
                {
                    // Save PIDs so we can track them next time
                    SaveRunningProcesses();
                    AppendLog("\n[INFO] Servers left running in background. They will be tracked on next launch.");
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnClosing(e);
        }
    }

    public class ServerProject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string Status { get; set; } = "Stopped";
        public int? ProcessId { get; set; }
        public string? ServerUrl { get; set; }
    }
}