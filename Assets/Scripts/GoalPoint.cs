// Scene切替に必要
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour
{
    // 次に移動するScene名
    [SerializeField]
    private string nextSceneName;

    // Triggerへ何か入った時に呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 接触相手がPlayerなら
        if (other.CompareTag("Player"))
        {
            // 次Sceneを読み込む
            SceneManager.LoadScene(nextSceneName);
        }
    }
}