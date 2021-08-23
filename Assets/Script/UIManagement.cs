using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagement : MonoBehaviour
{
    GameObject ScoreText;
    GameObject PauseUI;
    GameObject GameStartUI;
    GameObject Image;
    GameObject SuccessText;
    GameObject FailText;
    GameObject RestattText;
    List<GameObject> uiList = new List<GameObject>();

    private void Awake()
    {
        ScoreText = transform.Find("ScoreText").gameObject;
        PauseUI = transform.Find("PauseUI").gameObject;
        GameStartUI = transform.Find("GameStartUI").gameObject;
        Image = transform.Find("Image").gameObject;
        SuccessText = transform.Find("SuccessText").gameObject;
        FailText = transform.Find("FailText").gameObject;
        RestattText = transform.Find("RestartText").gameObject;
        uiList.Add(ScoreText);
        uiList.Add(PauseUI);
        uiList.Add(GameStartUI);
        uiList.Add(Image);
        uiList.Add(SuccessText);
        uiList.Add(FailText);
        uiList.Add(RestattText);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ÐÝÃßËùÓÐUI
    void DisableAllUI()
    {
        foreach(GameObject obj in uiList)
        {
            obj.SetActive(false);
        }
    }

    public void StartGame()
    {
        DisableAllUI();
        GameStartUI.SetActive(true);
        Image.SetActive(true);
    }

    public void EnterGame()
    {
        DisableAllUI();
        ScoreText.SetActive(true);
        RestattText.SetActive(true);
    }

    public void PauseGame()
    {
        PauseUI.SetActive(true);
    }

    public void ContinueGame()
    {
        PauseUI.SetActive(false);
    }


    public void UpdateScore(int score)
    {
        string s = string.Format("SCORE : {0:0000}", score);
        ScoreText.GetComponent<Text>().text = s;
    }

    public void Fail()
    {
        DisableAllUI();
        Image.SetActive(true);
        FailText.SetActive(true);
        RestattText.SetActive(true);
    }

    public void Win()
    {
        DisableAllUI();
        Image.SetActive(true);
        SuccessText.SetActive(true);
        RestattText.SetActive(true);
    }
}
