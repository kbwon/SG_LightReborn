using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityStandardAssets.CrossPlatformInput; // CrossPlatformInput 쓰는 경우만 활성화

public class ForcedReset : MonoBehaviour
{
    void Update()
    {
        // 예시: 스페이스바를 누르면 씬을 다시 로드함
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 현재 씬 다시 로드
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }
    }
}
