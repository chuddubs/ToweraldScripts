using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Godson : Singletroon<Godson>
{
    public bool init = false;
    public Animator animator;
    private int thremboCount = 0;
    private bool lastWasMeteor = false;
    
    #region ITEMS_VARIABLES
    [SerializeField] private float guaranteedArrowInterval = 15f;
    private Coroutine itemsRoutine;
    #endregion
    
    #region ROCKS_VARIABLES
    [Header("Rocks")]
    [SerializeField] private SoyRockZoneTracker rockZoneTracker;
    public Material[] stoneMaterials;
    public GameObject stonePrefab;
    private Coroutine rocksRoutine = null;
    #endregion

    #region ANGEL_VARIABLES
    [Header("Angels")]
    [SerializeField] private GameObject angelPrefab;
    [SerializeField] private Transform angelLeftSpawn;
    [SerializeField] private GameObject angelLeftWarning;
    [SerializeField] private Transform angelRightSpawn;
    [SerializeField] private GameObject angelRightWarning;
    private bool angelRoutineRunning = false;
    private Coroutine angelRoutine = null;
    #endregion

    #region METEORS_VARIABLES
    [Header("Meteors")]
    [SerializeField] private GameObject meteorWarningDisplay;
    [SerializeField] private GameObject meteorPrefab;
    private bool meteorRoutineRunning = false;
    private Coroutine meteorRoutine = null;
    #endregion

    #region WASPS_VARIABLES
    [Header("Wasps")]
    [SerializeField] private GameObject waspPrefab;
    [SerializeField] private GameObject[] numberSprites;
    [SerializeField] private GameObject thremboPrefab;
    private bool waspRoutineRunning = false;
    private Coroutine waspRoutine = null;
    private float timeBeforeNextSwarm;
    private int waspKillCount = 0;
    #endregion

    public void Init()
    {
        init = true;
        rocksRoutine = StartCoroutine(SpawnRocks());
        itemsRoutine = StartCoroutine(SpawnItems());
        timeBeforeNextSwarm = UnityEngine.Random.Range(8f, 12f);
    }

    #region ROCKS
    private Vector3 PickRockSpawn()
    {
        return new Vector3(UnityEngine.Random.Range(-2.2f, 2.2f), transform.position.y + 13.5f, transform.position.z);
    }

    public void ApplyRandomSpin(Rigidbody2D rigid)
    {
        float randomTorque = UnityEngine.Random.Range(40, 100);
        float randomDirection = UnityEngine.Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
        rigid.AddTorque(randomTorque * randomDirection * rigid.mass);
    }

    private IEnumerator SpawnRocks()
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;
        float timeUntilNextRock = 0.8f;

        while (true)
        {
            while (isPaused())
                yield return null;

            if (timeUntilNextRock <= 0f)
            {
                ApplyRandomSpin(SpawnStone().GetComponent<Rigidbody2D>());
                timeUntilNextRock = ToweraldStatic.TriangularDistribution(0.4f, 0.8f, 1f);
            }

            timeUntilNextRock -= Time.deltaTime * (waspRoutineRunning ? 0.25f : 1f);
            yield return null;
        }
    }


    private GameObject SpawnStone()
    {
        Vector3 spawnPos = PickRockSpawn();
        GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);
        float radius = UnityEngine.Random.Range(0.4f, 0.7f);
        stone.GetComponent<PolygonMeshGenerator>().Generate(UnityEngine.Random.Range(6, 12), radius, UnityEngine.Random.Range(0.01f, 0.55f));
        Material rockMat = stoneMaterials[UnityEngine.Random.Range(0, stoneMaterials.Length)];
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

    private void EndRocksRoutine()
    {
        if (rocksRoutine != null)
        {
            StopCoroutine(rocksRoutine);
            rocksRoutine = null;
        }
    }

    #endregion

    #region ANGELS
    private void AngelRoll()
    {
        EndRocksRoutine();
        angelRoutineRunning = true;
        angelRoutine = StartCoroutine(AngelRollCoroutine());
    }

    private IEnumerator AngelRollCoroutine()
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;

        bool fromLeft = UnityEngine.Random.value < 0.5f;

        float angelSpeed = 5f;
        float warningDuration = 1f;
        float screenTime = 5.6f / angelSpeed;
        float exitTime = 0.92f / angelSpeed;

        // First Angel
        Transform spawnPoint1 = fromLeft ? angelLeftSpawn : angelRightSpawn;
        GameObject angel1 = Instantiate(angelPrefab, spawnPoint1.position, Quaternion.identity);
        SoyAngel angelScript1 = angel1.GetComponent<SoyAngel>();
        angelScript1.direction = fromLeft ? 1 : -1;
        angelScript1.speed = angelSpeed;

        GameObject warning1 = fromLeft ? angelLeftWarning : angelRightWarning;
        warning1.SetActive(true);
        yield return CoroutineUtils.WaitForUnpausedSeconds(1f, isPaused);
        warning1.SetActive(false);

        yield return CoroutineUtils.WaitForUnpausedSeconds(screenTime + exitTime, isPaused);
        Destroy(angel1);

        yield return CoroutineUtils.WaitForUnpausedSeconds(1f, isPaused);

        // Second Angel
        Transform spawnPoint2 = fromLeft ? angelRightSpawn : angelLeftSpawn;
        GameObject angel2 = Instantiate(angelPrefab, spawnPoint2.position, Quaternion.identity);
        SoyAngel angelScript2 = angel2.GetComponent<SoyAngel>();
        angelScript2.direction = fromLeft ? -1 : 1;
        angelScript2.speed = angelSpeed;

        GameObject warning2 = fromLeft ? angelRightWarning : angelLeftWarning;
        warning2.SetActive(true);
        yield return CoroutineUtils.WaitForUnpausedSeconds(warningDuration, isPaused);
        warning2.SetActive(false);

        yield return CoroutineUtils.WaitForUnpausedSeconds(screenTime + exitTime, isPaused);
        Destroy(angel2);

        EndAngelRoutine();
    }

    public void EndAngelRoutine(bool fromThrembo = false)
    {
        if (!fromThrembo)
        {
            if (rocksRoutine == null)
                rocksRoutine = StartCoroutine(SpawnRocks());
        }
        if (angelRoutineRunning && angelRoutine != null)
        {
            if (fromThrembo)
                StopCoroutine(angelRoutine);
            angelRoutine = null;
        }
        angelRoutineRunning = false;
    }
    #endregion
    

    #region METEORS
    private void MeteorWave()
    {
        EndRocksRoutine();
        meteorRoutineRunning = true;
        meteorRoutine = StartCoroutine(MeteorWaveCoroutine());
    }

    private IEnumerator MeteorWaveCoroutine()
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;

        yield return CoroutineUtils.WaitForUnpausedSeconds(1f, isPaused);

        int meteorCount = 7;
        float interval = 1.6f;
        float xMin = -1.86f;
        float xMax = 1.86f;
        float spawnY = transform.position.y + 13.5f;

        // Generate evenly spaced X positions
        List<float> xSlots = new List<float>();
        for (int i = 0; i < meteorCount; i++)
        {
            float t = i / (float)(meteorCount - 1);
            float x = Mathf.Lerp(xMin, xMax, t);
            xSlots.Add(x);
        }

        // Randomly choose a safe index to skip
        int safeIndex = UnityEngine.Random.Range(0, xSlots.Count);

        // Shuffle remaining indices (for spawn order)
        List<int> spawnOrder = new List<int>();
        for (int i = 0; i < xSlots.Count; i++)
        {
            if (i != safeIndex) spawnOrder.Add(i);
        }
        for (int i = 0; i < spawnOrder.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, spawnOrder.Count);
            (spawnOrder[i], spawnOrder[j]) = (spawnOrder[j], spawnOrder[i]);
        }

        // Show warning, wait, then spawn meteor
        for (int i = 0; i < spawnOrder.Count; i++)
        {
            float xRandomOffset = UnityEngine.Random.Range(-0.15f, 0.15f);
            float x = Mathf.Clamp(xSlots[spawnOrder[i]] + xRandomOffset, xMin, xMax);
            Vector3 spawnPosition = new Vector3(x, spawnY, transform.position.z);

            // Set and show warning
            Vector3 warningPos = new Vector3(x, meteorWarningDisplay.transform.position.y, 0f);
            meteorWarningDisplay.transform.position = warningPos;
            meteorWarningDisplay.SetActive(true);

            yield return CoroutineUtils.WaitForUnpausedSeconds(interval, isPaused);

            GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
            float speed = Mathf.Lerp(5f, 8f, i / (float)(meteorCount - 1));
            SoyMeteor script = meteor.GetComponent<SoyMeteor>();
            script.speed = speed;

            Destroy(meteor, 4f);
        }
        meteorWarningDisplay.SetActive(false);

        yield return CoroutineUtils.WaitForUnpausedSeconds(1f, isPaused);
        EndMeteorRoutine();
    }

    public void EndMeteorRoutine(bool fromThrembo = false)
    {
        if (!fromThrembo)
        {
            if (rocksRoutine == null)
                rocksRoutine = StartCoroutine(SpawnRocks());
        }
        if (meteorRoutineRunning && meteorRoutine != null)
        {
            if (fromThrembo)
                StopCoroutine(meteorRoutine);
            meteorRoutine = null;
        }
        meteorRoutineRunning = false;
        meteorWarningDisplay.SetActive(false);
    }
    #endregion

    #region WASPS
    private void WaspSwarm()
    {
        waspRoutineRunning = true;
        waspRoutine = StartCoroutine(WaspSwarmRoutine());
    }

    private IEnumerator WaspSwarmRoutine()
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;

        int waspsToSpawn = 7;
        float delayBetweenWasps = 1.2f;

        for (int i = 0; i < waspsToSpawn; i++)
        {
            bool leftToRight = UnityEngine.Random.value < 0.5f;
            float xOffset = 3.2f * (leftToRight ? -1 : 1);
            float spawnX = transform.position.x + xOffset;
            float spawnY = transform.position.y + UnityEngine.Random.Range(8f, 12f);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

            GameObject waspGO = Instantiate(waspPrefab, spawnPosition, Quaternion.identity);
            SoyWasp wasp = waspGO.GetComponent<SoyWasp>();
            if (wasp != null)
            {
                wasp.Init(leftToRight);
            }

            yield return CoroutineUtils.WaitForUnpausedSeconds(delayBetweenWasps, isPaused);
        }

        EndWaspRoutine();
    }

    public void EndWaspRoutine(bool fromThrembo = false)
    {
        if (waspRoutineRunning && waspRoutine != null)
        {
            if (fromThrembo)
                StopCoroutine(waspRoutine);
            waspRoutine = null;
        }
        waspRoutineRunning = false;
    }

    public void OnWaspDied(Vector3 waspPos)
    {
        OnMinionDie();
        waspKillCount += 1;
        if (waspKillCount >= 7)
        {
            waspKillCount = 0;
            SpawnThrembo(waspPos);
        }
        else
        {
            GameObject number = Instantiate(numberSprites[waspKillCount - 1], waspPos, Quaternion.identity);
            Destroy(number, 1f);
        }

    }
    #endregion
    
    #region THREMBO
    public void OnPickedUpThrembo()
    {
        OnTakeHit();
        EndAngelRoutine(true);
        EndWaspRoutine(true);
        EndMeteorRoutine(true);
        thremboCount += 1;
        if (thremboCount >= 3)
        {
            EndRocksRoutine();
            StopItemSpawning();
            SoyGameController.Instance.WinGame();
        }
        else if (rocksRoutine == null)
            rocksRoutine = StartCoroutine(SpawnRocks());
    }

    public void SpawnThrembo(Vector3 pos)
    { 
        Instantiate(thremboPrefab, pos, Quaternion.identity);
    }
    #endregion

    #region ITEMS
    private IEnumerator SpawnItems()
    {
        Func<bool> isPaused = () => SoyGameController.Instance.IsPaused;

        Vector3 itemDistrib = new Vector3(6f, 12f, 18f);
        float nextArrowTime = Time.unscaledTime + 3f;

        // Initial delay for the first random item
        float nextItemTime = Time.unscaledTime + ToweraldStatic.TriangularDistribution(3f , 6f, 9f);

        while (true)
        {
            while (isPaused())
                yield return null;

            if (Time.unscaledTime >= nextArrowTime)
            {
                GameObject item = SpawnItemOfType(Item.GreenArrow);
                ApplyRandomSpin(item.GetComponent<Rigidbody2D>());
                nextArrowTime = Time.unscaledTime + guaranteedArrowInterval;
            }

            if (Time.unscaledTime >= nextItemTime)
            {
                Item itemType = ToweraldStatic.WeightedRandomChoice(ToweraldStatic.bossItemRarity);
                GameObject item = SpawnItemOfType(itemType);
                ApplyRandomSpin(item.GetComponent<Rigidbody2D>());
                nextItemTime = Time.unscaledTime + ToweraldStatic.TriangularDistribution(itemDistrib.x , itemDistrib.y, itemDistrib.z);
            }

            yield return null;
        }
    }

    private GameObject SpawnItemOfType(Item itemType)
    {
        Vector3 spawnpos = transform.position + (Vector3.up * 11.5f);

        if (itemType == Item.Helmet)
        {
            Helmet helmetType = ToweraldStatic.WeightedRandomChoice(ToweraldStatic.helmetRarity);
            return Instantiate(ObjectSpawner.Instance.helmetsByType[helmetType], spawnpos, Quaternion.identity);
        }
        else
        {
            return Instantiate(ObjectSpawner.Instance.itemPrefabs[(int)itemType], spawnpos, Quaternion.identity);
        }
    }

    public void StopItemSpawning()
    {
        if (itemsRoutine != null)
            StopCoroutine(itemsRoutine);
    }

    #endregion

    void Update()
    {
        if (SoyGameController.Instance.IsPaused)
            return;
        if (!init || thremboCount >= 3)
            return;

        if (angelRoutineRunning || meteorRoutineRunning || waspRoutineRunning)
            return;

        timeBeforeNextSwarm -= Time.deltaTime;
        // Rocks piled up: alternate meteor and angel
        if (rockZoneTracker.LandedRockCount > 3)
        {
            OnStartAttack();
            if (!lastWasMeteor)
            {
                MeteorWave();
                lastWasMeteor = true;
            }
            else
            {
                AngelRoll();
                lastWasMeteor = false;
            }
        }
        else if (timeBeforeNextSwarm <= 0f)
        {
            WaspSwarm();
            timeBeforeNextSwarm = UnityEngine.Random.Range(8f, 12f);
        }
    }

    #region ANIMATION
    private bool busyLaserEyes = false;

    public void OnPlayerDie()
    {
        if (!busyLaserEyes)
            animator.SetTrigger("PlayerDie");
    }   

    public void OnPlayerHit()
    {
        if (!busyLaserEyes)
            animator.SetTrigger("PlayerHit");
    }

    public void OnMinionDie()
    {
        if (!busyLaserEyes)
            animator.SetTrigger("WaspDie");
    }

    public void OnTakeHit()
    {
        if (!busyLaserEyes)
            animator.SetTrigger("ThremboXplode");
    }

    public void OnStartAttack()
    {
        busyLaserEyes = true;
        animator.SetTrigger("Attacking");
        Invoke("NoLongeBusyLaserEyes", 4f);
    }

    private void NoLongeBusyLaserEyes()
    {
        busyLaserEyes = false;
    }

    #endregion
}
