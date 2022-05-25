using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneMove : MonoBehaviour
{
    
    // Start is called before the first frame update
    public Detail dt;
    public string d_name = "empty";
    void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        { 
            if (SceneManager.GetActiveScene().name == "AR")
            {
                SceneManager.LoadScene("LoadingMap");
            }
            else if (SceneManager.GetActiveScene().name == "Map")
            {
                Application.Quit();
            }
            else if (SceneManager.GetActiveScene().name == "Collections")
            {
                SceneManager.LoadScene("LoadingMap");
            }
            else if (SceneManager.GetActiveScene().name == "Detail")
            {
                TransDogam();
            }
            
        }
    }
    // Update is called once per frame
    public void TransAR()
    {
        SceneManager.LoadScene("LoadingAR");
    }

    public void TransMap()
    {
        SceneManager.LoadScene("Map");
    }

    public void TransDogam()
    {
        GameObject Select = GameObject.Find("Select");
        d_name = "empty";
        if (SceneManager.GetActiveScene().name == "Detail" && Select != null)
        {
            Destroy(Select);
        }
        SceneManager.LoadScene("Collections");
    }
    public void TransDetail()
    {
        GameObject Select = GameObject.Find("Select");
        SceneManager.LoadScene("Detail");
        DontDestroyOnLoad(Select);
    }
    public void TransDetailNext()
    {
        int a = (dt.D_NO + 1) % 21;
        if (a == 0) a = 1;
        SceneMove b = GameObject.Find("Select").GetComponent<SceneMove>();
        if (a < 10) b.d_name = "NO.0" + a;
        else b.d_name = "NO." + a;
        SceneManager.LoadScene("Detail");
    }
    public void TransDetailBack()
    {
        int a = dt.D_NO -1;
        if (a == 0) a = 20;
        SceneMove b = GameObject.Find("Select").GetComponent<SceneMove>();
        if (a < 10) b.d_name = "NO.0" + a;
        else b.d_name = "NO." + a;
        SceneManager.LoadScene("Detail");
    }
    public void TransPaint()
    {
        SceneManager.LoadScene("Paint");
    }
    
}
