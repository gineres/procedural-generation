using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, Mesh};
    
    public DrawMode drawMode;

    public const int mapChunkSize = 241; // As dimensoes da mesh de verdade eh 240 x 240, ou seja, -1
    [Range(0,6)]
    public int editorPreviewLOD;

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

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void RequestMapData(Vector2 center, Action<MapData> callback){
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start(); // Com isso, o metodo abaixo MapDataThread está rodando em outra thread
    }

    void MapDataThread(Vector2 center, Action<MapData> callback){
        MapData mapData = GenerateMapData(center); // Com isso, agora o GenerateMapData também rodará dentro dessa thread
        lock (mapDataThreadInfoQueue){ // Prendendo para evitar o acesso de outros lugares
            mapDataThreadInfoQueue.Enqueue( new MapThreadInfo<MapData> (callback, mapData) );
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback){
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue( new MapThreadInfo<MeshData> (callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public void DrawMapInEditor(){
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay> ();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    MapData GenerateMapData(Vector2 center){
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize,mapChunkSize,seed,noiseScale, octaves, persistence, lacunarity, center + offset);

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

    // Structs devem ser readonly, entao uma vez que eh chamado no codigo, nao muda mais os valores colocados pelo construtor
    struct MapThreadInfo<T> { // Generic para funcionar pra meshdata e mapdata 
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter){
            this.callback = callback;
            this.parameter = parameter;
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
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData (float[,] heightMap, Color[] colorMap){
        this.colorMap = colorMap;
        this.heightMap = heightMap;
    }
}