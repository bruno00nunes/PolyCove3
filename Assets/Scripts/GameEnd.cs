using System;
using System.Collections;
using TMPro;
using UnitScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
    public Unit boss;
    private bool _startCameraCheck;
    public GameObject endGameScreen;
    public TextMeshProUGUI endGameText;
    public Button escapeButton;

    private void Update()
    {
        if (boss) return;
        if (_startCameraCheck) return;
        transform.GetChild(0).gameObject.SetActive(true);
        _startCameraCheck = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_startCameraCheck) return;
        if (!other.CompareTag("MainCamera")) return;
        endGameScreen.SetActive(true);
        InputManager.Instance.FreezeControls(true);
        StartCoroutine(AnimateText("Congratulations, you finally defeated the AI and opened a" +
                                   " way baaaa... ck to fR#i$do1m ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE" +
                                   " ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE ESCAPE"));
    }
    
    private IEnumerator AnimateText(string strComplete){
        var i = 0;
        endGameText.text = "";
        var waitTime = 0.075f;
        
        while( i < strComplete.Length ){
            switch (i)
            {
                case 46:
                    ChangeTextColor(Color.red);
                    break;
                case 50:
                    ChangeTextColor(Color.white);
                    break;
                case 64:
                    ChangeTextColor(Color.red);
                    break;
                case 72:
                    ChangeTextColor(Color.white);
                    break;
                case 78:
                    ChangeTextColor(Color.red);
                    waitTime = 0.015f;
                    break;
                case 90:
                    ShowEscapeButton();
                    break;
            }

            endGameText.text += strComplete[i++];
            
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void ShowEscapeButton()
    {
        escapeButton.gameObject.SetActive(true);
    }

    private void ChangeTextColor(Color color){
        endGameText.color = color;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Credits");
    }
}
