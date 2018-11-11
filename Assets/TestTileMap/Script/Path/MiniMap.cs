using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class MiniMap : MonoBehaviour
	{
		public PathManager Manager;
		private Transform _t;
		private SpriteRenderer _sr;
		private const float _scale = 20f;
		private const float _border = 16f;
		private const int _layer = 11;
		private bool _shown = false;
		private const int _max = 32;
		private const float _time = Constants.TimeTween;
		private void Awake()
		{
			_t = transform;
			_sr = GetComponent<SpriteRenderer>();
		}
		private void OnEnable()
		{
			Orientation.OnChanged += OrientationChanged;
			OrientationChanged();
		}
		private void OnDisable()
		{
			Orientation.OnChanged -= OrientationChanged;
		}
		private void OrientationChanged()
		{
			var p = Camera.main.ViewportToWorldPoint(Vector2.up);
			_t.localPosition = new Vector3(p.x, p.y, _t.localPosition.z);
		}
		public void UpdateMiniMap(Vector2 center)
		{
			Set(GetMiniMap(center));
			if (!_shown)
				Show();
		}
		private void Set(Texture2D texture)
		{
			if (!texture) return;
			if (_sr.sprite != null)
				Object.Destroy(_sr.sprite);
			_sr.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0f, 1f));
		}
		public void Hide()
		{
			if (!Mathf.Approximately(_sr.color.a, 0f))
				Ease.Go(this, _sr.color.a, 0f, _time, SetAlpha);
			_shown = false;
		}
		public void Show()
		{
			if (!Mathf.Approximately(_sr.color.a, 1f))
				Ease.Go(this, _sr.color.a, 1f, _time, SetAlpha);
			_shown = true;
		}
		private void SetAlpha(float alpha)
		{
			_sr.color = _sr.color.SetAlpha(alpha);
		}
		public Texture2D GetMiniMap(Vector2 center)
		{
			var oWidth = Manager.Map.Width;
			var oHeight = Manager.Map.Height;
			var width = oWidth;
			var height = oHeight;
			var offsetX = 0;
			var offsetY = 0;
			if (width > _max)
			{
				width = _max;
				var halfWidth = width / 2;
				offsetX = (int)center.x - halfWidth;
				if (offsetX < 0) offsetX = 0;
				if (offsetX > oWidth - width) offsetX = oWidth - width;
			}
			if (height > _max)
			{
				height = _max;
				var halfHeight = height / 2;
				offsetY = (int)center.y - halfHeight;
				if (offsetY < 0) offsetY = 0;
				if (offsetY > oHeight - height) offsetY = oHeight - height;
			}
			var bounds = Manager.GameCamera.OrthographicBounds();
			var minX = Mathf.RoundToInt(bounds.min.x);
			var maxX = Mathf.RoundToInt(bounds.max.x);
			var minY = Mathf.RoundToInt(bounds.min.y);
			var maxY = Mathf.RoundToInt(bounds.max.y);
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var actualX = x + offsetX;
					var actualY = y + offsetY;
					var p = new Vector2(actualX, actualY);
					var character = Manager.Character.At(p);
					var screen =
						(
							((actualX >= minX) && (actualX <= maxX)) &&
							((actualY == minY) || (actualY == maxY))
						) ||
						(
							((actualY >= minY) && (actualY <= maxY)) &&
							((actualX == minX) || (actualX == maxX))
						);
					var color = character ? Colors.GreenLight :
						Manager.Map.GetMapColor(actualX, actualY, screen);
					texture.SetPixel(x, y, color);
				}
			}
			texture.filterMode = FilterMode.Point;
			texture.Apply();
			return texture;
		}
	}
}
