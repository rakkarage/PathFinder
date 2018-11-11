using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
namespace UnityEditor
{
	[CustomGridBrush(true, false, false, "Line Brush")]
	public class LineBrush : GridBrush
	{
		public bool LineStartActive = false;
		public bool FillGaps = false;
		public Vector3Int LineStart = Vector3Int.zero;
		public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			if (LineStartActive)
			{
				var startPos = new Vector2Int(LineStart.x, LineStart.y);
				var endPos = new Vector2Int(position.x, position.y);
				if (startPos == endPos)
					base.Paint(grid, brushTarget, position);
				else
					foreach (var p in GetPointsOnLine(startPos, endPos, FillGaps))
						base.Paint(grid, brushTarget, new Vector3Int(p.x, p.y, position.z));
				LineStartActive = false;
			}
			else
			{
				LineStart = position;
				LineStartActive = true;
			}
		}
		public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int startPos, Vector2Int endPos, bool fillGaps)
		{
			var points = GetPointsOnLine(startPos, endPos);
			if (fillGaps)
			{
				var rise = endPos.y - startPos.y;
				var run = endPos.x - startPos.x;
				if (rise != 0 || run != 0)
				{
					var extraStart = startPos;
					var extraEnd = endPos;
					if (Mathf.Abs(rise) >= Mathf.Abs(run))
					{
						if (rise > 0) // up
						{
							extraStart.y += 1;
							extraEnd.y += 1;
						}
						else // down
						{
							extraStart.y -= 1;
							extraEnd.y -= 1;
						}
					}
					else
					{
						if (run > 0) // right
						{
							extraStart.x += 1;
							extraEnd.x += 1;
						}
						else // left
						{
							extraStart.x -= 1;
							extraEnd.x -= 1;
						}
					}
					var extraPoints = GetPointsOnLine(extraStart, extraEnd);
					extraPoints = extraPoints.Except(new[] { extraEnd });
					points = points.Union(extraPoints);
				}
			}
			return points;
		}
		public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int p1, Vector2Int p2)
		{
			var x0 = p1.x;
			var y0 = p1.y;
			var x1 = p2.x;
			var y1 = p2.y;
			var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				int t;
				t = x0; // swap x0 and y0
				x0 = y0;
				y0 = t;
				t = x1; // swap x1 and y1
				x1 = y1;
				y1 = t;
			}
			if (x0 > x1)
			{
				int t;
				t = x0; // swap x0 and x1
				x0 = x1;
				x1 = t;
				t = y0; // swap y0 and y1
				y0 = y1;
				y1 = t;
			}
			var dx = x1 - x0;
			var dy = Math.Abs(y1 - y0);
			var error = dx / 2;
			var ystep = (y0 < y1) ? 1 : -1;
			var y = y0;
			for (var x = x0; x <= x1; x++)
			{
				yield return new Vector2Int((steep ? y : x), (steep ? x : y));
				error = error - dy;
				if (error < 0)
				{
					y += ystep;
					error += dx;
				}
			}
			yield break;
		}
	}
	[CustomEditor(typeof(LineBrush))]
	public class LineBrushEditor : GridBrushEditor
	{
		private LineBrush Brush => target as LineBrush;
		public override void OnPaintSceneGUI(GridLayout grid, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
		{
			base.OnPaintSceneGUI(grid, brushTarget, position, tool, executing);
			if (Brush.LineStartActive)
			{
				var tilemap = brushTarget.GetComponent<Tilemap>();
				if (tilemap != null)
					tilemap.ClearAllEditorPreviewTiles();
				var startPos = new Vector2Int(Brush.LineStart.x, Brush.LineStart.y);
				var endPos = new Vector2Int(position.x, position.y);
				if (startPos == endPos)
					PaintPreview(grid, brushTarget, position.min);
				else
					foreach (var p in LineBrush.GetPointsOnLine(startPos, endPos, Brush.FillGaps))
						PaintPreview(grid, brushTarget, new Vector3Int(p.x, p.y, position.z));
				if (Event.current.type == EventType.Repaint)
				{
					var min = Brush.LineStart;
					var max = Brush.LineStart + position.size;
					GL.PushMatrix();
					GL.MultMatrix(GUI.matrix);
					GL.Begin(GL.LINES);
					Handles.color = Color.blue;
					Handles.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
					Handles.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z));
					Handles.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z));
					Handles.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, min.y, min.z));
					GL.End();
					GL.PopMatrix();
				}
			}
		}
	}
}
