public enum TypeOfDeath
{
    hole,
    enemy
};

public class GameManager : MonoBehaviour
{
    //Member Variables
    [SerializeField] GameObject _player;
    [SerializeField] GameObject _tempPlatform;
    [SerializeField] GameObject _goal;
    [SerializeField] GameObject _optionsMenu, _optionsBackground;
    [SerializeField] int _respawnTime = 2;
    int _enemiesToDefeatPercentage = 50;
    int _coinsToCollectPercentage = 50;
    int _totalNumberOfEnemies;
    int _totalNumberOfCoins;
    int _levelNumber = 0;
    int _totalDeathsCounter = 0;
    int _holeDeathsCounter = 0;
    int _enemyDeathsCounter = 0;

    //Public Variables
    public GameObject InstantiatedPlayer;
    public GameObject InstantiatedPlatform;
    public GameObject InstantiatedGoal;
    public int SpawnedTerrain;
    public int LevelWidth;
    public bool LevelComplete;
    public Vector3 DeathPosition;

    //Cached References
    public TerrainManager TerrainManager;
    public EnemyManager EnemyManager;
    public ObstacleManager ObstacleManager;
    public PlayerController PlayerController;
    public CoinManager CoinManager;
    public ScoreManager ScoreManager;
    public CSVWriter CSVWriter;

    //UI
    public Slider EnemyPreferenceSlider;
    public Slider ObstaclePreferenceSlider;
    public Slider TerrainPreferenceSlider;
    public Slider CoinPreferenceSlider;
    public Slider GapWidthSlider;
    public Slider JumpHeightSlider;
    public Toggle Small, Medium, Large;
    public Toggle EnemyWincon, CoinWincon, GoalWinCon;
    public TMP_InputField EnemyField, CoinField;
    public TMP_Text EnemyText, CoinText;
    public GameObject CoinError, EnemyError, LevelCompleteText;

    public PolygonCollider2D CameraBounds;
    [SerializeField] Transform _rightBounds;

    Scene _currentScene;
    public bool FinalLevel;   

    private void Awake()
    {
        _currentScene = SceneManager.GetActiveScene();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_currentScene.name != "A Scene")
        {
            SetLevelSize();
            TerrainManager.GenerateTerrain();
            SpawnedTerrain = TerrainManager.TerrainPositionsDict.Count;
            ObstacleManager.SpawnedTerrain = SpawnedTerrain;
            ObstacleManager.SpawnObstacles();
            EnemyManager.SpawnEnemies();
            CoinManager.SpawnCoins();
            SetWincons(); //Must be set after generating enemies and coins            
            ToggleMenu(false);
            GoalInitialisation();
        }

