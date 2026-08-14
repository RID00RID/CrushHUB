FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CrushHUB/CrushHUB.csproj CrushHUB/
RUN dotnet restore CrushHUB/CrushHUB.csproj

COPY CrushHUB/ CrushHUB/
RUN dotnet publish CrushHUB/CrushHUB.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

# Скриншоты игроков кладутся сюда, том монтируется поверх.
RUN mkdir -p wwwroot/uploads

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CrushHUB.dll"]
