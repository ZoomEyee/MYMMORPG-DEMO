using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenesMgr : Singleton<ScenesMgr>
{
    public void LoadScene(string sceneName, UnityAction callBack)
    {
        SceneManager.LoadScene(sceneName);
        callBack();
    }
    public void LoadSceneAsync(string sceneName, UnityAction callBack)
    {
        PublicMonoMgr.Instance.StartCoroutine(CoroutineLoadSceneAsync(sceneName, callBack));
    }
    private IEnumerator CoroutineLoadSceneAsync(string sceneName, UnityAction callBack)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        while (!ao.isDone)
        {
            EventCenter.Instance.EventTrigger(EventType.LOADING_SCENE_PROGRESS_BAR_UPDATE, ao.progress);
            yield return ao.progress;
        }
        callBack();
    }
}
