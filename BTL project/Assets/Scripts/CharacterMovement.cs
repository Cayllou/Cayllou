using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
*   L'objectif de CharacterMovement est de s'occuper des déplacements d'un personnage en particulier. 
*   Il détermine quand déplacer le personnage, utilise un script de pathfinding afin de déterminer le chemin puis enclenche le mouvement de celui-ci en verifiant l'accès à la tuile
*/

public class CharacterMovement : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//
    private Camera mainCamera;				          // Référence à la caméra principale
    private Pathfinding2D pathfinding;		            // Référence au script Pathfinding2D pour executer le déplacement du personnage

    private List<Vector3> currentPath = new List<Vector3>(); // Le chemin que le personnage doit suivre
    public float moveSpeed = 5f;            // La vitesse de déplacement du personnage
    
    private Vector3Int targetTilePosition;     // La case finale (cible) que le personnage veut atteindre
    private Vector3 currentTilePosition;    // La position de la case actuelle du personnage
    private Vector3 lockedTile;          // La tuile actuellement verrouillée par ce personnage

//=======================================================Initialisation, et récupération des tilemaps=======================================//

    void Start()
    {
        // Initialiser la caméra principale si elle n'est pas assignée
        if (mainCamera == null)
        {
            mainCamera = Camera.main;  // Assigne la caméra principale de la scène
        }
        // Vérifier que le pathfinding est assigné (si tu utilises un composant ou un script attaché)
        pathfinding = GetComponent<Pathfinding2D>();
        if (pathfinding == null)
        {
            Debug.LogError("Pathfinding2D script is missing on this GameObject.");
        }
        
    }

    public void InitialisePosition(Tilemap groundTilemap, Tilemap wallTilemap, Tilemap doorTilemap, string team)
    {
        if(team == "Enemy")
        {
            currentTilePosition = groundTilemap.LocalToCell(transform.position + new Vector3(1, 0, 0));
        }
        else
        {
            // Obtenir la position actuelle du personnage et la convertir en coordonnées de tuile
            currentTilePosition = groundTilemap.LocalToCell(transform.position);
        }

        lockedTile = currentTilePosition; // Initialement, la tuile verrouillée est celle sur laquelle le personnage commence.

        // Verrouiller la tuile actuelle où le personnage est positionné
        CharacterLockingTile.Instance.LockTile(currentTilePosition, groundTilemap); // Utilise la tilemap appropriée en fonction de l'équipe
    }

//////////////////////////////////////////////////////////////////////////////////////////MainUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

