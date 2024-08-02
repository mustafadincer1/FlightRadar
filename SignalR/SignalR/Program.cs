using SignalR.DataTransfer;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekleyin
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy=>
        policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(origin=> true)
));
builder.Services.AddHostedService<TcpDataReceiver>();

var app = builder.Build();

// Geli�tirme ortam� i�in �zel yap�land�rmalar
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<MyHub>("/myhub");
});

app.Run();
        