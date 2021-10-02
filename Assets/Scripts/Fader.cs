using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public GameObject RestartButton;

    public GameObject FaderBackground;
    public Image FaderImage;
    public Animation faderAnimation;
    public AnimationClip fadeInAnim;

    public void StartFadeIn()
    {
        FindObjectOfType<PlayerController>().canControlPlayer = false;
        FaderBackground.SetActive(true);
        faderAnimation.clip = fadeInAnim;
        faderAnimation.Play();
        Invoke("ShowButton", 1f);
    }

    private void ShowButton()
    {
        RestartButton.SetActive(true);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(0);
    }
}
