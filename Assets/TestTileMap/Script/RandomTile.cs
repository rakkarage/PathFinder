using System;
namespace UnityEngine.Tilemaps
{
	[CreateAssetMenu]
	[Serializable]
	public class RandomTile : TileBase
	{
		public RandomTileData Data;
		public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
		{
			Data.NextTile.GetTileData(position, tilemap, ref tileData);
			tileData.flags = TileFlags.LockTransform;
			tileData.transform = Data.NextMatrix;
		}
	}
}
