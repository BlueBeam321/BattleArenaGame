using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshProFollow : MonoBehaviour
{
    public float yOffset = 1.5f;  // the vertical offset from the player's head

    private void LateUpdate()
    {
        // update the position of the TextMeshPro object

        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }
}
