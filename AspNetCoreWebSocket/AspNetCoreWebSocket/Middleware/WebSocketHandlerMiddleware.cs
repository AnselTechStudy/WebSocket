using AspNetCoreWebSocket.Models;
using System.Net.WebSockets;

namespace AspNetCoreWebSocket.Middleware
{
    public class WebSocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketHandler _handler;

        public WebSocketHandlerMiddleware(RequestDelegate next, WebSocketHandler handler)
        {
            _next = next;
            _handler = handler;
        }

        public async Task Invoke(HttpContext context)
        {
            // 中介軟體邏輯
            // 可以在這裡處理請求、修改請求或回應的內容
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket ws = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await _handler.ProcessWebSocket(ws);
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
