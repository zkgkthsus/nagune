using UnityEngine;
using System.Collections;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;
// For uGUI event system
using UnityEngine.EventSystems;
using UnityEngine.Android;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

// For uGUI components
using UnityEngine.UI;

// For "List"
using System.Collections.Generic;

public class EP_DrawPadRT : MonoBehaviour
{

     // easy print asset
    // 저작권 관련 제거
    // easy print asset
    public FirebaseFirestore db;
    public FirebaseStorage storage;

    FirebaseAuth auth = null;

    String latitude_text;
    String longitude_text;
    public float maxWaitTime = 10.0f;
    public float resendTime = 1.0f;

    //  GPS 관련 인자들
    public Double latitude = 0;
    public Double longitude = 0;
	// public float altitude = 0;

    

    bool receiveGPS = false;
    float waitTime = 0;
    void Start()
    {



        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        // easy print asset
        // 저작권 관련 제거
        // easy print asset

        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;


    }

    private String time = "";

    public float yRotation = 5.0f;
    void Update()
    {
    // easy print asset
    // 저작권 관련 제거
    // easy print asset

        GetCurrentDate();

    }



    public IEnumerator GPS_On()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                yield return null;
            }
        }
        if (!Input.location.isEnabledByUser)
        {
            latitude_text = "GPS off";
            longitude_text = "GPS off";
            yield break;
        }
        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(1.0f);
            waitTime++;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            // latitude_text = "위도 실패";
            // longitude_text = "경도 실패";
            
        }
        if (waitTime >= maxWaitTime)
        {
            // latitude_text = "위도 시간 초과";
            // longitude_text = "경도 시간 초과";
        }
        LocationInfo li = Input.location.lastData;
        
        
        latitude = li.latitude;
        longitude = li.longitude;
        receiveGPS = true;

        while (receiveGPS)
        {
            yield return new WaitForSeconds(resendTime);

            li = Input.location.lastData;
            latitude = li.latitude;
            longitude = li.longitude;
            
        }
        
    }
    // easy print asset
    // 저작권 관련 제거
    // easy print asset
    public void GetCurrentDate()
    {
        string MonthAndDay = DateTime.Now.ToString(("MM월 dd일"));
        time = MonthAndDay;

        string DayTime = DateTime.Now.ToString("t");
        time += DayTime;
    }
    // Set the status of canvas and ready the points for painting
   

    public void OnclickBack()
    {
        SceneManager.LoadScene("LoadingAR");
    }

    public void UploadGraffiti()
    {

        String useremail = auth.CurrentUser.Email;
        StartCoroutine(GPS_On());
        Texture2D txt = new Texture2D(rtSource.width, rtSource.height, TextureFormat.RGB24, false);
        RenderTexture.active = rtSource;
        txt.ReadPixels(new Rect(0, 0, rtSource.width, rtSource.height), 0, 0);
        txt.Apply();
        byte[] uploadImg = txt.EncodeToPNG();
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://logintest-84027.appspot.com/");
        StorageReference imagesRef = storageRef.Child("images");
        StorageReference userImagesRef = imagesRef.Child(useremail);
        StorageReference imageRef = userImagesRef.Child(time);

        imageRef.PutBytesAsync(uploadImg).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
            }
            else
            {
            // Metadata contains file metadata such as size, content-type, and md5hash.
                StorageMetadata metadata = task.Result;
                string md5Hash = metadata.Md5Hash;
                Debug.Log("Finished uploading...");
                Debug.Log("md5 hash = " + md5Hash);
                DocumentReference graffitiRef = db.Collection("Graffiti").Document(String.Format("{0}:{1}", useremail, imageRef.Name));
                Dictionary<string, object> graffiti = new Dictionary<string, object>
				{
					{ "useremail", useremail },
					{ "graffiti", imageRef.Path },
					{ "latitude", latitude },       // 위도
					{ "longitude", longitude },     // 경도
                    { "searchPosition", String.Format("{0:F4},{1:F4}",latitude,longitude)},
					{ "like", 0 }                   // 혹시 좋아요 구현시 사용 + 좋아요 구현하게 되면 user컬렉션에 likeGraffiti 컬렉션 추가로 만들어 관리
				};
                graffitiRef.SetAsync(graffiti);
            }
            SceneManager.LoadScene("LoadingAR");
        });
    }
}
