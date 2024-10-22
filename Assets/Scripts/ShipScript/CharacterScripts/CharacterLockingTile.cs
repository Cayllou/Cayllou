using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
*   L'objectif de CharacterLockingTile est de faire la vérification de l'accessibilité des tuiles, afin d'eviter que 2 personnages ne se retrouvent sur la même tuile.
*/

public class CharacterLockingTile : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

	//====================================================Initialisation des variables============================================//
    public static CharacterLockingTile Instance { get; private set; } // L'instance statique pour accéder au singleton
	private Dictionary<GameObject, Vector3> playerPositionTiles = new Dictionary<GameObject, Vector3>(); // Set pour suivre les tuiles occupées
	private Dictionary<GameObject, Vector3> enemyPositionTiles = new Dictionary<GameObject, Vector3>(); // Set pour suivre les tuiles occupées

	//====================================================Verifier l'unicité du singleton CharacterLockingTile============================================//
	void Awake()
    {
        // Vérifier s'il existe déjà une instance du singleton
        if (Instance == null)
        {
            // Si aucune instance n'existe, définir cette instance comme unique
            Instance = this;

            // Optionnel: Empêcher la destruction du singleton lors des changements de scène
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si une instance existe déjà, détruire cette instance supplémentaire
            Destroy(gameObject);
        }
    }

//////////////////////////////////////////////////////////////////////////////////////////Update////////////////////////////////////////////////////////////////////////////////////////////////////////
	
    //=========================================================Methodes de gestion du verrouillage des tuiles de tilemaps====================//

	//====================================================Vérifie si une tuile est libre============================================//
    public bool IsTileFree(Vector3 tilePosition, Tilemap groundTilemap, string team)
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(tilePosition);

    	if(team == "Player")
    	{
    		return !playerPositionTiles.ContainsValue(tilePosition) && groundTilemap.HasTile(cellPosition);
    	}
    	else if(team == "Enemy")
    	{
    		return !enemyPositionTiles.ContainsValue(tilePosition) && groundTilemap.HasTile(cellPosition);
    	}
    	else
    	{
    		Debug.LogError("ERREUR : Dans aucune équipe");
    		return false;
    	}
    }

    //====================================================Verrouille une tuile============================================//
    public void LockTile(GameObject character, Vector3 tilePosition, Tilemap groundTilemap, string team)
    {
    	if(playerPositionTiles.ContainsKey(character) || enemyPositionTiles.ContainsKey(character))
    	{
    		Debug.LogError("ERREUR : Le GameObject " + character.name + " est déjà dans le dictionnaire.");
    	}
    	else if(groundTilemap.name == "Player Ground")
    	{
    		if (!playerPositionTiles.ContainsKey(character) && team == "Player")
	    	{
	        	playerPositionTiles.Add(character, tilePosition);
	            ChangeTileColor(tilePosition, Color.cyan, groundTilemap); // Change la couleur de la tuile en cyan  lorsqu'elle est occupée
		    }
	    	else if (!enemyPositionTiles.ContainsKey(character) && team == "Enemy")
	    	{
	        	enemyPositionTiles.Add(character, tilePosition);
	            ChangeTileColor(tilePosition, Color.red, groundTilemap); // Change la couleur de la tuile en rouge lorsqu'elle est occupée
	    	}
	    	else
	    	{
	    		Debug.LogError("ERREUR : Dans aucune équipe");
	    	}
    	}
    	else if(groundTilemap.name == "Enemy Ground")
    	{
    		if (!playerPositionTiles.ContainsKey(character) && team == "Player")
	    	{
	        	playerPositionTiles.Add(character, tilePosition);
	            ChangeTileColor(tilePosition, Color.cyan, groundTilemap); // Change la couleur de la tuile en cyan  lorsqu'elle est occupée
		    }
	    	else if (!enemyPositionTiles.ContainsKey(character) && team == "Enemy")
	    	{
	        	enemyPositionTiles.Add(character, tilePosition);
	            ChangeTileColor(tilePosition, Color.red, groundTilemap); // Change la couleur de la tuile en rouge lorsqu'elle est occupée
	    	}
	    	else
	    	{
	    		Debug.LogError("ERREUR : Dans aucune équipe");
	    	}
    	}
    	else
    	{
    		 Debug.LogError("ERREUR : Le GameObject " + character.name + " n'est pas associé à un vaisseau");
    	}
    }

    //====================================================Déverrouille une tuile============================================//
    public void UnlockTile(GameObject character, Vector3 tilePosition, Tilemap groundTilemap, string team)
    {
    	if (playerPositionTiles.ContainsKey(character))
    	{
	        	playerPositionTiles.Remove(character);

	    }
	    if (enemyPositionTiles.ContainsKey(character))
	    {
	        	enemyPositionTiles.Remove(character);
	    }
    	if(!playerPositionTiles.ContainsKey(character) && !enemyPositionTiles.ContainsKey(character))
    	{
    		 Debug.LogWarning("ERREUR WTF : Le GameObject " + character.name + " est dans aucun dictionnaire.");
    	}

        ChangeTileColor(tilePosition, Color.white, groundTilemap); // Rétablit la couleur d'origine
    }

    //====================================================change la couleur d'une tuile============================================//
    private void ChangeTileColor(Vector3 tilePosition, Color color, Tilemap groundTilemap)
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(tilePosition); // Convertit la position du monde en position de cellule
        if (groundTilemap.HasTile(cellPosition))
        {
            groundTilemap.SetTileFlags(cellPosition, TileFlags.None); // Désactive les flags pour permettre le changement de couleur
            groundTilemap.SetColor(cellPosition, color); // Applique la nouvelle couleur
        }
    }

    //====================================================Vérifie si une tuile est libre============================================//
    public KeyValuePair<GameObject, Vector3> IsTileEnemyFree(Vector3Int tilePosition, Tilemap groundTilemap, string team)
    {
        if(team == "Player")
        {
        	foreach (var character in enemyPositionTiles)
		    {
		    	CharacterManager charaManage = character.Key.GetComponent<CharacterManager>();
		    	Vector3Int characterTilePosition = groundTilemap.WorldToCell(character.Value); // Convertit les coordonnées mondiales en coordonnées de cellules Tilemap
		        // Vérifie si le personnage est un opponent et si la position correspond à la valeur dans le dictionnaire
		        if (characterTilePosition == tilePosition && character.Key != null && groundTilemap.HasTile(tilePosition))
		        {
			            return character; // Retourne l'association Key/Value trouvée
		        }
		    }  
        }
        else if(team == "Enemy")
        {
        	foreach (var character in playerPositionTiles)
		    {
		    	CharacterManager charaManage = character.Key.GetComponent<CharacterManager>();
		    	Vector3Int characterTilePosition = groundTilemap.WorldToCell(character.Value); // Convertit les coordonnées mondiales en coordonnées de cellules Tilemap
		        // Vérifie si le personnage est un opponent et si la position correspond à la valeur dans le dictionnaire
		        if (characterTilePosition == tilePosition && character.Key != null && groundTilemap.HasTile(tilePosition))
		        {
			            return character; // Retourne l'association Key/Value trouvée
		        }
		    }    
        }
    	return default(KeyValuePair<GameObject, Vector3>);

        
    }
}
