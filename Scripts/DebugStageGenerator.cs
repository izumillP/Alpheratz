using UnityEngine;
using UnityEditor;

public class DebugStageGeneratorParameter : ScriptableSingleton<DebugStageGeneratorParameter>
{
    public const int BOX_MAX_X = 10;
    public const int BOX_MAX_Y = 20;

    public int GetBoxMaxX()
    {
        return BOX_MAX_X;
    }
    public int GetBoxMaxY()
    {
        return BOX_MAX_Y;
    }

    public bool IsDebugMode { get; set; } = false;
    public bool[] DebugStageMap { get; set; } = new bool[BOX_MAX_X * BOX_MAX_Y];
    public MinoType minoType { get; set; } = MinoType.Empty;
}
public class DebugStageGenerator : UnityEditor.EditorWindow
{
    [MenuItem("Window/DebugStageGenerator")] static void Init()
    {
        DebugStageGenerator window = CreateInstance<DebugStageGenerator>();
        window.Show();
    }

    void OnGUI()
    {
        var instance = DebugStageGeneratorParameter.instance;

        instance.IsDebugMode = EditorGUILayout.Toggle("DebugMode", instance.IsDebugMode);
        if (GUILayout.Button("Dummy Reset"))
        {
            for(int i = 0; i < instance.DebugStageMap.Length; i++)
            {
                instance.DebugStageMap[i] = false;
            }
        }
        instance.minoType = (MinoType)EditorGUILayout.EnumPopup("Select Initial Hold Mino", instance.minoType);

        EditorGUILayout.BeginVertical();
        for (int y = 0; y < instance.GetBoxMaxY(); y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < instance.GetBoxMaxX(); x++)
            {
                instance.DebugStageMap[x + instance.GetBoxMaxX() * y] = EditorGUILayout.Toggle(instance.DebugStageMap[x + instance.GetBoxMaxX() * y]);
            }
            EditorGUILayout.LabelField((y + 2).ToString());
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        for (int x = 0; x < instance.GetBoxMaxX(); x++)
        {
                EditorGUILayout.LabelField((x + 3).ToString(), GUILayout.MaxWidth(14f));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}