using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Tilemap background;
    [SerializeField] Tilemap backgroundOverlay;
    [SerializeField] Tilemap player;
    [SerializeField] Tilemap foreground;
    [SerializeField] Tilemap foregroundOverlay;

    [SerializeField] int width = 150;
    [SerializeField] int height = 75;

    [SerializeField] int minimumRoomWidth = 17;
    [SerializeField] int minimumRoomHeight = 10;

    [SerializeField] int iterations = 6;
    [SerializeField] float reconnectThreshold = 0.2f;

    [SerializeField] List<DungeonRoomType> roomTypes;
    [SerializeField] DungeonRoomType backupRoomType;

    [SerializeField] Transform playerTransform;

    //[SerializeField] GameObject textOverlayPrefab;
    [SerializeField] GameObject torchPrefab;
    [SerializeField] Transform torchParent;
    [SerializeField] GameObject treasurePrefab;

    [SerializeField] List<ItemObject> loot;

    Dictionary<int, Room> generatedRooms;

    //GameObject debugOverlay;

    void Start()
    {
        //debugOverlay = Instantiate(textOverlayPrefab);

        var startTime = System.DateTimeOffset.UtcNow;
        //Random.InitState(0);

        CreateRoomsAndDoors();

        foreach (var room in generatedRooms.Values)
        {
            PopulateRoom(room);
        }

        SpawnPlayer();
        var elapsed = (System.DateTimeOffset.UtcNow - startTime).TotalSeconds;
        Debug.Log($"{elapsed:0.0} seconds elapsed for dungeon generation");
    }

    void Update()
    {
        var screenPosition = Mouse.current.position.ReadValue();

        //var p = Camera.main.ScreenToWorldPoint(screenPosition);
        //p.z = -1f;
        //debugOverlay.transform.position = p;
        //var tmp = debugOverlay.GetComponent<TextMeshPro>();
        //tmp.text = $"({(int)(p.x * 2f)},{(int)(p.y * 2f)})";
    }

    void SpawnPlayer()
    {
        var startingRoom = generatedRooms.Values.First(r => r.isStartingRoom);

        playerTransform.position = new Vector3((startingRoom.x + (startingRoom.width / 2f)) / 2f, (startingRoom.y + (startingRoom.height / 2f)) / 2f);
    }

    class RectInt
    {
        public RectInt(Vector2Int corner1, Vector2Int corner2)
        {
            BottomLeft = new Vector2Int(Mathf.Min(corner1.x, corner2.x), Mathf.Min(corner1.y, corner2.y));
            BottomRight = new Vector2Int(Mathf.Max(corner1.x, corner2.x), Mathf.Min(corner1.y, corner2.y));
            TopRight = new Vector2Int(Mathf.Max(corner1.x, corner2.x), Mathf.Max(corner1.y, corner2.y));
            TopLeft = new Vector2Int(Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner1.y, corner2.y));
        }

        public Vector2Int BottomLeft;
        public Vector2Int BottomRight;
        public Vector2Int TopLeft;
        public Vector2Int TopRight;

        public void Add(Vector2Int v)
        {
            BottomLeft += v;
            BottomRight += v;
            TopLeft += v;
            TopRight += v;
        }

        public void PushToBoundary(RectInt boundary)
        {
            if (BottomLeft.x < boundary.BottomLeft.x)
            {
                Add(new Vector2Int(boundary.BottomLeft.x - BottomLeft.x, 0));
            }
            if (BottomRight.x > boundary.BottomRight.x)
            {
                Add(new Vector2Int(boundary.BottomRight.x - BottomRight.x, 0));
            }
            if (BottomLeft.y < boundary.BottomLeft.y)
            {
                Add(new Vector2Int(0, boundary.BottomLeft.y - BottomLeft.y));
            }
            if (TopLeft.y > boundary.TopLeft.y)
            {
                Add(new Vector2Int(0, boundary.TopLeft.y - TopLeft.y));
            }
        }

        public void ForEach(System.Action<int, int> xyAction)
        {
            for (var y = BottomLeft.y; y <= TopLeft.y; y++)
            {
                for (var x = BottomLeft.x; x <= BottomRight.x; x++)
                {
                    xyAction.Invoke(x, y);
                }
            }
        }

        public int GetArea() => (BottomRight.x - BottomLeft.x) * (TopLeft.y - BottomLeft.y);
    }

    void PopulateRoom(Room room)
    {
        var lootTable = loot.Select(l => new ItemProbability
        {
            item = l,
            randomWeight = 5,
        }).ToList();

        var roomColor = Random.ColorHSV();

        var possibleRooms = roomTypes.Where(roomType => roomType.maxWidth >= room.width && roomType.maxHeight >= room.height
            && roomType.minWidth <= room.width && roomType.minHeight <= room.height && roomType.maxDoors >= room.doors.Count)
            .ToList();

        var type = room.selectedType = possibleRooms.Count <= 0 ? backupRoomType : possibleRooms.GetRandomByWeights(r => r.randomWeight);

        var open = new bool[room.height, room.width];
        var roomInteriorBoundary = new RectInt(new Vector2Int(1, 1), new Vector2Int(room.width - 2, room.height - 2));

        for (var y = 0; y < room.height; y++)
        {
            for (var x = 0; x < room.width; x++)
            {
                if (y == 0 || x == 0 || y == room.height - 1 || x == room.width - 1)
                {
                    continue;
                }

                if (room.isStartingRoom && Mathf.Abs((room.width / 2f) - x) <= 4 && Mathf.Abs((room.height / 2f) - y) <= 4)
                {
                    open[y, x] = true;
                    continue;
                }
            }
        }

        void carvePath(RectInt o)
        {
            var objMaxDim = Mathf.Max(o.BottomRight.x - o.BottomLeft.x, o.TopLeft.y - o.BottomLeft.y) + 1;

            var centerMinX = Mathf.Clamp((int)Mathf.Floor((room.width / 2f) - (objMaxDim / 2f)), 1, room.width - 2);
            var centerMaxX = Mathf.Clamp((int)Mathf.Ceil((room.width / 2f) + (objMaxDim / 2f)), 1, room.width - 2);
            var centerMinY = Mathf.Clamp((int)Mathf.Floor((room.height / 2f) - (objMaxDim / 2f)), 1, room.height - 2);
            var centerMaxY = Mathf.Clamp((int)Mathf.Ceil((room.height / 2f) + (objMaxDim / 2f)), 1, room.height - 2);

            for (var y = centerMinY; y <= centerMaxY; y++)
            {
                for (var x = centerMinX; x <= centerMaxX; x++)
                {
                    open[y, x] = true;
                }
            }

            o.ForEach((x, y) => open[y, x] = true);

            var newRect = new RectInt(o.BottomLeft, o.BottomLeft + new Vector2Int(Mathf.Min(objMaxDim, room.width - 2) - 1, Mathf.Min(objMaxDim, room.height - 2) - 1));
            newRect.PushToBoundary(roomInteriorBoundary);
            newRect.ForEach((x, y) => open[y, x] = true);
            while (!(newRect.BottomLeft.x >= centerMinX && newRect.BottomRight.x <= centerMaxX))
            {
                newRect.Add(new Vector2Int(o.BottomLeft.x < room.width / 2f ? 1 : -1, 0));
                newRect.ForEach((x, y) => open[y, x] = true);
            }

            while (!(newRect.BottomLeft.y >= centerMinY && newRect.TopLeft.y <= centerMaxY))
            {
                newRect.Add(new Vector2Int(0, o.BottomLeft.y < room.height / 2f ? 1 : -1));
                newRect.ForEach((x, y) => open[y, x] = true);
            }
        }

        var roomPos = new Vector2Int(room.x, room.y);
        foreach (var door in room.doors)
        {
            carvePath(new RectInt(door.Start - roomPos, door.End - roomPos));
        }

        var treasureLocations = new List<Vector2Int>();
        if (!room.isStartingRoom && room.width >= 5 && room.height >= 5)
        {
            var treasureAttempts = room.width * room.height / 10;
            for (var i = 0; i < treasureAttempts && treasureLocations.Count < 3; i++)
            {
                var pos = new Vector2Int(Random.Range(2, room.width - 2), Random.Range(2, room.height - 2));

                if (open[pos.y - 2, pos.x] || open[pos.y - 2, pos.x - 1] || open[pos.y - 2, pos.x + 1])
                {
                    continue;
                }

                var nearby = false;
                foreach (var existing in treasureLocations)
                {
                    if (Vector2Int.Distance(pos, existing) <= type.minTreasureDistanceTiles)
                    {
                        nearby = true;
                    }
                }

                if (nearby)
                {
                    continue;
                }

                treasureLocations.Add(pos);
                carvePath(new RectInt(pos + new Vector2Int(-1, -1), pos + new Vector2Int(1, 1)));

                var treasureType = type.treasureTypes.GetRandomByWeights(t => t.randomWeight);

                var inst = Instantiate(treasurePrefab, torchParent);
                var treasure = inst.GetComponent<Treasure>();
                treasure.Type = treasureType.treasureType;
                treasure.minItems = 1;
                treasure.maxItems = 3;
                treasure.items = lootTable;
                treasure.transform.position = new Vector3(((room.x + pos.x) / 2f) + .25f, ((room.y + pos.y) / 2f) + .05f);
                treasure.name = "T: " + pos.ToString() + "; R: " + room.id;
                treasure.playerInventory = playerTransform.gameObject.GetComponent<ItemInventory>();
            }
        }

        for (var y = 0; y < room.height; y++)
        {
            for (var x = 0; x < room.width; x++)
            {
                var noise = Mathf.PerlinNoise(x * .75f, y * .75f);
                var pos = new Vector3Int(x + room.x, y + room.y, 1);

                var isOpen = open[y, x];

                if (!isOpen)
                {
                    var tile = type.borders.GetRandomByWeights(b => b.randomWeight);
                    var rescaledNoise = (tile.darknessVariation * noise) + (1f - tile.darknessVariation);
                    var newColor = new Color(tile.color.r * rescaledNoise, tile.color.g * rescaledNoise, tile.color.b * rescaledNoise, 1);
                    foreground.SetTile(new TileChangeData(pos, tile.tile, newColor, Matrix4x4.identity), true);

                    if (Random.value <= tile.overlayChance)
                    {
                        var overlayTile = tile.overlays[Random.Range(0, tile.overlays.Count)];
                        foregroundOverlay.SetTile(new TileChangeData(pos, overlayTile, newColor, Matrix4x4.identity), true);
                    }
                }

                var bgTile = type.backgrounds.GetRandomByWeights(b => b.randomWeight, type.usePerlinBackground ? Mathf.PerlinNoise((x + room.x) * 0.2f, (y + room.y) * 0.2f) : null);
                var bgRescaledNoise = (bgTile.darknessVariation * noise) + (1f - bgTile.darknessVariation);
                var bgNewColor = new Color(bgTile.color.r * bgRescaledNoise, bgTile.color.g * bgRescaledNoise, bgTile.color.b * bgRescaledNoise, 1);
                background.SetTile(new TileChangeData(pos, bgTile.tile, bgNewColor, Matrix4x4.identity), true);

                if (Random.value <= bgTile.overlayChance)
                {
                    var overlayTile = bgTile.overlays[Random.Range(0, bgTile.overlays.Count)];
                    backgroundOverlay.SetTile(new TileChangeData(pos, overlayTile, bgNewColor, Matrix4x4.identity), true);
                }
            }
        }

        var torchPositions = new List<Vector2Int>();
        for (var i = 0; i < room.width * room.height / type.torchDensity; i++)
        {
            var ran = new Vector2Int(Random.Range(1, room.width - 2), Random.Range(1, room.height - 2));
            if (!open[ran.y, ran.x])
            {
                continue;
            }

            var nearby = false;
            foreach (var existing in torchPositions)
            {
                if (Vector2Int.Distance(ran, existing) <= type.minTorchDistanceTiles)
                {
                    nearby = true;
                }
            }

            if (!nearby)
            {
                torchPositions.Add(ran);
            }
        }

        foreach (var pos in torchPositions)
        {
            var light = type.lighting.GetRandomByWeights(l => l.randomWeight);

            var torch = Instantiate(torchPrefab, torchParent);
            torch.transform.position = new Vector3(((room.x + pos.x) / 2f) + .25f, ((room.y + pos.y) / 2f) + .25f, 1);
            torch.GetComponent<DistanceToggle>().Tracking = playerTransform;
            torch.GetComponent<Light2D>().color = light.mainColor;
            foreach (Transform child in torch.transform)
            {
                if (child.gameObject.name == "Undertone")
                {
                    child.gameObject.GetComponent<Light2D>().color = light.undertoneColor;
                }
            }
        }

        //var textOverlay = Instantiate(textOverlayPrefab, torchParent);
        //textOverlay.transform.position = new Vector3((room.x + (room.width / 2f)) / 2f, (room.y + (room.height / 2f)) / 2f);
        //textOverlay.name = "Text overlay; R: " + room.id +  "; T: " + room.selectedType.name;
        //var tmp = textOverlay.GetComponent<TextMeshPro>();
        //tmp.text = $"Room: {room.id}\nAt: {room.x},{room.y}\nType: {room.selectedType.name}\nDoors:\n  {string.Join("\n  ", room.doors.Select(d => $"{d.ConnectedToRoomId}: {d.Start} - {d.End}"))}\n" +
        //    $"Treasure:\n  {string.Join("\n  ", treasureLocations.Select(t => t.ToSafeString()))}";

        //var lr = textOverlay.GetComponent<LineRenderer>();
        //lr.widthMultiplier = 0.1f;
        //lr.startColor = Color.orange;
        //lr.endColor = Color.orange;
        //lr.positionCount = 5;
        //lr.SetPositions(new Vector3[]
        //{
        //    new(room.x, room.y),
        //    new(room.x + room.width, room.y),
        //    new(room.x + room.width, room.y + room.height),
        //    new(room.x, room.y + room.height),
        //    new(room.x, room.y),
        //}.Select(x => x / 2f).ToArray());
    }

    void CreateRoomsAndDoors()
    {
        var rooms = new List<Room>()
        {
            new()
            {
                x = 0,
                y = 0,
                width = width,
                height = height,
            }
        };

        for (var i = 0; i < iterations; i++)
        {
            rooms = SplitRooms(rooms, minimumRoomWidth, minimumRoomHeight);
        }


        for (var i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];
            var distance = new Vector2((width / 2f) - (r.x + (r.width / 2f)), (height / 2f) - (r.y + (r.height / 2f))).magnitude;
            if (Random.value < distance / (Mathf.Sqrt(width * height) / 2f))
            {
                rooms.RemoveAt(i);
                i--;
            }
        }

        generatedRooms = rooms.ToDictionary(r => r.id, r => r);

        var minConnOverlap = 4;

        foreach (var r in generatedRooms.Values)
        {
            foreach (var r2 in generatedRooms.Values)
            {
                if (r == r2)
                { continue; }
                if (r2.doors.Any(d => d.ConnectedToRoomId == r.id))
                {
                    continue;
                }

                Door doorR = null;
                Door doorR2 = null;

                var maxDoorSize = 4;

                if (r.x == r2.x + r2.width || r.x + r.width == r2.x)
                {
                    if (r.y >= r2.y - r.height + minConnOverlap && r.y <= r2.y + r2.height - minConnOverlap)
                    {
                        var doorRX = r.x == r2.x + r2.width ? r.x : r.x + r.width - 1;

                        var doorY1 = Mathf.Max(r.y, r2.y) + 1;
                        var doorY2 = Mathf.Min(r.y + r.height - 1, r2.y + r2.height - 1) - 1;
                        var doorYSelected = Random.Range(doorY1, doorY2);
                        var size = Random.Range(2, Mathf.Min(maxDoorSize + 1, doorY2 - doorYSelected + 1));

                        doorR = new()
                        {
                            Start = new Vector2Int(doorRX, doorYSelected),
                            End = new Vector2Int(doorRX, doorYSelected + size - 1),
                            ConnectedToRoomId = r2.id,
                            IsHorizontal = false,
                        };

                        var doorR2X = r.x == r2.x + r2.width ? r2.x + r2.width - 1 : r2.x;
                        doorR2 = new()
                        {
                            Start = new Vector2Int(doorR2X, doorYSelected),
                            End = new Vector2Int(doorR2X, doorYSelected + size - 1),
                            ConnectedToRoomId = r.id,
                            IsHorizontal = false,
                        };
                    }
                }

                if (r.y == r2.y + r2.height || r.y + r.height == r2.y)
                {
                    if (r.x >= r2.x - r.width + minConnOverlap && r.x <= r2.x + r2.width - minConnOverlap)
                    {
                        var doorRY = r.y == r2.y + r2.height ? r.y : r.y + r.height - 1;

                        var doorX1 = Mathf.Max(r.x, r2.x) + 1;
                        var doorX2 = Mathf.Min(r.x + r.width - 1, r2.x + r2.width - 1) - 1;
                        var doorXSelected = Random.Range(doorX1, doorX2);
                        var size = Random.Range(2, Mathf.Min(maxDoorSize + 1, doorX2 - doorXSelected + 1));

                        doorR = new()
                        {
                            Start = new Vector2Int(doorXSelected, doorRY),
                            End = new Vector2Int(doorXSelected + size - 1, doorRY),
                            ConnectedToRoomId = r2.id,
                            IsHorizontal = true,
                        };

                        var doorR2Y = r.y == r2.y + r2.height ? r2.y + r2.height - 1 : r2.y;
                        doorR2 = new()
                        {
                            Start = new Vector2Int(doorXSelected, doorR2Y),
                            End = new Vector2Int(doorXSelected + size - 1, doorR2Y),
                            ConnectedToRoomId = r.id,
                            IsHorizontal = true,
                        };
                    }
                }

                if (doorR != null)
                {
                    r.doors.Add(doorR);
                    r2.doors.Add(doorR2);
                }
            }
        }

        var edges = new List<Edge>();
        foreach (var r in generatedRooms.Values)
        {
            foreach (var r2 in r.doors)
            {
                var lower = r2.ConnectedToRoomId < r.id ? r2.ConnectedToRoomId : r.id;
                var higher = r2.ConnectedToRoomId < r.id ? r.id : r2.ConnectedToRoomId;

                if (edges.Any(e => e.A == lower && e.B == higher))
                {
                    continue;
                }

                edges.Add(new()
                {
                    A = lower,
                    B = higher,
                });
            }
        }

        var result = GraphUtils.BuildConnections(edges);
        foreach (var e in edges)
        {
            if (!result.Any(e2 => (e.A == e2.A && e.B == e2.B) || (e.A == e2.B && e.B == e2.A)))
            {
                if (Random.value < reconnectThreshold)
                {
                    result.Add(e);
                }
            }
        }

        var startingRoom = generatedRooms.Values.OrderBy(r => new Vector2((width / 2f) - (r.x + r.width / 2f), (height / 2f) - (r.y + r.height / 2f)).magnitude).First();
        startingRoom.isStartingRoom = true;

        result = GraphUtils.FilterConnectedComponent(result, startingRoom.id);
        var accessibleRooms = result.Select(r => r.A).Union(result.Select(r => r.B)).Distinct().ToList();

        var toRemove = new List<int>();
        foreach (var r in generatedRooms.Values)
        {
            if (!accessibleRooms.Contains(r.id))
            {
                toRemove.Add(r.id);
                continue;
            }

            var toRemoveDoors = new List<Door>();
            foreach (var door in r.doors)
            {
                if (!result.Any(c => (c.A == r.id && c.B == door.ConnectedToRoomId) || (c.B == r.id && c.A == door.ConnectedToRoomId)))
                {
                    toRemoveDoors.Add(door);
                }
            }
            toRemoveDoors.ForEach(d => r.doors.Remove(d));
        }
        toRemove.ForEach(t => generatedRooms.Remove(t));
    }

    static List<Room> SplitRooms(List<Room> rooms, int minRoomWidth, int minRoomHeight)
    {
        var toReturn = new List<Room>();

        for (var i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];

            var isHorizontal = minRoomWidth * 2 > r.width ? false : minRoomHeight * 2 > r.height ? true : r.width > r.height;
            var maxSize = isHorizontal ? r.width : r.height;
            var minSize = isHorizontal ? minRoomWidth : minRoomHeight;

            if (maxSize <= minSize * 2)
            {
                toReturn.Add(r);
                continue;
            }

            var size = Random.Range(minSize, maxSize - minSize + 1);

            var r1 = new Room
            {
                x = r.x,
                y = r.y,
                width = isHorizontal ? size : r.width,
                height = isHorizontal ? r.height : size,
            };
            toReturn.Add(r1);

            var r2 = new Room
            {
                x = isHorizontal ? r.x + size : r.x,
                y = isHorizontal ? r.y : r.y + size,
                width = isHorizontal ? r.width - size : r.width,
                height = isHorizontal ? r.height : r.height - size,
            };
            toReturn.Add(r2);
        }

        return toReturn;
    }
}

class Room
{
    private static int nextId = 0;

    public int id;

    public Room()
    {
        id = nextId++;
    }

    public int x;
    public int y;
    public int width;
    public int height;
    public bool isStartingRoom;

    public DungeonRoomType selectedType;
    public List<Door> doors = new();
}

class Door
{
    public Vector2Int Start;
    public Vector2Int End;
    public int ConnectedToRoomId;
    public bool IsHorizontal;
    public bool IsVertical => !IsHorizontal;
}
