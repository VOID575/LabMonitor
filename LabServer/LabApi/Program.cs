using LabApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddAllServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularFrontend");

app.UseAuthorization();
app.MapControllers();
app.MapAllHubs();

app.Run();