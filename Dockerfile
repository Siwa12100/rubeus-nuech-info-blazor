# Étape de base pour exécuter l'application avec .NET 9
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5010
RUN useradd -m appuser

# Étape de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copie du fichier projet principal et restauration des dépendances
COPY NuitInfo.Rubeus.csproj ./
RUN dotnet restore NuitInfo.Rubeus.csproj

# Copie du reste du projet (sauf les dossiers exclus)
COPY . ./
RUN rm -rf Tests/bin Tests/obj

# Build du projet principal
WORKDIR /src
RUN dotnet build NuitInfo.Rubeus.csproj -c $BUILD_CONFIGURATION -o /app/build

# Étape de publication
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet publish NuitInfo.Rubeus.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Étape finale : exécution avec un utilisateur non-root
FROM base AS final
WORKDIR /app

# Copie des fichiers publiés
COPY --from=publish /app/publish .

# Configuration des permissions
RUN chown -R appuser:appuser /app
USER appuser

# Point d'entrée
ENTRYPOINT ["dotnet", "NuitInfo.Rubeus.dll"]
