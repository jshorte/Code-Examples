public class CoinManager : MonoBehaviour
{
    //Cache
    [SerializeField] GameManager _gameManager;
    [SerializeField] GameObject _coin;

    //Initialise
    public List<GameObject> CoinList = new List<GameObject>();
    List<GameObject> _spawnedCoins = new List<GameObject>();

    //Counters
    const float COINFREQUENCYMULTIPLIER = 0.35f; //Multipied to _enemyFrequencyPreference for ratio of possible spawn locations to enemies 
    public int SpawnedTerrain; //Amount of terrain spawned (Used for determining available tiles for spawning enemies ect)
    public int CoinsCollected = 0;
    int _coinFrequencyPreference; //User defined value for how frequency of enemies (Range 0 to 10)   
    int _levelWidth;
    bool _forceSpawn;

    /// <summary>
    /// Spawn enemies reflected by players preference onto generated terrain
    /// </summary>
    public void SpawnCoins()
    {
        _levelWidth = _gameManager.LevelWidth;
        var tileList = _gameManager.ObstacleManager.ObstacleTileList;

        ResetCoinCollection();

        _coinFrequencyPreference = (int)_gameManager.CoinPreferenceSlider.value;

        int remainingTiles = 0;

        for (int i = 0; i < tileList.Count; i++)
        {            
            int remainingAttempts = tileList.Count - remainingTiles; //How many tiles remain before we reach the end of the level
            float coinFrequencyPreference = tileList.Count * (_coinFrequencyPreference * 0.1f * COINFREQUENCYMULTIPLIER);
            float spawnChance = (coinFrequencyPreference - _spawnedCoins.Count) / remainingAttempts * 100;
            float spawnChanceModifier = ((i / 20) % 2) == 1 ? 1.25f : 0.75f; //Modifiers spawn chance by +/- 25% every 20 tiles
            float random = Random.Range(0, 101);
            bool success = (spawnChance * spawnChanceModifier) > random;

            remainingTiles++;

            //Spawn an enemy on the terrain on successful roll
            if (success || _forceSpawn)
            {                
                GameObject spawnedCoin;
                Debug.Log("Tile Type: " + tileList[i]);

                if (tileList[i] == TerrainManager.TileType.Gap)
                {
                    spawnedCoin = Instantiate(_coin, new Vector2(i, (_gameManager.JumpHeightSlider.value * 0.5f) - 1f), Quaternion.identity);
                    _spawnedCoins.Add(spawnedCoin);
                    CoinList.Add(spawnedCoin);
                }
                else if (tileList[i] == TerrainManager.TileType.Terrain)
                {
                    float randomHeight = Random.Range(1, 3) + 0.5f;
                    spawnedCoin = Instantiate(_coin, new Vector2(i, randomHeight), Quaternion.identity);
                    _spawnedCoins.Add(spawnedCoin);
                    CoinList.Add(spawnedCoin);
                }
                else if (tileList[i] == TerrainManager.TileType.Obstacle)
                {
                    //KVP (Position & Height) + Jumpheight
                    spawnedCoin = Instantiate(_coin, new Vector2(
                        i, _gameManager.ObstacleManager.ObstaclePositions[i] + ((
                        _gameManager.JumpHeightSlider.value * 0.5f) - 1f)),
                        Quaternion.identity);
                    _spawnedCoins.Add(spawnedCoin);
                    CoinList.Add(spawnedCoin);
                }                
                _forceSpawn = false;                
            }
            else if (success)
            {
                _forceSpawn = true;
            }
        }
    }

    private void ResetCoinCollection()
    {
        foreach (var item in _spawnedCoins)
        {
            Destroy(item);
        }

        _spawnedCoins.Clear();
        CoinList.Clear();       
        CoinsCollected = 0;
    }
}