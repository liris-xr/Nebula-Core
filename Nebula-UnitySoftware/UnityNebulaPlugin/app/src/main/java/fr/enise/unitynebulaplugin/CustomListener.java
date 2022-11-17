package fr.enise.unitynebulaplugin;

import com.hoho.android.usbserial.util.SerialInputOutputManager;

public class CustomListener implements SerialInputOutputManager.Listener {

    private String readBuffer = "";
    private PluginCallback callback;

    public CustomListener(PluginCallback callback) {
        this.callback = callback;
    }

    private void ReadLineHandler(byte[] data) {
        String incoming = new String(data);
        readBuffer += incoming;
        while (readBuffer.length() > 0 && readBuffer.contains("\n")) {
            String line = readBuffer.substring(0, readBuffer.indexOf("\n")).trim();
            //Handle this line here

            callback.ReceiveMessage(line);

            //Trim the processed line from the readBuffer
            readBuffer = readBuffer.substring(readBuffer.indexOf("\n") + 1);
        }
    }

    @Override
    public void onNewData(byte[] data) {
        ReadLineHandler(data);
    }

    @Override
    public void onRunError(Exception e) {

    }

}
