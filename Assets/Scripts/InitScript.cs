using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InitScript : MonoBehaviour
{
    public static InitScript Instance { get; private set; }
    public Canvas canvas;
    public EventSystem eventSystem;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        DontDestroyOnLoad(canvas);
        DontDestroyOnLoad(eventSystem);
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            LancerLeMenu();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LancerLeMenu()
    {
        SceneManager.LoadScene(1);
    }
}
