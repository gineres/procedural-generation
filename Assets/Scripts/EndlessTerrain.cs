using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float maxViewDistance = 300;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunkVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); // Dicionario para armazenar as posições vistas (para assim prevenir o caso de instanciar mais de uma vez)

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1; // -1 porque o numero la tinha 1 a mais
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance/chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks(){
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize); // O valor das coordenadas após essa divisão se tornam (0,0) (1,0) (2,0) (-1,1) ... (enquanto das dimensões reais seriam (0,0) (240,0) (480,0) ...)
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDistance; yOffset < chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDistance; xOffset < chunkVisibleInViewDistance; xOffset++) // loopando ao redor do ponto
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)){
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size){
            position = coord*size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y); // Posicao para o plano

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f; // Divisao por 10 porque planes tem o plane tem 10m
            SetVisible(false);
        }

        public void UpdateTerrainChunk(){
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }
    }
}
