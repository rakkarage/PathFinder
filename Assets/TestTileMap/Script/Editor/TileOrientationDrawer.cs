using UnityEditor;
namespace UnityEngine.Tilemaps
{
	[CustomPropertyDrawer(typeof(TileOrientation))]
	public class TileOrientationDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUI.EnumFlagsField(position, label, (TileOrientation)property.intValue);
			if (EditorGUI.EndChangeCheck())
				property.intValue = (int)(TileOrientation)newValue;
			EditorGUI.EndProperty();
		}
	}
}
