using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager obj;                //this will create a static object which can be used from any script

    public GameObject weaponOverheatedMessage;
    public Slider tempSlider;
    public GameObject deathScreen;
    public Text deathText;
    public Text healthText;
    public Text KDtext;
    public GameObject gotHitScreen;
    public GameObject leaderBoard;
    public LeaderBoard leaderBoardPlayerDisplay;
    public GameObject endScreenImage;
    public Text timeText;
    public GameObject optionsScreen;
    public GameObject sniperScope;
    public GameObject crosshair;
    public Slider normalSensitivitySlider;
    public Slider adsSensitivitySlider;
    public GameObject mvpText;
    public Slider musicSlider;
    public FixedJoystick joystick;

    public Button shootButton;
    public Button jumpButton;
    public Button adsButton;

    private void Awake()                        //awake is called before start
    {
        obj = this;                             //that static object will contain this object's instance
    }

    private void Start()
    {
        adsButton.onClick.AddListener(ADS);
        adsSensitivitySlider.value = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
        if(optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!optionsScreen.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (optionsScreen.activeInHierarchy)
        {
            crosshair.SetActive(false);
            sniperScope.SetActive(false);
            Camera.main.fieldOfView = 60f;
        }
        else
        {
            crosshair.SetActive(true);
        }

        if (endScreenImage.activeInHierarchy)
        {
            optionsScreen.SetActive(false);
        }

        if (deathScreen.activeInHierarchy)
        {
            sniperScope.SetActive(false);               
        }

    }

    public void GotHit()
    {
        StartCoroutine(GotHitEffect());
    }

    IEnumerator GotHitEffect()
    {
        UIManager.obj.gotHitScreen.SetActive(true);
        UIManager.obj.gotHitScreen.GetComponent<Image>().CrossFadeAlpha(0, 2.0f, false);
        yield return new WaitForSeconds(2.0f);
        UIManager.obj.gotHitScreen.SetActive(false);
        var color = UIManager.obj.gotHitScreen.GetComponent<Image>().color;
        color.a = 0.5f;
    }

    void ADS()
    {
        if (!sniperScope.activeInHierarchy)
        {
            sniperScope.SetActive(true);
        }
        else
        {
            sniperScope.SetActive(false);
        }
    }

    public void ShowHideOptions()
    {
        if (!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
        }
        else
        {
            optionsScreen.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
}
