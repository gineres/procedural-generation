using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail){
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);  // Criando um objeto próprio para cada thread ter sua própria animation curve
        // DO JEITO QUE TAVA ANTES, THREADS DIFERENTES ACESSAVAM A MESMA HEIGHTCURVE E CRIAVA RESULTADOS MALUCOS
        // UMA DAS SOLUÇÕES ERA DAR UM "LOCK" NA _HEIGHCURVE, POREM ISSO NÃO É OPTIMAL VISTO QUE CADA THREAD TERIA Q ESPERAR A OUTRA ACABAR PARA CONTINUAR O TRABALHO
        // (EXEMPLO DE LOCK COMENTADO)

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1)/-2f;
        float topLeftZ = (height - 1)/2f;

        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y+=meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+=meshSimplificationIncrement)
            {
                /*lock (_heightCurve)
                {
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, _heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                }*/
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

                // Para ignorar os vertices da borda direita e borda abaixo, subtrai -1
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine); // Triangulo 1 do quadrado
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1); // Triangulo 2 do quadrado
                } // O width and height antes era referenciado apenas para calcular a quantidade de vertices por linha, mas agora que temos o level of detail, essa quantidade de vertices foi recalculada para o verticesPerLine!

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    int triangleIndex;
    public Vector2[] uvs;

    public MeshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6]; // Triangulos sao a combinacao de triangulos possiveis dado os pontinhos dos vertices
        // Esse calculo faz essa operacao para saber quantos triangulos sao formados dado um numero de vertices
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
