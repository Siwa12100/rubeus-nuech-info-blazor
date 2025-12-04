#!/bin/bash

# === 📦 Chargement des variables d'environnement ===
ENV_FILE=".env"
if [ -f "$ENV_FILE" ]; then
  echo "📦 Chargement des variables depuis $ENV_FILE..."
  set -a           # 👉 toutes les variables définies ensuite sont exportées
  . "$ENV_FILE"    # 👉 on source le fichier .env
  set +a
else
  echo "⚠️  Fichier $ENV_FILE non trouvé. Certaines variables peuvent manquer."
fi

# (Optionnel) debug : afficher une ou deux vars utiles
echo "🧩 AUTH_STRING_POSTGREE = ${AUTH_STRING_POSTGREE:-<non défini>}"
echo "🧩 AUTH_STRING_MONGO   = ${AUTH_STRING_MONGO:-<non défini>}"

# === 🧾 Initialisation des options ===
VERBOSE=false
CLASS_FILTER=""
TEST_FILTER=""

# === 🧰 Lecture des arguments ===
# Usage attendu :
#   ./lancement-tests.sh                -> tous les tests
#   ./lancement-tests.sh -l             -> tous les tests en verbeux + coverage
#   ./lancement-tests.sh -c MaClasse    -> tests de MaClasse
#   ./lancement-tests.sh -c MaClasse -t MonTest  -> test précis
while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    -l)
      VERBOSE=true
      shift
      ;;
    -c)
      CLASS_FILTER="$2"
      shift 2
      ;;
    -t)
      TEST_FILTER="$2"
      shift 2
      ;;
    *)
      echo "❌ Option inconnue : $key"
      echo "Utilisation : ./lancement-tests.sh [-l] [-c NomClasse] [-t NomTest]"
      echo "  -l            : mode verbeux"
      echo "  -c NomClasse  : exécute uniquement les tests de la classe spécifiée"
      echo "  -t NomTest    : exécute un test spécifique (nécessite -c)"
      exit 1
      ;;
  esac
done

# === ⚠️ Validation logique ===
if [[ -n "$TEST_FILTER" && -z "$CLASS_FILTER" ]]; then
  echo "❌ Erreur : L'option -t nécessite l'option -c"
  exit 1
fi

# === 🏁 Construction de la commande ===
# On cible explicitement le projet de tests pour éviter MSB1011
CMD="dotnet test rubeus-nuech-info-blazor.sln"

if [[ -n "$CLASS_FILTER" && -n "$TEST_FILTER" ]]; then
  CMD="$CMD --filter \"FullyQualifiedName~$CLASS_FILTER.$TEST_FILTER\""
elif [[ -n "$CLASS_FILTER" ]]; then
  CMD="$CMD --filter \"FullyQualifiedName~$CLASS_FILTER\""
fi

if [ "$VERBOSE" = true ]; then
  CMD="$CMD --logger \"console;verbosity=detailed\" --collect:\"XPlat Code Coverage\""
fi

# === ⏱ Lancement avec mesure du temps ===
echo "🚀 Commande exécutée : $CMD"
START_TIME=$(date +%s)

eval $CMD

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo "✅ Tests terminés en $DURATION secondes"
