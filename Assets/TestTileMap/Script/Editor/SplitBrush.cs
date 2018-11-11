using UnityEditor;
namespace UnityEngine.Tilemaps
{
	[CreateAssetMenu]
	[CustomGridBrush(false, true, false, "Split Brush")]
	public class SplitBrush : GridBrush
	{
		public TileBase Back;
		public string BackName = "Back";
		public Tilemap BackTilemap;
		public Vector2Int BackOffset;
		public TileBase Fore;
		public string ForeName = "Fore";
		public Tilemap ForeTilemap;
		public Vector2Int ForeOffset;
		public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (BackTilemap != null && ForeTilemap != null)
			{
				var bp = new Vector3Int(position.x + BackOffset.x, position.y + BackOffset.y, position.z);
				BackTilemap.SetTile(bp, Back);
				BackTilemap.SetTransformMatrix(bp, Matrix4x4.identity);
				var fp = new Vector3Int(position.x + ForeOffset.x, position.y + ForeOffset.y, position.z);
				ForeTilemap.SetTile(fp, Fore);
				ForeTilemap.SetTransformMatrix(fp, Matrix4x4.identity);
			}
		}
		public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (BackTilemap != null && ForeTilemap != null)
			{
				var bp = new Vector3Int(position.x + BackOffset.x, position.y + BackOffset.y, position.z);
				BackTilemap.SetTile(bp, null);
				BackTilemap.SetTransformMatrix(bp, Matrix4x4.identity);
				var fp = new Vector3Int(position.x + ForeOffset.x, position.y + ForeOffset.y, position.z);
				ForeTilemap.SetTile(fp, null);
				ForeTilemap.SetTransformMatrix(fp, Matrix4x4.identity);
			}
		}
	}
}
