using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireframeRenderer : MonoBehaviour
{
    public Material wireframeMaterial; // Reference to the wireframe material

    void Start()
    {
        // Check if a Renderer component exists on this GameObject
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            // Assign the wireframe material
            renderer.material = wireframeMaterial;
        }
    }
}
