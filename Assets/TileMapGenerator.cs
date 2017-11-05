using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("http://github.com/tjhillard")]
public class TileMapGenerator : MonoBehaviour
{

    Transform mapContainer;

    [Header("General")]
    [Tooltip("Length in Unity Units.")]
    public Vector2 mapSize;
    [Tooltip("The position in worldspace the map will be centered at.")]
    public Vector3 mapPositionOffset;
    [Tooltip("The areas in which you want to prevent objects from being spawned at.")]
    public ClearedArea[] clearedAreas;
    [Header("Tile")]
    [Tooltip("Prefab GameObject to use as tile.")]
    public Transform tilePrefab;
    [Range(0, 1)]
    [Tooltip("The visibility of individual tile padding.")]
    public float padding;

    [Header("Terrain Objects (Trees, Rocks, Foliage, etc).")]
    public TerrainObjectInfo[] terrainObjects;


    public void GenerateMap()
    {
        mapContainer = CreateGeneratedMapContainer("Generated Map");

        Transform groundContainer = new GameObject("Ground").transform;
        groundContainer.parent = transform.Find(MapContainerName());

        // Generate Tiles
        for (int x = 0; x < mapSize.x; x += (int)tilePrefab.localScale.x)
        {
            for (int y = 0; y < mapSize.y; y += (int)tilePrefab.localScale.z)
            {
                SpawnTile(x, y, groundContainer);
            }
        }

        // Create GameObject containers for all Terrain object
        CreateTerrainObjectContainer();

        // Generate Terrain Objects
        for (int x = 0; x < mapSize.x; x += (int)tilePrefab.localScale.x)
        {
            for (int y = 0; y < mapSize.y; y += (int)tilePrefab.localScale.z)
            {
                bool objectPlacedOnThisTile = false;
                foreach (TerrainObjectInfo obj in terrainObjects)
                {
                    int rand = Random.Range(0, 1000);
                    if (rand < obj.chance && !objectPlacedOnThisTile)
                    {
                        SpawnTerrainObject(obj, x, y);
                        objectPlacedOnThisTile = true;
                    }
                }
            }
        }

        // Remove specified objects from clear areas
        foreach (ClearedArea area in clearedAreas)
        {
            ClearArea(area);
        }
    }

    private void SpawnTile(float x, float y, Transform container)
    {
        Vector3 tilePosition = new Vector3(-mapSize.x / 2 + x, mapContainer.position.y, -mapSize.y / 2 + y);
        Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as Transform;
        newTile.localScale = tilePrefab.localScale * (1 - padding);
        newTile.parent = container;
    }

    private void CreateTerrainObjectContainer()
    {
        foreach (TerrainObjectInfo obj in terrainObjects)
        {
            Transform objContainer = new GameObject(obj.name).transform;
            objContainer.parent = transform.Find(MapContainerName());
        }
    }

    private void SpawnTerrainObject(TerrainObjectInfo terrainObject, float x, float y)
    {
        Vector3 objPosition = new Vector3(-mapSize.x / 2 + x, terrainObject.heightOffset, -mapSize.y / 2 + y);
        Transform newObj = Instantiate(terrainObject.prefabs[Random.Range(0, terrainObject.prefabs.Length)], objPosition, Quaternion.identity) as Transform;
        newObj.parent = mapContainer.transform.Find(terrainObject.name);
        if (terrainObject.randomRotation)
        {
            newObj.eulerAngles = new Vector3(Quaternion.identity.x, Random.Range(0, 360), Quaternion.identity.z);
        }
    }

    private void ClearArea(ClearedArea area)
    {
        Collider[] colliders;
        if ((colliders = Physics.OverlapSphere(area.position, area.clearRadius)).Length > 1)
        {
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    if (area.objectTags.Contains(collider.gameObject.tag))
                    {
                        DestroyImmediate(collider.gameObject);
                    }
                }
            }
        }
    }

    private Transform CreateGeneratedMapContainer(string name)
    {
        if (transform.Find(name))
        {
            DestroyImmediate(transform.Find(name).gameObject);
        }

        Transform mapContainer = new GameObject(name).transform;
        Vector3 mapContainerPos = mapContainer.position;
        mapContainerPos.y = mapPositionOffset.y;
        mapContainer.parent = transform;

        return mapContainer;
    }

    private string MapContainerName()
    {
        return mapContainer.name;
    }

    [System.Serializable]
    public struct ClearedArea
    {
        public string name;
        [Tooltip("WorldSpace position of the area you want cleared.")]
        public Vector3 position;
        [Tooltip("Radius in Unity units for the cleared area from the position provided above.")]
        public float clearRadius;
        [Tooltip("The GameObject tags to clear.")]
        public List<string> objectTags;
    }

    [System.Serializable]
    public struct TerrainObjectInfo
    {
        [Tooltip("Must be a unique name from other GameObjects in your scene")]
        public string name;
        public Transform[] prefabs;
        [Tooltip("Weight value that determinies likliness of this TerrainObject to be spawned. The higher the value, the higher the chance.")]
        [Range(0, 1000)]
        public int chance;
        [Tooltip("Gives the object a random rotation along the Y-Axis if true.")]
        public bool randomRotation;
        [Tooltip("Y-Axis offset for the spawned TerrainObject.")]
        public float heightOffset;
    }
}


