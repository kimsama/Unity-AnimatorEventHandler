using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

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
    private SerializedProperty mAnimatorEventStates;

    

    private void OnEnable()
    {
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
                mAnimatorEventStates = PropertyFromName(serializedObject, "m_AnimatorStateEvents");
                mReorderableStateList = new ReorderableList(serializedObject, mAnimatorEventStates,
                    true/*draggable*/, false/*display header*/,
                    true/*add*/, true/*remove*/);
                mReorderableStateList.drawElementCallback = OnStateEventListDraw;
                mReorderableStateList.onReorderCallback = OnStateEventListReorder;
                //mReorderableStateList.onAddCallback = OnStateEventListAdd;
                //mReorderableStateList.onRemoveCallback = OnStateEventListRemove;
                mReorderableStateList.onSelectCallback = OnStateEventListSelect;
                if (animatorEventHandler.SelectedState != -1)
                {
                    mReorderableStateList.index = animatorEventHandler.SelectedState;
                }
            }
            mReorderableStateList.DoLayoutList();
            if (mReorderableStateList.index != -1)
            {
                if (mReorderableStateList.index < mAnimatorEventStates.arraySize)
                {
                    //DrawSelectedState(...);
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

            //UnityEditor.Animations.AnimatorController controller = null;
        }
    }

    private void OnStateEventListDraw(Rect rect, int index, bool isActive, bool isFocused)
    {
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
}