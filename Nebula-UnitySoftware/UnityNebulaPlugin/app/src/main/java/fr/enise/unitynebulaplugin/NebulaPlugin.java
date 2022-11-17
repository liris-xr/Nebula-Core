package fr.enise.unitynebulaplugin;

import android.app.Activity;
import android.content.Context;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;

import com.hoho.android.usbserial.driver.CdcAcmSerialDriver;
import com.hoho.android.usbserial.driver.ProbeTable;
import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;
import com.hoho.android.usbserial.util.SerialInputOutputManager;

import java.io.IOException;
import java.util.List;

public class NebulaPlugin extends Activity {

    public static NebulaPlugin instance;

    private Context context;
    private PluginCallback callback;

    private UsbSerialPort port;

    public NebulaPlugin() {
    }

    public NebulaPlugin(Context context, PluginCallback callback) {
        this.context = context;
        this.callback = callback;
    }

    public void SendMessage(String msg) {
        try {
            port.write(msg.getBytes("US-ASCII"), 9999);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void InitializeSerialPort() {

        UsbManager manager = (UsbManager) context.getSystemService(Context.USB_SERVICE);

        ProbeTable customTable = new ProbeTable();
        customTable.addProduct(0x2341, 0x0058, CdcAcmSerialDriver.class);

        UsbSerialProber prober = new UsbSerialProber(customTable);
        List<UsbSerialDriver> drivers = prober.findAllDrivers(manager);

        // Open a connection to the first available driver.
        UsbSerialDriver driver = drivers.get(0);
        UsbDeviceConnection connection = manager.openDevice(driver.getDevice());

        UsbSerialPort port = driver.getPorts().get(0); // Most devices have just one port (port 0)
        try {
            port.open(connection);
            port.setParameters(115200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);

            this.port = port;

            CustomListener listener = new CustomListener(callback);
            SerialInputOutputManager usbIoManager = new SerialInputOutputManager(port, listener);
            usbIoManager.start();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public static NebulaPlugin InitializePlugin(Context context, PluginCallback callback) {
        instance = new NebulaPlugin(context, callback);
        instance.InitializeSerialPort();
        return instance;
    }

}
