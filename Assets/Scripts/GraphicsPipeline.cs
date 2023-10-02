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
        List<Vector4> verts = ConvertToHomg(myModel.vertices);

        // myModel.CreateUnityGameObject(); 
        Vector3 axis = (new Vector3(-2, 1, 1)).normalized;

        Matrix4x4 matrixRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(17, axis), Vector3.one);
        Matrix4x4 matrixScale = Matrix4x4.TRS(Vector3.one, Quaternion.identity, new Vector3(5, 4, 4));
        Matrix4x4 matrixTranslation = Matrix4x4.TRS(new Vector3(-3, 1, 1), Quaternion.identity, Vector3.one);
        Matrix4x4 matrixViewing = Matrix4x4.LookAt(new Vector3(19, 3, 50), new Vector3(-5, 0, 3), new Vector3(1, 1, 0));
        Matrix4x4 matrixProjection = Matrix4x4.Perspective(90, 16 / 9, 1, 1000);

        SaveMatrixToFile(matrixRotation, "matrixRotation.txt");
        SaveMatrixToFile(matrixScale, "matrixScale.txt");
        SaveMatrixToFile(matrixTranslation, "matrixTranslation.txt");
        SaveMatrixToFile(matrixViewing, "matrixViewing.txt");
        SaveMatrixToFile(matrixProjection, "matrixProjection.txt");


        List<Vector4> imageAfterRotation = ApplyTransformation(verts, matrixRotation);

        List<Vector4> imageAfterScale = ApplyTransformation(imageAfterRotation, matrixScale);

        List<Vector4> imageAfterTranslation = ApplyTransformation(imageAfterScale, matrixTranslation);

        SaveVector4ListToFile(imageAfterRotation, "imageAfterRotation.txt");
        SaveVector4ListToFile(imageAfterScale, "imageAfterScale.txt");
        SaveVector4ListToFile(imageAfterTranslation, "imageAfterTranslation.txt");


        //World Transform Matrix Test
        Matrix4x4 worldTransformMatrix = matrixTranslation * matrixScale * matrixRotation;
        SaveMatrixToFile(worldTransformMatrix, "worldTransformMatrix.txt");
        List<Vector4> imageAfterWorldTransformMatrix = ApplyTransformation(verts, worldTransformMatrix);
        SaveVector4ListToFile(imageAfterWorldTransformMatrix, "imageAfterWorldTransformMatrix.txt");


        //Continue with Pipeline
        List<Vector4> viewVertices3D = ApplyTransformation(imageAfterTranslation, matrixViewing);

        List<Vector4> viewVertices2D = ApplyTransformation(viewVertices3D, matrixProjection);

    }




    private List<Vector4> ConvertToHomg(List<Vector3> vertices)
    {
        List<Vector4> output = new List<Vector4>();

        foreach (Vector3 v in vertices)
        {
            output.Add(new Vector4(v.x, v.y, v.z, 1.0f));

        }
        return output;

    }

    private List<Vector4> ApplyTransformation
        (List<Vector4> verts, Matrix4x4 tranformMatrix)
    {
        List<Vector4> output = new List<Vector4>();
        foreach (Vector4 v in verts)
        { output.Add(tranformMatrix * v); }

        return output;

    }

    private void DisplayMatrix(Matrix4x4 rotationMatrix)
    {
        for (int i = 0; i < 4; i++)
        { print(rotationMatrix.GetRow(i)); }
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

    private void SaveVector4ListToFile(List<Vector4> vectorList, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (Vector4 vector in vectorList)
            {
                writer.WriteLine($"{vector.x}, {vector.y}, {vector.z}, {vector.w}");
            }
        }
    }



    // Update is called once per frame
    void Update()
    {

    }
}