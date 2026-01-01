using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LockDownKiosk.Shared.Networking
{
    public static class TcpJsonMessenger
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false
        };

        // Sends ONE newline-delimited JSON line to the target and returns the response line (if any).
        public static async Task<string?> SendAppMessageAsync(
            string host,
            int port,
            LockDownKiosk.Shared.AppMessage message,
            int timeoutMs = 2500)
        {
            using var cts = new CancellationTokenSource(timeoutMs);

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cts.Token);
            client.NoDelay = true;

            using NetworkStream ns = client.GetStream();

            // Send one JSON line
            var json = JsonSerializer.Serialize(message, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");
            await ns.WriteAsync(bytes, 0, bytes.Length, cts.Token);
            await ns.FlushAsync(cts.Token);

            // Read one response line (optional)
            var buffer = new byte[4096];
            var sb = new StringBuilder();

            while (!cts.IsCancellationRequested)
            {
                int read = await ns.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (read <= 0) break;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, read));

                // We only expect one line
                if (sb.ToString().Contains("\n")) break;
            }

            var response = sb.ToString().Trim();
            return string.IsNullOrWhiteSpace(response) ? null : response;
        }
    }
}
