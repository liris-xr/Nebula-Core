enum Mode
{
  OFF,
  ATOMIZE_LEFT,
  ATOMIZE_RIGHT,
  ATOMIZE_BOTH,
  EXTRACT_AIR,
  FANLESS_ATOMIZE_BOTH,
};

const int ATOMIZER_FAN_PIN = A0;
const int EXTRACTION_FAN_PIN = 9;
const int ATOMIZER_RIGHT_PIN = 6;
const int ATOMIZER_LEFT_PIN = 5;

const int ATOMIZATION_FAN_SPEED = 255;
const int EXTRACTION_FAN_SPEED = 255;

/**
 * @brief When stopping the atomization, the extraction fan runs for a few seconds. This constant determines for how long it will run.
 */
const long EXTRACTION_DURATION = 2000; // ms

/**
 * @brief Square signal definition for the atomisation
 */
int atomization_period = 1000;   // ms
int atomization_duty_cycle = 50; // in %

/**
 * @brief Atomization square signal used for activation.
 * Prevent a long atomization of volatile product to the nose of the participant
 */
bool atomization_sq_sig = false;

/**
 * @brief Keeps track of when the extraction fan started after stopping atomizing.
 * Used to stop the extraction fan after the duration is expired.
 */
long extraction_fan_start_time = -1;

long atomizations_counter = 0;
long last_atomization_update = 0;

bool atomize_left = false;
bool atomize_right = false;


enum Mode current_mode = OFF;

void setup()
{
  pinMode(ATOMIZER_LEFT_PIN, OUTPUT);
  pinMode(ATOMIZER_RIGHT_PIN, OUTPUT);
  pinMode(ATOMIZER_FAN_PIN, OUTPUT);
  pinMode(EXTRACTION_FAN_PIN, OUTPUT);
  Serial.begin(115200); // Open COM PORT => WARNING : if NANO EVERY think to enable DTR while opening COM port on Unity!!
  while (!Serial) {
      ;
  }
  Serial.println("Nebula");
}

void update_atomization()
{
  long duration = atomization_sq_sig ? (atomization_duty_cycle / 100.0 * atomization_period) : (atomization_period - atomization_duty_cycle / 100.0 * atomization_period);

  // Toggle atomisation every period
  if (millis() - last_atomization_update >= duration)
  {
    last_atomization_update = millis();
    atomization_sq_sig = !atomization_sq_sig;

    if (atomization_sq_sig)
    {
      atomizations_counter++;
      Serial.print("Number of atomizations ");
      Serial.println(atomizations_counter);
    }
  }
}

void reset_atomization_counter()
{
  atomizations_counter = 0;
}

void loop()
{
  String serialReceived;
  char cmdChar;
  String args;

  while (Serial.available() > 0)
  {
    serialReceived = Serial.readStringUntil('\n');
    Serial.print("Received `");
    Serial.print(serialReceived);
    Serial.println("`");
    cmdChar = serialReceived.charAt(0);
    args = serialReceived.substring(1);

    switch (cmdChar)
    {
    case 'L':
      Serial.println("Left atomization");

      if (current_mode == ATOMIZE_RIGHT)
      {
        current_mode = ATOMIZE_BOTH;
      }
      else
      {
        current_mode = ATOMIZE_LEFT;
      }

      break;
    case 'R':
      Serial.println("Right atomization");

      if (current_mode == ATOMIZE_LEFT)
      {
        current_mode = ATOMIZE_BOTH;
      }
      else
      {
        current_mode = ATOMIZE_RIGHT;
      }

      break;
    case 'l':
      Serial.println("Stop left atomization");

      if (current_mode == ATOMIZE_BOTH)
      {
        current_mode = ATOMIZE_RIGHT;
      }
      else if (current_mode == ATOMIZE_LEFT)
      {
        extraction_fan_start_time = millis();
        current_mode = OFF;
      }

      break;
    case 'r':
      Serial.println("Stop right atomization");

      if (current_mode == ATOMIZE_BOTH)
      {
        current_mode = ATOMIZE_LEFT;
      }
      else if (current_mode == ATOMIZE_RIGHT)
      {
        extraction_fan_start_time = millis();
        current_mode = OFF;
      }

      break;
    case 'F':
      Serial.println("Fanless atomization");
      current_mode = FANLESS_ATOMIZE_BOTH;
      break;
    case 'E':
      Serial.println("Extracting air");
      current_mode = EXTRACT_AIR;
      break;
    case 'S':
      Serial.println("Stopped atomization");

      // If previous mode was atomizing, extract air for a few seconds to clean up the air chamber
      if (current_mode == ATOMIZE_LEFT || current_mode == ATOMIZE_RIGHT || current_mode == ATOMIZE_BOTH)
      {
        extraction_fan_start_time = millis();
      }

      current_mode = OFF;
      break;
    case 'C':
      if (sscanf(args.c_str(), "%d;%d", &atomization_period, &atomization_duty_cycle) == 2)
      {
        Serial.println("Set atomization configuration to period=" + String(atomization_period) + "ms, duty cycle=" + String(atomization_duty_cycle) + "%");
      }
      else
      {
        Serial.println("Invalid arguments, expecting command formatted as `CX;Y` with X the atomization period (ms) and Y the duty cycle (%)");
      }
      break;
    default:
      Serial.println("Unknown command");
      break;
    }
  }

  switch (current_mode)
  {
  case ATOMIZE_LEFT:
    update_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, atomization_sq_sig ? HIGH : LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, 0);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case ATOMIZE_RIGHT:
    update_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, 0);
    digitalWrite(ATOMIZER_RIGHT_PIN, atomization_sq_sig ? HIGH : LOW);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case ATOMIZE_BOTH:
    update_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, atomization_sq_sig ? HIGH : LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, atomization_sq_sig ? HIGH : LOW);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case EXTRACT_AIR:
    reset_atomization_counter();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, LOW);
    analogWrite(EXTRACTION_FAN_PIN, EXTRACTION_FAN_SPEED);
    break;
  case FANLESS_ATOMIZE_BOTH:
    update_atomization();
    analogWrite(ATOMIZER_FAN_PIN, 0);
    digitalWrite(ATOMIZER_LEFT_PIN, HIGH);
    digitalWrite(ATOMIZER_RIGHT_PIN, HIGH);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  default: // OFF
    reset_atomization_counter();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, LOW);

    // extraction_fan_start_time != -1 is used to prevent activating the fan on boot

    if (extraction_fan_start_time != -1 && millis() - extraction_fan_start_time <= EXTRACTION_DURATION)
    {
      analogWrite(EXTRACTION_FAN_PIN, EXTRACTION_FAN_SPEED);
    }
    else
    {
      analogWrite(EXTRACTION_FAN_PIN, 0);
    }

    break;
  }
}