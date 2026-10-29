# EventosApp

Laboratorio Semana 11 — ASP.NET Core MVC con EF Core Code First.

## Estructura

```
EventosApp/
├── scripts/comandos.txt
├── capturas/
└── Eventos.Web/
    ├── Models/          Categoria · Evento · Asistente · Inscripcion
    ├── Data/            EventosContext
    ├── Migrations/
    ├── Repositorios/    Evento · Asistente · Inscripcion
    ├── Controllers/     Eventos · Asistentes · Inscripciones
    └── Views/
```

## Cómo ejecutarlo

```bash
cd Eventos.Web
dotnet user-secrets set "ConnectionStrings:Eventos" "Server=localhost,1433;Database=EventosDB;User Id=sa;Password=tu_password;TrustServerCertificate=True"
dotnet ef database update
dotnet run          # http://localhost:5090
```

## Migraciones

| # | Nombre | Cambio |
|---|--------|--------|
| 1 | `InitialCreate` | Tablas Categorias, Eventos, Asistentes e Inscripciones |
| 2 | `AgregarModalidadEvento` | `Eventos.Modalidad` (defaultValue `"Presencial"`) |
| 3 | `AgregarTelefonoAsistente` | `Asistentes.Telefono` (opcional, máx. 9) |
