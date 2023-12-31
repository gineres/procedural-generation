using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public GameObject portalPrefab;
    const float scale = .5f;
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float squareViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate*viewerMoveThresholdForChunkUpdate;


    public static float maxViewDistance; // static para poder mudar valores em runtime(?)
    public LODInfo[] detailLevels;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    int chunkSize;
    int chunkVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); // Dicionario para armazenar as posições vistas (para assim prevenir o caso de instanciar mais de uma vez)
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();

    static MapGenerator mapGenerator;
    
    void Start()
    {
        maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1; // -1 porque o numero la tinha 1 a mais
        //chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance/chunkSize);
        chunkVisibleInViewDistance = 1;
        mapGenerator = FindObjectOfType<MapGenerator> ();

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        // Apenas atualiza os chunks se o jogador andar por uma certa distancia
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > squareViewerMoveThresholdForChunkUpdate) // sqrMagnitude = square distance
        {
            UpdateVisibleChunks();
            viewerPositionOld = viewerPosition;
        }
        
    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
        {
            terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunkVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize); // O valor das coordenadas após essa divisão se tornam (0,0) (1,0) (2,0) (-1,1) ... (enquanto das dimensões reais seriam (0,0) (240,0) (480,0) ...)
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++) // loopando ao redor do ponto
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)){
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    /*if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunkVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }*/
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, portalPrefab));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject portalPrefab;
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LODInfo[] detailLevels;
        LODMesh[] lODMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, GameObject portal){
            this.detailLevels = detailLevels;
            position = coord*size;
            portalPrefab = portal;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y); // Posicao para o plano

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            //meshObject.layer = 6;
            //meshObject.tag = "Ground";
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            //meshObject.transform.localScale = Vector3.one * size / 10f; // Divisao por 10 porque planes tem o plane tem 10m
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = scale * Vector3.one;
            SetVisible(false);

            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData){
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
            //print("Map data received");
            //mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        /*void OnMeshDataReceived(MeshData meshData){ 
            meshFilter.mesh = meshData.CreateMesh();
        }*/

        public void UpdateTerrainChunk(){
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        } else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lODMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                            HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
                            float minDistanceBetweenObjects = 20.0f;

                            for (int x = 0; x < mapData.heightMap.GetLength(0); x++)
                            {
                                for (int y = 0; y < mapData.heightMap.GetLength(1); y++)
                                {
                                    if (mapData.heightMap[x,y] < 0.01f)
                                    {
                                        float offsetX = (x - mapData.heightMap.GetLength(0) / 2f) * scale;
                                        float offsetY = (y - mapData.heightMap.GetLength(1) / 2f) * scale;
                                        
                                        Vector3 spawnPosition = new Vector3(offsetX, 0, offsetY) + meshObject.transform.position;

                                        // Checa ao redor pra evitar acumulo de spawns
                                        bool positionOccupied = false;

                                        foreach (Vector3 pos in occupiedPositions)
                                        {
                                            if (Vector3.Distance(pos, spawnPosition) < minDistanceBetweenObjects)
                                            {
                                                positionOccupied = true;
                                                break;
                                            }
                                        }

                                        if (!positionOccupied)
                                        {
                                            GameObject portalGameObject = Instantiate(portalPrefab, spawnPosition, Quaternion.identity, meshObject.transform);
                                            portalGameObject.tag = "Portal";
                                            occupiedPositions.Add(spawnPosition);
                                        }
                                    }
                                }
                            }

                        } else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunkVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }

        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback){
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData){
            mesh = meshData.CreateMesh();
            hasMesh = true;

        
            updateCallback(); // Vai chamar a funcao UpdateTerrainChunk assim que conseguir o mesh data!!!
        }

        public void RequestMesh(MapData mapData){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
