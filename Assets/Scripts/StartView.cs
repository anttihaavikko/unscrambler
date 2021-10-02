using UnityEngine;

public class StartView : MonoBehaviour
{
    public void StarGame()
    {
        SceneChanger.Instance.ChangeScene("Main");
    }

    public void Quit()
    {
        Application.Quit();
    }
}