#!/bin/bash

echo "📦 Chargement des variables d'environnement depuis .env..."

if [ -f .env ]; then
  set -a
  . ./.env
  set +a
else
  echo "❌ Fichier .env introuvable !"
  exit 1
fi

echo "🔗 Chaîne de connexion utilisée :"
echo "   AUTH_STRING_POSTGREE = $AUTH_STRING_POSTGREE"
echo

echo "🏗  Application des migrations EF Core..."
dotnet tool run dotnet-ef database update \
  --project NuitInfo.Rubeus.csproj \
  --startup-project NuitInfo.Rubeus.csproj

echo
echo "✅ Base initialisée / migrations appliquées."
