using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine.Android;
using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PaintLoader : MonoBehaviour
{
    public FirebaseFirestore db;
    public FirebaseStorage storage;

    //AR paint
    public GameObject paintLoader;
    public GameObject image;
    ARRaycastManager arManager;
    GameObject placedObject;

    //UpUI
    public GameObject priview;
    public Image modeButton;
    public Text modeDetail;
    public String mode = "normal";
    string normalMode = "주변을 둘러보세요.\n왼쪽 버튼을 눌러 모드를 바꿔보세요";
    string selectMode = "남길 흔적을 골라보세요.\n왼쪽 버튼을 눌러 모드를 바꿔보세요";
    string paintMode = "선택한 흔적을 원하는 곳에 남겨보세요.\n왼쪽 버튼을 눌러 모드를 바꿔보세요";
    public int cnt = 0;

    Sprite[] modeMaterial;

    //DownUI
    public GameObject rightLeft;
    public GameObject stamp;

    

    

    public Double latitude;
    public Double longitude;

    bool receiveGPS = false;
    float waitTime = 0;

    public float maxWaitTime = 10.0f;
    public float resendTime = 1.0f;

    
    GameObject arPlane;
    ARPlaneManager arPM;

    // public Text lalong;

    public List<Texture2D> images;
    public List<Sprite> Sprites;

    



    FirebaseAuth auth = null;
    // Start is called before the first frame update
    void Start()
    {
        paintLoader.SetActive(false);
        arManager = GetComponent<ARRaycastManager>();
        arPlane = GameObject.Find("AR Session Origin");
        arPM= arPlane.GetComponent<ARPlaneManager>();
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
        arPM.enabled = false;
        Sprites = new List<Sprite>();
        images = new List<Texture2D>();
        modeMaterial = Resources.LoadAll<Sprite>("mode");
        priview.SetActive(false);
        stamp.SetActive(false);
        StartCoroutine(GPS_On());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == "paint") DetectGround();
        
    }
    public void PlusPaintCnt()
    {
        Image priviewImage = priview.GetComponent<Image>();
        if (mode == "paint")
        {
            modeDetail.text = selectMode;
            mode = "select";
            modeButton.sprite = modeMaterial[1];
            priview.SetActive(true);
            priview.transform.localScale = new Vector3(4.3f,4.3f,1);
            priview.transform.localPosition = new Vector3(350,-620,0);
            rightLeft.SetActive(true);
            stamp.SetActive(false);
        }
        else if (mode == "select")
        {
            modeDetail.text = paintMode;
            mode = "paint";
            modeButton.sprite = modeMaterial[2];
            arPM.enabled = true;
            rightLeft.SetActive(false);
            stamp.SetActive(true);
            priview.transform.localScale = new Vector3(1,1,1);
            priview.transform.localPosition = new Vector3(-25,-325,0);
        }
        
    }
    public void WritePaint()
    {
        Material matPaint = new Material(Shader.Find("Standard"));
        matPaint.SetTexture("_MainTex", images[cnt]);

        GameObject child = image.transform.GetChild(0).gameObject;
        MeshRenderer sampleImg = child.GetComponent<MeshRenderer>();
        sampleImg.material = matPaint;
        placedObject = Instantiate(image, paintLoader.transform.position, paintLoader.transform.rotation);
    }

    public void ModeChange()
    {
        print(mode);
        if (mode == "normal") 
        {
            modeDetail.text = selectMode;
            mode = "select";
            modeButton.sprite = modeMaterial[1];
        }
        else if (mode == "select")
        {
            modeDetail.text = paintMode;
            mode = "paint";
            modeButton.sprite = modeMaterial[2];
        }
        else if (mode == "paint")
        {
            modeDetail.text = normalMode;
            mode = "normal";
            modeButton.sprite = modeMaterial[0];
        }

        if (images.Count > 0 && (mode == "paint" || mode == "select"))
        {
            print("startPriview");
            Image priviewImage = priview.GetComponent<Image>();
            if (mode == "select")
            {
                priview.SetActive(true);
                priview.transform.localScale = new Vector3(4.3f,4.3f,1);
                priview.transform.localPosition = new Vector3(350,-620,0);
                rightLeft.SetActive(true);
            }
            else
            {
                arPM.enabled = true;
                rightLeft.SetActive(false);
                stamp.SetActive(true);
                priview.transform.localScale = new Vector3(1,1,1);
                priview.transform.localPosition = new Vector3(-25,-325,0);
            }
            
            priviewImage.sprite = Sprites[cnt];

        }
        else
        {
            arPM.enabled = false;
            priview.SetActive(false);
            stamp.SetActive(false);
            
        }
        

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
            // lalong.text = "위도 경도 실패";
            
        }
        if (waitTime >= maxWaitTime)
        {
            // latitude_text = "위도 시간 초과";
            // longitude_text = "경도 시간 초과";
            // lalong.text = "시간초과";
        }
        LocationInfo li = Input.location.lastData;
        latitude = li.latitude;
        longitude = li.longitude;
        receiveGPS = true;
        DownloadGraffiti();
    }

    void DetectGround()
    {
        Vector2 screenSize = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        List<ARRaycastHit> hitinfos = new List<ARRaycastHit>();

        if (arManager.Raycast(screenSize, hitinfos, TrackableType.Planes))
        {
            paintLoader.SetActive(true);
            paintLoader.transform.position = hitinfos[0].pose.position;
            paintLoader.transform.rotation = hitinfos[0].pose.rotation;
            paintLoader.transform.position += paintLoader.transform.up * 0.1f;
        }
        else
        {
            paintLoader.SetActive(false);
        }
    }
    public void DownloadGraffiti()
    {
        Double latitude_gps = System.Math.Round(latitude, 4);
        Double longitude_gps = System.Math.Round(longitude, 4);
        CollectionReference graffitiRef = db.Collection("Graffiti");
        for (int i = -1; i <= 1; i++)
        {
            double searchLatitude = latitude_gps + i * Mathf.Pow(10, -4);
            for (int j = -1; j <= 1; j++)
            {
                double searchLongitude = longitude_gps + j * Mathf.Pow(10, -4);
                Query query = graffitiRef.WhereEqualTo("searchPosition", String.Format("{0:F4},{1:F4}", searchLatitude, searchLongitude));
                query.GetSnapshotAsync().ContinueWithOnMainThread((task) =>
                {
                    
                    foreach (DocumentSnapshot document in task.Result.Documents)
                    {
                        Dictionary<string, object> dictionaryItem = document.ToDictionary();
                        // Storage에서 이미지를 받아와 dictionaryItem에 추가
                        StorageReference pathReference = storage.GetReference((String)dictionaryItem["graffiti"]);
                        // 최대용량 설정 : 현재 1MB
                        const long maxAllowedSize = 1 * 1024 * 1024;
                        pathReference.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                Debug.LogException(task.Exception);
                            }
                            else
                            {
                                byte[] fileContents = task.Result;
                                Debug.Log("Finished downloading!");
                                // RawImage객체에 받아온 byte[]형식의 파일을 Texture2D형태로 변환해서 바꿔줌
                                // sampleImg.texture = Base64ToTexture2D(fileContents);
                                // 받아온 이미지를 byte[]에서 texture로 바꿔서 넣는다 
                                
                                Sprite preSprite = Sprite.Create(Base64ToTexture2D(fileContents), new Rect(0, 0, Base64ToTexture2D(fileContents).width, Base64ToTexture2D(fileContents).height), new Vector2(0.5f, 0.5f));
                                images.Add(Base64ToTexture2D(fileContents));
                                Sprites.Add(preSprite);
                                
                            }
                        });
                    }
                });
            }
        }
        

    }
    private Texture2D Base64ToTexture2D(byte[] imageData)
    // 다운받은 이미지를 byte[]에서 texture로 변환
    {
        int width, height;
        GetImageSize(imageData, out width, out height);

        // 매프레임 new를 해줄경우 메모리 문제 발생 -> 멤버 변수로 변경
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(imageData);
        texture.Apply();

        return texture;
    }
    private void GetImageSize(byte[] imageData, out int width, out int height)
    {
        width = ReadInt(imageData, 3 + 15);
        height = ReadInt(imageData, 3 + 15 + 2 + 2);
    }

    private int ReadInt(byte[] imageData, int offset)
    {
        return (imageData[offset] << 8 | imageData[offset + 1]);
    }
   
}
