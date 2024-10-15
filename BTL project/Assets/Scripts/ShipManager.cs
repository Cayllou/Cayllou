using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
*   L'objectif de ShipManager est de faire le lien entre le joueur et l'interface du vaisseau, 
*   c'est à dire le vaisseau lui même, ses systemes, ses personnages 
*   et ses interractions avec la zone de combat et des vaisseaux ennemis.
*/

public class ShipManager : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Initialisation des variables============================================//
    private Camera mainCamera;
    public TileManager tileManager;
    // Variables pour la sélection multiple
    private bool isSelecting = false;
    private Vector3 mouseStartPos;
    private Rect selectionRect;
    private Texture2D selectionTexture; // Pour dessiner le rectangle de sélection

    private GameObject[] allCharacters;
    private List<GameObject> ListCharacters = new List<GameObject>();
    private List<GameObject> highlightedCharacters = new List<GameObject>(); // Personnages en surbrillance
    public List<GameObject> selectedCharacters = new List<GameObject>(); // Liste de personnages sélectionnés

    //====================================================Initialisation des dépendances============================================//
    void Start()
    {
        mainCamera = Camera.main;

        tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("TileManager n'a pas été trouvé !");
        }

        // Crée une texture blanche pour le rectangle de sélection
        selectionTexture = new Texture2D(1, 1);
        selectionTexture.SetPixel(0, 0, Color.green);
        selectionTexture.Apply();

        // Démarrer une coroutine qui attend un frame avant d'exécuter le reste
        StartCoroutine(LateStartCoroutine());
    }

    //====================================================Initialisation des personnages============================================//
    IEnumerator LateStartCoroutine() // Utilise IEnumerator sans générique
    {
        // Attendre la fin de ce frame pour être sûr que tout est initialisé
        yield return null;
        tileManager.InitializeTilemaps();

        allCharacters = GameObject.FindGameObjectsWithTag("Character");
        foreach(GameObject character in allCharacters)
        {
            ListCharacters.Add(character);
        }
        tileManager.InitializeCharacters(ListCharacters);
    }

//////////////////////////////////////////////////////////////////////////////////////////MainUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Boucle principale du jeu============================================//
    void Update()
    {
        HandleSelectionInput();
    }

    //====================================================Methode de selection des personnages============================================//
    void HandleSelectionInput()
    {
        // Début du clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            mouseStartPos = Input.mousePosition;
            allCharacters = GameObject.FindGameObjectsWithTag("Character");
        }

        // Pendant que le clic gauche est maintenu
        if (Input.GetMouseButton(0) && isSelecting)
        {
            DrawSelectionRectangle();
            HighlightCharactersInSelection();
        }

        // Relâchement du clic gauche
        if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            // Si la souris n'a pas beaucoup bougé, on considère que c'est un clic simple
            if (Vector3.Distance(mouseStartPos, Input.mousePosition) < 5f)
            {
                SelectSingleCharacter();
            }
            else
            {
                SelectCharactersInRectangle();
            }
            tileManager.SelectedCharacterList(selectedCharacters, ListCharacters);
            isSelecting = false;
        }
    }
