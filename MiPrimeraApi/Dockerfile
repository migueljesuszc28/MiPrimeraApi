# 1. SDK de .NET 10 para compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar la publicación
COPY . ./
RUN dotnet publish -c Release -o /app/out

# 2. Runtime de .NET 10 para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Configurar el puerto para Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Ejecutar la API
ENTRYPOINT ["dotnet", "MiPrimeraApi.dll"]
