public class Reticule : MonoBehaviour
{
    public RectTransform reticule;
    public GameObject player;

    public static Reticule instance;

    public float defaultSize = 6; //reticule defaultSize
    public float maxSize = 10; //reticlules maxSize
    public float currentSize; //reticules currentSize 
    public float changeRate; //the rate at which the reticule changes size

    bool sizeIncreasing = true;
    public bool inRangeOfInteractable; //To communicate with other scripts

    RaycastHit hit; //to log Raycast collisions
    public int rayDistance = 3; //length of Raycast  

    private Flowchart flowchart;

    private void Start()
    {
        if (instance == null) // If no instances of this script exist
        {
            instance = this; // Refer to this script
        }

        reticule = GetComponent<RectTransform>(); //Grab reference
        flowchart = null; //Set bool
        inRangeOfInteractable = false; //Set bool
    }
    private void OnDestroy()
    {
        instance = null; // Remove all instances of script (When changing scenes)
    }

    private void Update()
    {
        isHitting();
    }
    /// <summary>
    /// Detects if the player is looking at an interactable object or not, causing reticule changes depending on whats being viewed
    /// </summary>
    void isHitting()
    {
        Vector3 rayForward = player.transform.TransformDirection(Vector3.forward); //Ray sent from players location (Forward)       

        if (Physics.Raycast(player.transform.position, rayForward, out hit, rayDistance)) //If a ray hits "something" infront of it (rayForward) within a certain distance (rayDistance) then log it (hit)
        {
            if (hit.transform.tag == "Interactable") //If we he something tagged as an Interactable            
            {
                inRangeOfInteractable = true; //Say we are looking at an interactable

                if (hit.collider.gameObject.GetComponent<Flowchart>() != null) //If the interactable object has a flowchart component
                {
                    flowchart = hit.collider.gameObject.GetComponent<Flowchart>(); //Get the flowchart component of the interactable                    

                    if (flowchart.GetIntegerVariable("interactableCount") >= 0) //if the fungus text hasn't reached its end
                    {
                        if (sizeIncreasing) //If reticule is getting larger
                        {
                            currentSize = Mathf.Lerp(currentSize, maxSize, Time.deltaTime * changeRate); //Increase its size up to maxSize over deltaTime 

                            if (currentSize > maxSize - 1) //If the recticule is larger than maxSize -1 in size
                            {
                                sizeIncreasing = false; //Start making the reticule smaller
                            }
                        }
                        else
                        {
                            currentSize = Mathf.Lerp(currentSize, defaultSize, Time.deltaTime * changeRate); //Decrease reticule size up to defaultSize over deltaTime 

                            if (currentSize < defaultSize + 1) //If the recticule is smaller than defaultSize +1 in size
                            {
                                sizeIncreasing = true; //Start making the reticule larger
                            }
                        }
                    }
                    flowchart.SetBooleanVariable("activate", true); //Set the bool on the flowchart to true to allow textflow

                    reticule.sizeDelta = new Vector2(currentSize, currentSize); // increase/decrease reticule size by currentSize
                    Debug.DrawRay(player.transform.position, rayForward * rayDistance, Color.green); //Green ray to show Interactable hit detection

                    if (flowchart.GetIntegerVariable("interactableCount") == -1) //Once the fungus text has reached the end
                    {
                        reticule.sizeDelta = new Vector2(defaultSize, defaultSize); //Reset reticule size
                    }
                }
            }
            else
            {
                reticule.sizeDelta = new Vector2(defaultSize, defaultSize); //Reset reticule size
            }
        }
        else
        {
            inRangeOfInteractable = false; //If we're not looking at an interactable thats in range

            currentSize = 6; //default size

            if (flowchart != null) //Will run after raycast has detected its first Interactable
            {
                flowchart.SetBooleanVariable("activate", false); //Set the bool on the flowchart to false to stop textflow 
            }
            reticule.sizeDelta = new Vector2(defaultSize, defaultSize); //Set default reticule
            Debug.DrawRay(player.transform.position, rayForward * rayDistance, Color.red); //Red ray to show no hit detection

        }
    }
}