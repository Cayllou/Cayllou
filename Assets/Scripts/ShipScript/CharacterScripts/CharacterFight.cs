using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterFight : MonoBehaviour
{
	private List<KeyValuePair<GameObject, Vector3>> enemyTileList = new List<KeyValuePair<GameObject, Vector3>>();
    private bool superposed = true;
    private bool CharacterMovedUpRight = false;
    private bool CharacterMovedDownLeft = false;

	public int positionCACRandomValue; // Valeur aléatoire propre à chaque personnage

	void Start()
	{
		positionCACRandomValue = Random.Range(0, 100);
	}

	public KeyValuePair<GameObject, Vector3> DetectEnemiesOnTile(Vector3 characterPosition, Tilemap groundTilemap, string team)
	{
		Vector3Int tilePosition = groundTilemap.WorldToCell(characterPosition); // Convertit les coordonnées mondiales en coordonnées de cellules Tilemap
		return CharacterLockingTile.Instance.IsTileEnemyFree(tilePosition, groundTilemap, team);
	}

	//========================================================Recherche d'une tuile alternative dans la même salle==================================================//
    public List<KeyValuePair<GameObject, Vector3>> DetectEnemiesInRoom(Vector3 characterPosition, Tilemap groundTilemap, Tilemap wallTilemap, string spacecraft, string team)
    {
    	if(enemyTileList != null)
    	{
    		enemyTileList.Clear();
    	}

    	Vector3Int startTile = groundTilemap.WorldToCell(characterPosition); // Convertit les coordonnées mondiales en coordonnées de cellules Tilemap

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

        Queue<Vector3Int> tileQueueOutsideRoom  = new Queue<Vector3Int>();

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
            KeyValuePair<GameObject, Vector3> opponentData = CharacterLockingTile.Instance.IsTileEnemyFree(currentTile, groundTilemap, team);
            if (opponentData.Key != null)
            {
                // Si on trouve une tuile libre dans la salle, on la retourne
                enemyTileList.Add(opponentData);
            }

            // Exploration des tuiles adjacentes dans les 4 directions
            foreach (var direction in directions)
            {
                Vector3Int adjacentTile = currentTile + direction;

                // Si la tuile adjacente n'a pas encore été visitée
                if (!visitedTiles.Contains(adjacentTile))
                {
                    // Vérifier s'il n'y a pas un mur entre la tuile courante et la tuile adjacente
                    if (!IsBlocked(currentTile, adjacentTile, groundTilemap, wallTilemap, spacecraft))
                    {
                        // Ajouter la tuile adjacente à la file pour l'explorer plus tard
                        tileQueue.Enqueue(adjacentTile);
                    }
                    else if(IsBlocked(currentTile, adjacentTile, groundTilemap, wallTilemap, spacecraft))
                    {
                        // Ajouter la tuile adjacente à la file pour l'explorer plus tard
                        tileQueueOutsideRoom.Enqueue(adjacentTile);
                    }

                    // Marquer cette tuile comme visitée
                    visitedTiles.Add(adjacentTile); 
                }
            }
        }
        // Si aucune tuile libre n'a été trouvée dans la salle, retourner la tuile de départ
        return enemyTileList;
    }  

    //========================================================trouve les murs de la piece==================================================//
    private bool IsBlocked(Vector3Int currentCell, Vector3Int neighborCell, Tilemap groundTilemap, Tilemap wallTilemap, string spacecraft)
    {
        Vector3 currentPos;
        Vector3 neighborPos;

         // Calculer la position entre deux tuiles adjacentes
        currentPos = currentCell + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);
        neighborPos = neighborCell + new Vector3(groundTilemap.cellSize.x / 2f, groundTilemap.cellSize.y / 2f, 0);

        Vector3 wallCheckPos = (currentPos + neighborPos) / 2; // Milieu entre les deux tuiles

        // Convertir la position du mur en coordonnées de cellule
        Vector3Int wallCell = wallTilemap.WorldToCell(wallCheckPos);
        
        // Vérifier si une tuile Wall est présente
        return wallTilemap.HasTile(wallCell);
    }

	public bool proximityFight_UnsuperposeCharacters(KeyValuePair<GameObject, Vector3> opponentData) //decaler les personnages pour eviter la superposition
	{
		GameObject opponentCharacter  = opponentData.Key;
		Vector3 position = opponentData.Value;

		CharacterFight opponentFightScript = opponentCharacter.GetComponent<CharacterFight>();

    	if(positionCACRandomValue == opponentFightScript.positionCACRandomValue)
    	{
    		positionCACRandomValue = Random.Range(0, 100);
    	}
		else if (positionCACRandomValue > opponentFightScript.positionCACRandomValue)
        {
            // Ce personnage se décale en haut à droite
            MoveUpRight();
            CharacterMovedUpRight = true;
            superposed = false;
        }
        else if (positionCACRandomValue < opponentFightScript.positionCACRandomValue)
        {
            // Ce personnage se décale en bas à gauche
            MoveDownLeft();
            CharacterMovedDownLeft = true;
            superposed = false;
        }
		return superposed;
	}

    public bool proximityFight_ReCentreCharacters(KeyValuePair<GameObject, Vector3> opponentData)
    {
        GameObject opponentCharacter  = opponentData.Key;
        Vector3 position = opponentData.Value;

        if (CharacterMovedUpRight)
        {
            //Recentre le personnage
            MoveDownLeft();
            superposed = true;
        }
        else if (CharacterMovedDownLeft)
        {
            //Recentre le personnage
            MoveUpRight();
            superposed = true;
        }
        return superposed;
    }

	private void MoveDownLeft()
    {
        // Trouver l'enfant "Character Sprite"
        Transform characterSpriteTransform = gameObject.transform.Find("Character Sprite");
        if (characterSpriteTransform != null)
        {
            // Code pour déplacer ce personnage à gauche
            characterSpriteTransform.transform.position = characterSpriteTransform.transform.position + new Vector3(-0.25f, -0.25f, 0);
        }
        else
        {
            Debug.LogWarning($"L'enfant 'Character Sprite' n'a pas été trouvé dans {gameObject.name}");
        }
    }

    private void MoveUpRight()
    {
        // Trouver l'enfant "Character Sprite"
        Transform characterSpriteTransform = gameObject.transform.Find("Character Sprite");
        if (characterSpriteTransform != null)
        {
            // Code pour déplacer ce personnage à droite
            characterSpriteTransform.transform.position = characterSpriteTransform.transform.position + new Vector3(0.25f, 0.25f, 0); // Par exemple
        }
        else
        {
            Debug.LogWarning($"L'enfant 'Character Sprite' n'a pas été trouvé dans {gameObject.name}");
        }
    }

    public void AttackCAC(KeyValuePair<GameObject, Vector3> opponentData)
    {    
        GameObject opponentCharacter  = opponentData.Key;
        Vector3 position = opponentData.Value;

        CharacterHealth opponentCharacterHealth = opponentCharacter.GetComponent<CharacterHealth>(); //identifier la cible de l'attaque

        int damage = 10;
        opponentCharacterHealth.TakeDamage(damage); //envoyer les dégats à la cible (appeler charactherHealth)
    }
    
    /*
    public void DistanceFight(List<KeyValuePair<GameObject, Vector3>> opponentsInRoom)
    {
        GameObject aimOpponent = null;
        Vector3 targetPosition = Vector3.zero;
        int shootOpponent = 0; //variable de priorité de cible

        // Parcourir les adversaires dans la salle
        foreach (var opponentPair in opponentsInRoom)
        {
            GameObject opponent = opponentPair.Key;
            Vector3 position = opponentPair.Value;

            // Déterminer la priorité d'attaque
            int priorityTest = Random.Range(0, 100);

            // Choisir l'adversaire avec la priorité la plus élevée
            if (priorityTest > shootOpponent)
            {
                shootOpponent = priorityTest;

                aimOpponent = opponent;
                targetPosition = position;
            }
        }

        // Si un adversaire a été choisi
        if (aimOpponent != null)
        {
            // Tirer dans la direction de l'adversaire sélectionné
            // Appeler la fonction d'attaque ici
            ShootProjectile(targetPosition, aimOpponent);
        }
    }
    
    private void ShootProjectile(Vector3 targetPosition)
    {
        // Instancier le projectile à la position de tir
        GameObject projectile = Instantiate(projectilePrefab, shootPosition.position, Quaternion.identity);

        // Calculer la direction du tir
        Vector3 shootDirection = (targetPosition - shootPosition.position).normalized;

        // Appliquer la vitesse au projectile (en supposant qu'il possède un Rigidbody)
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb != null)
        {
            projectileRb.velocity = shootDirection * projectileSpeed;
        }
        else
        {
            // Si pas de Rigidbody, on peut aussi directement ajuster la position dans une autre méthode Update
            projectile.transform.position += shootDirection * projectileSpeed * Time.deltaTime;
        }
    }
    */
}

