using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace UnityEditor
{
	[CustomEditor(typeof(SplitBrush))]
	public class SplitBrushEditor : GridBrushInspector
	{
		private SplitBrush Brush => target as SplitBrush;
		public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var b = Brush;
			var maps = brushTarget?.transform.parent.GetComponentsInChildren<Tilemap>();
			for (var i = 0; i < maps.Length; i++)
			{
				var map = maps[i];
				var name = map.name;
				if (name == b.BackName)
				{
					var p = new Vector3Int(position.x + b.BackOffset.x, position.y + b.BackOffset.y, position.z);
					map.SetEditorPreviewTile(p, b.Back);
					map.SetEditorPreviewTransformMatrix(p, Matrix4x4.identity);
					b.BackTilemap = map;
				}
				else if (name == b.ForeName)
				{
					var p = new Vector3Int(position.x + b.ForeOffset.x, position.y + b.ForeOffset.y, position.z);
					map.SetEditorPreviewTile(p, b.Fore);
					map.SetEditorPreviewTransformMatrix(p, Matrix4x4.identity);
					b.ForeTilemap = map;
				}
			}
		}
		public override void ClearPreview()
		{
			var b = Brush;
			if (b != null)
			{
				if (b.BackTilemap != null)
					b.BackTilemap.ClearAllEditorPreviewTiles();
				if (b.ForeTilemap != null)
					b.ForeTilemap.ClearAllEditorPreviewTiles();
			}
		}
		public override GameObject[] validTargets
		{
			get
			{
				var brush = Brush;
				var list = from map in GameObject.FindObjectsOfType<Tilemap>()
						   where (map.gameObject.name == brush.BackName) || (map.gameObject.name == brush.ForeName)
						   select map.gameObject;
				return list.ToArray();
			}
		}
	}
}
