using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GraphicsPipeline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Model myModel = new Model();
        List<Vector3> verts = myModel.vertices;

        myModel.CreateUnityGameObject();
        Vector3 axis = new Vector3(17, 0, 0).normalized;
        Matrix4x4 rotationMatrix =
            Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(12, axis), Vector3.one);

        SaveMatrixToFile(rotationMatrix, "rotationMatrix.txt");

        Matrix4x4 translationMatrix =
            Matrix4x4.TRS(new Vector3(-3, 1, 1), Quaternion.identity, Vector3.one);

        SaveMatrixToFile(translationMatrix, "translationMatrix.txt");
    }
    private void SaveMatrixToFile(Matrix4x4 matrix, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < 4; i++)
            {
                Vector4 row = matrix.GetRow(i);
                writer.WriteLine($"{row.x}, {row.y}, {row.z}, {row.w}");
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}