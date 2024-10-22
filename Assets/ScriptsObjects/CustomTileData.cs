using UnityEngine;

[CreateAssetMenu(fileName = "CustomTileData", menuName = "Tiles/CustomTileData", order = 1)]
public class CustomTileData : ScriptableObject
{
    public string tileName;      // Nom de la tuile
    public bool isTeleportTile;  // Indicateur pour savoir si c'est une tuile de téléportation
}
