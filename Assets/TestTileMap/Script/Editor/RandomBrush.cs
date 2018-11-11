using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace UnityEditor
{
	[CreateAssetMenu]
	[CustomGridBrush(false, true, false, "Random Brush")]
	public class RandomBrush : GridBrush
	{
		public RandomTileData Data;
		private Vector3Int? lastPosition;
		private Dictionary<Vector3Int, Tuple<TileBase, Matrix4x4>> cache = new Dictionary<Vector3Int, Tuple<TileBase, Matrix4x4>>();
		public Tuple<TileBase, Matrix4x4> this[Vector3Int key]
		{
			get
			{
				if (!cache.ContainsKey(key))
					this[key] = new Tuple<TileBase, Matrix4x4>(Data.NextTile, Data.NextMatrix);
				return cache[key];
			}
			set
			{
				cache[key] = value;
				lastPosition = key;
			}
		}
		public void CacheClear(Vector3Int position)
		{
			if (position != lastPosition)
				cache.Clear();
		}
		public void CacheRemove(Vector3Int position) => cache.Remove(position);
		private FlipAxis? flipAxis;
		private RotationDirection? rotationDirection;
		public void CacheUpdate()
		{
			if (flipAxis.HasValue || rotationDirection.HasValue)
			{
				foreach (var i in cache.ToList())
				{
					var m = i.Value.Item2;
					var p = m.GetColumn(3);
					var r = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
					var s = new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
					if (flipAxis.HasValue)
						s = new Vector3(flipAxis.Value == FlipAxis.X ? s.x * -1f : s.x,
							flipAxis.Value == FlipAxis.Y ? s.y * -1f : s.y, s.z);
					if (rotationDirection.HasValue)
						r *= rotationDirection.Value == RotationDirection.Clockwise ? RandomTileData.RotateClockwise : RandomTileData.RotateCounter;
					m.SetTRS(p, r, s);
					cache[i.Key] = new Tuple<TileBase, Matrix4x4>(i.Value.Item1, m);
				}
				flipAxis = null;
				rotationDirection = null;
			}
		}
		public BoundsInt Bounds(Vector3Int position)
		{
			var min = position - pivot;
			var max = min + size;
			return new BoundsInt(min, max - min);
		}
		public override void Flip(FlipAxis flip, GridLayout.CellLayout layout)
		{
			flipAxis = flip;
			base.Flip(flip, layout);
		}
		public override void Rotate(RotationDirection direction, GridLayout.CellLayout layout)
		{
			rotationDirection = direction;
			base.Rotate(direction, layout);
		}
		public override void Select(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			foreach (var i in position.allPositionsWithin)
			{
				var tile = map.GetTile(i);
				this[i] = new Tuple<TileBase, Matrix4x4>(tile, map.GetTransformMatrix(i));
			}
		}
		public override void Move(GridLayout gridLayout, GameObject brushTarget, BoundsInt from, BoundsInt to)
		{
			var offset = to.min - from.min;
			foreach (var i in cache.ToList())
			{
				cache[i.Key] = null;
				cache[i.Key + offset] = i.Value;
			}
			base.Move(gridLayout, brushTarget, from, to);
		}
		public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			if (Data.RandomTiles?.Length > 0)
				Common(map, gridLayout, brushTarget, position);
		}
		public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			if (Data.RandomTiles?.Length > 0)
				foreach (var i in position.allPositionsWithin)
					Common(map, gridLayout, brushTarget, i);
		}
		public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var map = brushTarget?.GetComponent<Tilemap>();
			if (!map)
				return;
			if (Data.RandomTiles?.Length > 0)
				FloodFill(map, gridLayout, brushTarget, position);
		}
		private void FloodFill(Tilemap map, GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			var exist = map.GetTile(position);
			var points = new Stack<Vector3Int>();
			var used = new List<Vector3Int>();
			points.Push(position);
			while (points.Count > 0)
			{
				var p = points.Pop();
				used.Add(p);
				Common(map, gridLayout, brushTarget, p);
				for (var y = p.y - 1; y <= p.y + 1; y++)
				{
					for (var x = p.x - 1; x <= p.x + 1; x++)
					{
						var test = new Vector3Int(x, y, p.z);
						if ((test.y != p.y || test.x != p.x) && map.cellBounds.Contains(test) &&
							(exist ? map.GetTile(test) : !map.GetTile(test)) && !used.Contains(test))
							points.Push(test);
					}
				}
			}
		}
		private void Common(Tilemap map, GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			foreach (var i in Bounds(position).allPositionsWithin)
			{
				var c = this[i];
				map.SetTile(i, c.Item1);
				map.SetTransformMatrix(i, c.Item2);
			}
		}
	}
}
