# EV Tracking — Notice d'utilisation
**Baud Industries — Outil de suivi de production hebdomadaire**

---

## Sommaire

1. [Présentation générale](#1-présentation-générale)
2. [Démarrage — Importer le fichier Excel](#2-démarrage--importer-le-fichier-excel)
3. [Conditions du fichier Excel](#3-conditions-du-fichier-excel)
4. [Suivi Presse](#4-suivi-presse)
5. [Assemblage Automatique](#5-assemblage-automatique)
6. [Assemblage Manuel](#6-assemblage-manuel)
7. [Suivi Tri](#7-suivi-tri)
8. [Suivi Joints](#8-suivi-joints)
9. [Export PDF](#9-export-pdf)
10. [Erreurs fréquentes](#10-erreurs-fréquentes)

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

## 3. Conditions du fichier Excel

EV Tracking lit le fichier Excel en se basant sur une structure précise. Pour que l'analyse soit correcte, plusieurs règles doivent être respectées.

### Conserver la structure des feuilles

Le logiciel identifie les données par le **nom exact des feuilles** et par la **position des colonnes**. Il ne faut pas :

- Renommer les feuilles Excel
- Déplacer, ajouter ou supprimer des colonnes
- Modifier les lignes d'en-tête ou de date en haut du tableau
- Fusionner ou découper des cellules dans les zones de données

Le fichier doit rester au format standard de production Baud Industries. Tout écart de structure peut entraîner une lecture incorrecte ou une erreur à l'import.

### Utiliser un tableur standardisé

Chaque semaine, le fichier Excel doit être **une copie du modèle standard**, pas un fichier recopié manuellement. Les saisies se font uniquement dans les cellules de production prévues à cet effet — jamais dans les colonnes de formule ou d'en-tête.

### Règle de saisie — Feuille Tri : deux personnes dans la même équipe

Sur la feuille **Suivi Tri**, chaque cellule de production correspond à la production d'une équipe pour un jour donné. Si **deux personnes ont produit dans la même équipe le même jour**, il ne faut **pas saisir deux valeurs séparément** — le logiciel ne lit qu'une seule cellule par équipe et par jour.

La règle est de **saisir la somme des deux productions directement dans la cellule**, à l'aide d'une formule Excel :

```
= Valeur X + Valeur Y
```

**Exemple :** si l'équipe 1 a produit 320 pièces (opérateur A) et 180 pièces (opérateur B) :

```
= 320 + 180
```

Excel affichera `500` dans la cellule, et le logiciel lira `500` comme production de l'équipe 1 pour ce jour.

---

## 4. Suivi Presse

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

## 5. Assemblage Automatique

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

## 6. Assemblage Manuel

L'assemblage manuel est divisé en deux familles, accessibles via deux boutons distincts dans le menu :

### 6a. Assemblage Manuel — EV

Cliquer sur **Ass. Manuel — EV** dans le menu à gauche.

Contient les références numériques (ex. 004xxx, 008xxx). Les opérations suivies sont typiquement **Capuchon**, **Insert** et, pour certaines références, **Goupille**. Le nombre de graphiques s'adapte automatiquement au nombre d'opérations présentes dans le fichier Excel.

### 6b. Assemblage Manuel — Tige de poussée

Cliquer sur **Ass. Manuel — Tige** dans le menu à gauche.

Contient les références alphanumériques (ex. 90028R, 40030R). Les opérations suivies sont typiquement **Boitier**, **Sertissage** et **Soufflet**.

### Séparation EV / Tige

Les deux familles sont issues de la même feuille Excel "Suivi ASS manuel". Le logiciel les sépare automatiquement à l'import selon le format de la référence :
- **Référence numérique** → famille EV → page **Ass. Manuel — EV** dans le logiciel et section dédiée dans le PDF
- **Référence alphanumérique** → famille Tige → page **Ass. Manuel — Tige** dans le logiciel et section dédiée dans le PDF

### Structure commune des deux pages

Une **barre de boutons** en haut liste toutes les références ayant une production ou un objectif renseigné. Cliquer sur un bouton pour afficher le détail de la référence correspondante.

#### Bandeau de résumé

Sous le titre de la référence, une ligne de résumé affiche pour chaque opération :

```
Capuchon : 1 250 / 2 000 pcs    Insert : 980 / 2 000 pcs    Obj semaine : 2 000
```

#### Commentaire

Si une remarque est saisie dans le fichier Excel pour la référence sélectionnée, elle s'affiche dans un **bandeau jaune** sous le résumé.

#### Graphiques

Un **graphique en barres** est affiché pour chaque opération de la référence (le nombre de graphiques s'adapte automatiquement au nombre d'opérations).

Chaque graphique représente la production journalière (somme des équipes actives) sur 7 jours (lundi → dimanche).

**Code couleur des barres :**
- **Vert** : production du jour ≥ objectif journalier
- **Rouge** : production du jour < objectif journalier
- **Bleu** : journée de week-end (samedi ou dimanche)

Une **ligne orange horizontale** matérialise l'objectif journalier.

#### Calcul de l'objectif journalier

L'objectif journalier est calculé dynamiquement : **objectif équipe × nombre d'équipes ayant produit sur la semaine**. Si une seule équipe est active, l'objectif reste celui d'une équipe ; s'il y en a deux, il est doublé. Cela permet d'avoir un seuil cohérent avec la réalité de la semaine.

---

## 7. Suivi Tri

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

## 8. Suivi Joints

### Accéder à la page

Cliquer sur **Suivi Joints** dans le menu à gauche.

### Fonctionnement

La page Suivi Joints fonctionne de manière identique à la page **Suivi Tri** :

- Colonne de gauche avec la liste des références
- 7 camemberts par référence (un par jour)
- Même code couleur par équipe (vert foncé / vert moyen / vert clair / rouge / gris)

**Particularité :** le titre de chaque référence affiche également l'**ancien code** correspondant (`Référence : XXX | Ancien code : YYY`).

---

## 9. Export PDF

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

1. Choisir le format selon l'usage :
   - **Réunion HPC** — génère un rapport sans pages de séparation, pensé pour être imprimé et présenté sur feuille lors des réunions HPC. Le document est compact et enchaîne directement les sections.
   - **Mail / Transfert service** — génère un rapport avec des pages de séparation titrées entre chaque section, adapté pour être envoyé par mail ou transmis à un autre service de l'usine. La mise en page longue facilite la lecture à l'écran et l'identification rapide de chaque atelier.
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

## 10. Erreurs fréquentes

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
