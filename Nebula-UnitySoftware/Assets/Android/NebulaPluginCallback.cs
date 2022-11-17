using UnityEngine;

class NebulaPluginCallBack : AndroidJavaProxy
{
    public NebulaPluginCallBack() : base("fr.enise.unitynebulaplugin.PluginCallback") { }

    public void ReceiveMessage(string msg)
    {
        Debug.Log("ENTER callback ReceiveMessage: " + msg);
    }
}