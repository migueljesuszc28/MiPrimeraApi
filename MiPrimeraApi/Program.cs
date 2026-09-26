using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar servicios para Explorer, Swagger y CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 2. Habilitar Swagger en el entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interfaz visual en /swagger
}

// 3. Middlewares para CORS y archivos estáticos (UI)
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

// Datos iniciales en memoria
List<Estudiante> ObtenerDatosIniciales() => new()
{
    new Estudiante(1, "Ana Gómez", "Ingeniería de Sistemas", "Ana123!"),
    new Estudiante(2, "Carlos López", "Ingeniería de Sistemas", "Carlos123!"),
    new Estudiante(3, "María Rodríguez", "Ingeniería de Software", "Maria123!"),
    new Estudiante(4, "Juan Martínez", "Ingeniería Electrónica", "Juan123!"),
    new Estudiante(5, "Laura Hernández", "Ingeniería Industrial", "Laura123!"),
    new Estudiante(6, "Diego González", "Ingeniería de Sistemas", "Diego123!"),
    new Estudiante(7, "Sofía Pérez", "Ingeniería de Software", "Sofia123!"),
    new Estudiante(8, "Andrés Sánchez", "Ingeniería Civil", "Andres123!"),
    new Estudiante(9, "Valentina Ramírez", "Ingeniería de Sistemas", "Valentina123!"),
    new Estudiante(10, "Mateo Torres", "Ingeniería Mecánica", "Mateo123!")
};

var estudiantes = ObtenerDatosIniciales();

// Utilidad: restablecer datos de prueba
app.MapPost("/api/reset", () =>
{
    estudiantes.Clear();
    estudiantes.AddRange(ObtenerDatosIniciales());
    return Results.Ok(new { mensaje = "Base de datos restablecida correctamente.", total = estudiantes.Count });
});

// Utilidad: estado de la API
app.MapGet("/api/status", () => Results.Ok(new 
{ 
    estado = "online", 
    mensaje = "API de Retos HTTP activa", 
    estudiantesRegistrados = estudiantes.Count,
    fecha = DateTime.UtcNow 
}));

// ====================================================================
// LISTA DE RETOS DE CÓDIGOS DE ESTADO HTTP
// ====================================================================

// RETO 1: 200 OK (Exitoso) - Listado general
app.MapGet("/api/estudiantes", () => estudiantes);

// RETO 7: 404 Not Found (o 200 OK si existe) - Búsqueda por ID
app.MapGet("/api/estudiantes/{id}", (int id) => 
{
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);
    return estudiante is not null ? Results.Ok(estudiante) : Results.NotFound("Estudiante no encontrado");
});

// RETO 2: 201 Created | RETO 4: 400 Bad Request | RETO 8: 409 Conflict
app.MapPost("/api/estudiantes", (Estudiante nuevoEstudiante) => 
{
    // RETO 4: 400 Bad Request - Validamos que el nombre no venga vacío
    if (string.IsNullOrWhiteSpace(nuevoEstudiante.Nombre))
        return Results.BadRequest("El nombre es un campo obligatorio.");

    // RETO 8: 409 Conflict - Validamos que el ID no exista previamente
    if (estudiantes.Any(e => e.Id == nuevoEstudiante.Id))
        return Results.Conflict($"Ya existe un estudiante registrado con el ID {nuevoEstudiante.Id}.");

    estudiantes.Add(nuevoEstudiante);
    return Results.Created($"/api/estudiantes/{nuevoEstudiante.Id}", nuevoEstudiante);
});

// RETO 3: 204 No Content (Exitoso - Cuerpo vacío)
app.MapDelete("/api/estudiantes/{id}", (int id) => 
{
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);
    if (estudiante is null) return Results.NotFound();
    
    estudiantes.Remove(estudiante);
    return Results.NoContent();
});

// Actualizar un estudiante (PUT) - 200 OK / 404 Not Found
app.MapPut("/api/estudiantes/{id}", (int id, Estudiante estudianteActualizado) =>
{
    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);
    if (estudiante is null) return Results.NotFound("Estudiante no encontrado");

    var indice = estudiantes.IndexOf(estudiante);
    estudiantes[indice] = estudianteActualizado with { Id = id };

    return Results.Ok(estudiantes[indice]);
});

// RETO 5: 401 Unauthorized (Error de Autenticación)
app.MapGet("/api/recurso-protegido", (HttpRequest request) =>
{
    if (!request.Headers.ContainsKey("Authorization"))
        return Results.Unauthorized(); // Falta el encabezado Authorization

    return Results.Ok(new { mensaje = "Acceso concedido al recurso protegido.", tokenRecibido = request.Headers["Authorization"].ToString() });
});

// RETO 6: 403 Forbidden (Error de Autorización/Roles)
app.MapDelete("/api/admin/estudiantes/{id}", (int id, HttpRequest request) =>
{
    // Simulamos que el cliente envía su rol en un encabezado llamado "Rol"
    var rol = request.Headers["Rol"].ToString();

    if (string.Equals(rol, "Estudiante", StringComparison.OrdinalIgnoreCase))
        return Results.StatusCode(403); // El estudiante está autenticado pero no tiene permisos para borrar

    var estudiante = estudiantes.FirstOrDefault(e => e.Id == id);
    if (estudiante is null) return Results.NotFound("Estudiante no encontrado para eliminar");
    
    estudiantes.Remove(estudiante);
    return Results.NoContent();
});

// RETO 9: 422 Unprocessable Entity (Error de Reglas de Negocio)
app.MapPost("/api/estudiantes/{id}/notas", (int id, Calificacion calificacion) =>
{
    // La estructura del JSON es correcta sintácticamente, pero viola una regla de negocio
    if (calificacion.Nota < 0.0 || calificacion.Nota > 5.0)
        return Results.UnprocessableEntity(new 
        { 
            error = "Entidad no procesable", 
            detalle = "Error de regla de negocio: La nota debe estar en un rango estricto de 0.0 a 5.0.",
            notaRecibida = calificacion.Nota
        });

    return Results.Ok(new 
    { 
        mensaje = $"Nota {calificacion.Nota} guardada exitosamente para el estudiante {id}.",
        estudianteId = id,
        nota = calificacion.Nota
    });
});

// RETO 10: 500 Internal Server Error (Fallo no controlado)
app.MapGet("/api/error-interno", () =>
{
    // Forzamos una división por cero para que la aplicación lance una excepción no controlada
    int divisor = 0;
    int resultado = 10 / divisor; 
    
    return Results.Ok(resultado);
});

// Ruta de respaldo para la SPA
app.MapFallbackToFile("index.html");

app.Run();

// Modelos (Records)
record Estudiante(int Id, string Nombre, string Carrera, string Contraseña);
record Calificacion(double Nota);