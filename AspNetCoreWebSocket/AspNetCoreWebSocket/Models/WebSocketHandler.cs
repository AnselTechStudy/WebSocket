using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace AspNetCoreWebSocket.Models
{
    public class WebSocketHandler
    {

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            this.logger = logger;
        }
        
        ConcurrentDictionary<int, WebSocket> WebSockets = new ConcurrentDictionary<int, WebSocket>();
        private readonly ILogger<WebSocketHandler> logger;

        public async Task ProcessWebSocket(WebSocket webSocket)
        {
            WebSockets.TryAdd(webSocket.GetHashCode(), webSocket);
            // 創建一個用於接收 WebSocket 消息的緩衝區。
            // 如果預計接收的消息大小可能超過 1024 * 4 個字節，
            // 則需要相應地增加緩衝區的大小以確保可以完整接收消息。
            var buffer = new byte[1024 * 4];
            var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var userName = "anonymous";
            // 確認連線
            while (!res.CloseStatus.HasValue)
            {
                var cmd = Encoding.UTF8.GetString(buffer, 0, res.Count);
                if (!string.IsNullOrEmpty(cmd))
                {
                    logger.LogInformation(cmd);
                    if (cmd.StartsWith("/USER "))
                        userName = cmd.Substring(6);
                    else
                        Broadcast($"{userName}:\t{cmd}");
                }
                res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            // 連線段開
            await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            WebSockets.TryRemove(webSocket.GetHashCode(), out var removed);
            Broadcast($"{userName} left the room.");
        }

        public void Broadcast(string message)
        {
            var buff = Encoding.UTF8.GetBytes($"{DateTime.Now:MM-dd HH:mm:ss}\t{message}");
            var data = new ArraySegment<byte>(buff, 0, buff.Length);
            Parallel.ForEach(WebSockets.Values, async (webSocket) =>
            {
                if (webSocket.State == WebSocketState.Open)
                    await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            });
        }
    }
}
