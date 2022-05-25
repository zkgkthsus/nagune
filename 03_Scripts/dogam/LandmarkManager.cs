using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;


public class LandmarkManager : MonoBehaviour
{   
    public SceneMove sm;
    public GameObject landmark;
    public Transform _landmarkParent;

    public FirebaseFirestore db;
    FirebaseAuth auth = null;

    Sprite[] sprites;
    
    List<GameObject> _landmarkList = new List<GameObject>();

    


    

    void Start()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        sprites = Resources.LoadAll<Sprite>("icon");
        arr_landmarkInit();
    }

    void arr_landmarkInit(){
        String useremail = auth.CurrentUser.Email;
        CollectionReference LandmarkRef = db.Collection("User").Document(useremail).Collection("Landmark");
        LandmarkRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot snapshot = task.Result;
            
            int cnt = 0;
            foreach(DocumentSnapshot document in snapshot.Documents)
            {
                cnt += 1;
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                Boolean L_isVisited = (Boolean)documentDictionary["isVisited"];
                // print(Landmark_Name);
                GameObject _obj = Instantiate(landmark) as GameObject;
                _obj.transform.SetParent(_landmarkParent.transform, false);
                Image spriteR = _obj.GetComponent<Image>();
                Button button = _obj.GetComponent<Button>();
                Text buttonText = button.transform.GetChild(0).GetComponent<Text>();
                buttonText.text = "";
                Sprite known_L =Array.Find(sprites, x => x.name == "NO." + cnt);
                Sprite unknown_L =Array.Find(sprites, x => x.name == "NO." + cnt + "_veiled");
                if (cnt < 10) _obj.name = "NO.0"+ cnt;
                else _obj.name = "NO."+ cnt;
                button.onClick.AddListener(dataTake);
    
                
                if (L_isVisited)
                {
                    spriteR.sprite = known_L;
                }
                else
                {
                    spriteR.sprite = unknown_L;
                }
                
                _landmarkList.Add(_obj);
                
            
            }
        });
        
    }
    
    // Detail 씬으로 넘어가는 조건 확인
    public void dataTake(){
        sm.d_name = EventSystem.current.currentSelectedGameObject.name;
        if (sm.d_name != "empty")
        {
            sm.TransDetail();
        }
    }

}
