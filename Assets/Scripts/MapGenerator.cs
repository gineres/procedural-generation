using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;
    public float persistence;
    public float lacunarity;

    public bool autoUpdate;

    public int seed;
    public Vector2 offset;

    public void GenerateMap(){
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth,mapHeight,seed,noiseScale, octaves, persistence, lacunarity,offset);

        MapDisplay display = FindObjectOfType<MapDisplay> ();
        display.DrawNoiseMap(noiseMap);
    }
}
