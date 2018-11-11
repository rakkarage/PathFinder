using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace UnityEditor
{
	[CustomEditor(typeof(RandomBrush))]
	public class RandomBrushEditor : GridBrushInspector
	{
		private RandomBrush Brush => target as RandomBrush;
		private GameObject lastBrush;
		public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			var brush = Brush;
			brush.CacheClear(position);
			if (brush.Data.RandomTiles?.Length > 0)
				Common(map, gridLayout, brushTarget, position);
		}
		public override void BoxFillPreview(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			var brush = Brush;
			foreach (var i in position.allPositionsWithin)
				if (map.GetTile(i) != null && map.GetEditorPreviewTile(i) != null)
					brush.CacheRemove(i);
			if (brush.Data.RandomTiles?.Length > 0)
				foreach (var i in position.allPositionsWithin)
					Common(map, gridLayout, brushTarget, i);
		}
		public override void FloodFillPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			var brush = Brush;
			brush.CacheClear(position);
			if (brush.Data.RandomTiles?.Length > 0)
				FloodFill(map, gridLayout, brushTarget, position);
		}
		private void FloodFill(Tilemap map, GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var exist = map.GetTile(position);
			var points = new Stack<Vector3Int>();
			var used = new List<Vector3Int>();
			var bounds = map.cellBounds;
			var size = new Vector2Int((int)map.cellSize.x, (int)map.cellSize.y);
			points.Push(position);
			while (points.Count > 0)
			{
				var p = points.Pop();
				used.Add(p);
				if (!bounds.Contains(p))
					bounds.SetMinMax(new Vector3Int(Math.Min(bounds.xMin, p.x), Math.Min(bounds.yMin, p.y), 0),
						new Vector3Int(Math.Max(bounds.xMax, p.x) + size.x, Math.Max(bounds.yMax, p.y) + size.y, 1));
				Common(map, gridLayout, brushTarget, p);
				for (var y = p.y - 1; y <= p.y + 1; y++)
				{
					for (var x = p.x - 1; x <= p.x + 1; x++)
					{
						var test = new Vector3Int(x, y, p.z);
						if ((test.y != p.y || test.x != p.x) && bounds.Contains(test) &&
							(exist ? map.GetTile(test) : !map.GetTile(test)) && !used.Contains(test))
							points.Push(test);
					}
				}
			}
		}
		private void Common(Tilemap map, GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var brush = Brush;
			foreach (var i in brush.Bounds(position).allPositionsWithin)
			{
				var c = brush[i];
				map.SetEditorPreviewTile(i, c.Item1);
				map.SetEditorPreviewTransformMatrix(i, c.Item2);
			}
			lastBrush = brushTarget;
		}
		public override void ClearPreview()
		{
			if (lastBrush != null)
			{
				Brush.CacheUpdate();
				var map = lastBrush.GetComponent<Tilemap>();
				if (!map)
					return;
				map.ClearAllEditorPreviewTiles();
				lastBrush = null;
			}
		}
	}
}
