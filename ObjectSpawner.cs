using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectSpawner : Singletroon<ObjectSpawner>
{
    #region ROCKS_VARIABLES
    [Header("Rocks")]
    public Material[] stoneMaterials;
    public GameObject rockPrefab;
    private const float RockDelayMinOffset = -0.5f;
    private const float RockDelayMaxOffset = 0.2f;
    [SerializeField] private float averageDelayForRock = 1.0f;
    private const float minDelayForRock = 0.45f;
    private const float tightRockMinOffset = -0.1f;
    private const float tightRockMaxOffset = 0.05f;
    public float NextRockHorizontalPos = 0f;
    #endregion

    #region GEMS_VARIABLES
    [Header("Gems")]
    [SerializeField] private GameObject[] gemPrefabs;
    private const float GemDelayMinOffset = -5f;
    private const float GemDelayMaxOffset = 2f;
    [SerializeField] private float averageDelayForGem = 6.0f;
    private const float minDelayForGem = 2.5f;
    private const float tightGemMinOffset = -1.0f;
    private const float tightGemMaxOffset = 0.5f;
    #endregion
    
    #region ITEMS_VARIABLES
    [Header("Items")]
    
    [SerializeField] public GameObject[] itemPrefabs;
    [SerializeField] private GameObject[] helmetPrefabs;
    [SerializeField] private GameObject[] bugsPrefabs;
    private const float ItemDelayMinOffset = -12f;
    private const float ItemDelayMaxOffset = 30f;
    [SerializeField] private float averageDelayForItem = 20.0f;
    private const float bugSpawnInterval = 1.2f;
    public Dictionary<Helmet, GameObject> helmetsByType;
    #endregion


    private void Awake()
    {
        helmetsByType = new Dictionary<Helmet, GameObject>()
        {
            { Helmet.Tinfoil, helmetPrefabs[0] },
            { Helmet.Serious, helmetPrefabs[1] },
            { Helmet.HardHat, helmetPrefabs[2] },
            { Helmet.Miner, helmetPrefabs[3] },
            { Helmet.PickelHaube, helmetPrefabs[4] }
        };
    }

    private void Start()
    {
        Invoke("StartSpawning", 1.5f);
    }

    public void StartSpawning()
    {
        ChooseNextRockHorizontalPos();
        StartCoroutine(SpawnObjects());
    }

    private void ChooseNextRockHorizontalPos()
    {
        NextRockHorizontalPos = Random.Range(-2.2f, 2.2f);
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnObjects()
    {
        SoyGameController game = SoyGameController.Instance;
        SoyTimeController time = SoyTimeController.Instance;
        Func<bool> isPaused = () => game.IsPaused;

        float timeUntilNextRock = averageDelayForRock;
        float timeUntilNextGem = averageDelayForGem;
        float timeUntilNextItem = averageDelayForItem;
        float timeUntilNextBug = 1.2f;
        float elapsedTime = 0f;

        while (true)
        {
            while (isPaused())
                yield return null;

            if (game.bossBattle)
                yield break;

            float t = Mathf.Clamp01(elapsedTime / game.durationForMaxSpeed); // ramping factor

            float currentAverageRockDelay = Mathf.Lerp(averageDelayForRock, minDelayForRock, t);
            float currentRockMinOffset = Mathf.Lerp(RockDelayMinOffset, tightRockMinOffset, t);
            float currentRockMaxOffset = Mathf.Lerp(RockDelayMaxOffset, tightRockMaxOffset, t);

            float currentAverageGemDelay = Mathf.Lerp(averageDelayForGem, minDelayForGem, t);
            float currentGemMinOffset = Mathf.Lerp(GemDelayMinOffset, tightGemMinOffset, t);
            float currentGemMaxOffset = Mathf.Lerp(GemDelayMaxOffset, tightGemMaxOffset, t);

            if (timeUntilNextRock <= 0f)
            {
                transform.position = new Vector3(NextRockHorizontalPos, transform.position.y, transform.position.z);
                GameObject rockOrGem = game.HasActiveItem(Item.Meds) ? SpawnGem() : SpawnStone();
                ApplyRandomSpin(rockOrGem.GetComponent<Rigidbody2D>());
                ChooseNextRockHorizontalPos();

                timeUntilNextRock = GetTriangularDelay(currentAverageRockDelay, currentRockMinOffset, currentRockMaxOffset);
            }

            TrySpawn(ref timeUntilNextGem, GetTriangularDelay(currentAverageGemDelay, currentGemMinOffset, currentGemMaxOffset), SpawnGem);
            TrySpawn(ref timeUntilNextItem, GetTriangularDelay(averageDelayForItem, ItemDelayMinOffset, ItemDelayMaxOffset), SpawnItem);
            TrySpawn(ref timeUntilNextBug, bugSpawnInterval, SpawnBug);

            //wait twice as long to spawn stuff if sproke is active
            float timeStep = Time.deltaTime * (time.CurrentMultiplier == 1f ? 1f : 0.5f);

            timeUntilNextRock -= timeStep;
            timeUntilNextGem -= game.HasActiveItem(Item.SoyMilk) ? timeStep * 5f : timeStep; //spawn gems more often if soymilk is active
            timeUntilNextItem -= timeStep;
            if (game.HasActiveItem(Item.Kebab))
                timeUntilNextBug -= timeStep;

            elapsedTime += timeStep;
            yield return null;
        }
    }

    
    private void TrySpawn(ref float timer, float delay, Func<GameObject> spawnFunc)
    {
        if (timer <= 0f)
        {
            MovetoNewHorizontalPos();
            ApplyRandomSpin(spawnFunc().GetComponent<Rigidbody2D>());
            timer = delay;
        }
    }

    private float GetTriangularDelay(float average, float minOffset, float maxOffset)
    {
        return ToweraldStatic.TriangularDistribution(
            average + minOffset,
            average,
            average + maxOffset
        );
    }

    private void MovetoNewHorizontalPos()
    {
        transform.position = new Vector3(Random.Range(-2.2f, 2.2f), transform.position.y, transform.position.z);
    }

    private GameObject SpawnStone()
    {
        GameObject stone = Instantiate(rockPrefab, transform.position, Quaternion.identity);
        float radius = SoyGameController.Instance.HasActiveItem(Item.Kebab) ? Random.Range(0.3f, 0.5f) : Random.Range(0.4f, 0.7f);
        stone.GetComponent<PolygonMeshGenerator>().Generate(Random.Range(6, 12), radius, Random.Range(0.01f, 0.55f));
        Material rockMat = stoneMaterials[Random.Range(0, stoneMaterials.Length)];
        Material rockMatDark = new Material(rockMat);
        rockMatDark.color *= 0.7f;
        stone.GetComponent<MeshRenderer>().material = rockMat;
        stone.transform.GetChild(0).GetComponent<MeshRenderer>().material = rockMatDark;
        Outline outline = stone.AddComponent<Outline>();
        outline.OutlineWidth = 4f;
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        stone.GetComponent<Rigidbody2D>().mass *= radius;
        return stone;
    }

    #region SPAWN_GEMS
        private GameObject SpawnGem()
    {
        Vector3 spawnpos = transform.position + Vector3.up;
        GameObject gem = Instantiate(GetRandomGem(), spawnpos, Quaternion.identity);
        return gem;
    }

    public void SpawnGemWithParams(Vector3 position, GameObject gem = null, float delay = 0f)
    {
        if (delay <= 0f)
        {
            GameObject g = Instantiate(gem ?? GetRandomGem(), position, Quaternion.identity);
            ApplyRandomSpin(g.GetComponent<Rigidbody2D>());
        }
        else
        {
            StartCoroutine(SpawnGemDelayedRoutine(position, gem, delay));
        }
    }

    private IEnumerator SpawnGemDelayedRoutine(Vector3 position, GameObject gem, float delay)
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;
        yield return CoroutineUtils.WaitForUnpausedSeconds(delay, isPaused);
        GameObject g = Instantiate(gem ?? GetRandomGem(), position, Quaternion.identity);
        ApplyRandomSpin(g.GetComponent<Rigidbody2D>());
    }

    private GameObject GetRandomGem()
    {
        int index = ToweraldStatic.WeightedRandomChoice(ToweraldStatic.gemRarity);
        return gemPrefabs[index];
    }
    #endregion
    
    private GameObject SpawnItem()
    {
        Item itemType = ToweraldStatic.WeightedRandomChoice(ToweraldStatic.itemRarity);
        
        Vector3 spawnpos = transform.position + (Vector3.up * 2);
        if (itemType == Item.Helmet)
        {
            Helmet helmetType = ToweraldStatic.WeightedRandomChoice(ToweraldStatic.helmetRarity);
            return Instantiate(helmetsByType[helmetType], spawnpos, Quaternion.identity);
        }
        else
            return Instantiate(itemPrefabs[(int)itemType], spawnpos, Quaternion.identity);
    }

    private GameObject SpawnBug()
    {
        Vector3 spawnpos = transform.position + Vector3.up * 1.5f;
        GameObject bug = Instantiate(bugsPrefabs[Random.Range(0, bugsPrefabs.Length)], spawnpos, Quaternion.identity);
        return bug;
    }


    public void ApplyRandomSpin(Rigidbody2D rigid)
    {
        float randomTorque = Random.Range(40, 100);
        float randomDirection = Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
        rigid.AddTorque(randomTorque * randomDirection * rigid.mass);
    }
}
