using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Octaves: quantidade de gerações de perlin noise encima da geração
public class Noise
{
    // o perlin noise varia, mas valores inteiros sempre terão a mesma "tonalidade"
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistence, float lacunarity){
        float[,] noiseMap = new float[mapWidth,mapHeight];

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency;
                    float sampleY = y / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1; // O *2-1 faz com que o range da operação seja entre -1 e 1
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence; // Decreases
                    frequency *= lacunarity; // Increases by each lacunarity
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight; // Resultado da sobreposição de todas as octaves
            }
        }

        // Normalizando para os valores variarem de 0 a 1
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }
        return noiseMap;
    }
}
