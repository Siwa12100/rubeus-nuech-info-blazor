#!/bin/bash

# === 📌 Paramètres modifiables ===
NOM_PROJET="nuit_info_rubeus"
DOSSIER_PROJET="."          # le .csproj est dans ce dossier
PORT_PAR_DEFAUT="5010"

# === 📦 Charger .env AVANT de définir les chemins ===
if [ -f .env ]; then
  echo "📦 Chargement des variables depuis .env..."
  set -a           # 👉 toutes les variables définies sont exportées automatiquement
  . ./.env         # 👉 on source le fichier .env
  set +a
else
  echo "⚠️  Aucun fichier .env trouvé. Certaines variables peuvent manquer."
fi


# === 🌐 Port et mode watch ===
PORT="${1:-${PORT_PUBLIQUE:-$PORT_PAR_DEFAUT}}"
USE_WATCH=false
if [ "$2" == "-w" ]; then
  USE_WATCH=true
fi

# === 🔎 Affichage infos ===
echo "🚀 Projet         : $NOM_PROJET"
echo "📁 Dossier        : $DOSSIER_PROJET"
echo "🌐 Port utilisé   : $PORT"
echo

# === 🔥 Lancement ===
if [ "$USE_WATCH" = true ]; then
  echo "👀 Lancement avec dotnet watch..."
  dotnet watch --project "$DOSSIER_PROJET/NuitInfo.Rubeus.csproj" run --urls "http://0.0.0.0:$PORT"
else
  echo "🏃 Lancement standard..."
  dotnet run --project "$DOSSIER_PROJET/NuitInfo.Rubeus.csproj" --urls "http://0.0.0.0:$PORT"
fi
