using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSayings : MonoBehaviour
{
    [SerializeField]
    private string[] _sayings;
    [SerializeField]
    private Text _text;

    void Start()
    {
        _text.text = _sayings[Random.Range(0, _sayings.Length)];
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(1);
    }
}
