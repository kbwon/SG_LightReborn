using UnityEngine;
using UnityEngine.SceneManagement;

public  class GameSceneManager : MonoBehaviour
{

    public static GameSceneManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadScene(string sceneName)
    {
        Debug.Log($"📥 씬 전환: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadEscapeScene()
    {
        GameManager.Instance.SetPhase(GamePhase.GameOver);
        LoadScene("EscapeScene");
    }

    public  void LoadDeadEndingScene()
    {
        GameManager.Instance.SetPhase(GamePhase.GameOver);
        LoadScene("DeadEndingScene");
        Debug.Log("안녕");
    }

    public  void LoadHiddenEndingScene()
    {
        GameManager.Instance.SetPhase(GamePhase.GameOver);
        LoadScene("HiddenEndingScene");
    }

    public  void LoadTitleScene()
    {
        GameManager.Instance.SetPhase(GamePhase.Title);
        LoadScene("TitleScene");
    }

    public void LoadMainGameScene()
    {
        GameManager.Instance.SetPhase(GamePhase.Ingame);
        LoadScene("MainGameScene");
    }
}
