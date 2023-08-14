using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace AspNetCoreWebSocket.Models
{
    public class WebSocketAdmin
    {
        public WebSocketAdmin(ILogger<WebSocketHandler> logger)
        {
            this._logger = logger;
        }

        private readonly ILogger<WebSocketHandler> _logger;
        private ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();

        public bool CheckWebSockets(string userName)
        {
            return _webSockets.ContainsKey(userName);
        }

        public bool AddWebSocket(string userName, WebSocket webSocket)
        {
            if (CheckWebSockets(userName))
            {
                return false;
            }
            else
            {
                _webSockets.TryAdd(userName, webSocket);
                return true;
            }
        }

        public void RemoveWebSocket(string userName)
        {
            _webSockets.TryRemove(userName, out _);
        }

        public WebSocket GetWebSocket(string userName)
        {
            _webSockets.TryGetValue(userName, out var webSocket);
            return webSocket;
        }

        public async Task WSocketAdminHandler(WebSocket webSocket, WebSocketAdmin webSocketAdmin)
        {
            string userName = String.Empty;
            string receiver = String.Empty;

            // 創建一個用於接收 WebSocket 消息的緩衝區。
            // 如果預計接收的消息大小可能超過 1024 個字節，
            // 則需要相應地增加緩衝區的大小以確保可以完整接收消息。
            var buffer = new byte[1024];
            WebSocketReceiveResult result;

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (!string.IsNullOrEmpty(message))
                    {
                        _logger.LogInformation(message);
                        if (message.StartsWith("/USER "))
                            userName = SetupUser(message, webSocket, webSocketAdmin);

                        else if (message.StartsWith("/RECEIVER "))
                            receiver = SetupReceiver(message, webSocket, webSocketAdmin);

                        else if (message.StartsWith("/Close "))
                            webSocketAdmin.RemoveWebSocket(userName);

                        else if (userName != String.Empty && receiver != String.Empty)
                            SendMessage($"{userName}:\t{message}", webSocket, receiver, webSocketAdmin);
                    }

                }
            } while (!result.CloseStatus.HasValue);

            // 移除關閉的 WebSocket 連線
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            webSocketAdmin.RemoveWebSocket(userName);
        }

        private string SetupUser(string message, WebSocket webSocket, WebSocketAdmin webSocketAdmin)
        {
            var userName = message.Substring(6);
            if (webSocketAdmin.AddWebSocket(userName, webSocket))
            {
                return userName;
            }
            else
            {
                // 回傳消息到客戶端
                SendMessage("此使用者名稱已有人使用，請更換", webSocket);
                return String.Empty;
            }
        }

        private string SetupReceiver(string message, WebSocket webSocket, WebSocketAdmin webSocketAdmin)
        {
            var receiver = message.Substring(10);
            if (webSocketAdmin.CheckWebSockets(receiver))
            {
                return receiver;
            }
            else
            {
                // 回傳消息到客戶端
                SendMessage("收訊人不存在，請更改收訊人。", webSocket);
                return String.Empty;
            }
        }

        private async void SendMessage(string msg, WebSocket webSocket)
        {
            var buff = Encoding.UTF8.GetBytes(msg);
            await webSocket.SendAsync(new ArraySegment<byte>(buff, 0, buff.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void SendMessage(string msg, WebSocket webSocket, string receiver, WebSocketAdmin webSocketAdmin)
        {
            SendMessage(msg, webSocket);
            WebSocket receiverWebSocket = webSocketAdmin.GetWebSocket(receiver);
            SendMessage(msg, receiverWebSocket);
        }
    }
}
