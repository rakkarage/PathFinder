using System;
namespace UnityEngine.Tilemaps
{
	[CreateAssetMenu]
	[Serializable]
	public class AnimatedTile : TileBase
	{
		public Sprite[] AnimatedSprites;
		public bool RandomStartTime = true;
		[MinMaxSlider(1, 10)]
		public Vector2 Speed = Vector2.one;
		public Tile.ColliderType TileColliderType;
		public override void GetTileData(Vector3Int position, ITilemap tileMap, ref TileData tileData)
		{
			tileData.transform = Matrix4x4.identity;
			tileData.color = Color.white;
			if (AnimatedSprites?.Length > 0)
			{
				tileData.sprite = AnimatedSprites[RandomStartTime ? Random.Range(0, AnimatedSprites.Length) : 0];
				tileData.colliderType = TileColliderType;
			}
		}
		public override bool GetTileAnimationData(Vector3Int position, ITilemap tileMap, ref TileAnimationData tileAnimationData)
		{
			if (AnimatedSprites?.Length == 0)
				return false;
			tileAnimationData.animatedSprites = AnimatedSprites;
			tileAnimationData.animationSpeed = Random.Range(Speed.x, Speed.y);
			tileAnimationData.animationStartTime = RandomStartTime ? Random.value : 0;
			return true;
		}
	}
}
