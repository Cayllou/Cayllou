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
	private HashSet<Vector3> occupiedTiles = new HashSet<Vector3>(); // Set pour suivre les tuiles occupées

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
    public bool IsTileFree(Vector3 tilePosition, Tilemap groundTilemap)
    {
        Vector3Int cellPosition = groundTilemap.WorldToCell(tilePosition);

        //Debug.Log("occupiedTiles.Contains(tilePosition) : " + occupiedTiles.Contains(tilePosition));
        return !occupiedTiles.Contains(tilePosition) && groundTilemap.HasTile(cellPosition);
    }

    //====================================================Verrouille une tuile============================================//
    public void LockTile(Vector3 tilePosition, Tilemap groundTilemap)
    {
            occupiedTiles.Add(tilePosition);
            ChangeTileColor(tilePosition, Color.red, groundTilemap); // Change la couleur de la tuile en rouge lorsqu'elle est occupée
    }

    //====================================================Déverrouille une tuile============================================//
    public void UnlockTile(Vector3 tilePosition, Tilemap groundTilemap)
    {
            occupiedTiles.Remove(tilePosition);
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
}
