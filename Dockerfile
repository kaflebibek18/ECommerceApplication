FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ECommerce/ECommerce.csproj ECommerce/
RUN dotnet restore ECommerce/ECommerce.csproj

COPY . .
RUN dotnet publish ECommerce/ECommerce.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_RUNNING_IN_CONTAINER=true

# Railway provides the PORT env var at runtime; bind Kestrel to it.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet ECommerce.dll"]
