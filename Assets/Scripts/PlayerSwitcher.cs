using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher Instance;

    [Header("Personnages (Se remplissent tout seuls)")]
    public CharacterController3D character1;
    public CharacterController3D character2;

    private CharacterController3D activeCharacter;

    void Awake()
    {
        // Système de Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // PUNCHLINE : On force la recherche des joueurs dès le premier démarrage du jeu !
        InitializePlayers();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) return; // On ignore le menu principal

        InitializePlayers();
    }

    // Fonction unique pour trouver et activer les personnages
    void InitializePlayers()
    {
        GameObject p1 = GameObject.FindWithTag("Player1");
        GameObject p2 = GameObject.FindWithTag("Player2");

        if (p1 != null && p2 != null)
        {
            character1 = p1.GetComponent<CharacterController3D>();
            character2 = p2.GetComponent<CharacterController3D>();

            // On définit le personnage 1 comme actif par défaut
            activeCharacter = character1;
            character1.isCurrentPlayer = true;
            character2.isCurrentPlayer = false;

            Debug.Log("Joueurs initialisés avec succès ! Joueur actif : " + activeCharacter.gameObject.name);
        }
        else
        {
            Debug.LogWarning("PlayerSwitcher : Impossible de trouver 'Player1' ou 'Player2' dans la scène actuelle.");
        }
    }

    void Update()
    {
        if (activeCharacter == null) return;

        // Touche Tab pour switcher de perso
        if (Input.GetKeyDown(KeyCode.Tab))
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

        Debug.Log("Switch effectué ! Personnage actif : " + activeCharacter.gameObject.name);
    }
}