/* SerialFinder.h - Library for auto-detect Arduino Port for Unity (5.0+ compatible) Game Engine
through NET 2.0 SerialPort class from System.IO.Ports;
 Created by Lucas Cassiano P. Silva, August 28, 2015
 Version 1.0
 Last Update: August 28, 2015
 Released into Public domain under MIT License
*/
#ifndef SerialFinder_h
#define SerialFinder_h

#include "Arduino.h"

class SerialFinder
{
  private:
    bool detected = false;
    String handshakeINPUT = "Nebula";
    String handshakeOUTPUT;
  public:
    SerialFinder();
    SerialFinder(String handshakeINPUT, String handshakeOUTPUT);
    SerialFinder(String handshakeOUTPUT);
    bool findMe();
};


#endif
