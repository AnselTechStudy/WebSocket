using AspNetCoreWebSocket.Models;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 註冊DI WebSocketHandler 處理公開聊天室服務
builder.Services.AddSingleton<WebSocketHandler>();
// 註冊DI WebSocketAdmin 處理私人聊天室服務
builder.Services.AddSingleton<WebSocketAdmin>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 預設情況下 ASP.NET 6 的 WebSocket 連線不會有特定的閒置時間限制，也就是不會自動斷線。
// 網路中斷不會自動結束連線，直到主動由客戶端或伺服器端進行關閉操作。
// 如果你希望在伺服器端設定 WebSocket 的閒置時間限制，
// 可以使用 WebSocketOptions 的 KeepAliveInterval 屬性來指定閒置時間，這邊是設定閒置2分鐘會斷開連線。
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket ws = await context.WebSockets.AcceptWebSocketAsync())
            {
                // 在Configure使用DI註冊的 WebSocketHandler 服務
                var wsHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                await wsHandler.ProcessWebSocket(ws);
            }
        }
        else
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
    }
    else if (context.Request.Path == "/pws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket ws = await context.WebSockets.AcceptWebSocketAsync())
            {
                // 在Configure使用DI註冊的 WebSocketAdmin 服務
                var webSocketAdmin = context.RequestServices.GetRequiredService<WebSocketAdmin>();
                await webSocketAdmin.WSocketAdminHandler(ws, webSocketAdmin);
            }
        }
        else
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
    }
    else await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
