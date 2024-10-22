using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorIcon : MonoBehaviour
{
    // Texture pour le curseur personnalisé
    public Sprite TPToIconCursorSprite;

    // Position de point chaud du curseur (souvent 0,0 pour le coin supérieur gauche)
    public Vector2 hotSpot = Vector2.zero;

    // Type de curseur par défaut
    public CursorMode cursorMode = CursorMode.Auto;

    private Texture2D TPToIconCursorTexture;

    private void Start()
    {
        // Convertir le Sprite en Texture2D
        if (TPToIconCursorSprite != null)
        {
            TPToIconCursorTexture = SpriteToTexture2D(TPToIconCursorSprite);
        }
    }

    // Méthode pour changer le curseur
    public void ChangeCursorToTPToIcon()
    {
        // Change le curseur à la texture personnalisée
        Cursor.SetCursor(TPToIconCursorTexture, hotSpot, cursorMode);
    }

    // Méthode pour réinitialiser le curseur à l'original
    public void ResetCursorToDefault()
    {
        // Réinitialise le curseur à la texture par défaut
        Cursor.SetCursor(null, hotSpot, cursorMode);
    }

    private Texture2D SpriteToTexture2D(Sprite sprite)
	{
	    if (sprite == null)
	    {
	        Debug.LogError("Le Sprite est nul !");
	        return null;
	    }

	    // Créer une RenderTexture et copier le Sprite dessus
	    RenderTexture renderTex = RenderTexture.GetTemporary(
	        (int)sprite.rect.width,
	        (int)sprite.rect.height,
	        0,
	        RenderTextureFormat.Default,
	        RenderTextureReadWrite.Linear);

	    Graphics.Blit(sprite.texture, renderTex);

	    // Sauvegarder l'ancienne RenderTexture active et la définir
	    RenderTexture previous = RenderTexture.active;
	    RenderTexture.active = renderTex;

	    // Créer une nouvelle texture 2D
	    Texture2D readableTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
	    readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
	    readableTexture.Apply();

	    // Restaurer la RenderTexture active
	    RenderTexture.active = previous;
	    RenderTexture.ReleaseTemporary(renderTex);

	    return readableTexture;
	}
}
