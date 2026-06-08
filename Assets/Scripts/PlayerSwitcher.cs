using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher Instance;

    [Header("Inputs (Nouveau Système)")]
    public InputActionReference switchPlayerAction;

    [Header("Personnages")]
    public CharacterController3D character1;
    public CharacterController3D character2;

    private CharacterController3D activeCharacter;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        switchPlayerAction?.action.Enable();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        switchPlayerAction?.action.Disable();
    }

    void Start() { InitializePlayers(); }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) return;
        InitializePlayers();
    }

    void InitializePlayers()
    {
        GameObject p1 = GameObject.FindWithTag("Player1");
        GameObject p2 = GameObject.FindWithTag("Player2");

        if (p1 != null && p2 != null)
        {
            character1 = p1.GetComponent<CharacterController3D>();
            character2 = p2.GetComponent<CharacterController3D>();

            activeCharacter = character1;
            character1.isCurrentPlayer = true;
            character2.isCurrentPlayer = false;
        }
    }

    void Update()
    {
        if (activeCharacter == null) return;

        if (switchPlayerAction.action.WasPressedThisFrame())
        {
            SwitchCharacter();
        }
    }

    void SwitchCharacter()
    {
        if (character1 == null || character2 == null) return;

        if (activeCharacter == character1)
        {
            character1.isCurrentPlayer = false;
            character2.isCurrentPlayer = true;
            activeCharacter = character2;
        }
        else
        {
            character2.isCurrentPlayer = false;
            character1.isCurrentPlayer = true;
            activeCharacter = character1;
        }
    }
}