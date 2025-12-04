#!/bin/bash

# === 🎨 Couleurs pour l'affichage ===
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# === 📦 Fonction pour afficher les messages ===
info() { echo -e "${BLUE}ℹ️  $1${NC}"; }
success() { echo -e "${GREEN}✅ $1${NC}"; }
warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
error() { echo -e "${RED}❌ $1${NC}"; exit 1; }

# === 🏁 Début de l'installation ===
echo ""
echo "╔═══════════════════════════════════════════════════════╗"
echo "║  🚀 Installation du projet NuitInfo.Rubeus           ║"
echo "╚═══════════════════════════════════════════════════════╝"
echo ""

# === 🔍 Détection d'une installation existante ===
FIRST_INSTALL=true
if [ -d "Data/Migrations" ] && [ "$(ls -A Data/Migrations)" ]; then
    FIRST_INSTALL=false
    warning "⚠️  Migrations existantes détectées !"
    echo ""
    echo "Ce script va :"
    echo "  - Supprimer toutes les migrations existantes"
    echo "  - Recréer une migration from scratch"
    echo "  - Potentiellement écraser votre base de données"
    echo ""
    read -p "Êtes-vous SÛR de vouloir continuer ? (tapez 'OUI' en majuscules) : " confirm
    if [ "$confirm" != "OUI" ]; then
        error "Installation annulée par l'utilisateur"
    fi
fi

# === 🔍 Vérification de .NET ===
info "Vérification de l'installation de .NET..."
if ! command -v dotnet &> /dev/null; then
    error ".NET n'est pas installé. Installez-le depuis https://dotnet.microsoft.com/"
fi
DOTNET_VERSION=$(dotnet --version)
success ".NET version $DOTNET_VERSION détecté"

# === 📦 Vérification du fichier .env ===
info "Vérification du fichier .env..."
if [ ! -f ".env" ]; then
    error "Fichier .env introuvable. Créez-le avec AUTH_STRING_POSTGREE et AUTH_STRING_MONGO"
fi
success "Fichier .env trouvé"

# Chargement des variables d'environnement
set -a
. .env
set +a

if [ -z "$AUTH_STRING_POSTGREE" ]; then
    error "Variable AUTH_STRING_POSTGREE non définie dans .env"
fi
if [ -z "$AUTH_STRING_MONGO" ]; then
    error "Variable AUTH_STRING_MONGO non définie dans .env"
fi
success "Variables d'environnement chargées"

# === 🧰 Installation de dotnet-ef ===
info "Installation/mise à jour de dotnet-ef..."
dotnet tool restore || error "Échec de la restauration des outils dotnet"
success "Outil dotnet-ef installé/restauré"

# === 📦 Restauration des packages NuGet ===
info "Restauration des packages NuGet..."
dotnet restore rubeus-nuech-info-blazor.sln || error "Échec de la restauration des packages"
success "Packages NuGet restaurés"

# === 🏗️ Build du projet ===
info "Compilation du projet..."
dotnet build rubeus-nuech-info-blazor.sln --no-restore || error "Échec de la compilation"
success "Projet compilé avec succès"

# === 🗄️ Gestion des migrations ===
if [ "$FIRST_INSTALL" = true ]; then
    info "Première installation : création de la migration initiale..."
    
    # === 🗑️ Nettoyage des anciennes migrations (au cas où) ===
    if [ -d "Data/Migrations" ]; then
        rm -rf Data/Migrations
    fi
    
    # === 🗄️ Création de la migration initiale ===
    dotnet tool run dotnet-ef migrations add InitialCreate \
        --project NuitInfo.Rubeus.csproj \
        --startup-project NuitInfo.Rubeus.csproj \
        || error "Échec de la création de la migration"
    success "Migration InitialCreate créée"
    
    # === 🚀 Application de la migration dans la base de données ===
    info "Application de la migration dans PostgreSQL..."
    dotnet tool run dotnet-ef database update \
        --project NuitInfo.Rubeus.csproj \
        --startup-project NuitInfo.Rubeus.csproj \
        || error "Échec de l'application de la migration"
    success "Migration appliquée dans la base de données"
else
    warning "Installation existante détectée : migrations conservées"
    info "Pour appliquer les migrations existantes, utilisez :"
    echo "  dotnet ef database update"
fi

# === 🧪 Test de connexion MongoDB ===
info "Test de connexion MongoDB..."
dotnet test Tests/NuitInfo.Rubeus.Tests/NuitInfo.Rubeus.Tests.csproj \
    --filter "FullyQualifiedName~TestMongoConnexion" \
    --no-build --verbosity quiet 2>/dev/null
if [ $? -eq 0 ]; then
    success "Connexion MongoDB validée"
else
    warning "Test MongoDB échoué (non bloquant)"
fi

# === 🎉 Récapitulatif final ===
echo ""
echo "╔═══════════════════════════════════════════════════════╗"
echo "║  🎉 Installation terminée avec succès !               ║"
echo "╚═══════════════════════════════════════════════════════╝"
echo ""
success "Packages installés et restaurés"
if [ "$FIRST_INSTALL" = true ]; then
    success "Migration PostgreSQL créée et appliquée"
else
    success "Migrations existantes préservées"
fi
success "Projet compilé et prêt à l'emploi"
echo ""
info "Pour démarrer le projet :"
echo "  dotnet run --project NuitInfo.Rubeus.csproj"
echo ""
info "Pour lancer les tests :"
echo "  ./lancement-tests.sh"
echo ""
