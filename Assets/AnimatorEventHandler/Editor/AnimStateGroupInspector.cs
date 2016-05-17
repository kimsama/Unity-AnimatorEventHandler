using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(AnimStateGroup))]
public class AnimStateGroupInspector : Editor
{
    private AnimStateGroup animStateGroup;

    private ReorderableList mReorderableStateList;
    private SerializedProperty mAnimStateInfos;
    private Dictionary<int, SerializedObject> m_SerializedObjectMap = new Dictionary<int, SerializedObject>();

    private static List<Type> animStateGroupList = new List<Type>();

    void OnEnable()
    {
        ReflectionUtil.SearchType<AnimStateGroup>(ref animStateGroupList);
    }

    public override void OnInspectorGUI()
    {
        animStateGroup = target as AnimStateGroup;
        if (animStateGroup == null || serializedObject == null)
            return;

        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
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
            if (animStateGroup.SelectedState != -1)
            {
                mReorderableStateList.index = animStateGroup.SelectedState;
            }
        }
        mReorderableStateList.DoLayoutList();
        if (mReorderableStateList.index != -1)
        {

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
            mReorderableStateList.index = animStateGroup.SelectedState = -1;
            mAnimStateInfos.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(animStateGroup);
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

    private void OnStateEventListReorder(ReorderableList list)
    {
    }

    private void OnStateEventListAdd(ReorderableList list)
    {
    }

    private void OnStateEventListRemove(ReorderableList list)
    {
    }

    private void OnStateEventListSelect(ReorderableList list)
    {
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

    [MenuItem("Assets/Create/Animator/Create AnimStateGroup")]
    public static void CreateScriptMachineAsset()
    {
        AnimStateGroup inst = ScriptableObject.CreateInstance<AnimStateGroup>();
        string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback("New AnimStateGroup.asset");
        AssetDatabase.CreateAsset(inst, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = inst;
    }
}