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
 * @brief Square signal definition for the left atomisation
 */
int atomization_period_l = 1000;   // ms
int atomization_duty_cycle_l = 50; // in %
/**
 * @brief Square signal definition for the right atomisation
 */
int atomization_period_r = 1000;   // ms
int atomization_duty_cycle_r = 50; // in %

/**
 * @brief Atomization square signal used for activation.
 * Prevent a long atomization of volatile product to the nose of the participant
 */
bool atomization_sq_sig_l = false;
bool atomization_sq_sig_r = false;

/**
 * @brief Keeps track of when the extraction fan started after stopping atomizing.
 * Used to stop the extraction fan after the duration is expired.
 */
long extraction_fan_start_time = -1;

long atomizations_counter_l = 0;
long atomizations_counter_r = 0;
long last_atomization_update_l = 0;
long last_atomization_update_r = 0;
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

void update_left_atomization()
{
  long duration_l = atomization_sq_sig_l ? (atomization_duty_cycle_l / 100.0 * atomization_period_l) : (atomization_period_l - atomization_duty_cycle_l / 100.0 * atomization_period_l);


  // Toggle atomisation every period
  if (millis() - last_atomization_update_l >= duration_l)
  {
    last_atomization_update_l = millis();
    atomization_sq_sig_l = !atomization_sq_sig_l;

    if (atomization_sq_sig_l)
    {
      atomizations_counter_l++;
      Serial.print("Number of left atomizations ");
      Serial.println(atomizations_counter_l);
    }
  }
}

void update_right_atomization()
{
  long duration_r = atomization_sq_sig_r ? (atomization_duty_cycle_r / 100.0 * atomization_period_r) : (atomization_period_r - atomization_duty_cycle_r / 100.0 * atomization_period_r);

  // Toggle atomisation every period
  if (millis() - last_atomization_update_r >= duration_r)
  {
    last_atomization_update_r = millis();
    atomization_sq_sig_r = !atomization_sq_sig_r;

    if (atomization_sq_sig_r)
    {
      atomizations_counter_r++;
      Serial.print("Number of right atomizations ");
      Serial.println(atomizations_counter_r);
    }
  }
}


void reset_atomization_counter_l()
{
  atomizations_counter_l = 0;
}

void reset_atomization_counter_r()
{
  atomizations_counter_r = 0;
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
      if (sscanf(args.c_str(), "%d;%d", &atomization_period_l, &atomization_duty_cycle_l) == 2)
      {
        Serial.println("Set atomization left configuration to period=" + String(atomization_period_l) + "ms, duty cycle=" + String(atomization_duty_cycle_l) + "%");
      }
      else
      {
        Serial.println("Invalid arguments, expecting command formatted as `CX;Y` with X the atomization period (ms) and Y the duty cycle (%)");
      }
      break;
    case 'D':
      if (sscanf(args.c_str(), "%d;%d", &atomization_period_r, &atomization_duty_cycle_r) == 2)
      {
        Serial.println("Set atomization right configuration to period=" + String(atomization_period_r) + "ms, duty cycle=" + String(atomization_duty_cycle_r) + "%");
      }
      else
      {
        Serial.println("Invalid arguments, expecting command formatted as `DX;Y` with X the atomization period (ms) and Y the duty cycle (%)");
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
    update_left_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, atomization_sq_sig_l ? HIGH : LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, 0);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case ATOMIZE_RIGHT:
    update_right_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, 0);
    digitalWrite(ATOMIZER_RIGHT_PIN, atomization_sq_sig_r ? HIGH : LOW);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case ATOMIZE_BOTH:
    update_left_atomization();
    update_right_atomization();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, atomization_sq_sig_l ? HIGH : LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, atomization_sq_sig_r ? HIGH : LOW);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  case EXTRACT_AIR:
    reset_atomization_counter_l();
    reset_atomization_counter_r();
    analogWrite(ATOMIZER_FAN_PIN, ATOMIZATION_FAN_SPEED);
    digitalWrite(ATOMIZER_LEFT_PIN, LOW);
    digitalWrite(ATOMIZER_RIGHT_PIN, LOW);
    analogWrite(EXTRACTION_FAN_PIN, EXTRACTION_FAN_SPEED);
    break;
  case FANLESS_ATOMIZE_BOTH:
    update_left_atomization();
    update_right_atomization();
    analogWrite(ATOMIZER_FAN_PIN, 0);
    digitalWrite(ATOMIZER_LEFT_PIN, HIGH);
    digitalWrite(ATOMIZER_RIGHT_PIN, HIGH);
    analogWrite(EXTRACTION_FAN_PIN, 0);
    break;
  default: // OFF
    reset_atomization_counter_l();
    reset_atomization_counter_r();
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