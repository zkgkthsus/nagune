using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
// 슬라이드 기능 구현
public class SwipeReceiver: MonoBehaviour
{
	public Detail dt;
	public PaintLoader pt;

	GameObject priview;
	Image priviewImage;



	protected virtual void OnSwipeLeft()
	{
        // print("왼쪽 스와이프");
		
		if (SceneManager.GetActiveScene().name == "Detail")
		{
			TransDetailNext();  
		}
		else if(SceneManager.GetActiveScene().name == "AR" && pt.mode == "select")
		{
			TransPaintNext();
		}
	}


	protected virtual void OnSwipeRight()
	{
        // print("오른쪽 스와이프");
		if (SceneManager.GetActiveScene().name == "Detail")
		{
			TransDetailBack();  
		}
		else if(SceneManager.GetActiveScene().name == "AR" && pt.mode == "select")
		{
			TransPaintBack();
		}
	}

	protected virtual void Update()
	{
		if (SwipeManager.Instance.IsSwiping(SwipeManager.SwipeDirection.Right))
		{	
			OnSwipeRight();
		}

		if (SwipeManager.Instance.IsSwiping(SwipeManager.SwipeDirection.Left))
		{
			OnSwipeLeft();
		}
	}
	// 디테일 화면 전환
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
	// AR 화면 전환
	public void TransPaintNext()
    {
		priview = GameObject.Find("Preview");
		priviewImage = priview.GetComponent<Image>();
        pt.cnt += 1;
        if (pt.cnt >= pt.images.Count) pt.cnt = 0;
		priviewImage.sprite = pt.Sprites[pt.cnt];
		
    }
    public void TransPaintBack()
    {
		priview = GameObject.Find("Preview");
		priviewImage = priview.GetComponent<Image>();
        pt.cnt -= 1;
        if (pt.cnt <= -1) pt.cnt = pt.images.Count-1;
		priviewImage.sprite = pt.Sprites[pt.cnt];
    }

}