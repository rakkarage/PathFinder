using UnityEngine;
namespace UnityEditor
{
	[CustomGridBrush(true, false, false, "Coordinate Brush")]
	public class CoordinateBrush : GridBrush
	{
	}
	[CustomEditor(typeof(CoordinateBrush))]
	public class CoordinateBrushEditor : GridBrushEditor
	{
		public override void OnPaintSceneGUI(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
		{
			base.OnPaintSceneGUI(gridLayout, brushTarget, position, tool, executing);
			Handles.Label(position.position, position.position.ToString(), EditorStyles.whiteLargeLabel);
		}
	}
}
