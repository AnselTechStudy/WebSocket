using AspNetCoreWebSocket.Middleware;
using AspNetCoreWebSocket.Models;

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

// �[�J�ۭq�� WebSocket Middleware
app.UseMiddleware<WebSocketHandlerMiddleware>();
app.UseMiddleware<WebSocketAdminMiddleware>();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
