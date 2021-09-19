using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    //Fungsi Singleton
    private static LevelManager _instance = null;
    public static LevelManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }

            return _instance;
        }
    }

    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;

    [SerializeField] private Tower[] _towerPrefabs;
    [SerializeField] private Enemy[] _enemyPrefabs;

    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    private List<Tower> _spawnedTowers = new List<Tower>();
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    private int _currentLives;
    private int _enemyCounter;

    private float _runningSpawnDelay;

    public bool IsOver { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        InstantiateAllTowerUI();

        //Play BGM
        AudioPlayer.Instance.PlaySFX("BackgroundMusic");
    }

    // Update is called once per frame
    void Update()
    {
        //restart game jika menekan tombol R
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if(IsOver)
        {
            return;
        }

        //counter untuk spawn enemy dalam jeda waktu yang ditentukan
        //Time.unscaledDeltaTime hanya terpengaruh oleh game object itu sendiri sehingga bisa digunakan sebagai penghitung waktu
        _runningSpawnDelay -= Time.unscaledDeltaTime;
        if(_runningSpawnDelay <= 0f)
        {
            SpawnEnemy();
            _runningSpawnDelay = _spawnDelay;
        }

        foreach(Enemy enemy in _spawnedEnemies)
        {
            if(!enemy.gameObject.activeSelf)
            {
                continue;
            }

            //dipilih 0.1 untuk lebih mentoleransi perbedaan posisi karena perbedaan 0/sama persis akan terlalu sulit
            if(Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);

                if(enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives(1);
                    enemy.gameObject.SetActive(false);
                    AudioPlayer.Instance.PlaySFX("enemy-pass");
                }
            }
            else
            {
                enemy.MoveToTarget();
            }

        }

        //menggerakkan tower
        foreach(Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }

    }

    //showing all tower from UI tower selection
    private void InstantiateAllTowerUI()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObject = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);

            TowerUI newTowerUI = newTowerUIObject.GetComponent<TowerUI>();

            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    //mendaftarkan tower yg dispawn agar bisa dikontrol levelmanager
    public void RegisterSpawnedTower (Tower tower)
    {
        _spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        SetTotalEnemy(--_enemyCounter);
        if(_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;

            if(isAllEnemyDestroyed)
            {
                SetGameOver(true);
                AudioPlayer.Instance.PlaySFX("player-win");
            }

            return;
        }


        int randomIndex = Random.Range(0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();

        GameObject newEnemyObject = _spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;

        if(newEnemyObject == null)
        {
            newEnemyObject = Instantiate(_enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObject.GetComponent<Enemy>();
        if(!_spawnedEnemies.Contains(newEnemy))
        {
            _spawnedEnemies.Add(newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition(_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);   
    }

    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject newBulletObject = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;

        if(newBulletObject == null)
        {
            newBulletObject = Instantiate(prefab.gameObject);
        }

        Bullet newBullet = newBulletObject.GetComponent<Bullet>();
        if(!_spawnedBullets.Contains(newBullet))
        {
            _spawnedBullets.Add(newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach(Enemy enemy in _spawnedEnemies)
        {
            if(enemy.gameObject.activeSelf)
            {
                if(Vector2.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        SetCurrentLives(_currentLives - value);
        if(_currentLives <= 0)
        {
            SetGameOver(false);
            AudioPlayer.Instance.PlaySFX("player-lose");
        }
    }

    public void SetCurrentLives(int currentLives)
    {
        //Mathf.Max untuk mengambil angka terbesar sehingga _currentLives disini tidak akan lebih kecil dari 0
        _currentLives = Mathf.Max(currentLives, 0);
        _livesInfo.text = $"Lives: { _currentLives }";
    }

    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: { Mathf.Max(_enemyCounter, 0) }";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;

        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive(true);

    
        AudioPlayer.Instance.StopBGM("BackgroundMusic");
    }

    //untuk menampilkan garis penghubung dalam window scene tanpa perlu diplay terlebih dahulu
    private void OnDrawGizmos() 
    {
        for(int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }


}//class
