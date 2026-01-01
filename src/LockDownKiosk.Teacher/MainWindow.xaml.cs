using System;
using System.Windows;
using LockDownKiosk.Shared;
using LockDownKiosk.Shared.Networking;

namespace LockDownKiosk.Teacher
{
    public partial class MainWindow : Window
    {
        private const string StudentHost = "127.0.0.1";
        private const int StudentPort = 5050;

        public MainWindow()
        {
            InitializeComponent();
            AppendStatus("Teacher UI started.");
        }

        private async void StartSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var msg = new AppMessage
                {
                    Type = MessageType.StartSession,
                    Sender = "TeacherConsole",
                    Content = "Start lockdown session",
                    TimestampUtc = DateTime.UtcNow
                };

                var resp = await TcpJsonMessenger.SendAppMessageAsync(StudentHost, StudentPort, msg);

                StatusText.Text = "Status: StartSession sent";
                AppendStatus($"StartSession sent. Response: {resp ?? "(none)"}");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Status: StartSession failed";
                AppendStatus($"StartSession failed: {ex.Message}");
            }
        }

        private async void EndSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var msg = new AppMessage
                {
                    Type = MessageType.EndSession,
                    Sender = "TeacherConsole",
                    Content = "End lockdown session",
                    TimestampUtc = DateTime.UtcNow
                };

                var resp = await TcpJsonMessenger.SendAppMessageAsync(StudentHost, StudentPort, msg);

                StatusText.Text = "Status: EndSession sent";
                AppendStatus($"EndSession sent. Response: {resp ?? "(none)"}");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Status: EndSession failed";
                AppendStatus($"EndSession failed: {ex.Message}");
            }
        }

        private void AppendStatus(string msg)
        {
            try
            {
                StatusTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
                StatusTextBox.ScrollToEnd();
            }
            catch { }
        }
    }
}
