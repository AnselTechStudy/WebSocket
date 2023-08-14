using AspNetCoreWebSocket.Models;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ���UDI WebSocketHandler �B�z���}��ѫǪA��
builder.Services.AddSingleton<WebSocketHandler>();
// ���UDI WebSocketAdmin �B�z�p�H��ѫǪA��
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

// �w�]���p�U ASP.NET 6 �� WebSocket �s�u���|���S�w�����m�ɶ�����A�]�N�O���|�۰��_�u�C
// �������_���|�۰ʵ����s�u�A����D�ʥѫȤ�ݩΦ��A���ݶi�������ާ@�C
// �p�G�A�Ʊ�b���A���ݳ]�w WebSocket �����m�ɶ�����A
// �i�H�ϥ� WebSocketOptions �� KeepAliveInterval �ݩʨӫ��w���m�ɶ��A�o��O�]�w���m2�����|�_�}�s�u�C
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
                // �bConfigure�ϥ�DI���U�� WebSocketHandler �A��
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
                // �bConfigure�ϥ�DI���U�� WebSocketAdmin �A��
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
