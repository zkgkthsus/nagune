using Google;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;



public class Login : MonoBehaviour
{
    // Auth 용 instance
    FirebaseAuth auth = null;

    // 사용자 계정
    FirebaseUser user = null;

    public Text resulttext;

    public FirebaseFirestore db;

    [SerializeField] string email;
    [SerializeField] string password;

    public InputField inputTextEmail;
    public InputField inputTextPassword;
    // 기기 연동이 되어 있는 상태인지 체크한다.
    private bool signedIn = false;


    public void Start()
    {
        print("시작");
        db = FirebaseFirestore.DefaultInstance;
        if (SceneManager.GetActiveScene().name == "Login")
        {
            StartCoroutine(MoveScence());
        }
        else if (SceneManager.GetActiveScene().name == "Map")
        {
            LoginUser();
        }


    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Application.Quit();
        }
        
    }

    private void Awake()
    {
        // 초기화
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        // 유저의 로그인 정보에 어떠한 변경점이 생기면 실행되게 이벤트를 걸어준다.
        auth.StateChanged += AuthStateChanged;
        //AuthStateChanged(this, null);
    }

    // 계정 로그인에 어떠한 변경점이 발생시 진입.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            // 연동된 계정과 기기의 계정이 같다면 true를 리턴한다. 
            signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                UnityEngine.Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                UnityEngine.Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    //////////////
    // 익명 로그인 //
    //////////////
    public void AnonyLogin()
    {
        // 익명 로그인 진행
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            // 익명 로그인 연동 결과
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }

    //////////////
    // 구글 로그인 //
    //////////////
    public void GoogleLoginProcessing()
    {
        if (GoogleSignIn.Configuration == null)
        {
            // 설정
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                RequestEmail = true,
                // Copy this value from the google-service.json file.
                // oauth_client with type == 3
                WebClientId = "826859343712-9tbfjh8qv22vp6tsbo4toifuo46vth6g.apps.googleusercontent.com"

            };

        }

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                resulttext.text = "Google Login task.IsCanceled";
                Debug.Log("Google Login task.IsCanceled");
            }
            else if (task.IsFaulted)
            {
                resulttext.text = "Google Login task.IsFaulted" + task.Exception;
                Debug.Log("Google Login task.IsFaulted");
            }
            else
            {
                // CreateUser();
                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                        resulttext.text = "Google Login authTask.IsCanceled";
                        Debug.Log("Google Login authTask.IsCanceled");
                        return;
                    }
                    if (authTask.IsFaulted)
                    {
                        resulttext.text = "Google Login authTask.IsFaulted";
                        signInCompleted.SetException(authTask.Exception);
                        Debug.Log("Google Login authTask.IsFaulted");
                        return;
                    }

                    user = authTask.Result;
                    Debug.LogFormat("Google User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
                    StartCoroutine(MoveScence());
                    return;
                });
            }
        });
    }

    IEnumerator MoveScence()
    {
        yield return new WaitForSeconds(0.5f);
        if (auth.CurrentUser != null)
        {
            print(auth.CurrentUser.Email);
        }
        if (SceneManager.GetActiveScene().name == "Login" && auth.CurrentUser != null)
        {
            SceneManager.LoadScene("LoadingMap");
        }
        print("와2");

    }

    ////////////////
    // 이메일 로그인 //
    ////////////////
    public void Emailsignin()
    {
        // 적당한 UGUI 를 만들어 email, pw 를 입력받는다.
        email = inputTextEmail.text;
        password = inputTextPassword.text;

        if (email.Length < 1 || password.Length < 1)
        {
            Debug.Log("이메일 ID 나 PW 가 비어있습니다.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // firebase email user create
            Firebase.Auth.FirebaseUser newUser = task.Result;
            UnityEngine.Debug.LogFormat("Firebase Email user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            return;
        });
    }

    public void EmailLogin()
    {
        // 적당한 UGUI 를 만들어 email, pw 를 입력받는다.
        email = inputTextEmail.text;
        password = inputTextPassword.text;
        if (email.Length < 1 || password.Length < 1)
        {
            Debug.Log("이메일 ID 나 PW 가 비어있습니다.");
            return;
        }
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {

            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("SignInWithEmailAndPasswordAsync was canceled. was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // firebase email user create
            Firebase.Auth.FirebaseUser newUser = task.Result;
            UnityEngine.Debug.LogFormat("Firebase Email user login successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            StartCoroutine(MoveScence());
            return;

        });
        StartCoroutine(MoveScence());
    }

    // 연동 해제
    public void SignOut()
    {
        Debug.Log(auth.CurrentUser);
        if (auth.CurrentUser != null)
            SceneManager.LoadScene("Login");
        Debug.Log("로그아웃");
        auth.SignOut();

    }

    // 연동 계정 삭제
    public void UserDelete()
    {
        if (auth.CurrentUser != null)
            auth.CurrentUser.DeleteAsync();
    }
   
    public void LoginUser()
    {
        // 1. 유저 정보 세팅
        String useremail = auth.CurrentUser.Email;
        DocumentReference userRef = db.Collection("User").Document(useremail);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
        // 2. 유저 Document가 있는지 없는지 확인
            if (task.Result.Exists)
            {
            // 2.1. 유저 Document가 있다면, 새롭게 추가된 랜드마크에 대해서 업데이트
                CollectionReference Landmarks = db.Collection("Landmark");
                Landmarks.GetSnapshotAsync().ContinueWithOnMainThread(task2 =>
            {
                QuerySnapshot snapshot = task2.Result;
            // 모든 Landmark들에 대해서
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                // 유저가 해당 Landmark를 가지고 있지 않다면, 새롭게 추가
                    DocumentReference userLandmark = db.Collection("User").Document(useremail).Collection("Landmark").Document(document.Id);
                    userLandmark.GetSnapshotAsync().ContinueWithOnMainThread(task3 =>
                {
                    if (!task3.Result.Exists)
                    {
                        db.Collection("User").Document(useremail).Collection("Landmark").Document(document.Id).SetAsync(document.ToDictionary());
                    }
                });
                }
            });
            }
            else
            {
            // 2.2. 유저 Document가 없다면, CreateUser
                Dictionary<string, object> user = new Dictionary<string, object>{
          { "email", useremail }
            };
                userRef.SetAsync(user).ContinueWithOnMainThread(task =>
            {
                CollectionReference Landmarks = db.Collection("Landmark");
                Landmarks.GetSnapshotAsync().ContinueWithOnMainThread(task2 =>
            {
                  QuerySnapshot snapshot = task2.Result;
                  foreach (DocumentSnapshot document in snapshot.Documents)
                  {
                      db.Collection("User").Document(useremail).Collection("Landmark").Document(document.Id).SetAsync(document.ToDictionary());
                  }
              });
            });
            }
        });
    }

    public void PutSampleLandmark()
    {
        CollectionReference LandmarkRef = db.Collection("Landmark");
        LandmarkRef.Document("NO.01").SetAsync(new Dictionary<string, object>(){
          { "Name", "경복궁" },
          { "city", "서울" },
          { "NO", "1"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.578498381650775 },
          { "longitude", 126.97697642097256 },
          { "Detail", "경복궁은 조선 왕조 제일의 법궁입니다. 북으로 북악산을 기대어 자리 잡았고 정문인 광화문 앞으로는 넓은 육조거리(지금의 세종로)가 펼쳐져, 왕도인 한양(서울) 도시 계획의 중심이기도 합니다. 1395년 태조 이성계가 창건하였고, 1592년 임진 왜란으로 불타 없어졌다가, 고종 때인 1867년 중건되었습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.02").SetAsync(new Dictionary<string, object>(){
          { "Name", "남산서울타워" },
          { "city", "서울" },
          { "NO", "2"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.55141280687951  },
          { "longitude", 126.98859678060495 },
          { "Detail", "구름과 맞닿은 곳에서 남산의 자연과 21세기 첨단기술이 만들어낸 절묘한 조화, 여유로운 휴식과 다양한 문화가 함께하는 서울의 복합문화공간 N서울타워입니다. N서울타워는 1980년, 일반인에게 공개된 이후 남산의 살아있는 자연과 함께 서울시민의 휴식공간이자 외국인의 관광명소로 자리 잡았습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.03").SetAsync(new Dictionary<string, object>(){
          { "Name", "63빌딩" },
          { "city", "서울" },
          { "NO", "3"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.51948223569589 },
          { "longitude", 126.94017407277346 },
          { "Detail", "당대 동양에서 가장 높은 해발고도를 자랑하며 여의도 60번지에 우뚝 선 63빌딩은 강성해진 국력을 의미하는 상징이었고, 1988년 개최된 서울 올림픽과 더불어 ‘한강의 기적’이라 불리는 대한민국의 경제성장을 시각화하는 대표적인 건축물이 되었습니다. 63빌딩은 1985년에 완공되었고, 30여년이 흐른 지금까지도 서울의 상징적인 랜드마크로서 자리매김하고 있습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.04").SetAsync(new Dictionary<string, object>(){
          { "Name", "광화문 이순신 동상" },
          { "city", "서울" },
          { "NO", "4"},
          { "Type", "상징물"},
          { "isVisited", false },
          { "latitude", 37.570941249114924 },
          { "longitude", 126.9769192539411 },
          { "Detail", "광화문의 충무공 이순신 장군 동상은 정부의 산하 단체였던 애국선열 조상건립위원회와 서울신문사의 공동주관으로 1968년에 건립되었습니다. 전체 높이 17m(동상6.5m, 기단 10.5m)의 청동 입상 형태로 건립되었으며 주변 조형물로는 거북선 모형 1개와 북 2개가 위치해 있습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.05").SetAsync(new Dictionary<string, object>(){
          { "Name", "청계천" },
          { "city", "서울" },
          { "NO", "5"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.56902057566937 },
          { "longitude", 126.97944265168888 },
          { "Detail", "2003년 7월부터 2005년 9월까지 청계천은 엄청난 변신을 도모했습니다. 복개한 청계천로와 삼일로 주변 5.84km 구간을 복원하고 총 22개의 다리를 설치하는 등 시민들의 쉼터로 탈바꿈하기 위한 대대적인 공사였습니다. 청계광장을 중심으로 각종 문화행사 등이 열리면서 지금은 광장의 역할도 하고 있습니다. 최근에는 예술 공간으로서의 역할도 훌륭히 해내고 있습니다. 광교갤러리와 청계창작스튜디오 같은 창작 무대에서는 예술가들의 감성을 만날 수 있습니다. 주말에는 거리 예술가들의 공연이 펼쳐집니다. 청계천의 변신은 여전히 현재진행형입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.06").SetAsync(new Dictionary<string, object>(){
          { "Name", "롯데월드" },
          { "city", "서울" },
          { "NO", "6"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.51042588155397 },
          { "longitude", 127.09903508638081 },
          { "Detail", "서울 롯데월드는 신나는 여가와 엔터테인먼트를 꿈꾸는 사람들과 갈수록 늘어나는 외국인 관광 활성화를 위해 운영하는 테마파크입니다. 롯데월드는 모험과 신비를 테마로 하는 실내 공원인 롯데월드 어드벤처, 석촌호수 서호에 있는 매직 아일랜드, 민속박물관, 쇼핑몰, 호텔, 백화점, 아이스링크 등으로 구성되어 있습니다. 관광, 문화, 레저, 쇼핑을 원스톱으로 즐길 수 있는 대규모 복합 생활 공간입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.07").SetAsync(new Dictionary<string, object>(){
          { "Name", "여의도 공원" },
          { "city", "서울" },
          { "NO", "7"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.526128892746925 },
          { "longitude", 126.92229895366074 },
          { "Detail", "서울 영등포구 여의도동에 위치한 시민공원입니다. 한국전통의 숲, 문화의 마당, 잔디마당, 자연생태의 숲 등 네 개의 공간으로 이루어져 있스비다. 주변에 자전거 도로와 산책로가 조성되어있어 시민들에게 휴식공간이자 문화공간의 역할을 하고 있습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.08").SetAsync(new Dictionary<string, object>(){
          { "Name", "청와대" },
          { "city", "서울" },
          { "NO", "8"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.58624469953915 },
          { "longitude", 126.97489033068757 },
          { "Detail", "대한민국 종로구 청와대로(세종로)에 위치한 시민공원입니다. 1948년부터 2022년 5월 9일까지 대한민국의 대통령이 기거하는 대통령 관저이자 대한민국 헌법이 규정하는 헌법기관으로서의 대통령부(大統領府)와 관계된 행정기관이었습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.09").SetAsync(new Dictionary<string, object>(){
          { "Name", "국립고궁박물관" },
          { "city", "서울" },
          { "NO", "9"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.57661124849136 },
          { "longitude", 126.97552356878577 },
          { "Detail", "찬란했던 500년 역사와 문화를 자랑하는 조선 왕실의 문화재를 소장하고 있는 국립고궁박물관은 왕실문화유산의 보존과 활용을 위한 조사연구 및 전시·교육 활동을 통해 국민과 함께하고 있습니다. 앞으로도 옛 선조들이 남긴 고품격의 문화재를 통해 오랜 역사의 숨결을 느낄 수 있는 문화터전입니다. 국립고궁박물관에서 조선을 대표하는 화려하면서도 기품 있는 왕실문화유산을 감상하시면서 우리의 문화적 위상을 느껴보시기 바랍니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.10").SetAsync(new Dictionary<string, object>(){
          { "Name", "성산 일출봉" },
          { "city", "제주도" },
          { "NO", "10"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 33.45947716390698 },
          { "longitude", 126.93979628836172 },
          { "Detail", "성산일출봉은 높이 182m, 정상 분화구는 지름 600m, 면적 약 21.44ha입니다. 제주의 많은 분화구 중 예외적으로 바다에서 분출한 화산으로, 원래 섬이었지만 신양 해수욕장 쪽에서 쌓인 모래와 자갈로 육지와 연결되었습니다." }
    }))))))))));
        PutSampleLandmark2();
    }

    public void PutSampleLandmark2()
    {
        CollectionReference LandmarkRef = db.Collection("Landmark");
        LandmarkRef.Document("NO.11").SetAsync(new Dictionary<string, object>(){
          { "Name", "호미곶(상생의 손)" },
          { "city", "포항" },
          { "NO", "11"},
          { "Type", "상징물"},
          { "isVisited", false },
          { "latitude", 36.076727536335824 },
          { "longitude", 129.5700725930102 },
          { "Detail", "상생의 손은 포항시 호미곶에 있는 해맞이 광장에 위치한 기념물로 인류가 화합하고 화해하며 더불어 사는 사회를 만들어가자는 의미로 1999년에 만들어진 조각물입니다. 바다에는 오른손이, 육지에는 왼손이 있습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.12").SetAsync(new Dictionary<string, object>(){
          { "Name", "첨성대" },
          { "city", "경주" },
          { "NO", "12"},
          { "Type", "문화재"},
          { "isVisited", false },
          { "latitude", 35.83471223078294 },
          { "longitude", 129.21900041460282 },
          { "Detail", "첨성대는 경상북도 경주시 반월성 동북쪽에 위치한 신라 중기의 석조 건축물로, 선덕여왕때에 세워진 세계에서 현존하는 가장 오래된 천문대로 알려져 있습니다. 1962년 12월 20일 국보 제31호로 지정되었습니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.13").SetAsync(new Dictionary<string, object>(){
          { "Name", "불국사" },
          { "city", "경주" },
          { "NO", "13"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 35.7896707999508 },
          { "longitude", 129.33216329708415 },
          { "Detail", "석굴암과 같은 서기 751년 신라 경덕왕때 김대성이 창건하여 서기 774년 신라 혜공왕때 완공하였습니다. 토함산 서쪽 중턱의 경사진 곳에 자리한 불국사는 심오한 불교사상과 천재 예술가의 혼이 독특한 형태로 표현되어 세계적으로 우수성을 인정받는 기념비적인 예술품입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.14").SetAsync(new Dictionary<string, object>(){
          { "Name", "판문점(자유의 집)" },
          { "city", "경기도" },
          { "NO", "14"},
          { "Type", "건물"},
          { "isVisited", false },
          { "latitude", 37.95537349339703 },
          { "longitude", 126.67664835928105 },
          { "Detail", "1953년 10월 제25차 본희의에서 군사정전위원회의 원만한 운영을 위해서 군사정전위원회 본부 구역에군사분계선을 중심으로 유엔군 측과 공산군 측의 공동경비구역을 설정하기로 합의했고, 이 합의에 따라 동서 800m, 남북400m에 달하는 정방형의 공동경비구역이 만들어졌습니다" }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.15").SetAsync(new Dictionary<string, object>(){
          { "Name", "에버랜드(입구)" },
          { "city", "경기도" },
          { "NO", "15"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.29538363234761 },
          { "longitude", 127.20475370152894 },
          { "Detail", "에버랜드는 1976년에 개장하였으며, 다채로운 축제와 어트랙션, 동물원과 가든으로 구성된 글로벌 테마파크입니다. 5개의 테마존과 시즌마다 모습을 달리하는 다양한 축제, 최신 어트랙션 등 다채로운 시설과 서비스로 고객들에게 최고의 즐거움을 선사합니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.16").SetAsync(new Dictionary<string, object>(){
          { "Name", "수원화성(입구)" },
          { "city", "경기도" },
          { "NO", "16"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.28881592391372 },
          { "longitude", 127.01428020767281 },
          { "Detail", "수원화성은 1795년 정조 대왕이 창건하였으며, 중국, 일본 등지에서 찾아볼 수 없는 평산성의 형태로 군사적 방어기능과 상업적 기능을 함께 보유하고 있으며 시설의 기능이 가장 과학적이고 합리적이며, 실용적인 구조로 되어 있는 동양 성곽의 백미라 할 수 있습니다. " }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.17").SetAsync(new Dictionary<string, object>(){
          { "Name", "서울대공원" },
          { "city", "경기도" },
          { "NO", "17"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.43374379004223 },
          { "longitude", 127.02146665272693 },
          { "Detail", "서울대공원은 세계 각국의 야생동물들이 살아 숨 쉬는 서울동물원과 다양한 기후대의 식물들이 모여 사는 서울대공원 식물원, 자연과 사람의 특별한 이야기가 있는 테마가든, 자연 속에서의 휴식과 치유함이 있는 치유의 숲, 산림욕장, 캠핑장, 누구나 쉽게 즐기는 가족친화형 생활야구장 서울대공원 야구장, 다양한 재미와 즐거움을 주는 서울랜드, 근·현대 미술계의 흐름을 한눈에 볼 수 있는 국립현대미술관으로 구성되어 있으며, 많은 시민들의 추억과 사랑을 한 몸에 받고 있는 대한민국 대표 종합 테마파크입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.18").SetAsync(new Dictionary<string, object>(){
          { "Name", "국제시장" },
          { "city", "부산" },
          { "NO", "18"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 35.101343326455634 },
          { "longitude", 129.02816990055402 },
          { "Detail", "1945년 태어난 순간부터 한 생을 살아가는 동안의 필요한 모든 것이 다 있었다던 곳. 어느덧 70여년, 오늘날 먹자골목과 아리랑거리, 젊음의 거리, 구제골목과 함께 부산의 관광 상품으로 자리한 이곳은 한국 근현대사의 숱한 사연을 고스란히 담고 있는 부산의 국제시장입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.19").SetAsync(new Dictionary<string, object>(){
          { "Name", "남이섬" },
          { "city", "강원도" },
          { "NO", "19"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 37.79141934937881 },
          { "longitude", 127.52578717394488 },
          { "Detail", "남이 장군의 묘역이 있는 문화유적지이자 관광휴양지로 개발된 곳으로서 2016년 현재 세계 122개국으로부터 130만명의 외국인관광객을 포함, 연간 총 330만명이 찾는 대한민국의 대표적인 관광지입니다." }
    }).ContinueWithOnMainThread(task =>
    LandmarkRef.Document("NO.20").SetAsync(new Dictionary<string, object>(){
          { "Name", "전주한옥마을" },
          { "city", "전주" },
          { "NO", "20"},
          { "Type", "장소"},
          { "isVisited", false },
          { "latitude", 35.814804684787845 },
          { "longitude", 127.15268321760846 },
          { "Detail", "전주 풍남동 일대에 700여 채의 한옥이 군락을 이루고 있는 국내 최대 규모의 전통 한옥촌이며, 전국 유일의 도심 한옥군입니다. 1910년 조성되기 시작한 우리나라 근대 주거문화 발달과정의 중요한 공간으로, 경기전, 오목대, 향교 등 중요 문화재와 20여개의 문화시설이 산재되어 있으며, 한옥, 한식, 한지, 한소리, 한복, 한방 등 韓스타일이 집약된 대한민국 대표 여행지입니다." }
    }))))))))));
    }
}


