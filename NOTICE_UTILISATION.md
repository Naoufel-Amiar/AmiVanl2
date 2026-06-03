# EV Tracking — Notice d'utilisation
**Baud Industries — Outil de suivi de production hebdomadaire**

---

## Sommaire

1. [Présentation générale](#1-présentation-générale)
2. [Démarrage — Importer le fichier Excel](#2-démarrage--importer-le-fichier-excel)
3. [Suivi Presse](#3-suivi-presse)
4. [Assemblage Automatique](#4-assemblage-automatique)
5. [Assemblage Manuel](#5-assemblage-manuel)
6. [Suivi Tri](#6-suivi-tri)
7. [Suivi Joints](#7-suivi-joints)
8. [Export PDF](#8-export-pdf)
9. [Erreurs fréquentes](#9-erreurs-fréquentes)

---

## 1. Présentation générale

**EV Tracking** est un outil de bureau conçu pour les pilotes et responsables de production de Baud Industries. Il lit un fichier Excel de production hebdomadaire et génère automatiquement des graphiques de suivi pour chaque atelier.

### Ateliers couverts

| Atelier | Type de données affichées |
|---|---|
| Presse | Barres journalières par référence et par machine |
| Assemblage Automatique | Barres journalières par référence et par machine |
| Assemblage Manuel — EV | Barres journalières par opération (Capuchon / Insert) |
| Assemblage Manuel — Tige de poussée | Barres journalières par opération (Boitier / Sertissage / Soufflet) |
| Tri | Camemberts journaliers par équipe |
| Joints | Camemberts journaliers par équipe |

### Principe de fonctionnement

1. L'utilisateur importe le fichier Excel de la semaine
2. EV Tracking lit toutes les feuilles et charge les données en mémoire
3. Les pages de suivi deviennent accessibles via le menu à gauche
4. Un rapport PDF peut être généré à tout moment

---

## 2. Démarrage — Importer le fichier Excel

### Accéder à la page

Cliquer sur **Accueil / Import** dans le menu à gauche (premier bouton).

### Importer le fichier

Deux méthodes sont disponibles :

- **Glisser-déposer** : faire glisser le fichier Excel directement dans la zone centrale de l'écran
- **Parcourir** : cliquer sur le bouton **Parcourir** et sélectionner le fichier manuellement

Le nom du fichier sélectionné s'affiche sous la zone d'import. La bordure passe au vert pour confirmer la sélection.

### Lancer la génération

Cliquer sur le bouton **DÉMARRER LA GÉNÉRATION**.

Une barre de chargement s'affiche pendant la lecture du fichier Excel. Le traitement dure généralement quelques secondes.

À la fin, un récapitulatif s'affiche avec le nombre de références et de lignes lues pour chaque atelier :

```
PRESSE            : X réf. en production  (X lignes lues)
ASS. AUTO      : X réf. en production  (X lignes lues)
JOINTS             : X réf. en production  (X lignes lues)
TRI                    : X réf. en production  (X lignes lues)
ASS. MANU EV : X réf. en production  (X lignes lues)
ASS. TIGE         : X réf. en production  (X lignes lues)
```

Les boutons du menu se déverrouillent ensuite pour accéder aux pages de suivi.

### Important

- Le fichier Excel doit être au format `.xlsx` ou `.xls`
- Le fichier peut rester ouvert dans Excel en même temps — si l'import échoue avec un message "fichier en cours d'utilisation", fermer Excel puis réessayer
- Si un nouveau fichier est importé, les données précédentes sont effacées et remplacées

---

## 3. Suivi Presse

### Accéder à la page

Cliquer sur **Suivi Presse** dans le menu à gauche.

### Page 1 — Vue d'ensemble

La page affiche deux graphiques en barres côte à côte, regroupant toutes les références et machines de l'atelier Presse.

**Lecture des barres :**
- Chaque barre représente la production d'une référence/machine pour un jour de la semaine
- **Barre verte** : l'objectif journalier est atteint ou dépassé
- **Barre rouge** : la production est inférieure à l'objectif
- **Barre bleue** : journée du week-end (samedi ou dimanche)

Une **ligne orange horizontale** matérialise l'objectif journalier sur chaque graphique.

En bas de page, une **liste de commentaires** apparaît si des remarques ont été saisies dans le fichier Excel pour certaines références.

### Page 2 — Détail par référence

Cliquer sur une référence dans la colonne de gauche pour afficher son détail.

La page affiche :
- Le **graphique en barres** de la référence sélectionnée (production journalière)
- Un **camembert de taux d'atteinte** : part de l'objectif hebdomadaire réalisée (vert) vs. restante (rouge)
- Un **tableau récapitulatif** avec les productions par jour et les totaux
- Le **commentaire** associé à la référence, si disponible

---

## 4. Assemblage Automatique

### Accéder à la page

Cliquer sur **Assemblage Auto** dans le menu à gauche.

### Fonctionnement

La page Assemblage Automatique fonctionne de manière identique à la page **Suivi Presse** :

- **Page 1** : vue d'ensemble avec deux graphiques regroupant toutes les références et machines
- **Page 2** : détail par référence avec graphique, camembert de taux d'atteinte et tableau

**Code couleur des barres :**
- **Vert** : objectif journalier atteint
- **Rouge** : objectif non atteint
- **Bleu** : week-end

---

## 5. Assemblage Manuel

L'assemblage manuel est divisé en deux familles, accessibles via deux boutons distincts dans le menu :

### 5a. Assemblage Manuel — EV

Cliquer sur **Ass. Manuel — EV** dans le menu à gauche.

Contient les références numériques (ex. 004xxx, 008xxx). Les opérations suivies sont typiquement **Capuchon** et **Insert**.

### 5b. Assemblage Manuel — Tige de poussée

Cliquer sur **Ass. Manuel — Tige** dans le menu à gauche.

Contient les références alphanumériques (ex. 90028R, 40030R). Les opérations suivies sont typiquement **Boitier**, **Sertissage** et **Soufflet**.

### Structure commune des deux pages

La barre de boutons en haut liste toutes les références disponibles. Cliquer sur une référence pour afficher son détail.

Pour chaque référence, un **graphique en barres par opération** est affiché (autant de graphiques que d'opérations pour la référence).

**Code couleur des barres :**
- **Vert** : production du jour ≥ objectif journalier
- **Rouge** : production du jour < objectif journalier
- **Bleu** : week-end

L'**objectif journalier** affiché correspond à l'objectif équipe multiplié par le nombre d'équipes actives sur la semaine.

Un **commentaire** s'affiche si une remarque est associée à la référence dans le fichier Excel.

---

## 6. Suivi Tri

### Accéder à la page

Cliquer sur **Suivi Tri** dans le menu à gauche.

### Structure de la page

La colonne de gauche liste toutes les références ayant une production enregistrée. Cliquer sur une référence pour afficher son détail.

### Données affichées

Pour chaque référence, 7 **camemberts** sont affichés — un par jour de la semaine (du lundi au dimanche).

**Lecture d'un camembert :**
- Chaque camembert montre la répartition de la production entre les équipes du jour
- **Vert foncé** — Équipe 1
- **Vert moyen** — Équipe 2
- **Vert clair** — Équipe 3
- **Rouge** — Production manquante par rapport à l'objectif journalier
- **Gris** — Aucune production ce jour-là

Le pourcentage affiché à l'intérieur de chaque part représente la proportion dans le total du camembert.

Pour le **week-end** (samedi/dimanche), un camembert simplifié indique uniquement si une production a eu lieu (vert) ou non (gris).

Un **commentaire** s'affiche si une remarque est associée à la référence.

---

## 7. Suivi Joints

### Accéder à la page

Cliquer sur **Suivi Joints** dans le menu à gauche.

### Fonctionnement

La page Suivi Joints fonctionne de manière identique à la page **Suivi Tri** :

- Colonne de gauche avec la liste des références
- 7 camemberts par référence (un par jour)
- Même code couleur par équipe (vert foncé / vert moyen / vert clair / rouge / gris)

**Particularité :** le titre de chaque référence affiche également l'**ancien code** correspondant (`Référence : XXX | Ancien code : YYY`).

---

## 8. Export PDF

### Accéder à la page

Cliquer sur **Export PDF** dans le menu à gauche.

### Vérification des données

Avant de générer, la page affiche la liste des sections incluses dans le rapport avec leur statut :
- **✓ Inclus** (vert) — des données sont disponibles pour cette section
- **⚠ Aucune donnée** (orange) — la section sera absente du rapport

### Deux formats de génération

| Bouton | Format | Usage |
|---|---|---|
| **Réunion HPC** | PDF sans pages de séparation entre les sections | Impression pour les réunions HPC — format compact |
| **Mail / Transfert service** | PDF avec pages de séparation titrées entre chaque section | Envoi par mail ou transfert aux autres services — format long |

Les deux versions contiennent les mêmes données et graphiques — seule la mise en page diffère.

### Générer le PDF

1. Cliquer sur **Réunion HPC** ou **Mail / Transfert service**
2. Choisir l'emplacement de sauvegarde dans la fenêtre qui s'ouvre
3. Patienter pendant la génération (barre de chargement visible)
4. Le fichier PDF est créé à l'emplacement choisi

### Contenu du rapport

Le rapport est généré au **format A3 paysage**. Il contient une page par atelier avec :

- **Presse** : graphiques en barres journalières + camembert de taux d'atteinte par référence
- **Assemblage Automatique** : idem Presse
- **Assemblage Manuel — EV** : tableau par opération (Capuchon / Insert) avec barres et objectifs par équipe
- **Assemblage Manuel — Tige** : tableau par opération (Boitier / Sertissage / Soufflet) avec barres et objectifs par équipe
- **Tri** : camemberts par équipe et par jour pour chaque référence
- **Joints** : idem Tri

Dans les tableaux Assemblage Manuel et Tri, la **valeur de production** est affichée dans chaque case, ainsi que **l'objectif journalier** en gris dessous (ex. `450` / `600`).

---

## 9. Erreurs fréquentes

### Le fichier Excel ne s'importe pas

| Message | Cause | Solution |
|---|---|---|
| "Format refusé" | Le fichier n'est pas au format `.xlsx` ou `.xls` | Vérifier l'extension du fichier |
| "Impossible de lire le fichier" | Excel a verrouillé le fichier (modifications non enregistrées) | Enregistrer ou fermer Excel puis réessayer |
| Message d'erreur avec "introuvable" | Une feuille attendue est absente du fichier | Vérifier que le fichier correspond au format standard de production |

### Les menus de suivi restent grisés

Les pages de suivi ne sont accessibles qu'après avoir cliqué sur **DÉMARRER LA GÉNÉRATION** et que le chargement soit terminé.

### Le bouton "Export PDF" est grisé

L'export PDF nécessite que la génération ait été effectuée au préalable. Importer le fichier Excel et lancer la génération avant d'accéder à l'export.

### Les graphiques apparaissent vides

Si une page de suivi ne contient aucun graphique, cela signifie que la feuille correspondante du fichier Excel ne contient pas de données de production pour la semaine.

---

*EV Tracking - Baud Industries - v1.3*
