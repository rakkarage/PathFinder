using System.Collections;
using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	public class PathTarget : MonoBehaviour
	{
		public Vector3 TargetTarget { get; private set; }
		public SpriteRenderer Back;
		public SpriteRenderer Fore;
		private IEnumerator _easeTargetShow;
		private IEnumerator _easeTargetHide;
		private IEnumerator _easeTargetPosition;
		private Transform _t;
		public void Awake()
		{
			_t = transform;
		}
		public void AnimateTarget(bool show)
		{
			if (show)
			{
				TargetOn();
				if (_easeTargetHide != null) StopCoroutine(_easeTargetHide);
				_easeTargetShow = Ease.Go(this, 0f, 1f, Constants.TimeTween, SetAlpha);
			}
			else
			{
				if (_easeTargetShow != null) StopCoroutine(_easeTargetShow);
				_easeTargetHide = Ease.Go(this, 0f, 1f, Constants.TimeTween, SetAlpha, TargetOff);
			}
		}
		public void ActivateTarget(bool show)
		{
			if (show)
			{
				TargetOn();
			}
			else
			{
				TargetOff();
			}
		}
		public void TargetOn()
		{
			Back.gameObject.SetActive(true);
			Fore.gameObject.SetActive(true);
		}
		public void TargetOff()
		{
			Back.gameObject.SetActive(false);
			Fore.gameObject.SetActive(false);
		}
		public void SetTargetPosition(Vector3 p, bool animate)
		{
			TargetTarget = new Vector3(p.x, p.y, _t.localPosition.z);
			if (animate)
			{
				AnimateTarget(true);
				if (_easeTargetPosition != null) StopCoroutine(_easeTargetPosition);
				_easeTargetPosition = Ease3.Go(this, _t.localPosition, TargetTarget, Constants.TimeTween, v => _t.localPosition = v, null, EaseType.Spring);
			}
			else
			{
				ActivateTarget(false);
				_t.localPosition = TargetTarget;
			}
		}
		public void SetTargetColor(Color color)
		{
			color = GetTargetColor(color);
			SetColor(color);
		}
		private void UpdateTargetColor(Vector3 color)
		{
			SetColor(color.GetColor());
		}
		private void SetColor(Color color)
		{
			Back.color = color.SetAlpha(Back.color.a);
			Fore.color = color.SetAlpha(Fore.color.a);
		}
		private void SetAlpha(float alpha)
		{
			Back.color = Back.color.SetAlpha(alpha);
			Fore.color = Fore.color.SetAlpha(alpha);
		}
		private Color GetTargetColor(Color color)
		{
			if (color.Equals(Colors.Green))
				color = Colors.GreenLight;
			else if (color.Equals(Colors.Blue))
				color = Colors.BlueLight;
			else if (color.Equals(Colors.Yellow))
				color = Colors.YellowLight;
			else if (color.Equals(Colors.Red))
				color = Colors.RedLight;
			return color;
		}
	}
}
