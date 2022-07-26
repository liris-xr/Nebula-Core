#include "Arduino.h"
#include "SerialFinder.h"

SerialFinder::SerialFinder(){

}

SerialFinder::SerialFinder(String handshakeINPUT, String handshakeOUTPUT){
      this->handshakeINPUT = handshakeINPUT;
      this->handshakeOUTPUT = handshakeOUTPUT;
}

SerialFinder::SerialFinder(String handshakeOUTPUT){
      this->handshakeINPUT = "connect";
      this->handshakeOUTPUT = handshakeOUTPUT;
}

bool SerialFinder::findMe(){
    if(!detected){
      String received = Serial.readString();
      if(received.equals(handshakeINPUT)){
        Serial.println(handshakeOUTPUT);
	detected=true;
      }
    }
    if(Serial.available()>0){
      String received = Serial.readString();
      if(received == "close"){
        detected = false;
      }
    }

    return detected;
}
