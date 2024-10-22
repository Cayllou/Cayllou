using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CustomTile", menuName = "Tiles/CustomTile", order = 2)]
public class CustomTile : Tile
{
    public CustomTileData customTileData; // Référence au Scriptable Object renommé

    // La méthode correcte pour surcharger GetTileData
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
    }
}
