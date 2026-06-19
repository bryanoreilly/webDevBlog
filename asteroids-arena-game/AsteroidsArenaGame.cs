using System.Collections.Generic;
using UnityEngine;

public class AsteroidsArenaGame : MonoBehaviour
{
    [Header("Arena")]
    public float arenaHalfWidth = 12f;
    public float arenaHalfHeight = 7f;
    public int startingLives = 3;
    public int startingRocks = 7;

    [Header("Ship")]
    public float shipAcceleration = 16f;
    public float shipTurnSpeed = 220f;
    public float shipMaxSpeed = 8f;
    public float shipDrag = 0.985f;
    public float shipRadius = 0.42f;
    public float invulnerableSeconds = 2f;

    [Header("Weapons")]
    public float bulletSpeed = 13f;
    public float bulletLifetime = 1.25f;
    public float fireCooldown = 0.16f;

    readonly List<AsteroidsRock> rocks = new List<AsteroidsRock>();
    readonly List<AsteroidsBullet> bullets = new List<AsteroidsBullet>();

    Material shipMaterial;
    Material thrustMaterial;
    Material bulletMaterial;
    Material[] rockMaterials;
    Material arenaMaterial;

    Transform ship;
    Transform thrustFlame;
    Vector2 shipVelocity;
    float nextFireTime;
    float invulnerableUntil;
    int score;
    int lives;
    int wave;
    bool gameOver;
    Camera mainCamera;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        mainCamera = Camera.main;
        ConfigureCamera();
        CreateMaterials();
        CreateArenaFrame();
        CreateShip();
        ResetGame();
    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                ResetGame();
            }
            return;
        }

        HandleShipInput();
        UpdateBullets();
        UpdateRocks();
        CheckBulletHits();
        CheckShipHits();

        if (rocks.Count == 0)
        {
            SpawnWave(wave + 1);
        }
    }

    void ResetGame()
    {
        ClearRuntimeObjects();
        score = 0;
        lives = startingLives;
        wave = 0;
        gameOver = false;
        RespawnShip();
        SpawnWave(1);
    }

    void ConfigureCamera()
    {
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.transform.rotation = Quaternion.identity;
        mainCamera.orthographic = true;
        float paddedHeight = arenaHalfHeight + 1.2f;
        float paddedWidthAsHeight = (arenaHalfWidth + 1.2f) / Mathf.Max(0.1f, mainCamera.aspect);
        mainCamera.orthographicSize = Mathf.Max(paddedHeight, paddedWidthAsHeight);
        mainCamera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    void CreateMaterials()
    {
        shipMaterial = MakeMaterial("AA Ship Cyan", new Color(0.1f, 0.95f, 1f));
        thrustMaterial = MakeMaterial("AA Thrust Amber", new Color(1f, 0.48f, 0.08f));
        bulletMaterial = MakeMaterial("AA Bullet Mint", new Color(0.65f, 1f, 0.55f));
        arenaMaterial = MakeMaterial("AA Arena Lines", new Color(0.18f, 0.45f, 0.75f));
        rockMaterials = new[]
        {
            MakeMaterial("AA Rock Lime", new Color(0.5f, 0.95f, 0.35f)),
            MakeMaterial("AA Rock Violet", new Color(0.8f, 0.55f, 1f)),
            MakeMaterial("AA Rock Coral", new Color(1f, 0.42f, 0.32f))
        };
    }

    Material MakeMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        material.name = materialName;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        return material;
    }

    void CreateArenaFrame()
    {
        GameObject frameRoot = new GameObject("Procedural Arena Frame");
        frameRoot.transform.SetParent(transform, false);

        CreateLine(frameRoot.transform, "Top Boundary", new Vector2(0f, arenaHalfHeight), new Vector2(arenaHalfWidth * 2f, 0.05f));
        CreateLine(frameRoot.transform, "Bottom Boundary", new Vector2(0f, -arenaHalfHeight), new Vector2(arenaHalfWidth * 2f, 0.05f));
        CreateLine(frameRoot.transform, "Left Boundary", new Vector2(-arenaHalfWidth, 0f), new Vector2(0.05f, arenaHalfHeight * 2f));
        CreateLine(frameRoot.transform, "Right Boundary", new Vector2(arenaHalfWidth, 0f), new Vector2(0.05f, arenaHalfHeight * 2f));
    }

    void CreateLine(Transform parent, string lineName, Vector2 position, Vector2 scale)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = lineName;
        line.transform.SetParent(parent, false);
        line.transform.position = new Vector3(position.x, position.y, 0.8f);
        line.transform.localScale = new Vector3(scale.x, scale.y, 0.05f);
        line.GetComponent<Renderer>().sharedMaterial = arenaMaterial;
        Destroy(line.GetComponent<Collider>());
    }

    void CreateShip()
    {
        GameObject shipObject = new GameObject("Player Ship");
        shipObject.transform.SetParent(transform, false);
        shipObject.AddComponent<MeshFilter>().mesh = CreateShipMesh();
        shipObject.AddComponent<MeshRenderer>().sharedMaterial = shipMaterial;
        ship = shipObject.transform;

        GameObject flame = new GameObject("Thrust Flame");
        flame.transform.SetParent(ship, false);
        flame.transform.localPosition = new Vector3(0f, -0.48f, 0.05f);
        flame.transform.localRotation = Quaternion.identity;
        flame.AddComponent<MeshFilter>().mesh = CreateFlameMesh();
        flame.AddComponent<MeshRenderer>().sharedMaterial = thrustMaterial;
        thrustFlame = flame.transform;
        thrustFlame.gameObject.SetActive(false);
    }

    Mesh CreateShipMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Ship Mesh";
        mesh.vertices = new[]
        {
            new Vector3(0f, 0.62f, 0f),
            new Vector3(-0.42f, -0.42f, 0f),
            new Vector3(0f, -0.2f, 0f),
            new Vector3(0.42f, -0.42f, 0f)
        };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreateFlameMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Flame Mesh";
        mesh.vertices = new[]
        {
            new Vector3(-0.18f, 0.16f, 0f),
            new Vector3(0.18f, 0.16f, 0f),
            new Vector3(0f, -0.42f, 0f)
        };
        mesh.triangles = new[] { 0, 2, 1 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    Mesh CreateRockMesh(float radius, int seed)
    {
        Random.State oldState = Random.state;
        Random.InitState(seed);

        int points = Random.Range(9, 15);
        Vector3[] vertices = new Vector3[points + 1];
        int[] triangles = new int[points * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < points; i++)
        {
            float angle = (Mathf.PI * 2f * i) / points;
            float wobble = Random.Range(0.68f, 1.15f);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius * wobble, Mathf.Sin(angle) * radius * wobble, 0f);
        }

        for (int i = 0; i < points; i++)
        {
            int offset = i * 3;
            triangles[offset] = 0;
            triangles[offset + 1] = i == points - 1 ? 1 : i + 2;
            triangles[offset + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Procedural Rock Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Random.state = oldState;
        return mesh;
    }

    Mesh CreateDiscMesh(float radius, int segments)
    {
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            int offset = i * 3;
            triangles[offset] = 0;
            triangles[offset + 1] = i == segments - 1 ? 1 : i + 2;
            triangles[offset + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Procedural Disc Mesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void HandleShipInput()
    {
        float turn = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) turn += 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) turn -= 1f;
        ship.Rotate(0f, 0f, turn * shipTurnSpeed * Time.deltaTime);

        bool thrusting = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        if (thrusting)
        {
            shipVelocity += (Vector2)ship.up * shipAcceleration * Time.deltaTime;
        }

        shipVelocity = Vector2.ClampMagnitude(shipVelocity, shipMaxSpeed);
        shipVelocity *= Mathf.Pow(shipDrag, Time.deltaTime * 60f);
        ship.position += (Vector3)(shipVelocity * Time.deltaTime);
        WrapTransform(ship);

        if (thrustFlame != null)
        {
            thrustFlame.gameObject.SetActive(thrusting);
            float pulse = 0.8f + Mathf.Sin(Time.time * 30f) * 0.2f;
            thrustFlame.localScale = new Vector3(1f, pulse, 1f);
        }

        bool firePressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (firePressed && Time.time >= nextFireTime)
        {
            FireBullet();
        }
    }

    void FireBullet()
    {
        nextFireTime = Time.time + fireCooldown;
        GameObject bulletObject = new GameObject("Bullet");
        bulletObject.transform.SetParent(transform, false);
        bulletObject.transform.position = ship.position + ship.up * 0.68f;
        bulletObject.AddComponent<MeshFilter>().mesh = CreateDiscMesh(0.08f, 10);
        bulletObject.AddComponent<MeshRenderer>().sharedMaterial = bulletMaterial;

        AsteroidsBullet bullet = bulletObject.AddComponent<AsteroidsBullet>();
        bullet.Initialize((Vector2)ship.up * bulletSpeed + shipVelocity, bulletLifetime, arenaHalfWidth, arenaHalfHeight);
        bullets.Add(bullet);
    }

    void SpawnWave(int newWave)
    {
        wave = newWave;
        int count = startingRocks + wave - 1;
        for (int i = 0; i < count; i++)
        {
            SpawnRock(RandomEdgePosition(), Random.Range(2, 4), Random.insideUnitCircle.normalized * Random.Range(0.8f, 2.1f));
        }
    }

    Vector2 RandomEdgePosition()
    {
        bool horizontal = Random.value > 0.5f;
        if (horizontal)
        {
            return new Vector2(Random.Range(-arenaHalfWidth, arenaHalfWidth), Random.value > 0.5f ? arenaHalfHeight : -arenaHalfHeight);
        }

        return new Vector2(Random.value > 0.5f ? arenaHalfWidth : -arenaHalfWidth, Random.Range(-arenaHalfHeight, arenaHalfHeight));
    }

    void SpawnRock(Vector2 position, int size, Vector2 velocity)
    {
        size = Mathf.Clamp(size, 1, 3);
        float radius = size == 3 ? 1.0f : size == 2 ? 0.66f : 0.36f;

        GameObject rockObject = new GameObject("Rock Size " + size);
        rockObject.transform.SetParent(transform, false);
        rockObject.transform.position = new Vector3(position.x, position.y, 0f);
        rockObject.AddComponent<MeshFilter>().mesh = CreateRockMesh(radius, Random.Range(1, int.MaxValue));
        rockObject.AddComponent<MeshRenderer>().sharedMaterial = rockMaterials[Random.Range(0, rockMaterials.Length)];

        AsteroidsRock rock = rockObject.AddComponent<AsteroidsRock>();
        rock.Initialize(size, radius, velocity, Random.Range(-120f, 120f), arenaHalfWidth, arenaHalfHeight);
        rocks.Add(rock);
    }

    void UpdateBullets()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i] == null)
            {
                bullets.RemoveAt(i);
            }
        }
    }

    void UpdateRocks()
    {
        for (int i = rocks.Count - 1; i >= 0; i--)
        {
            if (rocks[i] == null)
            {
                rocks.RemoveAt(i);
            }
        }
    }

    void CheckBulletHits()
    {
        for (int b = bullets.Count - 1; b >= 0; b--)
        {
            AsteroidsBullet bullet = bullets[b];
            if (bullet == null) continue;

            for (int r = rocks.Count - 1; r >= 0; r--)
            {
                AsteroidsRock rock = rocks[r];
                if (rock == null) continue;

                float hitDistance = rock.radius + 0.1f;
                if (Vector2.Distance(bullet.transform.position, rock.transform.position) <= hitDistance)
                {
                    DestroyBullet(bullet);
                    BreakRock(rock);
                    break;
                }
            }
        }
    }

    void CheckShipHits()
    {
        if (Time.time < invulnerableUntil) return;

        for (int i = 0; i < rocks.Count; i++)
        {
            AsteroidsRock rock = rocks[i];
            if (rock == null) continue;

            if (Vector2.Distance(ship.position, rock.transform.position) <= rock.radius + shipRadius)
            {
                LoseLife();
                return;
            }
        }
    }

    void BreakRock(AsteroidsRock rock)
    {
        if (rock == null) return;

        int rockSize = rock.size;
        Vector2 position = rock.transform.position;
        Vector2 baseVelocity = rock.velocity;
        rocks.Remove(rock);
        Destroy(rock.gameObject);

        score += rockSize == 3 ? 50 : rockSize == 2 ? 100 : 200;

        if (rockSize > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 splitVelocity = (Random.insideUnitCircle.normalized * Random.Range(1.4f, 3.2f)) + baseVelocity * 0.35f;
                SpawnRock(position + Random.insideUnitCircle * 0.25f, rockSize - 1, splitVelocity);
            }
        }
    }

    void DestroyBullet(AsteroidsBullet bullet)
    {
        bullets.Remove(bullet);
        if (bullet != null) Destroy(bullet.gameObject);
    }

    void LoseLife()
    {
        lives--;
        if (lives <= 0)
        {
            gameOver = true;
            ship.gameObject.SetActive(false);
            return;
        }

        RespawnShip();
    }

    void RespawnShip()
    {
        ship.gameObject.SetActive(true);
        ship.position = Vector3.zero;
        ship.rotation = Quaternion.identity;
        shipVelocity = Vector2.zero;
        nextFireTime = Time.time + 0.35f;
        invulnerableUntil = Time.time + invulnerableSeconds;
    }

    void ClearRuntimeObjects()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i] != null) Destroy(bullets[i].gameObject);
        }
        bullets.Clear();

        for (int i = rocks.Count - 1; i >= 0; i--)
        {
            if (rocks[i] != null) Destroy(rocks[i].gameObject);
        }
        rocks.Clear();
    }

    void WrapTransform(Transform target)
    {
        Vector3 p = target.position;
        if (p.x > arenaHalfWidth) p.x = -arenaHalfWidth;
        if (p.x < -arenaHalfWidth) p.x = arenaHalfWidth;
        if (p.y > arenaHalfHeight) p.y = -arenaHalfHeight;
        if (p.y < -arenaHalfHeight) p.y = arenaHalfHeight;
        target.position = p;
    }

    void OnGUI()
    {
        GUI.color = new Color(0.78f, 1f, 0.95f);
        GUI.Label(new Rect(18, 14, 360, 30), "ASTEROIDS ARENA");
        GUI.Label(new Rect(18, 38, 360, 30), "Score: " + score + "   Lives: " + lives + "   Wave: " + wave);
        GUI.Label(new Rect(18, Screen.height - 34, 700, 30), "Move: WASD/Arrows   Fire: Space/Mouse   Wrap the arena, split rocks, survive waves");

        if (Time.time < invulnerableUntil && !gameOver)
        {
            GUI.Label(new Rect(Screen.width * 0.5f - 70f, 18, 180, 30), "SHIELDS ONLINE");
        }

        if (gameOver)
        {
            GUI.color = new Color(1f, 0.82f, 0.38f);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 26;
            GUI.Label(new Rect(0, Screen.height * 0.5f - 50f, Screen.width, 40), "GAME OVER", style);
            style.fontSize = 16;
            GUI.Label(new Rect(0, Screen.height * 0.5f - 10f, Screen.width, 40), "Press Space or Enter to restart", style);
        }
    }
}

