using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
*   L'objectif de Pathfinding2D est de déterminer le potentiel chemin le plus court entre le personnage est la destination.
*/

public class Pathfinding2D : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//
    private List<Vector3> path = new List<Vector3>(); // Liste des positions du chemin que le personnage doit suivre

//////////////////////////////////////////////////////////////////////////////////////////Update////////////////////////////////////////////////////////////////////////////////////////////////////////

    //==========================================================Processus principal du pathfinding==========================================//
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, Tilemap groundTilemap, Tilemap wallTilemap, Tilemap doorTilemap, string team)
    {
        Vector3Int startCell = groundTilemap.WorldToCell(startPos); // Convertit les coordonnées mondiales en coordonnées de cellules Tilemap
        Vector3Int targetCell = groundTilemap.WorldToCell(targetPos);

        // Vérifie si la destination est une case valide (GroundTile)
        if (groundTilemap.HasTile(targetCell))
        {
        	// Centre la destination sur la tuile cible
	        if (team == "Enemy") // verifie si on est dans le cas du vaisseau en rotation à 90°
	        {
	        	Vector3 centeredTargetPos = groundTilemap.CellToWorld(targetCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
	        }
	        else
	        {
	        	Vector3 centeredTargetPos = groundTilemap.CellToWorld(targetCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
	        }
             // Calcule le chemin en utilisant l'algorithme A*
        	List<Vector3> path = AStarAlgorithm(startCell, targetCell, groundTilemap, wallTilemap, doorTilemap, team);

            if (path != null && path.Count > 0)
            {
                Debug.Log("Chemin trouvé !");
                return path; // Retourne le chemin trouvé
            }
            else
            {
                Debug.Log("Erreur : pas de chemin trouvé !");
                return null;
            }
        }
        else
        {
            Debug.Log("Erreur : la destination n'est pas une GroundTile valide !");
            return null;
        }
    }

    //====================================================Algorithme A*============================================//
    List<Vector3> AStarAlgorithm(Vector3Int startCell, Vector3Int targetCell, Tilemap groundTilemap, Tilemap wallTilemap, Tilemap doorTilemap, string team)
    {
        // Set de cellules déjà explorées
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        // Set de cellules à explorer
        HashSet<Vector3Int> openSet = new HashSet<Vector3Int> { startCell };

        // Dictionnaire pour stocker le chemin
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        // Coût total du début jusqu'à cette cellule
        Dictionary<Vector3Int, float> gCost = new Dictionary<Vector3Int, float>();
        gCost[startCell] = 0;

        // Coût total estimé du début jusqu'à la fin (fCost = gCost + hCost)
        Dictionary<Vector3Int, float> fCost = new Dictionary<Vector3Int, float>();
        fCost[startCell] = Heuristic(startCell, targetCell);


        // Tant que l'ensemble des cellules à explorer n'est pas vide
        while (openSet.Count > 0)
        {
        	// Trouve la cellule avec le coût total (fCost) le plus bas
            Vector3Int currentCell = GetCellWithLowestFCost(openSet, fCost);
            // Si on est arrivé à la cellule cible
            if (currentCell == targetCell)
            {
                return ReconstructPath(cameFrom, currentCell, groundTilemap, team);
            } 

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            // Parcourt les voisins de la cellule actuelle
            foreach (Vector3Int neighbor in GetNeighbors(currentCell))
            {
                if (closedSet.Contains(neighbor) || !groundTilemap.HasTile(neighbor) || (IsWallBetween(currentCell, neighbor, groundTilemap, wallTilemap, team) && !IsDoorBetween(currentCell, neighbor, groundTilemap, doorTilemap, team)))
                {
                    continue; // Ignore les cellules déjà explorées ou non valides (pas des GroundTiles)
                }

                // Calcul du coût g pour cette cellule voisine
                float tentativeGCost = gCost[currentCell] + Vector3Int.Distance(currentCell, neighbor);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor); // Découvrir une nouvelle cellule
                }
                else if (tentativeGCost >= gCost[neighbor])
                {
                    continue; // Ce n'est pas un chemin plus court
                }

                // Mise à jour du chemin optimal pour cette cellule
                cameFrom[neighbor] = currentCell;
                gCost[neighbor] = tentativeGCost;
                fCost[neighbor] = gCost[neighbor] + Heuristic(neighbor, targetCell);
            }
        }
        // Si on termine la boucle sans avoir trouvé la destination, aucun chemin n'est disponible
		Debug.Log("Erreur : chemin introuvable");
        return null; // Pas de chemin trouvé
    }

    //==============================================Formule de distance de Manhattan en 2D==============================//
    float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    //==============================================Récupère la cellule avec le fCost le plus faible====================//
    Vector3Int GetCellWithLowestFCost(HashSet<Vector3Int> openSet, Dictionary<Vector3Int, float> fCost)
    {
        Vector3Int lowestCell = default;
        float lowestFCost = float.MaxValue;

        foreach (Vector3Int cell in openSet)
        {
            float cost = fCost.ContainsKey(cell) ? fCost[cell] : float.MaxValue;

            if (cost < lowestFCost)
            {
                lowestFCost = cost;
                lowestCell = cell;
            }
        }

        return lowestCell;
    }

    //==============================================Reconstruit le chemin=================================================//
	List<Vector3> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int currentCell, Tilemap groundTilemap, string team)
	{
	    List<Vector3> path = new List<Vector3>();

	    while (cameFrom.ContainsKey(currentCell))
	    {
	        
	    	//Conversion de la cellule en position mondiale, avec un décalage pour centrer sur la tuile
	        if (team == "Enemy") // verifie si on est dans le cas du vaisseau en rotation à 90°
	        {
	        	Vector3 cellCenterPosition = groundTilemap.CellToWorld(currentCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
		        path.Add(cellCenterPosition);
		        currentCell = cameFrom[currentCell];
	        }
	        else
	        {
	        	Vector3 cellCenterPosition = groundTilemap.CellToWorld(currentCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
		        path.Add(cellCenterPosition);
		        currentCell = cameFrom[currentCell];
	        }
	    }

	    path.Reverse(); // On retourne le chemin pour qu'il aille de la position de départ à la destination
	    return path;
	}

    //==============================================liste des cellules voisines adjacentes=================================================//
    List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            new Vector3Int(cell.x + 1, cell.y, cell.z),
            new Vector3Int(cell.x - 1, cell.y, cell.z),
            new Vector3Int(cell.x, cell.y + 1, cell.z),
            new Vector3Int(cell.x, cell.y - 1, cell.z)
        };

        return neighbors;
    }

    //==============================================Vérifie s'il y a un mur entre deux tuiles adjacentes=================================================//
    public bool IsWallBetween(Vector3Int currentCell, Vector3Int neighborCell, Tilemap groundTilemap, Tilemap wallTilemap, string team)
    {
    	Vector3 currentPos;
    	Vector3 neighborPos;
    	Vector3 wallCheckPos;
    	Vector3Int wallCell;

    	if (team == "Enemy") // verifie si on est dans le cas du vaisseau en rotation à 90°
	    {
	    	currentPos = groundTilemap.CellToWorld(currentCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        	neighborPos = groundTilemap.CellToWorld(neighborCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);

	    }
	    else
	    {
	    	currentPos = groundTilemap.CellToWorld(currentCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        	neighborPos = groundTilemap.CellToWorld(neighborCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);

	    }

	    wallCheckPos = (currentPos + neighborPos) / 2; // Milieu entre les deux tuiles

        wallCell = wallTilemap.WorldToCell(wallCheckPos);

        // Vérifier si une tuile Wall est présente
        return wallTilemap.HasTile(wallCell);

    }

    //==============================================Vérifie s'il y a une porte entre deux tuiles adjacentes=================================================//
    bool IsDoorBetween(Vector3Int currentCell, Vector3Int neighborCell, Tilemap groundTilemap, Tilemap doorTilemap, string team)
    {
    	Vector3 currentPos;
    	Vector3 neighborPos;
    	Vector3 doorCheckPos;
    	Vector3Int doorCell;

    	if (team == "Enemy") // verifie si on est dans le cas du vaisseau en rotation à 90°
	    {
	    	currentPos = groundTilemap.CellToWorld(currentCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        	neighborPos = groundTilemap.CellToWorld(neighborCell) + new Vector3(-groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
	    }
	    else
	    {
	    	currentPos = groundTilemap.CellToWorld(currentCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        	neighborPos = groundTilemap.CellToWorld(neighborCell) + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
	    }

	    doorCheckPos = (currentPos + neighborPos) / 2; // Milieu entre les deux tuiles

        doorCell = doorTilemap.WorldToCell(doorCheckPos);

        return doorTilemap.HasTile(doorCell);
    }
}
