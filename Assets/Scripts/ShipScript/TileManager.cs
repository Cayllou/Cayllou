using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
*   L'objectif de TileManager est de s'occuper de la gestion des tuiles du vaisseau du joueur et de l'ennemi affiché. (Voir pour une séparation potentielle en 2 scripts avec la Maj Multijoueur)
*   C'est lui qui gère l'interaction entre le vaisseau et ses composants, ici les personnages, et les systèmes.
*/

public class TileManager : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//

    // Dictionnaires pour stocker les tilemaps du joueur et de l'ennemi
    private Dictionary<string, Tilemap> playerTilemaps = new Dictionary<string, Tilemap>();
    private Dictionary<string, Tilemap> enemyTilemaps = new Dictionary<string, Tilemap>();

    private Vector3 characterPosition;
    public CharacterSystemInterface characterSystemInterface;

    public Tilemap playerGround;
    public Tilemap playerWall;
    public Tilemap playerDoor;
    public Tilemap playerSystem;
    public Tilemap enemyGround;
    public Tilemap enemyWall;
    public Tilemap enemyDoor;
    public Tilemap enemySystem;
    
    private string spacecraft;
    private string getSpacecraft;
    private string team;

    private List<Vector3Int> TPTilesPosition = new List<Vector3Int>();
    private List<GameObject> characterOnTP;
    private Vector3 TPDestination;

    //====================================================Initialisation des dépendances============================================//
    void Start()
    {
        characterSystemInterface = FindObjectOfType<CharacterSystemInterface>();
        if (characterSystemInterface == null)
        {
            Debug.LogError("characterSystemInterface n'a pas été trouvé !");
        }
    }

    //===================================================Initialisation des tilesmaps=======================================//
    public void InitializeTilemaps()
    {
        playerGround = GameObject.Find("Player Ground").GetComponent<Tilemap>();
        playerWall = GameObject.Find("Player Wall").GetComponent<Tilemap>();
        playerDoor = GameObject.Find("Player Door").GetComponent<Tilemap>();
        playerSystem = GameObject.Find("Player Systems").GetComponent<Tilemap>();  // Nouvelle tilemap
        
        enemyGround = GameObject.Find("Enemy Ground").GetComponent<Tilemap>();
        enemyWall = GameObject.Find("Enemy Wall").GetComponent<Tilemap>();
        enemyDoor = GameObject.Find("Enemy Door").GetComponent<Tilemap>();
        enemySystem = GameObject.Find("Enemy Systems").GetComponent<Tilemap>();  // Nouvelle tilemap

        // Vérifie si les tilemaps sont nulles avant de les stocker
        if (playerGround == null) Debug.LogError("PlayerGround Tilemap is null.");
        if (playerWall == null) Debug.LogError("PlayerWall Tilemap is null.");
        if (playerDoor == null) Debug.LogError("PlayerDoor Tilemap is null.");
        if (playerSystem == null) Debug.LogError("PlayerSystem Tilemap is null.");
        if (enemyGround == null) Debug.LogError("EnemyGround Tilemap is null.");
        if (enemyWall == null) Debug.LogError("EnemyWall Tilemap is null.");
        if (enemyDoor == null) Debug.LogError("EnemyDoor Tilemap is null.");
        if (enemySystem == null) Debug.LogError("EnemySystem Tilemap is null.");

        // Stocker les tilemaps du joueur
        playerTilemaps["Ground"] = playerGround;
        playerTilemaps["Wall"] = playerWall;
        playerTilemaps["Door"] = playerDoor;
        playerTilemaps["System"] = playerSystem;  // Nouvelle entrée pour System

        // Stocker les tilemaps de l'ennemi
        enemyTilemaps["Ground"] = enemyGround;
        enemyTilemaps["Wall"] = enemyWall;
        enemyTilemaps["Door"] = enemyDoor;
        enemyTilemaps["System"] = enemySystem;  // Nouvelle entrée pour System

        TPTilesPosition = characterSystemInterface.GetTeleporterTilePosition(playerSystem);
    }

    //===================================================Attribution des tilesmaps aux scripts CharcterManager's=======================================//
    public void InitializeCharacters(List<GameObject> allCharacters, bool reInit)
    {
        //fait le tour de chaque Personnage
        foreach (GameObject character in allCharacters)
        {
            characterPosition = character.transform.position;

            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            if (charaManage != null)
            {
                spacecraft = charaManage.WhichSpacecraft();  // Verifie le vaisseau dans lequel le personnage est situé au départ
                team = charaManage.WhichTeam();  // Verifie l'équipe du personnage
            }

            charaManage.groundTilemap = GetTilemap(spacecraft, "Ground");
            charaManage.wallTilemap = GetTilemap(spacecraft, "Wall");
            charaManage.doorTilemap = GetTilemap(spacecraft, "Door");
            if(!reInit)
            {
                charaManage.InitialiseCharacterTiles();
            }
            characterOnTP = characterSystemInterface.interactionCharactersSystems(character, characterPosition, playerSystem);
        }
    }

    //===================================================Recuperation d'une tilemap en fonction de l'équipe=======================================//
    public Tilemap GetTilemap(string spacecraft, string type)
    {
        if (spacecraft == "Player")
        {
            return playerTilemaps.ContainsKey(type) ? playerTilemaps[type] : null;
        }
        else if (spacecraft == "Enemy")
        {
            return enemyTilemaps.ContainsKey(type) ? enemyTilemaps[type] : null;
        }
        else
        {
            Debug.LogError("Équipe non reconnue.");
            return null;
        }
    }

