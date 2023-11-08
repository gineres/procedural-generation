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

    void UpdateVisibleChunks(){
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize); // O valor das coordenadas após essa divisão se tornam (0,0) (1,0) (2,0) (-1,1) ... (enquanto das dimensões reais seriam (0,0) (240,0) (480,0) ...)
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDistance; yOffset < chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDistance; xOffset < chunkVisibleInViewDistance; xOffset++) // loopando ao redor do ponto
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)){
                    //
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk());
                }
            }
        }
    }

    public class TerrainChunk {

    }
}
