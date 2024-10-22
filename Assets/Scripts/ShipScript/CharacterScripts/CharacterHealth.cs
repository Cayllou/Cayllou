using UnityEngine;

/*
*   L'objectif de CharacterHealth est de Gerer la variable de santé du personnage ainsi que l'affichage de sa barre de vie.
* 	Il gère donc le gain ou la perte de santé ainsi que les conséquences d'une mauvaise hygiène de vie.. je veux dire d'une santé à 0.
*/

public class CharacterHealth : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////Initialisation////////////////////////////////////////////////////////////////////////////////////////////////////////

	//====================================================Initialisation des variables============================================//
    // Variables pour la gestion de la vie
    public int maxHealth = 100;
    private int currentHealth;

    // Références aux GameObjects pour les barres de vie
    public GameObject healthBarRed;  // Fond rouge
    public GameObject healthBarGreen; // Barre verte

    // Référence au Transform pour ajuster la taille et la position
    private Transform healthBarGreenTransform;

    // Largeur initiale de la barre verte pour calculer le décalage
    private float initialHealthBarWidth;

//====================================================Initialisation de la vie du personnage==========================================//

    void Start()
    {
        // Initialiser la vie du personnage
        currentHealth = maxHealth;

        // Obtenir le Transform de la barre verte
        healthBarGreenTransform = healthBarGreen.transform;

        // Stocker la largeur initiale de la barre verte
        initialHealthBarWidth = healthBarGreenTransform.localScale.x;

        // Mise à jour initiale de la barre de vie
        UpdateHealthBar();
    }

//////////////////////////////////////////////////////////////////////////////////////////Update////////////////////////////////////////////////////////////////////////////////////////////////////////

    //=======================================================Maj de la barre de vie=========================================================//
    private void UpdateHealthBar()
    {

        // Calculer la proportion de vie restante
        float healthPercentage = (float)currentHealth / maxHealth;

        // Ajuster la taille de la barre verte en fonction de la vie
        Vector3 localScale = healthBarGreenTransform.localScale;
        localScale.x = healthPercentage * initialHealthBarWidth;  // Ajuster la largeur en fonction de la vie
        healthBarGreenTransform.localScale = localScale;

        // Calculer la position du décalage pour aligner la barre verte avec l'extrémité gauche
        Vector3 localPosition = healthBarGreenTransform.localPosition;
        localPosition.x = -(initialHealthBarWidth - localScale.x)/ 2 *(float)0.94  ;  // Décaler vers la gauche en fonction de la taille
        healthBarGreenTransform.localPosition = localPosition;
    }

    //===========================================================Réduire la santé===================================//
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //===========================================================Augmenter la santé===================================//
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }
   
	//===========================================================Gestion de mort du personnage=============================================//
    private void Die()
    {
        Debug.Log("Le personnage est mort !");
        // Ajouter des actions pour gérer la mort
    }
}
