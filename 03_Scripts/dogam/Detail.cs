using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class Detail : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject Select;

    SceneMove sm;
    string nameObject;
    public FirebaseFirestore db;

    public Text Name;
    public Text Latitude;
    public Text Longitude;
    public Text Details;
    public Text City;
    public Text Type;
    public Text NO;
    public Image Symbol;
    String L_NO;
    public int D_NO;
    
    FirebaseAuth auth = null;
    Sprite[] sprites;

    void Start()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        sprites = Resources.LoadAll<Sprite>("icon");
        Select = GameObject.Find("Select");
        nameObject = Select.GetComponent<SceneMove>().d_name;
        print(nameObject);
        Detail_l();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Detail_l()
    {
    String useremail = auth.CurrentUser.Email;
    DocumentReference LandmarkRef = db.Collection("User").Document(useremail).Collection("Landmark").Document(nameObject);
    LandmarkRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
        
        // document.Id    => String
        Dictionary<string, object> documentDictionary = task.Result.ToDictionary();
        String L_detail = (String)documentDictionary["Detail"];
        String L_name = (String)documentDictionary["Name"];
        String L_city = (String)documentDictionary["city"];
        String L_latitude = (String)documentDictionary["latitude"].ToString();
        String L_longitude = (String)documentDictionary["longitude"].ToString();
        Boolean L_isVisited = (Boolean)documentDictionary["isVisited"];
        L_NO = (String)documentDictionary["NO"].ToString();
        String L_Type = (String)documentDictionary["Type"];
        D_NO = Convert.ToInt32(L_NO);
        Sprite known_L =Array.Find(sprites, x => x.name == "NO." + D_NO);
        Sprite unknown_L =Array.Find(sprites, x => x.name == "NO." + D_NO + "_veiled");
        Latitude.text = L_latitude;
        Longitude.text = L_longitude;
        Symbol.color = new Color32(255,255,255,255);
        if (L_isVisited)
        {
            Symbol.sprite = known_L;
            Name.text = L_name;
            City.text = L_city;
            Details.text = L_detail;
            NO.text = "NO."+L_NO;
            Type.text = L_Type;
        }
        else
        {
            Name.text = "이름";
            Symbol.sprite = unknown_L;
        }
    });
    }
   
}
