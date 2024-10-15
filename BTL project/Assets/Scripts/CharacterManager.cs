using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
*   L'objectif de CharacterManager est de s'occuper de la gestion d'un personnage en particulier. 
*   il détermine l'equipe du personnage et reçoit les instructions pour les tilemaps de TileManager. 
*   Ensuite c'est lui qui fait le lien avec les différents paramettres du personnage. Sa vie, ses stats, ses equipements, ses déplacements.
*/

public class CharacterManager : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public Tilemap doorTilemap;

    private bool isSelected = false;

    private CharacterMovement characterMovement;
    private List<Vector3> currentPath = new List<Vector3>(); // Le chemin que le personnage doit suivre

    private string team;

    public enum Team // Enum pour identifier l'équipe d'un personnage
    {
        Player,  // Équipe du joueur
        Enemy    // Équipe de l'ennemi
    }

    // Variable publique qui peut être modifiée dans l'Inspector d'Unity
    public Team characterTeam;

    //====================================================Initialisation des dépendances============================================//
    void Start()
    {
        // Récupérer le composant CharacterMovement attaché au même GameObject
        characterMovement = GetComponent<CharacterMovement>();
        if (characterMovement == null)
        {
            Debug.LogError("characterMovement n'a pas été trouvé !");
        }
    }
    
    //====================================================Attribution de l'equipe pour TileManager============================================//
    public string WhichTeam()
    {
        team = characterTeam.ToString();
        return team; // Retourne "Player" ou "Enemy" selon la valeur de characterTeam
    }

    //====================================================Verification et transmission des tilesmap pour le déplacement============================================//
    public void InitialiseCharacterTiles()
    {
        // Vérifie que les tilemaps sont correctement récupérées
        if (groundTilemap == null)
            Debug.LogError($"Tilemap Ground pour {team} introuvable.");
        if (wallTilemap == null)
            Debug.LogError($"Tilemap Wall pour {team} introuvable.");
        if (doorTilemap == null)
            Debug.LogError($"Tilemap Door pour {team} introuvable.");
        characterMovement.InitialisePosition(groundTilemap, wallTilemap, doorTilemap, team);
    }

//////////////////////////////////////////////////////////////////////////////////////////MainUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Obtention par tileManager de l'information de selection du personnage géré============================================//
    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    //====================================================Boucle principale des personnages============================================//
    void Update()
    {
        // Si le personnage est sélectionné, il peut interagir
        if (isSelected)
        {
            currentPath = characterMovement.DetectRightClick(groundTilemap, wallTilemap, doorTilemap, team);
        }
        if (currentPath != null && currentPath.Count > 0)
        {
            currentPath = characterMovement.MoveAlongPath(); //envoie à charactermanager que le perso peut se déplacer
        }      
    }
}
