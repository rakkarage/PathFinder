using SubjectNerd.Utilities;
using UnityEngine;
using UnityEditor;
public class GridBrushInspector : GridBrushEditor
{
	ReorderableArrayInspector editor;
	private void OnGUI()
	{
		if (!editor)
			editor = Editor.CreateEditor(target, typeof(ReorderableArrayInspector)) as ReorderableArrayInspector;
		editor.OnInspectorGUI();
	}
}
