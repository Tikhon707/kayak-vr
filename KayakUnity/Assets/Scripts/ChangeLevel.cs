using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLevel : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private string sceneName;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SceneManager.LoadScene(sceneName);
    }
}