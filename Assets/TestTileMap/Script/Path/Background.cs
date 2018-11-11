using UnityEngine;
using UnityEngine.UI;
namespace ca.HenrySoftware.Rage
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Image))]
	public class Background : MonoBehaviour
	{
		private RectTransform _t;
		private DrivenRectTransformTracker _driven;
		private Image _image;
		private void Awake()
		{
			_t = transform as RectTransform;
			_driven = new DrivenRectTransformTracker();
			_driven.Clear();
			_driven.Add(this, _t, DrivenTransformProperties.Scale | DrivenTransformProperties.Rotation);
			_image = GetComponent<Image>();
		}
		private void OnEnable()
		{
			Orientation.OnChanged += OrientationChanged;
			OrientationChanged();
		}
		private void OnDisable()
		{
			Orientation.OnChanged -= OrientationChanged;
			_driven.Clear();
		}
		private void OrientationChanged()
		{
			_driven.Clear();
			_driven.Add(this, _t, DrivenTransformProperties.Scale | DrivenTransformProperties.Rotation);
			if (Screen.width > Screen.height)
			{
				_t.localScale = new Vector3(Screen.height / _image.preferredWidth, Screen.width / _image.preferredHeight, 1);
				_t.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
			}
			else
			{
				_t.localScale = new Vector3(Screen.width / _image.preferredWidth, Screen.height / _image.preferredHeight, 1);
				_t.localRotation = Quaternion.identity;
			}
		}
	}
}
