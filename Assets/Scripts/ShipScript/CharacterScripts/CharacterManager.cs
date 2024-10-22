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
    private bool initialisationEnd = false;

    private bool isSelected = false;

    private CharacterMovement characterMovement;
    private List<Vector3> currentPath = new List<Vector3>(); // Le chemin que le personnage doit suivre

    private CharacterFight characterFight;
    private bool characterInCACBattleSuperposed = true;
    private float delay = 1f;
    private bool canAttack = true;

    public string spacecraft;
    public string team;

    public enum Spacecraft // Enum pour identifier l'équipe d'un personnage
    {
        Player,  // Équipe du joueur
        Enemy    // Équipe de l'ennemi
    }

    public enum Team // Enum pour identifier l'équipe d'un personnage
    {
        Player,  // Équipe du joueur
        Enemy    // Équipe de l'ennemi
    }


    // Variable publique qui peut être modifiée dans l'Inspector d'Unity
    public Spacecraft characterSpacecraft;
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

        // Récupérer le composant CharacterFight attaché au même GameObject
        characterFight = GetComponent<CharacterFight>();
        if (characterFight == null)
        {
            Debug.LogError("CharacterFight n'a pas été trouvé !");
        }
    }
    
    //====================================================Attribution de l'equipe pour TileManager============================================//
    public string WhichSpacecraft()
    {
        spacecraft = characterSpacecraft.ToString();
        return spacecraft; // Retourne "Player" ou "Enemy" selon la valeur de characterspacecraft
    }

    //====================================================Attribution de l'equipe pour TileManager============================================//
    public string WhichTeam()
    {
        team = characterTeam.ToString();
        return team; // Retourne "Player" ou "Enemy" selon la valeur de team
    }

    //====================================================Verification et transmission des tilesmap pour le déplacement============================================//
    public void InitialiseCharacterTiles()
    {
        // Vérifie que les tilemaps sont correctement récupérées
        if (groundTilemap == null)
            Debug.LogError($"Tilemap Ground pour {spacecraft} introuvable.");
        if (wallTilemap == null)
            Debug.LogError($"Tilemap Wall pour {spacecraft} introuvable.");
        if (doorTilemap == null)
            Debug.LogError($"Tilemap Door pour {spacecraft} introuvable.");
        characterMovement.InitialisePosition(groundTilemap, wallTilemap, doorTilemap, spacecraft, team);
        initialisationEnd = true;
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
        if(initialisationEnd)
        {
            // Si le personnage est sélectionné, il peut interagir
            if (isSelected)
            {           
                currentPath = characterMovement.DetectRightClick(groundTilemap, wallTilemap, doorTilemap, spacecraft, team);
            }
            if (currentPath != null && currentPath.Count > 0)
            {
                currentPath = characterMovement.MoveAlongPath(); //envoie à charactermanager que le perso peut se déplacer
            }      

            KeyValuePair<GameObject, Vector3> opponentNextToMe = characterFight.DetectEnemiesOnTile(transform.position, groundTilemap, team);
            if (opponentNextToMe.Key != null && characterInCACBattleSuperposed)
            {
                //Debug.Log("opponentNextToMe non null");
                characterInCACBattleSuperposed = characterFight.proximityFight_UnsuperposeCharacters(opponentNextToMe);
            }
            else if(opponentNextToMe.Key != null && !characterInCACBattleSuperposed && canAttack)
            {
                if(canAttack)
                {
                    // Démarrer la coroutine pour retarder l'exécution de MyFunction
                    StartCoroutine(DelayAttack(delay, opponentNextToMe)); // Délai de 1 seconde
                }            
            }
            else if(opponentNextToMe.Key == null && !characterInCACBattleSuperposed)
            {
                characterInCACBattleSuperposed = characterFight.proximityFight_ReCentreCharacters(opponentNextToMe);
            }

            if(opponentNextToMe.Key == null)
            {
                //Debug.Log("opponentNextToMe NULL");
                characterInCACBattleSuperposed = true;
                List<KeyValuePair<GameObject, Vector3>> opponentInRoom = characterFight.DetectEnemiesInRoom(transform.position, groundTilemap, wallTilemap, spacecraft, team);
                // Vérifie s'il y a des ennemis dans la liste
                if (opponentInRoom != null) 
                {
                    //characterFight.distanceFight(opponentInRoom); // Appelle distanceFight pour chaque ennemi
                }
            }
        }
    }

    // Coroutine qui attend un certain temps avant d'appeler une fonction
    private IEnumerator DelayAttack(float delay, KeyValuePair<GameObject, Vector3> opponentNextToMe)
    {
        canAttack = false; // Commencer à attaquer
        // Attendre le délai spécifié
        yield return new WaitForSeconds(delay);

        // Appeler la fonction après le délai
            //ajouter l'animation personnage ici
        characterFight.AttackCAC(opponentNextToMe);
        canAttack = true; // Commencer à attaquer
    }

//////////////////////////////////////////////////////////////////////////////////////////TeleporterUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Transmission des informations de TP au travers des script du personnage : Actualisation du vaisseau ============================================//
    public void TPCharacters_ActualiseTile(string getSpacecraft, Vector3Int TPDestination, Tilemap newGroundTilemap, Tilemap newWallTilemap)
    {
        characterSpacecraft = (Spacecraft)System.Enum.Parse(typeof(Spacecraft), getSpacecraft, true); // converti la chaîne de caractères en une valeur de l'enum Spacecraft
        characterMovement.TPCharacters_MoveCharacter(TPDestination, groundTilemap, newGroundTilemap, newWallTilemap, getSpacecraft, team);
    }

    public void TPCharactersBack(List<Vector3Int> TPTilesPosition, Tilemap playerGround, Tilemap enemyGround)
    {
        characterSpacecraft = (Spacecraft)System.Enum.Parse(typeof(Spacecraft), "Player", true); // converti la chaîne de caractères en une valeur de l'enum Spacecraft
        characterMovement.TPCharactersBack_MoveCharacter(TPTilesPosition, playerGround, enemyGround, team);
    }
}
