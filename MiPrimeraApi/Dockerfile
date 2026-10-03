# 1. Imagen base del SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar la publicación
COPY . ./
RUN dotnet publish -c Release -o /app/out

# 2. Imagen base ligera de ejecución (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configurar el puerto para que Render pueda comunicarse
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Iniciar la aplicación (reemplaza 'MiProyecto' por el nombre real de tu archivo .csproj)
ENTRYPOINT ["dotnet", "MiPrimeraApi.dll"]