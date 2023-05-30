public class MenuNavigation : MonoBehaviour
{
    [SerializeField] TreehouseInteractable _treehouseInteractable;
    [SerializeField] ObjectProperties.ItemSizes _itemSize;
    [SerializeField] ItemManager _itemManager;
    [SerializeField] MenuCall _menuCall;
    [SerializeField] TMP_Text _pageText;
    [SerializeField] GameObject[] _menuPanels;
    [SerializeField] GameObject _highlight;
    [SerializeField] int _rowLength;
    [SerializeField] int _columnHeight;
    GameObject[,] _menuArray;
    int _counter;
    [SerializeField] int _pageNumber = 0;
    int _xPosition = 0, _yPosition = 0;
    GameObject _selectedSpot;

    [SerializeField] public List<ItemDataSO> _itemList;
    List<ItemDataSO> _smallList = new List<ItemDataSO>();
    List<ItemDataSO> _mediumList = new List<ItemDataSO>();
    List<ItemDataSO> _largeList = new List<ItemDataSO>();
    ItemDataSO _inventorySO;

    [SerializeField] InputActionReference interact, up, down, left, right, exit, pageRight, pageLeft;

    // Start is called before the first frame update
    void Start()
    {
        _treehouseInteractable.OnItemSelectGO += GetItemSize;       

        //Reduce by one due to array starting from 0
        _rowLength--;
        _columnHeight--;
        _menuArray = new GameObject[_rowLength + 1, _columnHeight + 1];
        PopulateArray(pageNumber: _pageNumber);

        ///TODO: This currently pulls from our inventory but does not account
        ///for new items added (purchases from shop)
        PopulateLists();

        this.gameObject.SetActive(false);
    }
    /// <summary>
    /// Add small/medium/large items to their respective lists
    /// </summary>
    public void PopulateLists()
    {
        _smallList.Clear();
        _mediumList.Clear();
        _largeList.Clear();

        if (_itemList != null && _itemList.Count > 0)
        {
            foreach (var item in _itemList)
            {
                if (item.ItemSize == ObjectProperties.ItemSizes.Small)
                {
                    _smallList.Add(item);
                }
                else if (item.ItemSize == ObjectProperties.ItemSizes.Medium)
                {
                    _mediumList.Add(item);
                }
                else if (item.ItemSize == ObjectProperties.ItemSizes.Large)
                {
                    _largeList.Add(item);
                }
            }
        }
    }

    private void OnEnable()
    {
        //_treehouseInteractable.OnItemSelectGO += GetItemSize; 
    }

    private void OnDisable()
    {
        _treehouseInteractable.OnItemSelectGO -= GetItemSize;
    }

    private void Update()
    {
        _pageText.text = $"Page: {_pageNumber + 1}";
        ///TODO: Replace with input system when we know binds
        if (Input.anyKeyDown)
        {
            PopulateLists();
            UpdatePosition();
            ChangePage(_itemSize);
            SelectItem();
        }
    }

    /// <summary>
    /// Displays relevant items (Small/Large/Trophy) in the inventory 
    /// </summary>
    void DisplayItems()
    {
        ///Wipe displays before adding items
        for (int i = 1; i < _menuArray.Length; i++)
        {
            _menuPanels[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
            _menuPanels[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
        }

        ///Set top left panel as clear button
        if(_xPosition == 0 && _yPosition == 0)
        {
            //Display clear icon here
            _menuPanels[0].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
        }
        
        ///Display every item the player owns (limited to item size)
        switch (_itemSize)
        {
            case ObjectProperties.ItemSizes.Small:
                DisplayItem(_smallList);
                break;
            case ObjectProperties.ItemSizes.Medium:
               DisplayItem(_mediumList);
                break;
            case ObjectProperties.ItemSizes.Large:
                DisplayItem(_largeList);
                break;           
        }
    }

    private void DisplayItem(List <ItemDataSO> itemList)
    {
        int numberOfItems = itemList.Count >= (_pageNumber + 1) * (_menuArray.Length - 1) ? 
            _menuArray.Length : ((itemList.Count + (_pageNumber - 1)) % _menuArray.Length) + 2;

        Debug.Log($"No. of items {numberOfItems}");

        for (int i = 1, j = _pageNumber * (_menuArray.Length - 1); i < numberOfItems; i++, j++)
        {
            _menuPanels[i].transform.GetChild(0).gameObject.SetActive(true);                     
            _menuPanels[i].transform.GetChild(0).GetComponent<Image>().sprite = itemList[j].icon;
            
            //_menuPanels[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemList[j].displayName;
        }
    }

    /// <summary>
    /// Select highlighted item to spawn and reset selection
    /// </summary>
    void SelectItem()
    {
        //ItemDataSO currentSO;
        ItemDataSO itemSpotSO;

        if (/*Input.GetKeyDown(KeyCode.Return)*/ interact.action.triggered)
        {
            ///Clear button position -- removes display and adds to inventory
            if(_xPosition == 0 && _yPosition == 0)
            {
                if (_selectedSpot.GetComponentInChildren<ObjectUpdater>().DisplayedObjectSO != null)
                {
                    itemSpotSO = _selectedSpot.GetComponentInChildren<ObjectUpdater>().DisplayedObjectSO;
                    _itemList.Add(itemSpotSO);
                    _selectedSpot.GetComponentInChildren<ObjectUpdater>().SpawnOrRemoveDisplay(itemSpotSO, false);                   
                    PopulateLists();
                }

                for (int i = 1; i < _menuPanels.Length; i++)
                {
                    _menuPanels[i].transform.GetChild(0).gameObject.SetActive(false);
                }
                _pageNumber = 0;
                _menuCall.CloseMenu();
                return;
            }            
            switch (_itemSize)
            {
                case ObjectProperties.ItemSizes.Small:                  

                    _inventorySO = _smallList[(_yPosition * 3) + _xPosition + (_pageNumber * 9) - _pageNumber - 1];
                    break;
                case ObjectProperties.ItemSizes.Medium:

                    _inventorySO = _mediumList[(_yPosition * 3) + _xPosition + (_pageNumber * 9) - _pageNumber - 1];
                    break;
                case ObjectProperties.ItemSizes.Large:

                    _inventorySO = _largeList[(_yPosition * 3) + _xPosition + (_pageNumber * 9) - _pageNumber - 1];
                    break;                
            };

            ///If there is a displayed objece, remove the object from display and add to inventory
            if (_selectedSpot.GetComponentInChildren<ObjectUpdater>().DisplayedObjectSO != null)
            {
                itemSpotSO = _selectedSpot.GetComponentInChildren<ObjectUpdater>().DisplayedObjectSO;
                _itemList.Add(itemSpotSO);
                _selectedSpot.GetComponentInChildren<ObjectUpdater>().SpawnOrRemoveDisplay(itemSpotSO, false);
            }

            _selectedSpot.GetComponentInChildren<ObjectUpdater>().SpawnOrRemoveDisplay(_inventorySO, true);

            if (_itemList.Contains(_inventorySO))
            {                
                _itemList.Remove(_inventorySO);
            }         
           
            switch (_itemSize)
            {
                case ObjectProperties.ItemSizes.Small:
                    break;
                case ObjectProperties.ItemSizes.Medium:
                    break;
                case ObjectProperties.ItemSizes.Large:

                    if (_pageNumber > 0 && (_largeList.Count / (_pageNumber + 1)) < _menuArray.Length)
                    {                       
                        _pageNumber--;
                    }
                    break;
                default:
                    break;
            }

            PopulateLists();

            for (int i = 1; i < _menuPanels.Length; i++)
            {
                _menuPanels[i].transform.GetChild(0).gameObject.SetActive(false);
            }           

            _pageNumber = 0;
            _xPosition = 0;
            _yPosition = 0;
            _highlight.transform.position = _menuArray[_xPosition, _yPosition].transform.position;

            _menuCall.CloseMenu();
        }
    }

    /// <summary>
    /// Add inventory items to the array
    /// </summary>
    /// <param name="pageNumber"></param>
    void PopulateArray(int pageNumber)
    {
        _counter = 0;
        for (int y = 0; y <= _columnHeight; y++)
        {
            for (int x = 0; x <= _rowLength; x++)
            {
                _menuArray[x, y] = _menuPanels[_counter];
                //_menuArray[x, y] = _menuPanels[_counter + (pageNumber *_columnHeight * _rowLength)]; //Useful for when we're adding inventory items
                _counter++;
            }
        }
    }

    /// <summary>
    /// Highlights current square
    /// </summary>
    void UpdatePosition()
    {
        if (/*Input.GetKeyDown(KeyCode.RightArrow)*/ right.action.triggered) 
        {
            _xPosition++;

            if (_xPosition > _rowLength)
            {
                _xPosition = 0;
            }

            #region Next Line Code
            ////If we've exceeded the row (require new row) but we're in the last column
            ////Reset
            //if (_xPosition > _rowLength && _yPosition == _columnHeight)
            //{
            //    _xPosition = 0;
            //    _yPosition = 0;                
            //}
            ////If we've exceeded the row but there IS a new row to move too
            ////Move Row
            //else if (_xPosition > _rowLength)
            //{                               
            //    _xPosition = 0;
            //    _yPosition++;               
            //}
            #endregion
        }
        if (/*Input.GetKeyDown(KeyCode.LeftArrow)*/ left.action.triggered)
        {
            _xPosition--;

            if(_xPosition < 0)
            {
                _xPosition = _rowLength;
            }

            #region Previous Line Code
            ////If we've exceeded the row (require new row) but we're in the first column
            ////Reset
            //if (_xPosition < 0 && _yPosition == 0)
            //{
            //    _xPosition = _rowLength;
            //    _yPosition = _columnHeight;
            //}
            ////If we've exceeded the row but there IS a new row to move too
            ////Move Row
            //else if(_xPosition < 0)
            //{  
            //    _xPosition = _rowLength;
            //    _yPosition--;
            //}
            #endregion
        }
        if (/*Input.GetKeyDown(KeyCode.UpArrow)*/ up.action.triggered)
        {
            _yPosition--;

            //If we're at the top of the column, go back to the bottom
            if (_yPosition < 0)
            {
                _yPosition = _columnHeight;
            }
        }
        if (/*Input.GetKeyDown(KeyCode.DownArrow)*/ down.action.triggered)
        {
            _yPosition++;

            //If we're at the bottom of the column, go back to the top
            if (_yPosition > _columnHeight)
            {
                _yPosition = 0;
            }
        }
        _highlight.transform.position = _menuArray[_xPosition, _yPosition].transform.position;        
    }
    /// <summary>
    /// Allows switching between pages provided there are items to show on the page
    /// </summary>
    /// <param name="itemSizes"></param>
    void ChangePage(ObjectProperties.ItemSizes itemSizes)
    {
        bool canChangePage = false;

        switch (_itemSize)
        {
            case ObjectProperties.ItemSizes.Small:
                if(_smallList.Count >= (_pageNumber * (_menuArray.Length - 1)) + _menuArray.Length)
                {
                    canChangePage = true;
                }
                break;
            case ObjectProperties.ItemSizes.Medium:
                if (_mediumList.Count >= (_pageNumber * (_menuArray.Length - 1)) + _menuArray.Length)
                {
                    canChangePage = true;
                }
                break;
            case ObjectProperties.ItemSizes.Large:
                if (_largeList.Count >= (_pageNumber * (_menuArray.Length - 1)) + _menuArray.Length)
                {
                    canChangePage = true;
                }
                break;
            default:
                break;
        }

        if(/*Input.GetKeyDown(KeyCode.E)*/ pageRight.action.triggered && canChangePage)
        {
            _pageNumber++;
            //These don't need to be reset if we want to retain highlighted square position
            _xPosition = 0;
            _yPosition = 0;
            _highlight.transform.position = _menuArray[_xPosition, _yPosition].transform.position;
        }
        if (/*Input.GetKeyDown(KeyCode.Q)*/ pageLeft.action.triggered && _pageNumber > 0)
        {
            _pageNumber--;
            //These don't need to be reset if we want to retain highlighted square position
            _xPosition = 0;
            _yPosition = 0;
            _highlight.transform.position = _menuArray[_xPosition, _yPosition].transform.position;            
        }

        for (int i = 1; i < _menuPanels.Length; i++)
        {
            _menuPanels[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        DisplayItems();
    }

    /// <summary>
    /// Tells our script which inventory menu to display
    /// </summary>
    /// <param name="item"></param>
    public void GetItemSize(GameObject item)
    {
        _selectedSpot = item;
        _itemSize = item.GetComponent<ObjectProperties>().ItemSize;
        DisplayItems();
    }
}