public class AsteroidsRock : MonoBehaviour
{
    public int size { get; private set; }
    public float radius { get; private set; }
    public Vector2 velocity { get; private set; }

    float spin;
    float arenaHalfWidth;
    float arenaHalfHeight;

    public void Initialize(int rockSize, float rockRadius, Vector2 rockVelocity, float rockSpin, float halfWidth, float halfHeight)
    {
        size = rockSize;
        radius = rockRadius;
        velocity = rockVelocity;
        spin = rockSpin;
        arenaHalfWidth = halfWidth;
        arenaHalfHeight = halfHeight;
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, spin * Time.deltaTime);
        Wrap();
    }

    void Wrap()
    {
        Vector3 p = transform.position;
        if (p.x > arenaHalfWidth) p.x = -arenaHalfWidth;
        if (p.x < -arenaHalfWidth) p.x = arenaHalfWidth;
        if (p.y > arenaHalfHeight) p.y = -arenaHalfHeight;
        if (p.y < -arenaHalfHeight) p.y = arenaHalfHeight;
        transform.position = p;
    }
}

public class AsteroidsBullet : MonoBehaviour
{
    Vector2 velocity;
    float expiresAt;
    float arenaHalfWidth;
    float arenaHalfHeight;

    public void Initialize(Vector2 bulletVelocity, float lifetime, float halfWidth, float halfHeight)
    {
        velocity = bulletVelocity;
        expiresAt = Time.time + lifetime;
        arenaHalfWidth = halfWidth;
        arenaHalfHeight = halfHeight;
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        Wrap();

        if (Time.time >= expiresAt)
        {
            Destroy(gameObject);
        }
    }

    void Wrap()
    {
        Vector3 p = transform.position;
        if (p.x > arenaHalfWidth) p.x = -arenaHalfWidth;
        if (p.x < -arenaHalfWidth) p.x = arenaHalfWidth;
        if (p.y > arenaHalfHeight) p.y = -arenaHalfHeight;
        if (p.y < -arenaHalfHeight) p.y = arenaHalfHeight;
        transform.position = p;
    }
}
