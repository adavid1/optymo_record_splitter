# Optymo Record Splitter

Application permettant de découper des enregistrements radio journaliers en conversation triées et horodatées

## Mise au point du projet
Sujet de Crunch Time 2019 en collaboration avec l'UTBM

## Tutoriel
Les données d'entrée de l'aplication sont :
 - Le fichier audio (en .mp3)
 - La date et l'heure de début de l'enregistrement (permettant d'horodater les différents enregistrements)
 - Le tolérance de découpage minimale en minutes (permettant d'indiquer le temps minimun souhaité entre 2 conversation)
		valeur conseillée : 60
 - Tolérance du volume ignoré (valeur en % correspondant au volume sonore)
		valeur conseillée : 30 (à ajuster en fonction des parasites)

Une fois les données entrées et validées, l'application va créer un dossier au même endroit que le fichier audio source et le nommer à la date de l'enregistrement. Le dossier contiendra toutes les conversations découpées et datées.
