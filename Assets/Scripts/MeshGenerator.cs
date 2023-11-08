using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static void GenerateTerrainMesh(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1)/2f;
        float topLeftZ = (height - 2)/2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x,y], topLeftZ - y);

                // Para ignorar os vertices da borda direita e borda abaixo, subtrai -1
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width); // Triangulo 1 do quadrado
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1); // Triangulo 2 do quadrado
                }


                vertexIndex++;
            }
        }
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6]; // Triangulos sao a combinacao de triangulos possiveis dado os pontinhos dos vertices
        // Esse calculo faz essa operacao para saber quantos triangulos sao formados dado um numero de vertices
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }
}
