using UnityEngine;

public class OnLoadTriggerScript : MonoBehaviour
{
    void Start()
    {
        MainManager.instance.ImplementManagers();
    }
}
