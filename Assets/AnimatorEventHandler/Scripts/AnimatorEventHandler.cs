using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class AnimatorEventHandler : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    private AnimStateInfo[] m_AnimStateInfos = new AnimStateInfo[0];

#if UNITY_EDITOR
    // Used by the editor to keep the ability list selection.
    [SerializeField]
    private int m_SelectedState = -1;
    public int SelectedState { get { return m_SelectedState; } set { m_SelectedState = value; } }
#endif


}
