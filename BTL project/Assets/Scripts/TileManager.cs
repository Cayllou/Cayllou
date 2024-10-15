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

    public Tilemap playerGround;
    public Tilemap playerWall;
    public Tilemap playerDoor;
    public Tilemap playerSystem;
    public Tilemap enemyGround;
    public Tilemap enemyWall;
    public Tilemap enemyDoor;
    public Tilemap enemySystem;
    
    private string team;

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
    }

    //===================================================Attribution des tilesmaps aux scripts CharcterManager's=======================================//
    public void InitializeCharacters(List<GameObject> allCharacters)
    {
        
        foreach (GameObject character in allCharacters)
        {
            // Recherche le script du gameObject selectionné
            CharacterManager charaManage = character.GetComponent<CharacterManager>();
            if (charaManage != null)
            {
                team = charaManage.WhichTeam();  // Verifie l'équipe du personnage
            }

            charaManage.groundTilemap = GetTilemap(team, "Ground");
            charaManage.wallTilemap = GetTilemap(team, "Wall");
            charaManage.doorTilemap = GetTilemap(team, "Door");
            charaManage.InitialiseCharacterTiles();
        }
    }

    //===================================================Recuperation d'une tilemap en fonction de l'équipe=======================================//
    public Tilemap GetTilemap(string team, string type)
    {
        if (team == "Player")
        {
            return playerTilemaps.ContainsKey(type) ? playerTilemaps[type] : null;
        }
        else if (team == "Enemy")
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
}