//////////////////////////////////////////////////////////////////////////////////////////FonctionsUpdate////////////////////////////////////////////////////////////////////////////////////////////////////////

    //====================================================Creation mathématique du rectangle de sélection============================================//
    void DrawSelectionRectangle()
    {
        Vector3 mouseCurrentPos = Input.mousePosition;

        // Création du rectangle à partir du point de départ et de la position actuelle de la souris
        selectionRect = new Rect(
            Mathf.Min(mouseStartPos.x, mouseCurrentPos.x),
            Mathf.Min(mouseStartPos.y, mouseCurrentPos.y),
            Mathf.Abs(mouseStartPos.x - mouseCurrentPos.x),
            Mathf.Abs(mouseStartPos.y - mouseCurrentPos.y)
        );
    }

    //====================================================Affichage en temps réel============================================//
    void OnGUI()
    {   
        if (isSelecting)
        {
            // Dessiner uniquement les bords du rectangle de sélection
            DrawSelectionBorders(selectionRect, 2f); // 2f est l'épaisseur des bords, ajustable
        }
    }
    //====================================================Affichage du rectangle de sélection============================================//
    void DrawSelectionBorders(Rect rect, float thickness)
    {
        // Dessiner les bords en noir (modifiable à volonté)
        Color borderColor = Color.black;
        
        // Crée une texture unie pour les bords
        Texture2D borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, borderColor);
        borderTexture.Apply();

        // Haut
        GUI.DrawTexture(new Rect(rect.xMin, Screen.height - rect.yMax, rect.width, thickness), borderTexture);
        
        // Bas
        GUI.DrawTexture(new Rect(rect.xMin, Screen.height - rect.yMin, rect.width, thickness), borderTexture);
        
        // Gauche
        GUI.DrawTexture(new Rect(rect.xMin, Screen.height - rect.yMax, thickness, rect.height), borderTexture);
        
        // Droite
        GUI.DrawTexture(new Rect(rect.xMax - thickness, Screen.height - rect.yMax, thickness, rect.height), borderTexture);
    }

    //=========================================================Methode de Surbrillance de préselection=======================================//
    void HighlightCharactersInSelection()
    {
        // On déséclaire tous les personnages surlignés précédemment
        foreach (GameObject character in highlightedCharacters)
        {
            UnhighlightCharacter(character);
        }
        highlightedCharacters.Clear();

        foreach (GameObject character in allCharacters)
        {
            // Convertit la position du personnage en coordonnées d'écran
            Vector3 characterScreenPos = mainCamera.WorldToScreenPoint(character.transform.position);

            // Si le personnage est dans le rectangle de sélection
            if (selectionRect.Contains(characterScreenPos, true))
            {
                HighlightCharacter(character); // Met le personnage en surbrillance
                highlightedCharacters.Add(character);
            }
        }
    }
    
    void HighlightCharacter(GameObject character)
    {
        // Logique pour éclairer temporairement un personnage (par exemple, changer la couleur ou l'effet visuel)
        character.GetComponent<SpriteRenderer>().color = Color.yellow; // Exemple : changer la couleur en jaune
    }
    //=========================================================Annulation de la surbrillance d'un personnage=======================================//
    void UnhighlightCharacter(GameObject character)
    {
        // Logique pour enlever l'effet visuel temporaire
        character.GetComponent<SpriteRenderer>().color = Color.blue; // Revenir à la couleur par défaut (bleue)
    }

    //========================================================Selection d'un personnage unique par clic simple==========================================//
    void SelectSingleCharacter()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != null && hit.collider.CompareTag("Character"))
        {
            GameObject clickedCharacter = hit.collider.gameObject;

            bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

            if (!isShiftHeld) // Si Shift n'est pas maintenu, désélectionner tout
            {
                DeselectAllCharacters();
            }

            // Sélectionner le personnage cliqué
            if (!selectedCharacters.Contains(clickedCharacter))
            {
                //SelectCharacter(clickedCharacter);
                clickedCharacter.GetComponent<SpriteRenderer>().color = Color.green; // Exemple : changer la couleur en vert pour les personnages sélectionnés
                selectedCharacters.Add(clickedCharacter);
            }
        }
        else
        {
            // Si aucun personnage n'est cliqué, on désélectionne tout sauf si Shift est maintenu
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectAllCharacters();
            }
        }
    }

    //========================================================Selection de personnages multiples par rectangle==========================================//
    void SelectCharactersInRectangle()
    {
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (!isShiftHeld) // Si Shift n'est pas maintenu, désélectionner tout
        {
            DeselectAllCharacters();
        }

        // Sélectionne tous les personnages dans le rectangle
        foreach (GameObject character in highlightedCharacters)
        {
            if (!selectedCharacters.Contains(character))
            {
                //SelectCharacter(character);
                character.GetComponent<SpriteRenderer>().color = Color.green; // Exemple : changer la couleur en vert pour les personnages sélectionnés
                selectedCharacters.Add(character);
            }
        }

        highlightedCharacters.Clear(); // Réinitialise la surbrillance
    }

    //========================================================déselection des personnages==========================================//
    void DeselectAllCharacters()
    {
        foreach (GameObject character in selectedCharacters)
        {
            //DeselectCharacter(character);
            character.GetComponent<SpriteRenderer>().color = Color.blue; // Revenir à la couleur par défaut (bleue)
        }
        selectedCharacters.Clear();
    }
}
