using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	[RequireComponent(typeof(Animator))]
	public class PathMob : MonoBehaviour
	{
		public List<Vector3Int> Path;
		private Animator _animator;
		public PathManager Manager;
		private Transform _t;
		private void Awake()
		{
			_t = transform;
			_animator = GetComponent<Animator>();
			Manager.Map.Dark();
			Manager.Map.Light(Position);
		}
		public Vector3Int Position => new Vector3Int((int)_t.localPosition.x, (int)_t.localPosition.y, 0);
		public void Turn()
		{
			if (Path.Count > 0)
				StepPath();
		}
		public void FindPathTo(Vector3Int p)
		{
			var finder = Manager.Finder;
			finder.Find(Position, p);
			finder.Draw(true);
			Path = finder.Path;
		}
		public void StepPath()
		{
			var map = Manager.Map;
			var finder = Manager.Finder;
			if (Path == null || Path.Count == 0)
				return;
			if ((Path.Count == 2) && map.IsDoor(Path[Path.Count - 1]) && map.IsDoor(Path[1]))
			{
				ToggleDoor(Path[1]);
				ResetPath();
			}
			else if (Path.Count == 1)
			{
				map.IsStairs(Path[0]);
				ResetPath();
			}
			else
			{
				var delta = Path[1] - Path[0];
				var direction = Utility.GetDirection(delta.Vector2Int());
				Face(direction);
				Walk();
				Step(direction);
				var temp = Path[0];
				Path.RemoveAt(0);
				finder.RemovePathAt(map.BackMap.GetCellCenterWorld(temp));
				if ((Path.Count == 2) &&
				map.IsDoor(Path[Path.Count - 1]) && map.IsDoor(Path[1]))
				{
					ToggleDoor(Path[1]);
					ResetPath();
				}
				else if (Path.Count == 1)
				{
					map.IsStairs(Path[0]);
					ResetPath();
				}
				else
				{
					if (Path.Count > 1)
						Manager.ResetTurn();
					else
						ResetPath();
				}
			}
			map.Light(Position);
		}
		private void ResetPath()
		{
			if (Path == null) return;
			Path.Clear();
			Path = null;
			Manager.Finder.RemovePath();
			Manager.Target.TargetOff();
			Manager.Target.SetTargetPosition(_t.localPosition, false);
			Idle();
		}
		private void ToggleDoor(Vector3Int p)
		{
			Manager.Map.ToggleDoor(p);
			Manager.Finder.ReachableFromMain();
		}
		[ContextMenu("Attack")]
		public void Attack()
		{
			_animator.SetTrigger(Constants.AnimatorAttack);
		}
		[ContextMenu("Walk")]
		public void Walk()
		{
			_animator.SetBool(Constants.AnimatorWalk, true);
		}
		[ContextMenu("Idle")]
		public void Idle()
		{
			StartCoroutine(IdleAfter());
		}
		private IEnumerator IdleAfter()
		{
			yield return new WaitForFixedUpdate();
			_animator.SetBool(Constants.AnimatorWalk, false);
		}
		public bool At(Vector2 p)
		{
			var test = _t.localPosition;
			return Mathf.Approximately(p.x, test.x) && Mathf.Approximately(p.y, test.y);
		}
		public void Face(Vector3 p)
		{
			Face(Utility.GetDirection(new Vector2(p.x - _t.localPosition.x, p.y - _t.localPosition.y)));
		}
		public void Face(DirectionType d)
		{
			switch (d)
			{
				case DirectionType.North:
				case DirectionType.NorthEast:
				case DirectionType.East:
				case DirectionType.SouthEast:
					_t.localScale = new Vector3(-1f, 1f, 1f);
					break;
				case DirectionType.South:
				case DirectionType.SouthWest:
				case DirectionType.West:
				case DirectionType.NorthWest:
					_t.localScale = new Vector3(1f, 1f, 1f);
					break;
			}
		}
		private void Step(DirectionType direction)
		{
			var oldX = (int)_t.localPosition.x;
			var oldY = (int)_t.localPosition.y;
			var newX = oldX;
			var newY = oldY;
			switch (direction)
			{
				case DirectionType.North:
					newY += 1;
					break;
				case DirectionType.NorthEast:
					newX += 1;
					newY += 1;
					break;
				case DirectionType.East:
					newX += 1;
					break;
				case DirectionType.SouthEast:
					newX += 1;
					newY -= 1;
					break;
				case DirectionType.South:
					newY -= 1;
					break;
				case DirectionType.SouthWest:
					newX -= 1;
					newY -= 1;
					break;
				case DirectionType.West:
					newX -= 1;
					break;
				case DirectionType.NorthWest:
					newX -= 1;
					newY += 1;
					break;
			}
			var map = Manager.Map;
			if (((newX != oldX) || (newY != oldY)) && map.InsideMap(newX, newY))
				_t.localPosition = map.BackMap.GetCellCenterWorld(new Vector3Int(newX, newY, 0));
		}
	}
}
