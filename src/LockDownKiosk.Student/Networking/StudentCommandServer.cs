using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LockDownKiosk.Shared;

namespace LockDownKiosk.Student.Networking
{
    public sealed class StudentCommandServer : IDisposable
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _runTask;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public bool IsRunning { get; private set; }

        public event Action<string>? Log;
        public event Action<bool>? SessionActiveChanged;

        public StudentCommandServer(int port = 5050)
        {
            _port = port;
        }

        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();

            IsRunning = true;

            Log?.Invoke($"Student server listening on port {_port}.");

            _runTask = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (!IsRunning) return;

            try { _cts?.Cancel(); } catch { }
            try { _listener?.Stop(); } catch { }

            IsRunning = false;
            Log?.Invoke("Student server stopped.");
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            if (_listener == null) return;

            while (!token.IsCancellationRequested)
            {
                TcpClient? client = null;

                try
                {
                    client = await _listener.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
                catch (OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Log?.Invoke($"Server accept error: {ex.Message}");
                    try { client?.Dispose(); } catch { }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                client.NoDelay = true;

                using (client)
                using (NetworkStream ns = client.GetStream())
                using (var reader = new StreamReader(ns, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true))
                using (var writer = new StreamWriter(ns, new UTF8Encoding(false)) { AutoFlush = true })
                {
                    string? line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        await writer.WriteLineAsync("ERROR: Empty message");
                        return;
                    }

                    AppMessage? msg;
                    try
                    {
                        msg = JsonSerializer.Deserialize<AppMessage>(line, _jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        await writer.WriteLineAsync($"ERROR: Invalid JSON ({ex.Message})");
                        return;
                    }

                    if (msg == null)
                    {
                        await writer.WriteLineAsync("ERROR: Null message");
                        return;
                    }

                    Log?.Invoke($"Received: {msg.Type} from {msg.Sender}.");

                    // Normalize: allow Command messages to drive start/end too
                    if (msg.Type == MessageType.Command)
                    {
                        var cmd = (msg.Content ?? "").Trim();
                        if (cmd.Equals("StartSession", StringComparison.OrdinalIgnoreCase))
                            msg.Type = MessageType.StartSession;
                        else if (cmd.Equals("EndSession", StringComparison.OrdinalIgnoreCase))
                            msg.Type = MessageType.EndSession;
                    }

                    switch (msg.Type)
                    {
                        case MessageType.Hello:
                            Log?.Invoke($"HELLO content: {msg.Content}");
                            await writer.WriteLineAsync("OK: HELLO received");
                            break;

                        case MessageType.StartSession:
                            Log?.Invoke("Lockdown session started.");
                            SessionActiveChanged?.Invoke(true);
                            await writer.WriteLineAsync("OK: Session started");
                            break;

                        case MessageType.EndSession:
                            Log?.Invoke("Lockdown session ended.");
                            SessionActiveChanged?.Invoke(false);
                            await writer.WriteLineAsync("OK: Session ended");
                            break;

                        default:
                            await writer.WriteLineAsync($"ERROR: Unsupported message type {msg.Type}");
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Client handler error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
            try { _cts?.Dispose(); } catch { }
        }
    }
}
