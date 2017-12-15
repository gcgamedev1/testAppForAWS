using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Amazon;
using Amazon.CognitoSync;
using Amazon.CognitoIdentity;
using Amazon.Util.Internal;
using Amazon.CognitoSync.SyncManager;

using Amazon.MobileAnalytics.MobileAnalyticsManager;


public class LodingBetweenSceneScript : MonoBehaviour 
{

    public Slider slider;
    bool isDone = false;
    float fTime = 0f;
    AsyncOperation asyncOperation;

    private Dataset userInfo;
    private string userId;
    private string loginTime;
    public string IdentityPoolId = "ap-northeast-1:abcf121b-d37a-4ee9-a7a0-22af02f39575";
    public string mobileIdentityPoolId = "ap-northeast-1:9ef21a6c-f1dd-4458-b771-7228a364e7ab";
    public string mobileAppId = "d1c5b82b0d93431289415c98693ab617";

    private MobileAnalyticsManager analyticsManager;

    public string Region = RegionEndpoint.APNortheast1.SystemName;
    private RegionEndpoint AnalyticsRegion = RegionEndpoint.USEast1;

    private RegionEndpoint _Region
    {
        get { return RegionEndpoint.GetBySystemName(Region); }
    }

    private CognitoAWSCredentials _credentials;

    private CognitoAWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                //_credentials = new CognitoAWSCredentials(IdentityPoolId, _Region);
                _credentials = new CognitoAWSCredentials("378479961124", "ap-northeast-1:abcf121b-d37a-4ee9-a7a0-22af02f39575", "arn:aws:iam::378479961124:role/Cognito_MyAWSTestAppUnauth_Role", "arn:aws:iam::378479961124:role/Cognito_MyAWSTestAppAuth_Role", _Region);

            return _credentials;
        }
    }

    private CognitoSyncManager _syncManager;

    private CognitoSyncManager SyncManager
    {
        get
        {
            if (_syncManager == null)
            {
                _syncManager = new CognitoSyncManager(Credentials, new AmazonCognitoSyncConfig { RegionEndpoint = _Region });
            }
            return _syncManager;
        }
    }

    private string getLoginId(int num)
    {
        ArrayList nameList = new ArrayList();

        nameList.Add("userID1");
        nameList.Add("userID2");
        nameList.Add("userID3");
        nameList.Add("userID4");
        nameList.Add("userID5");
        nameList.Add("userID6");
        nameList.Add("userID7");
        nameList.Add("userID8");
        nameList.Add("userID9");
        nameList.Add("userID10");

        return nameList[num].ToString();
    }

	// Use this for initialization
	void Start () 
    {
        UnityInitializer.AttachToGameObject(this.gameObject);

        //AWS Mobile Analytics init
        analyticsManager = MobileAnalyticsManager.GetOrCreateInstance(mobileAppId, new CognitoAWSCredentials(mobileIdentityPoolId, _Region), AnalyticsRegion);

        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        // Open your datasets
        userInfo = SyncManager.OpenOrCreateDataset("UserInfo");

        userInfo.OnSyncSuccess += SyncSuccessCallback;
        userInfo.OnSyncFailure += SynFailure;

        // init user Info
        loginTime = DateTime.Now.ToString();
        float randomNum = UnityEngine.Random.Range(0f, 10.0f);
        userId = getLoginId((int)randomNum);

        userInfo.Put("userId", userId);
        userInfo.Put("loginTime", loginTime);

        Debug.Log("userId::::" + userId);
        Debug.Log("loginTime::::" + loginTime);

        userInfo.SynchronizeAsync();

        CustomEvent customEvent = new CustomEvent("SceneLoading");

        // Add attributes
        customEvent.AddAttribute("SceneName", "scene2");
        customEvent.AddAttribute("UserId", "user0000");
        customEvent.AddAttribute("Successful", "True");

        // Add metrics
        customEvent.AddMetric("Score", (int)UnityEngine.Random.Range(0f, 10000.0f));
        customEvent.AddMetric("TimeInLevel", (int)UnityEngine.Random.Range(0f, 500.0f));

        // Record the event
        analyticsManager.RecordEvent(customEvent);

        Debug.Log("SynchronizeAsync Called::::");

        StartCoroutine((StartLoad("scene3")));
	    	
	}
	
	// Update is called once per frame
	void Update ()
    {
        fTime += Time.deltaTime;
        slider.value = fTime;

        if (fTime >= 5)
        {
            asyncOperation.allowSceneActivation = true;
        }
		
	}

    public IEnumerator StartLoad(string strSceneName)
    {
        //asyncOperation = Application.LoadLevelAsync(strSceneName);
        asyncOperation = SceneManager.LoadSceneAsync(strSceneName);
        asyncOperation.allowSceneActivation = false;

        if (isDone == false)
        {
            isDone = true;

            while (asyncOperation.progress < 0.9f)
            {
                slider.value = asyncOperation.progress;
                yield return true;
            }
        }
    }

    void SyncSuccessCallback(object sender, SyncSuccessEventArgs e)
    {
        // Your handler code here
        var dataset = sender as Dataset;

        if (dataset.Metadata != null)
        {
            Debug.Log("[SCENE2] Successfully synced for dataset: " + dataset.Metadata);
        }
        else
        {
            Debug.Log("[SCENE2]  Successfully synced for dataset");
        }
    }

    void SynFailure(object sender, SyncFailureEventArgs e)
    {
        var dataset = sender as Dataset;
        Debug.Log("[SCENE2]  Sync failed for dataset : " + dataset.Metadata.DatasetName);
        Debug.LogException(e.Exception);
    }

    void OnApplicationFocus(bool focus)
    {
        if (analyticsManager != null)
        {
            if (focus)
            {
                analyticsManager.ResumeSession();
            }
            else
            {
                analyticsManager.PauseSession();
            }
        }
    }
}
