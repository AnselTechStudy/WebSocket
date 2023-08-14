using AspNetCoreWebSocket.Models;
using System.Net.WebSockets;

namespace AspNetCoreWebSocket.Middleware
{
    public class WebSocketAdminMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketAdmin _webSocketAdmin;

        public WebSocketAdminMiddleware(RequestDelegate next, WebSocketAdmin webSocketAdmin)
        {
            _next = next;
            _webSocketAdmin = webSocketAdmin;
        }

        public async Task Invoke(HttpContext context)
        {
            // 中介軟體邏輯
            // 可以在這裡處理請求、修改請求或回應的內容
            if (context.Request.Path == "/pws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket ws = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await _webSocketAdmin.WSocketAdminHandler(ws, _webSocketAdmin);
                    }
                }
                else
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            }
            else
                await _next(context);
        }
    }
}
