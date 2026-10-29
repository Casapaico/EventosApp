# EventosApp — Eventos e inscripciones con ASP.NET Core MVC y EF Core (Code First)

Laboratorio de la **Semana 11** del curso *Desarrollo de Aplicaciones Empresariales Avanzadas* (Tecsup).

Aplicación web para registrar eventos, asistentes e inscripciones. A diferencia de la semana 10 (Database First),
aquí no hay ningún script SQL: la base `EventosDB` se genera desde las clases C# con **migraciones**.

**Autor:** Alex Casapaico

## Tecnologías

- .NET 10 · ASP.NET Core MVC
- Entity Framework Core 10 (`SqlServer`, `Design` y `Tools`)
- SQL Server 2022
- VS Code con C# Dev Kit y la extensión SQL Server (mssql)

## Estructura

```
EventosApp/
├── scripts/comandos.txt            comandos usados en el laboratorio
├── capturas/                       evidencias (VS Code, SQL Server y vistas)
└── Eventos.Web/
    ├── Models/                     Categoria · Evento · Asistente · Inscripcion (solo Data Annotations)
    ├── Data/EventosContext.cs      un DbSet por entidad, sin Fluent API
    ├── Migrations/                 InitialCreate · AgregarModalidadEvento · AgregarTelefonoAsistente
    ├── Repositorios/               Evento · Asistente · Inscripcion (todas las consultas LINQ)
    ├── Controllers/                Eventos · Asistentes · Inscripciones
    └── Views/
```

## Cómo ejecutarlo

```bash
cd Eventos.Web
dotnet user-secrets set "ConnectionStrings:Eventos" "Server=localhost,1433;Database=EventosDB;User Id=sa;Password=tu_password;TrustServerCertificate=True"
dotnet ef database update          # crea EventosDB aplicando las 3 migraciones
dotnet run                         # http://localhost:5090
```

Al iniciar, `Program.cs` carga 3 categorías, 4 eventos y 5 asistentes solo si las tablas están vacías.
En VS Code también se puede presionar **F5**.

## Migraciones

| # | Nombre | Qué cambia |
|---|--------|------------|
| 1 | `InitialCreate` | Crea las 4 tablas, sus FK y los índices únicos (Nombre, Dni, Correo) |
| 2 | `AgregarModalidadEvento` | Agrega `Eventos.Modalidad`; edité el `defaultValue` a `"Presencial"` antes de aplicarla |
| 3 | `AgregarTelefonoAsistente` | Agrega `Asistentes.Telefono` (opcional, máx. 9) |

## Reglas implementadas

- **Eliminación lógica**: eliminar un evento pone `Activo = 0`; los listados solo muestran activos.
- **Asistentes**: DNI y correo únicos, validados antes de guardar con `ModelState.AddModelError`.
- **Inscripciones**: no se puede inscribir en un evento que ya pasó, ni dos veces al mismo asistente (Confirmada),
  ni si el aforo está completo. Cancelar cambia el estado a `Cancelada`, no borra la fila.
- Todo es `async`, las lecturas usan `AsNoTracking()` y no hay SQL escrito a mano.

## Observaciones

- Con Code First el modelo C# manda: cualquier cambio en una clase se vuelve una migración nueva, y la carpeta
  `Migrations` termina siendo el historial de la base.
- La migración 2 se generó con `defaultValue: ""`. Si la aplicaba así, los eventos que ya existían quedaban
  con Modalidad vacía; por eso se cambió a `"Presencial"` antes del `database update`.
- Los índices únicos protegen la base, pero si solo confiaba en ellos el usuario veía una excepción. Por eso
  primero consulto con `AnyAsync` y muestro el error en el campo.
- Para contar inscritos usé `Count` dentro del `Select`; EF lo traduce a una subconsulta y no trae las
  inscripciones a memoria.

## Conclusiones

- EF Core Code First permite crear y evolucionar la base sin escribir SQL, y deja trazabilidad en
  `__EFMigrationsHistory`.
- Las migraciones no se borran ni se editan después de aplicarlas; solo se revisa el archivo generado antes
  del `update` cuando hay datos que proteger.
- Separar el acceso a datos en repositorios dejó los controladores cortos: solo validan, llaman al repositorio
  y redirigen (Post/Redirect/Get).
