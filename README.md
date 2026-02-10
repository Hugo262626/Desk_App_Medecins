# Desk App Médecins — Prise et gestion de rendez-vous (application desktop)

Application **desktop** destinée à un usage “secrétariat médical” : création et gestion de **patients**, **médecins** et **rendez-vous**, avec des écrans de consultation / recherche.  
> Projet en cours.

---

## Objectif du projet

Centraliser la gestion quotidienne d’un cabinet (ou d’un centre) médical :
- référentiels (patients, praticiens)
- agenda / rendez-vous
- suivi administratif simple (listes, filtres, historiques)

---

## Fonctionnalités (cible)

- **Patients**
  - CRUD : création, modification, suppression, consultation)
  - recherche / tri
  - fiche patient : nom, prénom, coordonnées
- **Médecins**
  - CRUD
  - spécialité
- **Rendez-vous**
  - création de RDV (patient + médecin + date/heure + motif)
  - règles de validation (ex. créneau valide, conflit de planning)
  - liste des RDV filtrable et triable

---

## Stack technique

- **C# / .NET**
- framework UI (WPF/WinForms -  application desktop) et couche data (Entity Framework)

---

## Prérequis techniques

- **.NET SDK**
- Un moteur de base de données (SGBDR - ex : MySQL)
