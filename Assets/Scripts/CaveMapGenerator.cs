using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveMapGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap(){
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 4; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if ( x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x,y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x,y] = 1;
                }
            }
        }

        CaveMeshGenerator meshGenerator = GetComponent<CaveMeshGenerator>();
        meshGenerator.GenerateMesh(borderedMap, 1);

    }

    void ProcessMap(){
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize) // "Se a quantidade de tiles parede agrupadas for menor do que o threshold"
            {
                // Pinta todas as paredes como buracos
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX,tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize) // "Se a quantidade de tiles parede agrupadas for menor do que o threshold"
            {
                // Pinta todas as paredes como buracos
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX,tile.tileY] = 1;
                }
            }
            else // No caso de não irmos remover os quartinhos, vamos guardar ele na lista de quartos vivos
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true; // O maior quarto de todos eh o principal
        survivingRooms[0].isAccessibleFromMainRoom = true; // O quarto principal eh acessivel pelo quarto principal

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms (List<Room> allRooms, bool forceAccessibilityFromMainRoom = false){

        List<Room> roomListA = new List<Room>(); // Lista dos não acessiveis main room
        List<Room> roomListB = new List<Room>(); // Lista dos acessiveis main room

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                } else
                {
                    roomListA.Add(room);
                }
            }
        } else // Nada acontece feijoada, isso eh pra caso seja a primeira iteracao da funcao
        {
            roomListA = allRooms;
            roomListB = allRooms; 
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA){
            if (!forceAccessibilityFromMainRoom) // Garantindo que a comparacao seja feita apenas entre bolinhas que ainda nao estao conectadas
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            
            foreach (Room roomB in roomListB){
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }
                // Vai comparar cada pontinho das bordas de cada "quarto", e a menor distância encontrada vai ser conectada
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++){
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++){
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            possibleConnectionFound = true;
                            bestDistance = distanceBetweenRooms;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true); // Após rodar a conexão pela primeira vez, roda de novo para conectar certinho
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB){
        Room.ConnectRooms(roomA, roomB); // Por isso que o método é estático!

        //Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.blue, 100);
        List<Coord> line = GetLine(tileA, tileB);

        foreach (Coord c in line) // Para cada coordenada encontrada pela linha
        {
            DrawCircle(c, 2);
        }
    }

    void DrawCircle(Coord coordinate, int radius){
        for (int x = 0; x <= radius; x++)
        {
            for (int y = 0; y <= radius; y++)
            {
                if (x*x + y*y <= radius*radius)
                {
                    int drawX = coordinate.tileX + x;
                    int drawY = coordinate.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to){
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX; // Variação de x
        int dy = to.tileY - from.tileY; // Variação de y

        bool inverted = false;
        int step = Math.Sign(dx); // A funcao sign etorna apenas 1, 0 ou -1 (é uma forma de saber se o numero eh positivo ou negativo)
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest) { // Se a linha for decrescente, inverta tudo
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest/2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x,y));

            if (inverted) {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation+=shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile){
        return new Vector3 (-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    void RandomFillMap(){
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); // Pseudo random number generator
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) // Verificação pra preencher as bordas
                {
                    map [x, y] = 1;
                } else{
                    map[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap(){
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4){
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY){
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else{
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord (int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    List<Coord> GetRegionTiles(int startX, int startY){ // Parâmetro pega posição aleatória no mapa e preenche baseado no "pixel" escolhido (balde de tinta)
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY]; // O pixel aleatório escolhido foi chão ou parede?

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1; // "ja olhei"

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue(); 
            tiles.Add(tile);

            for (int x = tile.tileX -1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY -1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)) // GARANTINDO QUE NÃO FORMAM DIAGONAIS
                    {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType) // garantindo que não olhei pra o tile ainda, e que ele faz parte do mesmo grupo de coisas que quero pintar
                        {
                            mapFlags[x,y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    List<List<Coord>> GetRegions(int tileType){ // Pega todas as regiões existentes de balde de tinta e coloca numa lista de regioes
        List<List<Coord>> regions = new List<List<Coord>>();
        int [,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x,y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    bool IsInMapRange(int x, int y){
        return x >= 0 && x < width && y >=0 && y < height;
    }

    class Room : IComparable<Room> { // Ao aplicar essa interface, agora os quartos são ordenáveis!!
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room(){}
        public Room(List<Coord> roomTiles, int[,] map){
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles){
                for (int x = tile.tileX - 1; x < tile.tileX + 1; x++){ // Loop que pega o tile antes do tile, e o tile depois do tile
                    for (int y = tile.tileY - 1; y < tile.tileY + 1; y++)
                    {
                        if (y == tile.tileY || x == tile.tileX) // Garantindo que não tá pegando diagonais
                        {
                            if (map[x,y] == 1) // Se encontrar uma parede
                            {
                                edgeTiles.Add(tile); // Significa que esse tile é uma borda
                            }
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB){
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            } else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom){
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom) {
            return otherRoom.roomSize.CompareTo(roomSize); // Comparando e ordenando com base nessa comparação
        }

        public void SetAccessibleFromMainRoom(){
            if(!isAccessibleFromMainRoom) {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }
    }
}
