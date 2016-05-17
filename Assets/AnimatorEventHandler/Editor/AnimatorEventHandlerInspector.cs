using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class InspectorUtility
{
    private static GUIStyle m_BoldFoldout;
    public static GUIStyle BoldFoldout
    {
        get
        {
            if (m_BoldFoldout == null)
            {
                m_BoldFoldout = new GUIStyle(EditorStyles.foldout);
                m_BoldFoldout.fontStyle = FontStyle.Bold;
            }
            return m_BoldFoldout;
        }
    }
}

[CustomEditor(typeof(AnimatorEventHandler))]
public class AnimatorEventHandlerInspector : Editor
{
    private AnimatorEventHandler animatorEventHandler;

    [SerializeField]
    private static bool mStateFoldout = true;
    private ReorderableList mReorderableStateList;
    private SerializedProperty mAnimStateInfos;
    private Dictionary<int, SerializedObject> m_SerializedObjectMap = new Dictionary<int, SerializedObject>();

    private static List<Type> animStateInfoTypes = new List<Type>();

    private void OnEnable()
    {
        // Search through all of the assemblies to find any types that derive from AnimStateInfo.
        animStateInfoTypes.Clear();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; ++i)
        {
            var assemblyTypes = assemblies[i].GetTypes();
            for (int j = 0; j < assemblyTypes.Length; ++j)
            {
                // Must derive from AnimStateInfo;
                if (!typeof(AnimStateInfo).IsAssignableFrom(assemblyTypes[j]))
                {
                    continue;
                }

                // Ignore abstract classes.
                if (assemblyTypes[j].IsAbstract)
                {
                    continue;
                }

                animStateInfoTypes.Add(assemblyTypes[j]);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        animatorEventHandler = target as AnimatorEventHandler;

        if (animatorEventHandler == null || serializedObject == null)
            return;

        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        if ((mStateFoldout = EditorGUILayout.Foldout(mStateFoldout, "States", InspectorUtility.BoldFoldout)))
        {
            if (mReorderableStateList == null)
            {
                mAnimStateInfos = PropertyFromName(serializedObject, "m_AnimStateInfos");
                mReorderableStateList = new ReorderableList(serializedObject, mAnimStateInfos,
                    true/*draggable*/, false/*display header*/,
                    true/*add*/, true/*remove*/);
                mReorderableStateList.drawElementCallback = OnStateEventListDraw;
                mReorderableStateList.onReorderCallback = OnStateEventListReorder;
                mReorderableStateList.onAddCallback = OnStateEventListAdd;
                mReorderableStateList.onRemoveCallback = OnStateEventListRemove;
                mReorderableStateList.onSelectCallback = OnStateEventListSelect;
                if (animatorEventHandler.SelectedState != -1)
                {
                    mReorderableStateList.index = animatorEventHandler.SelectedState;
                }
            }
            mReorderableStateList.DoLayoutList();
            if (mReorderableStateList.index != -1)
            {
                if (mReorderableStateList.index < mAnimStateInfos.arraySize)
                {
                    var state = mAnimStateInfos.GetArrayElementAtIndex(mReorderableStateList.index).objectReferenceValue as AnimStateInfo;
                    if (state != null)
                        DrawSelectedState(state);
                }
                else
                {
                    mReorderableStateList.index = animatorEventHandler.SelectedState = -1;
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(animatorEventHandler, "Inspector");
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(animatorEventHandler);

        }
    }

    private void OnStateEventListDraw(Rect rect, int index, bool isActive, bool isFocused)
    {
        string label = string.Empty;
        var state = mAnimStateInfos.GetArrayElementAtIndex(index).objectReferenceValue as AnimStateInfo;

        // remove element which does no longer exist.
        if (ReferenceEquals(state, null))
        {
            mAnimStateInfos.DeleteArrayElementAtIndex(index);
            mReorderableStateList.index = animatorEventHandler.SelectedState = -1;
            mAnimStateInfos.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(animatorEventHandler);
            return;
        }

        // specify element name
        if (string.IsNullOrEmpty(state.stateName))
            label = state.GetType().Name;
        else
            label = state.stateName;

        var textDimension = GUI.skin.label.CalcSize(new GUIContent(label));
        int w = (int)textDimension.x + 10;
        EditorGUI.LabelField(new Rect(rect.x, rect.y, w, EditorGUIUtility.singleLineHeight), label);
        EditorGUI.ObjectField(new Rect(rect.x + w, rect.y, rect.width - w - 10, EditorGUIUtility.singleLineHeight), 
            state, typeof(AnimStateInfo));
    }

    /// <summary>
    /// Called whenever the elements of the ReorderableList are reordered.
    /// </summary>
    /// <param name="list"></param>
    private void OnStateEventListReorder(ReorderableList list)
    {
        EditorUtility.SetDirty(animatorEventHandler);
    }

    private struct CreationParams
    {
        public string Path;
    }

    private void OnStateEventListAdd(ReorderableList list)
    {
        var menu = new GenericMenu();

        // already exist AnimStateInfo asset files.
        var guids = AssetDatabase.FindAssets("t:AnimStateInfo");
        foreach(var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            menu.AddItem(new GUIContent(name), false, AddItemHandler, new CreationParams{ Path = path});
        }

        // newly create or no exist .asset file yet.
        for (int i = 0; i < animStateInfoTypes.Count; i++)
        {
            string name = animStateInfoTypes[i].ToString();// SplitCamelCase(animStateInfoTypes[i].ToString());
            menu.AddItem(new GUIContent("New/" + name), false, AddNewAnimStateInfo, animStateInfoTypes[i]);
        }
        menu.ShowAsContext();
    }

    private void AddItemHandler(object obj) 
    {
        CreationParams data = (CreationParams)obj;
        
        AnimStateInfo state = AssetDatabase.LoadAssetAtPath<AnimStateInfo>(data.Path);

        UpdateList(state);
    }

    private void AddNewAnimStateInfo(object obj)
    {
        AnimStateInfo state = ScriptableObject.CreateInstance<AnimStateInfo>();
        string assetpath = "Assets/";
        string assetname = "New AnimStateInfo.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath(assetpath + assetname);
        AssetDatabase.CreateAsset(state, path);

        UpdateList(state);

        AssetDatabase.SaveAssets();
    }

    private void UpdateList(AnimStateInfo state)
    {
        mAnimStateInfos.InsertArrayElementAtIndex(mAnimStateInfos.arraySize);
        mAnimStateInfos.GetArrayElementAtIndex(mAnimStateInfos.arraySize - 1).objectReferenceValue = state;
        mAnimStateInfos.serializedObject.ApplyModifiedProperties();
        animatorEventHandler.SelectedState = mReorderableStateList.index = mAnimStateInfos.arraySize - 1;

        EditorUtility.SetDirty(animatorEventHandler);
    }

    private void OnStateEventListRemove(ReorderableList list)
    {
        var state = mAnimStateInfos.GetArrayElementAtIndex(list.index).objectReferenceValue as AnimStateInfo;

        // The reference value must be null in order for the element to be removed from the SerializedProperty array.
        mAnimStateInfos.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
        mAnimStateInfos.DeleteArrayElementAtIndex(list.index);

        //HACK: causes abnormal crash of the editor.
        //Undo.DestroyObjectImmediate(state);

        list.index = -1;
        animatorEventHandler.SelectedState = -1;
        EditorUtility.SetDirty(animatorEventHandler);
    }

    /// <summary>
    /// An element is selected from the ReorderableList.
    /// </summary>
    /// <param name="list"></param>
    private void OnStateEventListSelect(ReorderableList list)
    {
        animatorEventHandler.SelectedState = list.index;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    private void DrawSelectedState(AnimStateInfo state)
    {
        SerializedObject stateSerializedObject;
        if (!m_SerializedObjectMap.TryGetValue(state.GetInstanceID(), out stateSerializedObject) ||
            stateSerializedObject.targetObject == null)
        {
            stateSerializedObject = new SerializedObject(state);
            m_SerializedObjectMap.Remove(state.GetInstanceID());
            m_SerializedObjectMap.Add(state.GetInstanceID(), stateSerializedObject);
        }
        stateSerializedObject.Update();

        //EditorGUILayout.LabelField();
        EditorGUILayout.Separator();

        EditorGUI.BeginChangeCheck();
        EditorGUI.indentLevel++;
        var property = stateSerializedObject.GetIterator();
        property.NextVisible(true);
        do
        {
            EditorGUILayout.PropertyField(property);
        } while (property.NextVisible(false));
        EditorGUI.indentLevel--;

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(state, "Inspector");
            stateSerializedObject.ApplyModifiedProperties();
            if (state != null)
                EditorUtility.SetDirty(state);
        }
    }

    private Dictionary<string, SerializedProperty> m_PropertyStringMap = new Dictionary<string, SerializedProperty>();
    public SerializedProperty PropertyFromName(SerializedObject serializedObject, string name)
    {
        SerializedProperty property = null;
        if (m_PropertyStringMap.TryGetValue(name, out property))
        {
            return property;
        }

        property = serializedObject.FindProperty(name);
        if (property == null)
        {
            Debug.LogError("Unable to find property " + name);
            return null;
        }
        m_PropertyStringMap.Add(name, property);
        return property;
    }

    private static Regex s_CamelCaseRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
    private Dictionary<string, string> m_CamelCaseSplit = new Dictionary<string, string>();

    /// <summary>
    /// Places a space before each capital letter in a word.
    /// </summary>
    private string SplitCamelCase(string s)
    {
        if (s.Equals(""))
            return s;
        if (m_CamelCaseSplit.ContainsKey(s))
        {
            return m_CamelCaseSplit[s];
        }

        var origString = s;
        s = s_CamelCaseRegex.Replace(s, " ");
        s = s.Replace("_", " ");
        s = (char.ToUpper(s[0]) + s.Substring(1)).Trim();
        m_CamelCaseSplit.Add(origString, s);
        return s;
    }
}