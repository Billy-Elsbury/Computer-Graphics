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

    int textureWidth = 256;
    int textureHeight = 256;

    Model myModel = new Model();

    // Set the color for the line
    UnityEngine.Color lineColour = UnityEngine.Color.red;
    UnityEngine.Color fillColour = UnityEngine.Color.cyan;
    UnityEngine.Color lightColor = UnityEngine.Color.blue;


    Vector3 lightDirection = new Vector3(1, -1, 1).normalized; // example direction
    float lightIntensity = 1.0f; // full intensity

    float angle = 0;
    private UnityEngine.Color backgroundColour;

    // Start is called before the first frame update
    public void Start()
    {
        Vector2 s1 = new Vector2(-0.09f, 0.61f), e1 = new Vector2(-1.11f, -1.69f);
        LineClip(ref s1, ref e1);

        ourScreen = FindObjectOfType<Renderer>();

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
        matrixWorld = matrixWorld * Matrix4x4.TRS(new Vector3(0, 0, 5), Quaternion.identity, Vector3.one);


        List<Vector4> verts = ConvertToHomg(myModel.vertices);

        // Multiply in reverse order, points are multiplied on the right, A * v
        Matrix4x4 matrixSuper = matrixProjection * matrixViewing * matrixWorld;

        List<Vector4> transformedVerts = DivideByZ(ApplyTransformation(verts, matrixSuper));

        List<Vector2Int> pixelPoints = Pixelise(transformedVerts, textureWidth, textureHeight);



        Texture2D screenTexture = new Texture2D(textureWidth, textureHeight);
        print(screenTexture.GetPixel(10, 10));

        backgroundColour = screenTexture.GetPixel(10, 10);

        Destroy(ourScreen.material.mainTexture);

        ourScreen.material.mainTexture = screenTexture;

        bool[,] frameBuffer = new bool[textureWidth, textureHeight];


        foreach (Vector3Int face in myModel.faces)
        {

            if (!ShouldCull(transformedVerts[face.x], transformedVerts[face.y], transformedVerts[face.z]))
            {
                Vector2Int total = new Vector2Int(0, 0);
                int count = 0;


                Vector2Int v = ClipAndPlot(transformedVerts[face.x], transformedVerts[face.y], screenTexture, ref frameBuffer);
                if (v.x >= 0)
                {
                    total = new Vector2Int(total.x + v.x, total.y + v.y);
                    count++;
                }

                Vector2Int v1 = ClipAndPlot(transformedVerts[face.y], transformedVerts[face.z], screenTexture, ref frameBuffer);
                if (v1.x >= 0)
                {
                    total = new Vector2Int(total.x + v1.x, total.y + v1.y);
                    count++;
                }

                Vector2Int v2 = ClipAndPlot(transformedVerts[face.z], transformedVerts[face.x], screenTexture, ref frameBuffer);
                if (v2.x >= 0)
                {
                    total = new Vector2Int(total.x + v2.x, total.y + v2.y);
                    count++;
                }


                if (count > 0)
                {
                    FloodFill(averagePosition(total, count), fillColour, screenTexture, ref frameBuffer);
                    //if result not in viewport do some work, weight towards point that is in viewport
                    // new method weightedAverage() 
                }
            }
        }

        foreach (Vector3Int face in myModel.faces)
        {
            // Calculate the normal of the face
            Vector3 v0 = transformedVerts[face.x];
            Vector3 v1 = transformedVerts[face.y];
            Vector3 v2 = transformedVerts[face.z];
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            // Calculate the dot product between the normal and light direction
            float dot = Mathf.Max(Vector3.Dot(normal, lightDirection), 0);
            UnityEngine.Color faceColor = lightColor * dot * lightIntensity;

            // Apply lighting to face color
            // ... [rest of your code for rendering the face]
        }

        screenTexture.Apply();
    }

    private Vector2Int averagePosition(Vector2Int total, int count)
    {
        return new Vector2Int(total.x / count, total.y / count);
    }

    private Vector2Int averagePosition(Vector4 v1, Vector4 v2, Vector4 v3)
    {
        Vector4 average = (v1 + v2 + v3) / 3;

        return Pixelise(average, textureWidth, textureHeight);
    }

    // Flood fill algorithm
    void FloodFill(Vector2Int startLocation, UnityEngine.Color fillColour, Texture2D screenTexture, ref bool[,] frameBuffer)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startLocation);

        while (stack.Count > 0)
        {
            Vector2Int location = stack.Pop();

            if (!IsWithinBounds(location) || frameBuffer[location.x, location.y])
            {
                continue;
            }

            SetPixel(location, fillColour, ref frameBuffer);

            // Add adjacent pixels to the stack
            stack.Push(new Vector2Int(location.x + 1, location.y));
            stack.Push(new Vector2Int(location.x - 1, location.y));
            stack.Push(new Vector2Int(location.x, location.y + 1));
            stack.Push(new Vector2Int(location.x, location.y - 1));
        }
    }


    // Method to check if a location is within screen border
    bool IsWithinBounds(Vector2Int location)
    {
        return (location.x >= 0) && (location.x < textureWidth) && (location.y >= 0) && (location.y < textureHeight);
    }



    // Method to set a pixel's color
    void SetPixel(Vector2Int location, UnityEngine.Color color, ref bool[,] frameBuffer)
    {
        (ourScreen.material.mainTexture as Texture2D).SetPixel(location.x, location.y, color);
        frameBuffer[location.x, location.y] = true;
    }

    private bool ShouldCull(Vector4 vert1, Vector4 vert2, Vector4 vert3)
    {
        Vector3 v1 = new Vector3(vert1.x, vert1.y, 0);
        Vector3 v2 = new Vector3(vert2.x, vert2.y, 0);
        Vector3 v3 = new Vector3(vert3.x, vert3.y, 0);

        return (Vector3.Cross(v2 - v1, v3 - v2).z <= 0);
    }

    //Converted to pixels before clipping

    //weight towards
    private Vector2Int ClipAndPlot(Vector4 startIn, Vector4 endIn, Texture2D lineDrawnTexture, ref bool[,] frameBuffer)
    {
        Vector2Int output = new Vector2Int(-1, -1);

        Vector2 start = new Vector2(startIn.x, startIn.y);
        Vector2 end = new Vector2(endIn.x, endIn.y);

        if (LineClip(ref start, ref end))
        {
            output = Pixelise((start + end) / 2, textureWidth, textureHeight);

            List<Vector2Int> pixels = Bresenham(Pixelise(start, textureWidth, textureHeight), Pixelise(end, textureWidth, textureHeight));

            DrawLineOnTexture(pixels, lineDrawnTexture, lineColour, ref frameBuffer);
        }

        return output;
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
        int x = (int)((textureWidth - 1) * (v.x + 1) / 2);
        int y = (int)((textureHeight - 1) * (v.y + 1) / 2);
        return new Vector2Int(x, y);
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

        if (startOutcode.up)
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "up");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode.down)
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "down");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode.left)
        {
            Vector2 hold = LineIntercept(startPoint, endPoint, "left");
            if (new Outcode(hold) == viewportOutcode)
            {
                startPoint = hold;
                return LineClip(ref endPoint, ref startPoint);
            }
        }
        if (startOutcode.right)
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

    public void DrawLineOnTexture(List<Vector2Int> linePoints, Texture2D texture, UnityEngine.Color color, ref bool[,] frameBuffer)
    {
        foreach (Vector2Int point in linePoints)
        {
            SetPixel(point, color, ref frameBuffer); ;
            texture.SetPixel(point.x, point.y, color);

        }

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