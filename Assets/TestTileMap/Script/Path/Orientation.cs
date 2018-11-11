using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	[ExecuteInEditMode]
	public class Orientation : MonoBehaviour
	{
		public delegate void Changed();
		public static event Changed OnChanged;
		private bool _wide;
		private float _width;
		private float _height;
		private void Start()
		{
			_width = Screen.width;
			_height = Screen.height;
			_wide = _width > _height;
			Trigger();
		}
		private void Update()
		{
			if (!Mathf.Approximately(_width, Screen.width) || !Mathf.Approximately(_height, Screen.height))
				Trigger();
			else
			{
				var oldWide = _wide;
				_wide = Screen.width > Screen.height;
				if ((_wide && !oldWide) || (!_wide && oldWide))
					Trigger();
			}
		}
		private void Trigger()
		{
			_width = Screen.width;
			_height = Screen.height;
			if (OnChanged != null)
				OnChanged();
		}
	}
}
