using System;
namespace UnityEngine.Tilemaps
{
	[Flags]
	public enum TileOrientation
	{
		None = 0,
		FlipX = 1,
		FlipY = (1 << 1),
		Rot90 = (1 << 2),
	}
}
