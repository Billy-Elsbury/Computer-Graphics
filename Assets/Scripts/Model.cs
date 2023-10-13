using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Model
{
    internal List<Vector3Int> faces;
    List<Vector3Int> texture_index_list; //3 coords for triangle on texture
    internal List<Vector3> vertices;
    List<Vector2> texture_coordinates; //u and v Coords
    List<Vector3> normals;
    public Model()
    {
        vertices = new List<Vector3>();
        addVertices();

        faces = new List<Vector3Int>();
        addFaces();

    }

    private void addFaces()
    {
        
        //Front Face List (13 triangles)
        faces.Add(new Vector3Int(0, 9, 1)); 
        faces.Add(new Vector3Int(9, 2, 1)); 
        faces.Add(new Vector3Int(10, 3, 2)); 
        faces.Add(new Vector3Int(10, 4, 3)); 
        faces.Add(new Vector3Int(13,12, 11)); 
        faces.Add(new Vector3Int(12, 13, 14)); 
        faces.Add(new Vector3Int(4, 16, 5)); 
        faces.Add(new Vector3Int(5, 16, 6)); 
        faces.Add(new Vector3Int(6, 15, 7)); 
        faces.Add(new Vector3Int(15, 8, 7));
        faces.Add(new Vector3Int(8, 15, 13));
        faces.Add(new Vector3Int(13, 0, 8));
        faces.Add(new Vector3Int(9, 0, 13));
        

        //All Sides of Letter (18 triangles) 
        faces.Add(new Vector3Int(0, 1, 17));
        faces.Add(new Vector3Int(1, 18, 17));
        faces.Add(new Vector3Int(1, 2, 18));
        faces.Add(new Vector3Int(2, 19, 18));
        faces.Add(new Vector3Int(2, 3, 19));
        faces.Add(new Vector3Int(3, 20, 19));
        faces.Add(new Vector3Int(5, 6, 22));
        faces.Add(new Vector3Int(6, 23, 22));
        faces.Add(new Vector3Int(6, 7, 23));
        faces.Add(new Vector3Int(7, 24, 23));
        faces.Add(new Vector3Int(7, 25, 24));
        faces.Add(new Vector3Int(8, 25, 7));
        faces.Add(new Vector3Int(8, 17, 25));
        faces.Add(new Vector3Int(0, 17, 8));

        faces.Add(new Vector3Int(22, 21, 4));
        faces.Add(new Vector3Int(22, 4, 5));
        faces.Add(new Vector3Int(20, 3, 4));
        faces.Add(new Vector3Int(20, 4, 21));

        faces.Add(new Vector3Int(28, 26, 9));
        faces.Add(new Vector3Int(28, 9, 11));
        faces.Add(new Vector3Int(29, 10, 27));
        faces.Add(new Vector3Int(29, 12, 10));
        faces.Add(new Vector3Int(30, 15, 32));
        faces.Add(new Vector3Int(30, 13, 15));
        faces.Add(new Vector3Int(31, 33, 16));
        faces.Add(new Vector3Int(31, 16, 14));

        faces.Add(new Vector3Int(10, 9, 26));
        faces.Add(new Vector3Int(10, 26, 27));
        faces.Add(new Vector3Int(11, 12, 29));
        faces.Add(new Vector3Int(11, 29, 28));
        faces.Add(new Vector3Int(14, 13, 30));
        faces.Add(new Vector3Int(14, 30, 31));
        faces.Add(new Vector3Int(15, 16, 33));
        faces.Add(new Vector3Int(15, 33, 32));


        //Back Face List (13 Triangles)
        faces.Add(new Vector3Int(18, 19, 26));
        faces.Add(new Vector3Int(17, 18, 26));
        faces.Add(new Vector3Int(17, 26, 30));
        faces.Add(new Vector3Int(17, 30, 25));
        faces.Add(new Vector3Int(28, 29, 30));
        faces.Add(new Vector3Int(29, 31, 30));
        faces.Add(new Vector3Int(30, 32, 25));
        faces.Add(new Vector3Int(32, 24, 25));
        faces.Add(new Vector3Int(32, 23, 24));
        faces.Add(new Vector3Int(23, 33, 22));
        faces.Add(new Vector3Int(33, 21, 22));
        faces.Add(new Vector3Int(20, 21, 27));
        faces.Add(new Vector3Int(19, 20, 27));
    }

    private void addVertices()
    {
        
        //Front Face Vertices
        vertices.Add(new Vector3(-3, 6, -1)); // 0
        vertices.Add(new Vector3(2, 6, -1)); // 1
        vertices.Add(new Vector3(3, 5, -1)); // 2
        vertices.Add(new Vector3(3, 1, -1)); // 3
        vertices.Add(new Vector3(2, 0, -1)); // 4
        vertices.Add(new Vector3(3, -1,-1)); // 5
        vertices.Add(new Vector3(3, -5, -1)); // 6
        vertices.Add(new Vector3(2, -6, -1)); // 7
        vertices.Add(new Vector3(-3, -6, -1)); // 8
        vertices.Add(new Vector3(-2, 5, -1)); // 9
        vertices.Add(new Vector3(2, 5, -1)); // 10
        vertices.Add(new Vector3(-2, 1, -1)); // 11
        vertices.Add(new Vector3(2, 1, -1)); // 12
        vertices.Add(new Vector3(-2, -1, -1)); // 13
        vertices.Add(new Vector3(2, -1, -1)); // 14
        vertices.Add(new Vector3(-2, -5, -1)); // 15
        vertices.Add(new Vector3(2, -5, -1)); // 16 

        vertices.Add(new Vector3(-3, 6, 1)); // 17
        vertices.Add(new Vector3(2, 6, 1)); // 18
        vertices.Add(new Vector3(3, 5, 1)); // 19
        vertices.Add(new Vector3(3, 1, 1)); // 20
        vertices.Add(new Vector3(2, 0, 1)); // 21
        vertices.Add(new Vector3(3, -1, 1)); // 22
        vertices.Add(new Vector3(3, -5, 1)); // 23
        vertices.Add(new Vector3(2, -6, 1)); // 24
        vertices.Add(new Vector3(-3, -6, 1)); // 25
        vertices.Add(new Vector3(-2, 5, 1)); // 26
        vertices.Add(new Vector3(2, 5, 1)); // 27
        vertices.Add(new Vector3(-2, 1, 1)); // 28
        vertices.Add(new Vector3(2, 1, 1)); // 29
        vertices.Add(new Vector3(-2, -1, 1)); // 30
        vertices.Add(new Vector3(2, -1, 1)); // 31
        vertices.Add(new Vector3(-2, -5, 1)); // 32
        vertices.Add(new Vector3(2, -5, 1)); // 33
    }

    public GameObject CreateUnityGameObject()
    {
        Mesh mesh = new Mesh();
        GameObject newGO = new GameObject();

        MeshFilter mesh_filter = newGO.AddComponent<MeshFilter>();
        MeshRenderer mesh_renderer = newGO.AddComponent<MeshRenderer>();

        List<Vector3> coords = new List<Vector3>();
        List<int> dummy_indices = new List<int>();
        /*List<Vector2> text_coords = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();*/

        for (int i = 0; i < faces.Count; i++)
        {
            //Vector3 normal_for_face = normals[i];

            //normal_for_face = new Vector3(normal_for_face.x, normal_for_face.y, -normal_for_face.z);

            coords.Add(vertices[faces[i].x]); dummy_indices.Add(i * 3); //text_coords.Add(texture_coordinates[texture_index_list[i].x]); normalz.Add(normal_for_face);

            coords.Add(vertices[faces[i].y]); dummy_indices.Add(i * 3 + 2); //text_coords.Add(texture_coordinates[texture_index_list[i].y]); normalz.Add(normal_for_face);

            coords.Add(vertices[faces[i].z]); dummy_indices.Add(i * 3 + 1); //text_coords.Add(texture_coordinates[texture_index_list[i].z]); normalz.Add(normal_for_face);
        }

        mesh.vertices = coords.ToArray();
        mesh.triangles = dummy_indices.ToArray();
        /*mesh.uv = text_coords.ToArray();
        mesh.normals = normalz.ToArray();*/
        mesh_filter.mesh = mesh;

        return newGO;
    }

}
