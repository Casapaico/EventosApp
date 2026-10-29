using System.Globalization;
using Eventos.Web.Data;
using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// La cadena de conexión (Database=EventosDB) viene de appsettings.json
builder.Services.AddDbContext<EventosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Eventos")));

// Repositorios: los controladores dependen de ellos, nunca del DbContext
builder.Services.AddScoped<EventoRepositorio>();
builder.Services.AddScoped<AsistenteRepositorio>();
builder.Services.AddScoped<InscripcionRepositorio>();

var app = builder.Build();

// Datos iniciales: la BD ya existe porque se creó con "dotnet ef database update".
// Cada tabla se llena solo si está vacía, así no se duplica al reiniciar la app.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventosContext>();
    await CargarDatosInicialesAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Formato de números y fechas de Perú (S/ como moneda)
var cultura = new CultureInfo("es-PE");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Eventos}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task CargarDatosInicialesAsync(EventosContext context)
{
    if (!await context.Categorias.AnyAsync())
    {
        context.Categorias.AddRange(
            new Categoria { Nombre = "Tecnología" },
            new Categoria { Nombre = "Música" },
            new Categoria { Nombre = "Emprendimiento" });
        await context.SaveChangesAsync();
    }

    if (!await context.Eventos.AnyAsync())
    {
        var categorias = await context.Categorias.ToDictionaryAsync(c => c.Nombre, c => c.Id);
        context.Eventos.AddRange(
            new Evento { Titulo = "Taller de ASP.NET Core MVC", Fecha = new DateTime(2026, 11, 14, 9, 0, 0), Lugar = "Tecsup - Auditorio", Aforo = 40, Precio = 0m, CategoriaId = categorias["Tecnología"] },
            new Evento { Titulo = "Concierto de rock peruano", Fecha = new DateTime(2026, 12, 5, 20, 0, 0), Lugar = "Parque de la Exposición", Aforo = 1500, Precio = 85.50m, CategoriaId = categorias["Música"] },
            new Evento { Titulo = "Feria de emprendedores de Gamarra", Fecha = new DateTime(2026, 10, 18, 10, 0, 0), Lugar = "Gamarra - La Victoria", Aforo = 300, Precio = 10m, CategoriaId = categorias["Emprendimiento"] },
            new Evento { Titulo = "Charla: IA para pymes", Fecha = new DateTime(2026, 11, 20, 18, 30, 0), Lugar = "Zoom", Modalidad = "Virtual", Aforo = 2, Precio = 25m, CategoriaId = categorias["Tecnología"] });
        await context.SaveChangesAsync();
    }

    if (!await context.Asistentes.AnyAsync())
    {
        context.Asistentes.AddRange(
            new Asistente { Nombres = "Lucía Fernández Rojas", Dni = "71234567", Correo = "lucia.fernandez@correo.pe" },
            new Asistente { Nombres = "Carlos Mendoza Quispe", Dni = "72345678", Correo = "carlos.mendoza@correo.pe" },
            new Asistente { Nombres = "Ana Torres Huamán", Dni = "73456789", Correo = "ana.torres@correo.pe" },
            new Asistente { Nombres = "Jorge Ramírez Salas", Dni = "74567890", Correo = "jorge.ramirez@correo.pe" },
            new Asistente { Nombres = "María Castillo Paredes", Dni = "75678901", Correo = "maria.castillo@correo.pe" });
        await context.SaveChangesAsync();
    }
}
