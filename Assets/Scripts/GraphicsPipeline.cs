using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GraphicsPipeline : MonoBehaviour
{
    Renderer ourScreen;

    int textureWidth = 255;
    int textureHeight = 255;

    Model myModel = new Model();

    // Set the color for the line
    UnityEngine.Color lineColour = UnityEngine.Color.red;

    float angle = 0;

    // Start is called before the first frame update
    public void Start()
    {
        ourScreen = FindObjectOfType<Renderer>();

        Model myModel = new Model();
        List<Vector4> verts = ConvertToHomg(myModel.vertices);

        myModel.CreateUnityGameObject();
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




        Outcode outcode = new Outcode(new Vector2(3, -3));

        print(outcode.outcodeString());

        Vector2 startPoint = new Vector2(-2, 1);
        Vector2 endPoint = new Vector2(3, 0);

        LineClip(ref startPoint, ref endPoint);

        print(startPoint + " " + endPoint);



        //_____Drawing Lines on Texture_______________________________________



        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(255, 255);

        List<Vector2Int> linePoints = Bresenham(start, end);

    }

    void Update()
    {
        angle++;

        Matrix4x4 matrixViewing = Matrix4x4.LookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix4x4 matrixProjection = Matrix4x4.Perspective(90, ((float)textureWidth / (float)textureHeight), 1, 1000);
        Matrix4x4 matrixWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.one.normalized), Vector3.one);

        
        List<Vector4> verts = ConvertToHomg(myModel.vertices);

        // Multiply in reverse order, points are multiplied on the right, A * v
        Matrix4x4 matrixSuper = matrixProjection * matrixViewing * matrixWorld;

        List<Vector4> transformedVerts = DivideByZ(ApplyTransformation(verts, matrixSuper));

        // List<Vector2Int> pixelPoints = pixelise(transformedVerts, textureWidth, textureHeight);


        Texture2D lineDrawnTexture = new Texture2D(textureWidth, textureHeight);

        ourScreen.material.mainTexture = lineDrawnTexture;
        foreach (Vector3Int face in myModel.faces)
        {
            ClipAndPlot(transformedVerts[face.x], transformedVerts[face.y],lineDrawnTexture);
            ClipAndPlot(transformedVerts[face.y], transformedVerts[face.z], lineDrawnTexture);
            ClipAndPlot(transformedVerts[face.z], transformedVerts[face.x], lineDrawnTexture);

        }

    }

    //Converted to pixels before clipping
    private void ClipAndPlot(Vector4 startIn, Vector4 endIn, Texture2D lineDrawnTexture)
    { 
        Vector2 start = new Vector2(startIn.x, startIn.y);
        Vector2 end = new Vector2(endIn.x, endIn.y);
        if (LineClip(ref start, ref end))
        {
           List<Vector2Int> pixels = Bresenham(Pixelise(start, textureWidth, textureHeight), Pixelise(end, textureWidth, textureHeight));

            DrawLineOnTexture(pixels, lineDrawnTexture, lineColour);
        }
    }

    private List<Vector2Int> Pixelise(List<Vector4> transformedVerts, int textureWidth, int textureHeight)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        foreach (Vector4 v in transformedVerts)
        {
            output.Add(Pixelise(v, textureWidth, textureHeight));
        }

        return output;
    }

    private Vector2Int Pixelise(Vector2 v, int textureWidth, int textureHeight)
    {
        int x = (int )((textureWidth - 1) * (v.x + 1) / 2);
        int y = (int)((textureHeight - 1) * (v.y + 1) / 2);
        return new Vector2Int( x,y );
    }

    //
    private List<Vector4> DivideByZ(List<Vector4> vector4s)
    {
        List<Vector4> output = new List<Vector4>();

        foreach (Vector4 v in vector4s) 
        {
            output.Add(new Vector4(v.x / v.w, v.y / v.w, v.z, v.w));
        }

        return output;
    }

    private bool LineClip(ref Vector2 startPoint, ref Vector2 endPoint)
    {
        Outcode startOutcode = new Outcode(startPoint);
        Outcode endOutcode = new Outcode(endPoint);

        Outcode viewportOutcode = new Outcode();

        if ((startOutcode + endOutcode == viewportOutcode)) return true; //Both Outcodes in viewport
        if ((startOutcode * endOutcode) != viewportOutcode) return false;
        //Both have a 1 in common in outcodes so either both up, down, left, right, so won't be in viewport

        //If neither return, more work to do...

        //if the code gets to here only concerned with clipping start
        if (startOutcode == viewportOutcode) return LineClip(ref endPoint, ref startPoint);

        if (startOutcode == new Outcode(true, false, false, false))
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "up");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode == new Outcode(false, true, false, false))
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "down");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode == new Outcode(false, false, true, false))
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "left");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode == new Outcode(false, false, false, true))
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "right");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }

        return false; //means no intercept was found.

        /*Get the outcodes of the two coordinates,
     * If both outcodes are 0000 we can 'trivial accept' the coords
     * If the 'AND' of the two outcodes IS NOT 0000 we can 'trivial reject'
     * If the 'AND' of the coords IS 0000 there is more work to do...*/
    }

    private Vector2 LineIntercept(Vector2 startPoint, Vector2 endPoint, String viewportSide)
    {
        float m = (endPoint.y - startPoint.y) / (endPoint.x - startPoint.x);

        if (viewportSide == "up") return new Vector2((startPoint.x + ((1 - startPoint.y) / m)), 1);
        if (viewportSide == "down") return new Vector2((startPoint.x + ((-1 - startPoint.y) / m)), -1);
        if (viewportSide == "left") return new Vector2(-1, (startPoint.y + (m * (-1 - startPoint.x))));
        if (viewportSide == "right") return new Vector2(1, (startPoint.y + (m * (1 - startPoint.x))));

        else throw new ArgumentOutOfRangeException(nameof(viewportSide), "The viewport Side is incorrect");
    }

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // Bresenham's line drawing algorithm
    public List<Vector2Int> Bresenham(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> output = new List<Vector2Int>();

        int dx = end.x - start.x;
        // If dx is negative, flip the line
        if (dx < 0)
            return Bresenham(end, start);

        int dy = end.y - start.y;
        // If dy is negative, line goes up. 
        if (dy < 0)
            return NegY(Bresenham(NegY(start), NegY(end)));

        // If dy > dx swap the axes.
        if ((dy) > (dx))
            return SwapXY(Bresenham(SwapXY(start), SwapXY(end)));

        int ddx = 2 * dy;
        int ddy = 2 * (dy - dx);
        int p = 2 * dy - dx;

        // Loop over the x-axis and calculate y-axis values.
        for (int x = start.x, y = start.y; x <= end.x; x++)
        {
            output.Add(new Vector2Int(x, y));
            if (p < 0)
            {
                p += ddx; // Move to next pixel on the right
            }
            else
            {
                p += ddy; // Move to next pixel diagonal
                y++;
            }
        }
        return output;
    }

    private List<Vector2Int> SwapXY(List<Vector2Int> vector2Ints)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        foreach (Vector2Int v in vector2Ints)
            output.Add(SwapXY(v));

        return output;
    }

    private List<Vector2Int> NegY(List<Vector2Int> vector2Ints)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        foreach (Vector2Int v in vector2Ints)
            output.Add(NegY(v));

        return output;
    }

    // For negative slopes
    private Vector2Int NegY(Vector2Int point)
    {
        return new Vector2Int(point.x, -point.y);
    }



    // Swap x and y for lines where slope > 1
    private Vector2Int SwapXY(Vector2Int point)
    {
        return new Vector2Int(point.y, point.x);
    }

    public void DrawLineOnTexture(List<Vector2Int> linePoints, Texture2D texture, UnityEngine.Color color)
    {
        foreach (Vector2Int point in linePoints)
        {
            texture.SetPixel(point.x, point.y, color);
        }
        texture.Apply();

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
}