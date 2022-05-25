using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class TouchEvent : MonoBehaviour
{
    public Camera getCamera;    
    public GameObject Panel;
    public FirebaseFirestore db;
    bool PanelCheck = false;
    NoticeUI _notice;

    

    float directionAR;
    FirebaseAuth auth = null;
    // Start is called before the first frame update

    private void Awake() 
    {
     _notice = FindObjectOfType<NoticeUI>();   
    }
    void Start()
    {
        
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(Camera.main.transform.position,Camera.main.transform.forward);
            RaycastHit hit = new RaycastHit();
            if(Physics.Raycast(ray, out hit))
            {
     
                float distance = Vector3.Distance(Camera.main.transform.position, hit.transform.position);
                if (distance < 30 && hit.transform.gameObject.name.Contains("NO.") == true)
                {
                    
                    dbUpate(hit.transform.gameObject.name);
                }
                else if((hit.transform.gameObject.name == "NO.11" || hit.transform.gameObject.name == "NO.13") && distance < 30) dbUpate(hit.transform.gameObject.name);
                else if(hit.transform.gameObject.name.Contains("NO.") == true && distance >= 30) distanceCancel();

            }
        }

        
    }
    public void dbUpate(string target)
  {
    Panel.SetActive(true);
    PanelCheck = true;
    RectTransform PanelPosition = Panel.GetComponent<RectTransform>();
    PanelPosition.SetAsLastSibling();
    String useremail = auth.CurrentUser.Email;
    DocumentReference randmarkRef = db.Collection("User").Document(useremail).Collection("Landmark").Document(target);
    db.RunTransactionAsync(transaction =>
    {
        return transaction.GetSnapshotAsync(randmarkRef).ContinueWith((snapshotTask)=>
        {
            DocumentSnapshot snapshot = snapshotTask.Result;
            Boolean visited = true;
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                {"isVisited", visited}
            };
            transaction.Update(randmarkRef,updates);
        });
    });
  }
  public void ButtonO ()
  {
      SceneManager.LoadScene("LoadingDogam");
  }

  public void ButtonX()
  {
      Panel.SetActive(false);
      PanelCheck = false;
  }

 public void distanceCancel()
 {
     _notice.SUB("거리가 부족합니다");
 }
}

