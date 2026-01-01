using System;
using System.Text.Json;
using System.Windows;
using LockDownKiosk.Shared;
using LockDownKiosk.Student.Networking;

namespace LockDownKiosk.Student
{
    public partial class MainWindow : Window
    {
        private StudentCommandServer? _server;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start listening immediately so Teacher can connect
            _server = new StudentCommandServer(port: 5050);
            _server.Log += OnServerLog;
            _server.SessionActiveChanged += OnSessionActiveChanged;

            _server.Start();

            SetStatus("Status: Connected (listener active)");
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _server?.Stop();
                _server?.Dispose();
            }
            catch { /* ignore */ }
        }

        private void OnServerLog(string text)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog(text);
                AppendStatusOutput(text);
            });
        }

        private void OnSessionActiveChanged(bool isActive)
        {
            Dispatcher.Invoke(() =>
            {
                if (isActive)
                    SetStatus("Status: Session ACTIVE (Lockdown)");
                else
                    SetStatus("Status: Session INACTIVE");
            });
        }

        private void SendHello_Click(object sender, RoutedEventArgs e)
        {
            // This button is just local UI feedback for now.
            // Teacher initiated commands arrive via the server listener.
            AppendLog("HELLO button pressed.");
            AppendStatusOutput("HELLO button pressed.");
        }

        private void SetStatus(string text)
        {
            StatusText.Text = text;
        }

        private void AppendLog(string text)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {text}";
            LogList.Items.Add(line);
            LogList.ScrollIntoView(line);
        }

        private void AppendStatusOutput(string text)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {text}";
            StatusOutputBox.AppendText(line + Environment.NewLine);
            StatusOutputBox.ScrollToEnd();
        }
    }
}
