# Build: docker build -f dockerfiles/Dockerfile -t voz-e-via-dotnet .
# Execução: docker run -p 8080:8080 -e JWT_SECRET=<chave com 32+ caracteres> voz-e-via-dotnet

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CaminhoLivre.sln ./
COPY src/CaminhoLivre.Api/CaminhoLivre.Api.csproj src/CaminhoLivre.Api/
COPY tests/CaminhoLivre.Tests/CaminhoLivre.Tests.csproj tests/CaminhoLivre.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/CaminhoLivre.Api/CaminhoLivre.Api.csproj --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Pastas graváveis para o SQLite e para as fotos; a aplicação roda sem privilégio de root
RUN mkdir -p /app/data /app/uploads && chown -R app:app /app/data /app/uploads
USER app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Sqlite="Data Source=/app/data/vozevia.db" \
    Uploads__Pasta=/app/uploads

EXPOSE 8080
ENTRYPOINT ["dotnet", "CaminhoLivre.Api.dll"]
