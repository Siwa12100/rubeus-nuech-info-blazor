# Checklist des Assets PC Rescue

Ce dossier doit contenir les images suivantes pour le jeu PC Rescue : Mission NIRD.

## Images de Boîtier et Structure

- [ ] `case_closed.png` - Boîtier PC fermé (phase Opening)
- [ ] `motherboard.png` - Carte mère du PC

## Images de Composants Anciens

- [ ] `old_gpu.png` - Carte graphique ancienne
- [ ] `old_ram.png` - Barrettes RAM anciennes
- [ ] `old_storage.png` - Disque dur / SSD ancien
- [ ] `old_cpu.png` - Processeur ancien (optionnel pour extension)

## Images de Composants Neufs

- [ ] `new_gpu.png` - Carte graphique neuve (pour la boutique)
- [ ] `new_ram.png` - Barrettes RAM neuves (pour la boutique)
- [ ] `new_storage.png` - SSD neuf (pour la boutique)
- [ ] `new_cpu.png` - Processeur neuf (optionnel pour extension)

## Images de Poubelles (Phase Tri)

- [ ] `bin_trash.png` - Poubelle normale (déchets)
- [ ] `bin_recycle.png` - Poubelle de recyclage
- [ ] `bin_reuse.png` - Bac de réutilisation

## Images de QTE (Quick Time Events)

- [ ] `qte_perfect.png` - Indicateur de réussite parfaite (optionnel)
- [ ] `qte_medium.png` - Indicateur de réussite moyenne (optionnel)
- [ ] `qte_fail.png` - Indicateur d'échec (optionnel)

## Images de Feedback

- [ ] `component_intact.png` - Badge composant intact (optionnel)
- [ ] `component_damaged.png` - Badge composant endommagé (optionnel)
- [ ] `component_broken.png` - Badge composant cassé (optionnel)

## Notes Techniques

- **Format recommandé** : PNG avec transparence
- **Dimensions suggérées** : 256x256px ou 512x512px
- **Nommage** : Respecter exactement les noms ci-dessus (sensible à la casse)
- **Chemins** : Les images sont référencées dans le code avec `/images/pc/[nom_fichier].png`

## Priorités pour le MVP

### Obligatoire (Minimum Viable Product)
1. case_closed.png
2. motherboard.png
3. old_gpu.png, old_ram.png, old_storage.png
4. bin_trash.png, bin_recycle.png, bin_reuse.png

### Recommandé
5. new_gpu.png, new_ram.png, new_storage.png

### Optionnel (Amélioration future)
6. Badges de condition (intact/damaged/broken)
7. Indicateurs de QTE
8. Composants CPU
