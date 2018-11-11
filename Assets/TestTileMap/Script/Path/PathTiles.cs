using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu]
public class PathTiles : ScriptableObject
{
	public TileBase DoorOpen;
	public TileBase DoorShut;
	public List<TileBase> StairsDown;
	public List<TileBase> StairsUp;
	public List<TileBase> Wall;
	public List<TileBase> Floor;
	public List<TileBase> Light;
	public List<TileBase> Torches;
}
