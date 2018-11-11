using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
namespace ca.HenrySoftware.Rage
{
	public class PathManager : MonoBehaviour,
		IPointerClickHandler,
		IPinchHandler, IEndPinchHandler,
		IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Camera GameCamera;
		public float ZoomMin = 1f;
		public float ZoomMinMin = .8f;
		public float ZoomMax = 32f;
		public float ZoomMaxMax = 40f;
		public PathMob Character;
		public PathFinder Finder;
		public PathTarget Target;
		public PathMap Map;
		public MiniMap MiniMap;
		private bool _drag = false;
		private float _totalTime = 0f;
		private int _totalTurns = 0;
		private bool _turn = false;
		private IEnumerator _spring;
		private void Start()
		{
			GameCamera.orthographicSize = Prefs.Zoom;
			StartCoroutine(TurnTimer());
			StartCoroutine(TurnAfter());
		}
		public IEnumerator TurnAfter()
		{
			yield return null;
			_turn = true;
			var p = Character.transform.localPosition;
			Target.SetTargetPosition(p, false);
			Finder.ReachableFrom();
		}
		public IEnumerator TurnTimer()
		{
			yield return new WaitForEndOfFrame();
			while (true)
			{
				for (var timer = 0f; timer < Constants.TimeTurn; timer += Time.deltaTime, _totalTime += Time.deltaTime)
					yield return null;
				if (_turn)
				{
					_turn = false;
					_totalTurns++;
					Character.Turn();
					CheckCenter();
					MiniMap.UpdateMiniMap(Character.transform.localPosition);
				}
			}
		}
		public void ResetTurn()
		{
			_turn = true;
		}
		public void OnPointerClick(PointerEventData e)
		{
			if (_drag) return;
			var w = GameCamera.ScreenToWorldPoint(e.position);
			Character.Face(w);
			var p = Map.BackMap.WorldToCell(w);
			var b = Map.Bounds;
			p.Clamp(b.min, b.max - Vector3Int.one);
			var pw = Map.BackMap.GetCellCenterWorld(p);
			var c = Character.Position;
			if (p.Equals(c)) // on character remove path
			{
				if (Target.TargetTarget.Approximately(pw)) // no path skip turn
					ResetTurn();
				Character.FindPathTo(c);
				Finder.RemovePath();
				Target.TargetOff();
			}
			else if (Target.TargetTarget.Approximately(pw)) // on target walk path
			{
				ResetTurn();
			}
			else // else find path
			{
				Character.FindPathTo(p);
				Target.TargetOn();
				Target.SetTargetPosition(pw, true);
			}
		}
		public void OnBeginDrag(PointerEventData e)
		{
			_drag = true;
		}
		public void OnDrag(PointerEventData e)
		{
			var unit = Vector3.Distance(GameCamera.ScreenToWorldPoint(Vector3.zero), GameCamera.ScreenToWorldPoint(Vector3.right));
			GameCamera.transform.localPosition -= new Vector3(e.delta.x, e.delta.y, 0f) * unit;
			MiniMap.UpdateMiniMap(GameCamera.transform.localPosition);
		}
		public void OnEndDrag(PointerEventData e)
		{
			_drag = false;
			Spring();
		}
		public void OnPinch(PinchEventData e)
		{
			var size = GameCamera.orthographicSize + e.PinchDelta;
			GameCamera.orthographicSize = size > ZoomMaxMax ? ZoomMaxMax : size < ZoomMinMin ? ZoomMinMin : size;
		}
		public void OnEndPinch(PinchEventData e)
		{
			Spring();
		}
		private void Update()
		{
			if (Input.mousePresent)
			{
				var scroll = Input.mouseScrollDelta;
				if (scroll.y > 0)
				{
					var newSize = Prefs.Zoom - 1;
					if (newSize >= ZoomMin)
						UpdateSize(newSize);
					MiniMap.UpdateMiniMap(GameCamera.transform.localPosition);
				}
				else if (scroll.y < 0)
				{
					var newSize = Prefs.Zoom + 1;
					if (newSize <= ZoomMax)
						UpdateSize(newSize);
					MiniMap.UpdateMiniMap(GameCamera.transform.localPosition);
				}
			}
		}
		private void UpdateSize(int size)
		{
			Prefs.Zoom = size;
			GameCamera.orthographicSize = size;
			Spring();
		}
		private const float _edgeOffset = 1.5f;
		private void CheckCenter()
		{
			var bottomLeft = GameCamera.ScreenToWorldPoint(Vector2.zero);
			var topRight = GameCamera.ScreenToWorldPoint(new Vector2(GameCamera.pixelWidth, GameCamera.pixelHeight));
			var test = Character.transform.localPosition;
			if ((test.x - _edgeOffset < bottomLeft.x) ||
				(test.y - _edgeOffset < bottomLeft.y) ||
				(test.x + _edgeOffset > topRight.x) ||
				(test.y + _edgeOffset > topRight.y))
			{
				CenterOnCharacter(true);
			}
		}
		public void CenterOnCharacter(bool animate = false)
		{
			if (!_drag)
				CenterOn(Character.transform.localPosition, animate);
		}
		public void CenterOn(Vector2 p, bool animate = false)
		{
			var start = GameCamera.transform.localPosition;
			var finish = new Vector3(p.x, p.y, start.z);
			if (animate)
				Ease3.Go(this, start, finish, Constants.TimeTween, v => GameCamera.transform.localPosition = v, null);
			else
				GameCamera.transform.localPosition = finish;
		}
		private void Spring()
		{
			var size = GameCamera.orthographicSize;
			var targetZoom = 0f;
			if (size < ZoomMin)
				targetZoom = ZoomMin;
			else if (size > ZoomMax)
				targetZoom = ZoomMax;
			if (!Mathf.Approximately(targetZoom, 0f))
				Ease.Go(this, size, targetZoom, Constants.TimeTween, v => GameCamera.orthographicSize = v, null, EaseType.Spring);
			else
				GameCamera.orthographicSize = Mathf.RoundToInt(size);
			var boundsCamera = GameCamera.OrthographicBounds();
			Bounds boundsMap = Map.BackMap.localBounds;
			var boundsTest = boundsMap;
			boundsTest.extents -= new Vector3(2f, 2f, 0f);
			var delta = Vector2.zero;
			if (!boundsCamera.Intersects(boundsTest))
				delta = Utility.ConstrainRect(boundsCamera, boundsMap);
			Spring(delta);
		}
		private void Spring(Vector2 delta)
		{
			var p = GameCamera.transform.localPosition;
			var isDeltaX = !Mathf.Approximately(delta.x, 0f);
			var x = p.x + delta.x;
			var isDeltaY = !Mathf.Approximately(delta.y, 0f);
			var y = p.y + delta.y;
			var center = Map.BackMap.GetCellCenterWorld(new Vector3Int((int)x, (int)y, 0));
			var to = new Vector3(center.x, center.y, p.z);
			if (_spring != null) StopCoroutine(_spring);
			_spring = Ease3.Go(this, p, to, Constants.TimeTween, v =>
			{
				GameCamera.transform.localPosition = v;
				if (isDeltaX || isDeltaY)
					MiniMap.UpdateMiniMap(v);
			}, () => MiniMap.UpdateMiniMap(GameCamera.transform.localPosition), EaseType.Spring);
		}
	}
}
