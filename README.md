APPLICATION GESTION DES ÉVÉNEMENTS CULTURELS
TECHNOLOGIES:

- C#: Langage de progAPPLICATION GESTION DES ÉVÉNEMENTS CULTURELS
TECHNOLOGIES:

- C#: Langage de programmation orienté objet.
- WinForms: frame work .Net pour le UI
- SQLite: Pour la persistance des données (sans serveur)

DÉPENDANCES:

- Vs code: l’extension C# (base), C# Dev Kit (Gestion de projet), SDK .NET 8/10(facultative dans ce cas, mais essentielle)
- Microsoft Visual Studio Community: Fichier exe et téléchargement du workload .NET 10 (après avoir installé le fichier exe i.e dans l’application)
- SQLite: telechargez DB Browser

USER GUIDE DE L’APPLICATION:

- Connexion: dans notre programme nous avons 3 acteurs qui sont créés dans la base de donnée par défaut si elle n’existe pas (admin, organisateur et le participant).
 Voici les informations par défaut pour ce connecter au différents rôles:
// ADMINISTRATEUR
Nom utilisateur: admin
Mot de passe : admin123

// ORGANISATEUR
Nom utilisateur: orga
Mot de passe : orga2025

// PARTICIPANT
Nom utilisateur:  etudiant
Mot de passe : 1234

N/B: Un événement crée par l’organisateur a un statut « Attente » par défaut et le participant (etudiant) ne peut que voir les événements Approuvés.

OPÉRATIONS DE CHAQUE ACTEUR:

1. Organisateur (Gestionnaire d'événements)
L'organisateur se concentre sur la création et le suivi de ses événements culturels. 

* Créer un événement : Saisir le titre, la description, le lieu, la date, et l'image de couverture.
* Gérer la billetterie : Définir le nombre de places, les prix (gratuit/payant), et les types de billets (VIP, standard).
* Suivi des inscriptions : Voir la liste des participants inscrits et valider les réservations.
* Analyser les résultats : Consulter des statistiques sur la fréquentation et les revenus. 

2. Admin (Administrateur système)
L'admin supervise l'ensemble de la plateforme, la sécurité et la modération. 

* Modération des contenus : Approuver, suspendre ou supprimer des événements publiés.
* Gestion des utilisateurs : Créer, modifier, ou désactiver des comptes (organisateurs, étudiants).
* Gestion des catégories : Ajouter/modifier les types d'événements (Concert, Théâtre, Exposition).
* Tableau de bord global : Voir les statistiques globales de la plateforme.   
3. Étudiant (Participant)
L'étudiant recherche et participe aux événements. 

* Explorer les événements : Parcourir une liste des événements culturels.
- Réserver/Acheter des billets : Sélectionner un événement et obtenir son ticket numérique.rammation orienté objet.
- WinForms: frame work .Net pour le UI
- SQLite: Pour la persistance des données (sans serveur)

DÉPENDANCES:

- Vs code: l’extension C# (base), C# Dev Kit (Gestion de projet), SDK .NET 8/10(facultative dans ce cas, mais essentielle)
- Microsoft Visual Studio Community: Fichier exe et téléchargement du workload .NET 10 (après avoir installé le fichier exe i.e dans l’application)
- SQLite: telechargez DB Browser

USER GUIDE DE L’APPLICATION:

- Connexion: dans notre programme nous avons 3 acteurs qui sont créés dans la base de donnée par défaut si elle n’existe pas (admin, organisateur et le participant).
 Voici les informations par défaut pour ce connecter au différents rôles:
// ADMINISTRATEUR
Nom utilisateur: admin
Mot de passe : admin123

// ORGANISATEUR
Nom utilisateur: orga
Mot de passe : orga2025

// PARTICIPANT
Nom utilisateur:  etudiant
Mot de passe : 1234

N/B: Un événement crée par l’organisateur a un statut « Attente » par défaut et le participant (etudiant) ne peut que voir les événements Approuvés.

OPÉRATIONS DE CHAQUE ACTEUR:

1. Organisateur (Gestionnaire d'événements)
L'organisateur se concentre sur la création et le suivi de ses événements culturels. 

* Créer un événement : Saisir le titre, la description, le lieu, la date, et l'image de couverture.
* Gérer la billetterie : Définir le nombre de places, les prix (gratuit/payant), et les types de billets (VIP, standard).
* Suivi des inscriptions : Voir la liste des participants inscrits et valider les réservations.
* Analyser les résultats : Consulter des statistiques sur la fréquentation et les revenus. 

2. Admin (Administrateur système)
L'admin supervise l'ensemble de la plateforme, la sécurité et la modération. 

* Modération des contenus : Approuver, suspendre ou supprimer des événements publiés.
* Gestion des utilisateurs : Créer, modifier, ou désactiver des comptes (organisateurs, étudiants).
* Gestion des catégories : Ajouter/modifier les types d'événements (Concert, Théâtre, Exposition).
* Tableau de bord global : Voir les statistiques globales de la plateforme.   
3. Étudiant (Participant)
L'étudiant recherche et participe aux événements. 

* Explorer les événements : Parcourir une liste des événements culturels.
* Réserver/Acheter des billets : Sélectionner un événement et obtenir son ticket numérique.
