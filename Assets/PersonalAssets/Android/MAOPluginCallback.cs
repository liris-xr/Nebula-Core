using UnityEngine;

class MAOPluginCallback : AndroidJavaProxy
{
    public MAOPluginCallback() : base("fr.enise.unitymaoplugin.PluginCallback") { }

    public void ReceiveMessage(string msg)
    {
        Debug.Log("ENTER callback ReceiveMessage: " + msg);
    }
}