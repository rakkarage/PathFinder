using System;
using System.Collections.Generic;
using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	[RequireComponent(typeof(Pool))]
	public class PathFinder : MonoBehaviour
	{
		private int _limit = 10;
		private PathNode[] _map;
		private PathNode[] _closed;
		private PathQueue _open;
		private PathNode _start;
		private PathNode _finish;
		private PathNode _current;
		public List<Vector3Int> Path = new List<Vector3Int>();
		public int SortingOrder = 1;
		public PathManager Manager;
		public PathMap Map;
		private Pool _pool;
		private int _opacityStep = 0;
		private const int _opacityStart = 64;
		private const int _opacityEnd = 192;
		private const int _opacityRange = _opacityEnd - _opacityStart;
		private void Awake()
		{
			_pool = GetComponent<Pool>();
		}
		private void SetupMap()
		{
			var count = Map.Width * Map.Height;
			_map = new PathNode[count];
			_closed = new PathNode[count];
			_open = new PathQueue(count);
			foreach (var p in Map.Bounds.allPositionsWithin)
			{
				var index = Map.TileIndex(p);
				_map[index] = new PathNode() { X = p.x, Y = p.y, TileIndex = index, Walkable = !Map.IsBlocked(p) };
			}
		}
		private void Search()
		{
			Array.Clear(_closed, 0, _closed.Length);
			_open.Clear();
			_map[_start.TileIndex].Parent = null;
			_open.Enqueue(_start);
			while (_open.Count > 0)
			{
				_current = _open.Dequeue();
				_closed[_current.TileIndex] = _current;
				if (_current.TileIndex == _finish.TileIndex)
					break;
				NeighbourCheck();
			}
		}
		public void Find(Vector3Int start, Vector3Int finish)
		{
			Path.Clear();
			SetStartAndFinish(start, finish);
			if (_start != null)
			{
				if (_finish == null)
					return;
				Search();
				var reached = Mathf.Approximately(finish.x, _current.X) && Mathf.Approximately(finish.y, _current.Y);
				if (!reached)
					Search();
				while (_current.Parent != null)
				{
					Path.Add(new Vector3Int(_current.X, _current.Y, 0));
					_current = _current.Parent;
				}
				var startPoint = new Vector3Int(_start.X, _start.Y, 0);
				Path.Add(startPoint);
				Path.Reverse();
			}
		}
		private void SetStartAndFinish(Vector3Int start, Vector3Int finish)
		{
			_start = FindClose(start, finish);
			_finish = FindClose(finish, start);
		}
		private PathNode FindClose(Vector3Int start, Vector3Int finish)
		{
			PathNode closestNode = null;
			var startIndex = Map.TileIndex(start);
			var startNode = _map[startIndex];
			var startDoor = Map.IsDoorShut(start);
			var finishIndex = Map.TileIndex(finish);
			var finishNode = _map[finishIndex];
			if (startDoor || (startNode.Walkable && startNode.Reachable))
			{
				return startNode;
			}
			else
			{
				var radius = 1;
				var nodes = new List<PathNode>();
				while ((nodes.Count < 1) && (radius < _limit))
				{
					nodes = FindClosestCheck(start, radius);
					radius++;
				}
				if (nodes.Count > 0)
				{
					var closest = float.MaxValue;
					var test0 = new Vector2Int(finishNode.X, finishNode.Y);
					foreach (var node in nodes)
					{
						var test1 = new Vector2Int(node.X, node.Y);
						var distance = Vector2.Distance(test0, test1);
						if (distance < closest)
						{
							closest = distance;
							closestNode = node;
						}
					}
				}
			}
			return closestNode;
		}
		private List<PathNode> FindClosestCheck(Vector3Int p, int r)
		{
			var nodes = new List<PathNode>();
			for (var yy = p.y - r; yy <= p.y + r; yy++)
			{
				for (var xx = p.x - r; xx <= p.x + r; xx++)
				{
					if (((yy == p.y - r) || (xx == p.x - r) || (yy == p.y + r) || (xx == p.x + r)) && Map.InsideEdge(xx, yy))
					{
						var index = Map.TileIndex(xx, yy);
						var node = _map[index];
						if (node.Walkable && node.Reachable)
						{
							nodes.Add(node);
						}
					}
				}
			}
			return nodes;
		}
		public bool IsReachable(Vector3Int p)
		{
			return _map[Map.TileIndex(p)].Reachable;
		}
		public void ReachableClear()
		{
			foreach (var node in _map)
			{
				node.Reachable = false;
			}
		}
		public void ReachableFrom()
		{
			SetupMap();
			ReachableFromMain();
		}
		public void ReachableFromMain()
		{
			ReachableClear();
			var points = new Stack<Vector2Int>();
			points.Push(Manager.Character.Position.Vector2Int());
			while (points.Count > 0)
			{
				var current = _map[Map.TileIndex(points.Pop())];
				current.Reachable = true;
				for (var yy = current.Y - 1; yy <= current.Y + 1; yy++)
				{
					for (var xx = current.X - 1; xx <= current.X + 1; xx++)
					{
						if ((yy != current.Y || xx != current.X) && Map.InsideEdge(xx, yy))
						{
							var index = Map.TileIndex(xx, yy);
							var node = _map[index];
							if (!node.Reachable && (node.Walkable && !Map.IsDoorShut(node.X, node.Y)))
							{
								points.Push(new Vector2Int(xx, yy));
							}
						}
					}
				}
			}
		}
		private void NeighbourCheck()
		{
			for (var y = _current.Y - 1; y <= _current.Y + 1; y++)
			{
				for (var x = _current.X - 1; x <= _current.X + 1; x++)
				{
					if ((y != _current.Y || x != _current.X) && Map.InsideEdge(x, y))
					{
						var index = Map.TileIndex(x, y);
						var finish = (index == _finish.TileIndex);
						var node = _map[index];
						var door = Map.IsDoorShut(node.X, node.Y);
						if ((finish && door) || (node.Walkable && !door) && !IsClosed(node))
						{
							if (!_open.Contains(node))
							{
								var addNode = node;
								addNode.H = GetHeuristics(node);
								addNode.G = GetMovementCost(node) + _current.G;
								addNode.F = addNode.H + addNode.G;
								addNode.Parent = _current;
								_open.Enqueue(addNode);
							}
							else
							{
								if (_current.G + GetMovementCost(node) < node.G)
								{
									node.Parent = _current;
									node.G = GetMovementCost(node) + _current.G;
									node.F = node.H + node.G;
									node.Parent = _current;
									_open.CascadeUp(node);
								}
							}
						}
					}
				}
			}
		}
		private bool IsClosed(PathNode node)
		{
			return (_closed[node.TileIndex] != null);
		}
		private int GetMovementCost(PathNode node)
		{
			return ((_current.X != node.X) && (_current.Y != node.Y)) ? 14 : 10;
		}
		private int GetHeuristics(PathNode node)
		{
			return GetHeuristics(node, _finish);
		}
		private int GetHeuristics(PathNode start, PathNode finish)
		{
			return (int)(Mathf.Abs(start.X - finish.X) * 10f) + (int)(Mathf.Abs(start.Y - finish.Y) * (10f));
		}
		public void RemovePathAt(Vector3 p)
		{
			Transform found = null;
			foreach (Transform child in transform)
			{
				if (child != transform)
				{
					if (child.gameObject.activeInHierarchy)
					{
						var c = child.localPosition;
						if (Mathf.Approximately(p.x, c.x) && Mathf.Approximately(p.y, c.y))
							found = child;
					}
				}
			}
			if (found != null)
				_pool.Exit(found.gameObject);
		}
		public void Draw(bool update)
		{
			RemovePath();
			if (Path == null || Path.Count == 0) return;
			_opacityStep = _opacityRange / Path.Count;
			var first = Path[0];
			var last = Path[Path.Count - 1];
			var color = Manager.Map.GetColor(last);
			Manager.Target.SetTargetColor(color);
			var pathDelta = GetDelta(first, last);
			float r = 0f;
			for (var i = 0; i < Path.Count; i++)
			{
				var p = Path[i];
				var nextExists = i + 1 < Path.Count;
				if (nextExists)
				{
					var next = Path[i + 1];
					var nodeDelta = GetDelta(p, next);
					r = GetRotation(nodeDelta, pathDelta);
				}
				var o = EnterPath();
				o.transform.localPosition = Map.BackMap.GetCellCenterWorld(p);
				o.transform.parent = transform;
				o.transform.localEulerAngles = new Vector3(0f, 0f, r);
				var sr = o.GetComponent<SpriteRenderer>();
				var opacity = (((i * _opacityStep) + _opacityStart) / 255f);
				sr.color = color.SetAlpha(opacity);
				sr.sortingOrder = SortingOrder;
			}
		}
		private GameObject EnterPath()
		{
			var path = _pool.Enter();
			path.SetActive(true);
			return path;
		}
		public void RemovePath()
		{
			foreach (Transform child in transform)
				_pool.Exit(child.gameObject);
		}
		private Vector2 GetDelta(Vector3 start, Vector3 finish)
		{
			return finish - start;
		}
		private int GetRotation(Vector3 nodeDelta, Vector3 pathDelta)
		{
			var rotation = 0;
			var trending = Mathf.Abs(pathDelta.y) > Mathf.Abs(pathDelta.x);
			if ((nodeDelta.x > 0) && (nodeDelta.y < 0))
			{
				rotation = trending ? 270 : 0;
			}
			else if ((nodeDelta.x > 0) && (nodeDelta.y > 0))
			{
				rotation = trending ? 90 : 0;
			}
			else if ((nodeDelta.x < 0) && (nodeDelta.y < 0))
			{
				rotation = trending ? 270 : 180;
			}
			else if ((nodeDelta.x < 0 && nodeDelta.y > 0))
			{
				rotation = trending ? 90 : 180;
			}
			else if (nodeDelta.x > 0)
			{
				rotation = 0;
			}
			else if (nodeDelta.x < 0)
			{
				rotation = 180;
			}
			else if (nodeDelta.y < 0)
			{
				rotation = 270;
			}
			else if (nodeDelta.y > 0)
			{
				rotation = 90;
			}
			return rotation;
		}
#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			if ((_map == null) || (_map.Length == 0)) return;
			for (var y = 0; y < Map.Height; y++)
			{
				for (var x = 0; x < Map.Width; x++)
				{
					var node = _map[Map.TileIndex(x, y)];
					var pi = new Vector3Int(x, y, 0);
					var p = Map.BackMap.GetCellCenterWorld(pi);
					var color = Color.white;
					if (_open.Contains(node) || IsClosed(node))
					{
						if (node.Parent != null)
						{
							var pp = Map.BackMap.GetCellCenterWorld(new Vector3Int(node.Parent.X, node.Parent.Y, 0));
							ArrowGizmo(p, pp, Color.white);
							if (Path.Contains(pi))
								Gizmos.DrawWireCube(p, Vector3.one);
						}
					}
					if (_open.Contains(node))
						color = Colors.ErrorBlue.SetAlpha(.5f);
					else if (IsClosed(node))
						color = Colors.ErrorYellow.SetAlpha(.5f);
					else if (node.Walkable && node.Reachable)
						color = Colors.ErrorGreen.SetAlpha(.5f);
					else
						color = Colors.ErrorRed.SetAlpha(.5f);
					Gizmos.color = color;
					Gizmos.DrawCube(p, Vector3.one);
				}
			}
		}
		private void ArrowGizmo(Vector2 child, Vector2 parent, Color c)
		{
			const float length = .25f;
			const float angle = 20f;
			Gizmos.color = c;
			var d = (parent - child) * .5f;
			Gizmos.DrawRay(child, d);
			var right = Quaternion.LookRotation(d, Vector3.right) * Quaternion.Euler(180f + angle, 0f, 0f) * Vector3.forward;
			var left = Quaternion.LookRotation(d, Vector3.right) * Quaternion.Euler(180f - angle, 0f, 0f) * Vector3.forward;
			Gizmos.DrawRay(child + d, right * length);
			Gizmos.DrawRay(child + d, left * length);
		}
#endif
	}
}