//////////////////////////////////////////////////////////////////////////////////////////MainUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //===================================================Transmission de L'information de sélection/déselection des personnages=======================================//
    public void SelectedCharacterList(List<GameObject> selectedCharacters, List<GameObject> allCharacters)
    {
        //fait le tour de chaque Personnage
        foreach (GameObject character in allCharacters)
        {
            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            charaManage.SetSelected(false);
        }
        //fait le tour de chaque GameObject sélectionné
        foreach (GameObject character in selectedCharacters)
        {
            // Recherche le script du gameObject selectionné
            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            charaManage.SetSelected(true);
        }
    }

//////////////////////////////////////////////////////////////////////////////////////////SystemUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //===================================================Prendre la position des personnages pour verifier ceux sur le TP=======================================//
    public void GetCharactersOnTP(GameObject character) //fonction déclenchée depuis un appel de characterMovement
    {
        characterPosition = character.transform.position;
        characterOnTP = characterSystemInterface.interactionCharactersSystems(character, characterPosition, playerSystem);
    }

    //===================================================Récuperer le clic de destinationTP et detecter sur quel vaisseau il se situe=======================================//
    public void TPCharactersTo_VerifyTPCell(Vector3 mouseWorldPosition)
    {
        if (characterOnTP == null)
        {
            Debug.Log("Personne sur le TP");
            return;
        }

        Vector3Int mouseCellPosition = playerGround.WorldToCell(mouseWorldPosition);
        // Vérifie si la tuile est une tuile de sol pour le joueur
        TileBase playerTile = playerGround.GetTile(mouseCellPosition);
        if (playerTile != null)
        {
            getSpacecraft = "Player";

            mouseCellPosition = playerGround.LocalToCell(mouseWorldPosition);
            foreach (GameObject character in characterOnTP)
            {
                CharacterManager charaManage = character.GetComponent<CharacterManager>();
                charaManage.TPCharacters_ActualiseTile(getSpacecraft, mouseCellPosition, playerGround, playerWall);
            }
        }

        mouseCellPosition = enemyGround.WorldToCell(mouseWorldPosition);
        // Vérifie si la tuile est une tuile de sol pour l'ennemi
        TileBase enemyTile = enemyGround.GetTile(mouseCellPosition);
        if (enemyTile != null)
        {
            getSpacecraft = "Enemy";
            
            mouseCellPosition = enemyGround.LocalToCell(mouseWorldPosition);
            foreach (GameObject character in characterOnTP)
            {
                CharacterManager charaManage = character.GetComponent<CharacterManager>();
                charaManage.TPCharacters_ActualiseTile(getSpacecraft, mouseCellPosition, enemyGround, enemyWall);
            }
        }     
    }

    public void PrepareReturnTP(List<GameObject> allCharacters)
    {   
        List<GameObject> CharactersOutsidePlayerShip = new List<GameObject>();

        foreach (GameObject character in allCharacters)
        {   
            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            team = charaManage.WhichTeam();  // Verifie l'équipe du personnage

            if(team == "Player")
            {
                characterPosition = character.transform.position;   //obtenir la liste des personnages téléportés
                spacecraft = charaManage.WhichSpacecraft();  // Verifie l'équipe du personnage

                if(spacecraft == "Enemy")
                {
                    CharactersOutsidePlayerShip.Add(character);
                }
            }
            
        }
        
        //teleporter les personnages concernés vers le teleporteur.
        foreach (GameObject character in CharactersOutsidePlayerShip)
        {
            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            charaManage.TPCharactersBack(TPTilesPosition, playerGround, enemyGround);
        }

    }
}