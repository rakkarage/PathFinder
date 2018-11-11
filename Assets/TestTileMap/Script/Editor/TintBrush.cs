using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace UnityEditor
{
	[CreateAssetMenu]
	[CustomGridBrush(false, false, false, "Tint Brush")]
	public class TintBrush : GridBrushBase
	{
		public Color Color = Color.white;
		private static int reservedLayer = 31;
		public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (brushTarget.layer == reservedLayer)
				return;
			var map = brushTarget?.GetComponent<Tilemap>();
			if (map != null)
				SetColor(map, position, Color);
		}
		public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (brushTarget.layer == reservedLayer)
				return;
			var map = brushTarget?.GetComponent<Tilemap>();
			if (map != null)
				SetColor(map, position, Color.white);
		}
		private static void SetColor(Tilemap tilemap, Vector3Int position, Color color)
		{
			var tile = tilemap?.GetTile(position);
			if (tile != null)
			{
				if ((tilemap.GetTileFlags(position) & TileFlags.LockColor) != 0)
				{
					if (tile is Tile)
						Debug.LogWarning("Tint brush cancelled, because Tile (" + tile.name +
							") has TileFlags.LockColor set. Unlock it from the Tile asset debug inspector.");
					else
						Debug.LogWarning("Tint brush cancelled. because Tile (" + tile.name +
							") has TileFlags.LockColor set. Unset it in GetTileData().");
				}
				tilemap.SetColor(position, color);
			}
		}
	}
	[CustomEditor(typeof(TintBrush))]
	public class TintBrushEditor : GridBrushEditorBase
	{
		public override GameObject[] validTargets => FindObjectsOfType<Tilemap>().Select(x => x.gameObject).ToArray();
	}
}
