using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class AnimatorEventHandler : MonoBehaviour
{

    [SerializeField]
    private AnimatorStateEvent[] m_AnimatorStateEvents = new AnimatorStateEvent[0];

#if UNITY_EDITOR
    // Used by the editor to keep the ability list selection.
    [SerializeField]
    private int m_SelectedState = -1;
    public int SelectedState { get { return m_SelectedState; } set { m_SelectedState = value; } }
#endif


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