//=====================================================Gestion des mouvements du personnage===================================================//

    public List<Vector3> DetectRightClick(Tilemap groundTilemap, Tilemap wallTilemap, Tilemap doorTilemap, string team)
    {
        // Vérifie si l'utilisateur fait un clic droit (bouton de la souris 1)
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Assurez-vous que la position est sur le plan 2D

            // Appelle la méthode FindPath de Pathfinding2D pour trouver un chemin
            List<Vector3> path3D = pathfinding.FindPath(transform.position, mousePosition, groundTilemap, wallTilemap, doorTilemap, team);

            // Si un chemin est trouvé, démarrer le mouvement
            if (path3D != null && path3D.Count > 0)
            {
                // Convertir la dernière position (cible) en position de tuile
                Vector3 targetWorldPos = path3D[path3D.Count - 1];

                // Déterminer l'équipe du personnage (joueur ou ennemi)
                if(team == "Enemy")
                {
                    targetTilePosition = groundTilemap.LocalToCell(targetWorldPos + new Vector3(1, 0, 0));
                }
                else
                {
                    // Obtenir la position actuelle du personnage et la convertir en coordonnées de tuile
                    targetTilePosition = groundTilemap.LocalToCell(targetWorldPos);

                }

                // Vérifier si la case de destination est libre
                if (CharacterLockingTile.Instance.IsTileFree(targetTilePosition, groundTilemap))
                {
                    // Déverrouiller l'ancienne tuile verrouillée
                    if (lockedTile != null)
                    {
                        CharacterLockingTile.Instance.UnlockTile(lockedTile, groundTilemap);
                    }

                    // Verrouiller la tuile de destination
                    CharacterLockingTile.Instance.LockTile(targetTilePosition, groundTilemap);
                    lockedTile = targetTilePosition;  // Mettre à jour la tuile verrouillée

                    // Définir le chemin et commencer le déplacement
                    currentPath = path3D;
                }
                else
                {
                    // Recherche une tuile adjacente valide
                    Vector3Int alternativeTile = FindAlternativeTileInRoom(targetTilePosition, groundTilemap, wallTilemap, team);
                    if (alternativeTile != targetTilePosition) // Vérifie si une alternative a été trouvée
                    {
                        // Si une alternative a été trouvée, trouver le chemin vers celle-ci
                        currentPath = pathfinding.FindPath(transform.position, groundTilemap.CellToLocal(alternativeTile), groundTilemap, wallTilemap, doorTilemap, team);
                        
                        // Déverrouiller l'ancienne tuile verrouillée
                        if (lockedTile != null)
                        {
                            CharacterLockingTile.Instance.UnlockTile(lockedTile, groundTilemap);
                        }

                        // Verrouiller la nouvelle tuile de destination
                        CharacterLockingTile.Instance.LockTile(alternativeTile, groundTilemap);
                        lockedTile = alternativeTile;  // Mettre à jour la tuile verrouillée
                    }
                    else
                    {
                        Debug.Log("Aucune tuile de destination accessible !");
                    }
                }
            }
        }
        return currentPath;
    }


    //========================================================Recherche d'une tuile alternative dans la même salle==================================================//
    public Vector3Int FindAlternativeTileInRoom(Vector3Int startTile, Tilemap groundTilemap, Tilemap wallTilemap, string team)
    {
        // Initialisation des directions des tuiles adjacentes (4 directions cardinales)
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(-1, 0, 0), // gauche
            new Vector3Int(1, 0, 0),  // droite
            new Vector3Int(0, -1, 0), // bas
            new Vector3Int(0, 1, 0)   // haut
        };

        // Liste des tuiles déjà explorées (pour éviter les répétitions)
        HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>();

        // File pour stocker les tuiles à explorer (commence par la tuile de départ)
        Queue<Vector3Int> tileQueue = new Queue<Vector3Int>();
        tileQueue.Enqueue(startTile);

        // Marquer la tuile de départ comme visitée
        visitedTiles.Add(startTile);

        int explorationLimit = 0;

        // Tant qu'il y a des tuiles à explorer dans la file
        while (tileQueue.Count > 0 && explorationLimit < 100)
        {
            explorationLimit = explorationLimit + 1; 
            // Extraire la tuile courante de la file
            Vector3Int currentTile = tileQueue.Dequeue();

            // Vérifier si la tuile courante est libre et non bloquée
            if (CharacterLockingTile.Instance.IsTileFree(currentTile, groundTilemap))
            {
                // Si on trouve une tuile libre dans la salle, on la retourne
                return currentTile;
            }

            // Exploration des tuiles adjacentes dans les 4 directions
            foreach (var direction in directions)
            {
                Vector3Int adjacentTile = currentTile + direction;

                // Si la tuile adjacente n'a pas encore été visitée
                if (!visitedTiles.Contains(adjacentTile))
                {
                    // Vérifier s'il n'y a pas un mur entre la tuile courante et la tuile adjacente
                    if (!IsBlocked(currentTile, adjacentTile, groundTilemap, wallTilemap, team))
                    {
                        // Ajouter la tuile adjacente à la file pour l'explorer plus tard
                        tileQueue.Enqueue(adjacentTile);
                    }

                    // Marquer cette tuile comme visitée
                    visitedTiles.Add(adjacentTile);
                }
            }
        }

        // Si aucune tuile libre n'a été trouvée dans la salle, retourner la tuile de départ
        return startTile;
    }  

//////////////////////////////////////////////////////////////////////////////////////////FonctionUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //========================================================Déplace le personnage le long du chemin trouvé==================================================//
    public List<Vector3> MoveAlongPath()
    {
        if (currentPath.Count > 0)
        {
            //for (int i = 0; i < currentPath.Count; i++)
            //{
            //    Debug.Log("currentPath[" + i + "] : " + currentPath[i]);
            //}
            //Debug.Log("currentPath.Count = " + currentPath.Count);
            Vector3 targetPosition = currentPath[0]; // Prochaine position cible dans le chemin
            //Debug.Log("targetPosition = " + targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            //Debug.Log("transform.position = " + transform.position);
            // Si le personnage atteint la prochaine position, on passe à la suivante
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPath.RemoveAt(0); // Supprime la première position atteinte
            }

            // Si on a atteint la dernière position, on arrête le déplacement
            if (currentPath.Count == 0)
            {
                currentTilePosition = targetTilePosition;  // Met à jour la position actuelle
                Debug.Log("Arrivé à destination.");
            }
        }
        return currentPath;
    }

    //========================================================trouve les murs de la piece==================================================//
    private bool IsBlocked(Vector3Int currentCell, Vector3Int neighborCell, Tilemap groundTilemap, Tilemap wallTilemap, string team)
    {
        Vector3 currentPos;
        Vector3 neighborPos;

        if (team == "Enemy") // verifie si on est dans le cas du vaisseau en rotation à 90°
        {
             // Calculer la position entre deux tuiles adjacentes
            currentPos = currentCell + new Vector3(groundTilemap.cellSize.x / 2f - 1, groundTilemap.cellSize.y / 2f, 0);
            neighborPos = neighborCell + new Vector3(groundTilemap.cellSize.x / 2f - 1, groundTilemap.cellSize.y / 2f, 0);
        }
        else
        {
             // Calculer la position entre deux tuiles adjacentes
            currentPos = currentCell + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
            neighborPos = neighborCell + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        }

        Vector3 wallCheckPos = (currentPos + neighborPos) / 2; // Milieu entre les deux tuiles

        // Convertir la position du mur en coordonnées de cellule
        Vector3Int wallCell = wallTilemap.WorldToCell(wallCheckPos);
        
        // Vérifier si une tuile Wall est présente
        return wallTilemap.HasTile(wallCell);
    }
}