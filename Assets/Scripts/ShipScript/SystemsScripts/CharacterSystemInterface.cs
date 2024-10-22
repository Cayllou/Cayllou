using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class CharacterSystemInterface : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//
    private List<Vector3Int> TPTilesPosition = new List<Vector3Int>();
    private List<GameObject> characterOnTP = new List<GameObject>();
    private Dictionary<GameObject, string> CharactersOutsidePlayerShip = new Dictionary<GameObject, string>();
    // Dictionnaire pour stocker les paires GameObject -> String
    private Dictionary<GameObject, Vector3> objectDictionary = new Dictionary<GameObject, Vector3>();
    public GameObject TPbuttonTo;

    void Start()
    {
    	GameObject TPbuttonTo = GameObject.Find("TPbuttonTo");
    }
//////////////////////////////////////////////////////////////////////////////////////////GetCharactersInvolvedUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Fonction pour rechercher une tuile spécifique dans une Tilemap
    public List<Vector3Int> GetTeleporterTilePosition(Tilemap playerSystem)
    {
    	Vector3Int startTile = new Vector3Int(0, 0, 0);
        // Directions pour explorer les tuiles adjacentes (4 directions cardinales)
	    Vector3Int[] directions = new Vector3Int[]
	    {
	        new Vector3Int(-1, 0, 0), // gauche
	        new Vector3Int(1, 0, 0),  // droite
	        new Vector3Int(0, -1, 0), // bas
	        new Vector3Int(0, 1, 0)   // haut
	    };

	    // Set pour stocker les tuiles déjà visitées
	    HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>();

	    // File pour gérer les tuiles à explorer
	    Queue<Vector3Int> tileQueue = new Queue<Vector3Int>();
	    tileQueue.Enqueue(startTile);

	    // Marquer la tuile de départ comme visitée
	    visitedTiles.Add(startTile);

	    // Limite d'exploration (pour éviter une boucle infinie si nécessaire)
	    int explorationLimit = 0;

	    // Boucle pour explorer les tuiles tant qu'il y en a dans la file
	    while (tileQueue.Count > 0 && explorationLimit < 1000) // Limite modifiable
	    {
	        explorationLimit++;

	        // Extraire la tuile courante de la file
	        Vector3Int currentTile = tileQueue.Dequeue();

	        // Récupérer la tuile à cette position
        	TileBase tile = playerSystem.GetTile(currentTile);
	        if (tile is CustomTile customTile)
	        {
	            TPTilesPosition.Add(currentTile);
	        }

	        // Exploration des tuiles adjacentes
	        foreach (var direction in directions)
	        {
	            Vector3Int adjacentTile = currentTile + direction;

	            // Si la tuile adjacente n'a pas encore été visitée
	            if (!visitedTiles.Contains(adjacentTile))
	            {
                    // Ajouter la tuile adjacente à la file pour exploration
                    tileQueue.Enqueue(adjacentTile);

	                // Marquer cette tuile comme visitée
	                visitedTiles.Add(adjacentTile);
	            }
	        }
	    }
	    return TPTilesPosition;
    }

    //==================================================Gérer l'interaction des personnages avec les systèmes du vaisseau=======================================//
    public List<GameObject> interactionCharactersSystems(GameObject character, Vector3 characterPosition, Tilemap playerSystem)
    {
        // Convertir la position en Vector3Int (les Tilemaps utilisent des coordonnées entières)
        Vector3Int tilePosition = playerSystem.WorldToCell(characterPosition);
        
        // Récupérer la tuile à cette position
        TileBase tile = playerSystem.GetTile(tilePosition);
        
        if (tile != null && tile is CustomTile customTile)
        {
            // Vérifie si la tuile est une tuile de téléportation via le Scriptable Object
            if (customTile.customTileData.isTeleportTile)
            {
                SaveCharacterOnTeleporter(character);
            }
            else
            {
                RemoveCharacterOnTeleporter(character);
            }
        }
        else
        {
            RemoveCharacterOnTeleporter(character);
        }

        if(characterOnTP.Count == 0)
        {
        	TPbuttonTo.SetActive(false);
        }
        else
        {
        	TPbuttonTo.SetActive(true);
        }

        return characterOnTP;
    }

    //==================================================Sauvegarder les personnages sur le TP=======================================//
    public void SaveCharacterOnTeleporter(GameObject character)
    {
        if (character != null && !characterOnTP.Contains(character)) // Vérifie si l'objet n'est pas nul et n'est pas déjà dans la liste
        {
            characterOnTP.Add(character); // Ajoute l'objet à la liste
        }
    }

    //==================================================Retirer les personnages sur le TP=======================================//
    public void RemoveCharacterOnTeleporter(GameObject character)
    {
         if (characterOnTP.Contains(character)) // Vérifie si l'objet est dans la liste
        {
            characterOnTP.Remove(character); // Retire l'objet de la liste
        }
    }

}
