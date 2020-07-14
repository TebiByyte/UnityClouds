using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugBounds : MonoBehaviour
{
    public bool showBounds = true;

    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
