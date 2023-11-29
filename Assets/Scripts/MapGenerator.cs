using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, Mesh};
    
    public DrawMode drawMode;

    public const int mapChunkSize = 241; // As dimensoes da mesh de verdade eh 240 x 240, ou seja, -1
    [Range(0,6)]
    public int levelOfDetail;

    public float noiseScale;
    public int octaves;
    [Range(0,1)] public float persistence;
    public float lacunarity;

    public bool autoUpdate;

    public TerrainType[] regions;

    public int seed;
    public Vector2 offset;

    public AnimationCurve meshHeightCurve;
    public float meshHeightMultiplier;

    public void DrawMapInEditor(){
        MapData mapData = GenerateMapData();

        MapDisplay display = FindObjectOfType<MapDisplay> ();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    MapData GenerateMapData(){
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,seed,noiseScale, octaves, persistence, lacunarity,offset);

        Color[] colorMap = new Color[mapChunkSize*mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x,y];
                //Colorindo
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    void OnValidate() // chamado quando muda uma variave l no inspetor
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0){
            octaves = 0;
        }
    }
}

// Para aparecer no inspetor bonitinho
[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public float[,] heightMap;
    public Color[] colorMap;

    public MapData (float[,] heightMap, Color[] colorMap){
        this.colorMap = colorMap;
        this.heightMap = heightMap;
    }
}