        InstantiatedPlayer = Instantiate(_player);
    }
    // Update is called once per frame
    void Update()
    {
        if (_currentScene.name != "A Scene")
        {
            if (EnemyPreferenceSlider.value == 0)
            {
                EnemyWincon.isOn = false;
            }
            if (CoinPreferenceSlider.value == 0)
            {
                CoinWincon.isOn = false;
            }

            EnemyWincon.gameObject.SetActive(EnemyPreferenceSlider.value != 0);
            EnemyField.gameObject.SetActive(EnemyPreferenceSlider.value != 0);
            CoinWincon.gameObject.SetActive(CoinPreferenceSlider.value != 0);
            CoinField.gameObject.SetActive(CoinPreferenceSlider.value != 0);

            //Create new level
            if (Input.GetKeyDown(KeyCode.Return) && LevelComplete)
            {                
                ScoreManager.ResetCurrentScore();
                LevelComplete = false;
                _levelNumber++;
                _totalDeathsCounter = 0;
                _holeDeathsCounter = 0;
                _enemyDeathsCounter = 0;
                Time.timeScale = 1;
                Destroy(InstantiatedPlayer);
                Destroy(InstantiatedGoal);
                SetLevelSize(); //Must be set before generating terrain            
                TerrainManager.GenerateTerrain();
                SpawnedTerrain = TerrainManager.TerrainPositionsDict.Count;
                ObstacleManager.SpawnedTerrain = SpawnedTerrain;
                ObstacleManager.SpawnObstacles();
                EnemyManager.SpawnEnemies();
                CoinManager.SpawnCoins();
                InstantiatedPlayer = Instantiate(_player);                
                ToggleMenu(false);
                GoalInitialisation();
                SetWincons(); //Must be set after generating enemies
                ErrorMessages();
            }
        }
    }
    /// <summary>
    /// Displays text and enables/disables ability to win with enemies defeated
    /// </summary>
    public void EnemyWinCondition()
    {

        _totalNumberOfEnemies = EnemyManager.KillableEnemies.Count;

        if(_currentScene.name != "A Scene")
        {
            _enemiesToDefeatPercentage = int.Parse(EnemyField.text);
            //_totalNumberOfEnemies = EnemyManager.KillableEnemies.Count;

            if (EnemyWincon.isOn && EnemyPreferenceSlider.value > 0)
            {
                EnemyText.gameObject.SetActive(true);           
                var enemyCountdown = Mathf.Clamp(Mathf.CeilToInt(
                    _totalNumberOfEnemies * (_enemiesToDefeatPercentage * 0.01f)), 1, _totalNumberOfEnemies) - EnemyManager.EnemiesDefeated;
                EnemyText.text = enemyCountdown > 0 ? $"Target Enemies: {enemyCountdown}" : $"Target Enemies: COMPLETED";

                if (Mathf.CeilToInt(_totalNumberOfEnemies * (_enemiesToDefeatPercentage * 0.01f)) <= EnemyManager.EnemiesDefeated)
                {
                    InstantiatedGoal.GetComponent<Goal>().EnemyWincon = true;
                }
            }
            else
            {
                EnemyText.text = "";
                EnemyText.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Displays text and enables/disables ability to win with coins collected
    /// </summary>
    public void CoinWinCondition()
    {
        _totalNumberOfCoins = CoinManager.CoinList.Count;

        if (_currentScene.name != "A Scene")
        {
            _coinsToCollectPercentage = int.Parse(CoinField.text);

            if (CoinWincon.isOn && CoinPreferenceSlider.value > 0)
            {
                var coinCountdown = Mathf.Clamp(Mathf.CeilToInt(
                    _totalNumberOfCoins * (_coinsToCollectPercentage * 0.01f)), 1, _totalNumberOfCoins) - CoinManager.CoinsCollected;
                CoinText.gameObject.SetActive(true);
                CoinText.text = coinCountdown > 0 ? $"Target Coins: {coinCountdown}" : $"Target Coins: COMPLETED";

                if (Mathf.CeilToInt(_totalNumberOfCoins * (_coinsToCollectPercentage * 0.01f)) <= CoinManager.CoinsCollected)
                {
                    Debug.Log("Coin Wincon Completed!!");
                    InstantiatedGoal.GetComponent<Goal>().CoinWincon = true;
                }
            }
            else
            {
                CoinText.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// Determines how large the level will be and sets up confines for camera and player
    /// </summary>
    private void SetLevelSize()
    {
        var points = CameraBounds.points;

        if (Small.isOn)
        {            
            LevelWidth = 50;
            _rightBounds.position = new Vector2(LevelWidth + 3, _rightBounds.position.y);
        }
        else if (Medium.isOn)
        {
            LevelWidth = 100;
            _rightBounds.position = new Vector2(LevelWidth + 3, _rightBounds.position.y);
        }
        else if (Large.isOn)
        {
            LevelWidth = 150;
            _rightBounds.position = new Vector2(LevelWidth + 3, _rightBounds.position.y);
        }

        SetCameraBounds(points);
    }
    /// <summary>
    /// Setup any win conditions set by the player
    /// </summary>
    private void SetWincons()
    {
        if (EnemyWincon.isOn)
        {           
           try
           {
               _enemiesToDefeat = int.Parse(EnemyField.text);
           }
           catch (System.Exception)
           {
               Debug.Log("Must be a whole numerical value (EG. 50)");
               throw;
           }
                  
           _totalNumberOfEnemies = EnemyManager.KillableEnemies.Count;                      
        }
        EnemyWinCondition();

        if (CoinWincon.isOn)
        {
           try
           {
               _coinsToCollect = int.Parse(CoinField.text);
           }
           catch (System.Exception)
           {
               Debug.Log("Must be a whole numerical value (EG. 50)");
               throw;
           }

           _totalNumberOfCoins = CoinManager.CoinList.Count;                     
        }
        CoinWinCondition();
    }
    /// <summary>
    /// Set area which camera is confined within
    /// </summary>
    /// <param name="points"></param>
    private void SetCameraBounds(Vector2[] points)
    {
        points[0].Set(-1.4f, -0.5f);
        points[1].Set(-1.4f, 10f);
        points[2].Set(LevelWidth + 4.6f, 10);
        points[3].Set(LevelWidth + 4.6f, -0.5f);
        CameraBounds.points = points;
    }
    /// <summary>
    /// Instantiate a new player after a period of time
    /// </summary>
    /// <returns></returns>
    public IEnumerator Respawn(TypeOfDeath typeOfDeath)
    {
        DisablePlayer();
        _totalDeathsCounter++;

        switch (typeOfDeath)
        {
            case TypeOfDeath.hole:
                _holeDeathsCounter++;
                break;
            case TypeOfDeath.enemy:
                _enemyDeathsCounter++;
                break;
        }

        yield return new WaitForSeconds(_respawnTime);
        Destroy(InstantiatedPlayer);
        InstantiatedPlayer = Instantiate(_player);
        InstantiatedPlayer.transform.position = new Vector3(DeathPosition.x, 5, DeathPosition.z);
        
    }
    /// <summary>
    /// Disable all elements of the player (beside the camera)
    /// </summary>
    private void DisablePlayer()
    {
        InstantiatedPlayer.GetComponent<SpriteRenderer>().enabled = false;
        InstantiatedPlayer.GetComponent<PlayerController>().enabled = false;
        InstantiatedPlayer.GetComponent<CapsuleCollider2D>().enabled = false;
        InstantiatedPlayer.transform.Find("Side Collision").gameObject.SetActive(false);
        InstantiatedPlayer.transform.Find("Goomba Detection").gameObject.SetActive(false);
        Destroy(InstantiatedPlayer.GetComponent<Rigidbody2D>());
    }
    /// <summary>
    /// Instantiate the Goal and set to correct position
    /// </summary>
    private void GoalInitialisation()
    {
        InstantiatedGoal = Instantiate(_goal);
        InstantiatedGoal.transform.position = new Vector2(TerrainManager.TileList.Count + 1, 3f);
    }

    void ErrorMessages()
    {
        if(EnemyWincon.isOn && EnemyPreferenceSlider.value <= 0)
        {
            Debug.Log("Can't defeat enemies which don't exist!");
        }
    }

    public void ToggleMenu(bool boo)
    {
        LevelComplete = boo;       

        if (_currentScene.name != "A Scene")
        {
            //LevelComplete = boo;
            _optionsMenu.SetActive(boo);
            _optionsBackground.SetActive(boo);
            LevelCompleteText.SetActive(boo);

            var coinPercentage = CoinWincon.isOn ? _coinsToCollectPercentage : 0;
            var coinTarget = CoinWincon.isOn ? Mathf.CeilToInt(_totalNumberOfCoins * (_coinsToCollectPercentage * 0.01f)) : 0;
            var enemyPercentage = EnemyWincon.isOn ? _enemiesToDefeatPercentage : 0;
            var enemyTarget = EnemyWincon.isOn ? Mathf.CeilToInt(_totalNumberOfEnemies * (_enemiesToDefeatPercentage * 0.01f)) : 0;

            if (boo)
            {                
                CSVWriter.WriteCSV(_levelNumber, ScoreManager.CurrentScore, _totalDeathsCounter, _holeDeathsCounter, _enemyDeathsCounter,
                    (int)CoinPreferenceSlider.value - 5, CoinManager.CoinList.Count, CoinManager.CoinsCollected,
                    (int)EnemyPreferenceSlider.value - 4, EnemyManager.KillableEnemies.Count, EnemyManager.EnemiesDefeated,
                    (int)ObstaclePreferenceSlider.value - 5, (int)TerrainPreferenceSlider.value - 9, (int)GapWidthSlider.value - 2);

                if (_levelNumber == 1)
                {
                    LoadMenu();
                }
            }
        }
        else
        {            
            CSVWriter.WriteShortCSV(ScoreManager.CurrentScore, _totalDeathsCounter, _holeDeathsCounter, _enemyDeathsCounter, CoinManager.CoinsCollected, EnemyManager.EnemiesDefeated);
            LoadMenu();
        }        
    }

    void LoadMenu()
    {        
        SceneManager.LoadScene("Menu");        
    }
}