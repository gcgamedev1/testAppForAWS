using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Amazon;
using Amazon.CognitoSync;
using Amazon.CognitoIdentity;
using Amazon.Util.Internal;
using Amazon.CognitoSync.SyncManager;

using Amazon.MobileAnalytics.MobileAnalyticsManager;


public class StartSceneScript : MonoBehaviour {

    Scene startScene;
    Scene toScene;

    private Dataset userInfo;
    private string userId;
    private string loginTime;

    //key for auth
    public string IdentityPoolId = "ap-northeast-1:abcf121b-d37a-4ee9-a7a0-22af02f39575";
    //key for using mobile analytics service 
    public string mobileIdentityPoolId = "ap-northeast-1:9ef21a6c-f1dd-4458-b771-7228a364e7ab";
    //application Id in mobile analytics service 
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
                _credentials = new CognitoAWSCredentials("378479961124","ap-northeast-1:abcf121b-d37a-4ee9-a7a0-22af02f39575","arn:aws:iam::378479961124:role/Cognito_MyAWSTestAppUnauth_Role","arn:aws:iam::378479961124:role/Cognito_MyAWSTestAppAuth_Role", _Region);

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

    /*
     * return random user Id 
     */
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
	void Start () {
        
        UnityInitializer.AttachToGameObject(this.gameObject);

        Debug.Log("mobileAppId::::" + mobileAppId);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;


        //AWS Mobile Analytics init
        analyticsManager = MobileAnalyticsManager.GetOrCreateInstance(mobileAppId,new CognitoAWSCredentials(mobileIdentityPoolId, _Region),AnalyticsRegion);


        /*
         * Amazon Cognito 
         * data synchronize 

        // Open your datasets
        userInfo = SyncManager.OpenOrCreateDataset("UserInfo");

        userInfo.OnSyncSuccess += SyncSuccessCallback;
        userInfo.OnSyncFailure += SynFailure;

        // init user Info
        loginTime = DateTime.Now.ToString();
        float randomNum = UnityEngine.Random.Range(0f, 10.0f);
        userId = getLoginId((int)randomNum);

        userInfo.Put("userId",userId);
        userInfo.Put("loginTime",loginTime);

        Debug.Log("userId::::"+userId);
        Debug.Log("loginTime::::" + loginTime);

        userInfo.SynchronizeAsync();
        */


        // Analystics For Custom Event 
        CustomEvent customEvent = new CustomEvent("SceneLoading");

        customEvent.AddAttribute("SceneName", "Level1");
        customEvent.AddAttribute("CharacterClass", "Warrior");
        customEvent.AddAttribute("Successful", "True");
        customEvent.AddMetric("Score", 12345);
        customEvent.AddMetric("TimeInLevel", 64);

        analyticsManager.RecordEvent(customEvent);

        //Analystics For Common Event 
        MonetizationEvent monetizationEvent = new MonetizationEvent();

        monetizationEvent.Quantity = 3.0;
        monetizationEvent.ItemPrice = 1.99;
        monetizationEvent.ProductId = "ProductId123";
        monetizationEvent.ItemPriceFormatted = "$1.99";
        monetizationEvent.Store = "Apple";
        monetizationEvent.TransactionId = "TransactionId123";
        monetizationEvent.Currency = "USD";

        analyticsManager.RecordEvent(monetizationEvent);

        Debug.Log("SynchronizeAsync Called::::");

        // call scene 2 
        StartCoroutine(LoadScene());


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // call scene 2 
    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3.0f); 

        yield return SceneManager.LoadSceneAsync("scene2", LoadSceneMode.Additive);

    }

    //callback after sync 
    void SyncSuccessCallback(object sender, SyncSuccessEventArgs e)
    {
        // Your handler code here
        var dataset = sender as Dataset;

        if (dataset.Metadata != null)
        {
            Debug.Log("Successfully synced for dataset: " + dataset.Metadata);
        }
        else
        {
            Debug.Log("Successfully synced for dataset");
        }
    }

    //callback after sync 
    void SynFailure(object sender, SyncFailureEventArgs e)
    {
        var dataset = sender as Dataset;
        Debug.Log("Sync failed for dataset : " + dataset.Metadata.DatasetName);
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
