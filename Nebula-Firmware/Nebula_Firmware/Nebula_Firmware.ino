// ---------------------------
// Pins
// ---------------------------
const int ATOMIZER_FAN_PIN     = A0;
const int EXTRACTION_FAN_PIN   = 9;
const int ATOMIZER_A_PIN       = 3;
const int ATOMIZER_B_PIN       = 4;
const int ATOMIZER_C_PIN       = 5;
const int ATOMIZER_D_PIN       = 6;

// ---------------------------
// Fan speeds and durations
// ---------------------------
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
bool is_diffusing = false;
bool was_diffusing = false;

// ---------------------------
// Setup
// ---------------------------
void setup() {
  pinMode(ATOMIZER_A_PIN, OUTPUT);
  pinMode(ATOMIZER_B_PIN, OUTPUT);
  pinMode(ATOMIZER_C_PIN, OUTPUT);
  pinMode(ATOMIZER_D_PIN, OUTPUT);
  pinMode(ATOMIZER_FAN_PIN, OUTPUT);
  pinMode(EXTRACTION_FAN_PIN, OUTPUT);

  Serial.begin(115200);
  while (!Serial) {}
  Serial.println("Nebula");
}

// ---------------------------
// Update atomization square signal
// ---------------------------
void update_atomization(bool &sq_sig, long &last_update, int period, int duty) {
  long duration = sq_sig ? (duty / 100.0 * period) : (period - duty / 100.0 * period);
  if (millis() - last_update >= duration) {
    last_update = millis();
    sq_sig = !sq_sig;
  }
}

// ---------------------------
// Main loop
// ---------------------------
void loop() {

  // --- Serial command handling ---
  while (Serial.available() > 0) {
    String serialReceived = Serial.readStringUntil('\n');
    Serial.print("Received: ");
    Serial.println(serialReceived);

    char cmdChar = serialReceived.charAt(0);
    String args = serialReceived.substring(1);

    switch (cmdChar) {
      case 'A':
        if (!atomize_a) atomization_start_a = millis();
        atomize_a = true;
        Serial.println("Atomizer A activated");
        break;

      case 'B':
        if (!atomize_b) atomization_start_b = millis();
        atomize_b = true;
        Serial.println("Atomizer B activated");
        break;

      case 'C':
        if (!atomize_c) atomization_start_c = millis();
        atomize_c = true;
        Serial.println("Atomizer C activated");
        break;

      case 'D':
        if (!atomize_d) atomization_start_d = millis();
        atomize_d = true;
        Serial.println("Atomizer D activated");
        break;

      case 'a':
        if (atomize_a) {
          Serial.print("Atomizer A stopped after ");
          Serial.print(millis() - atomization_start_a);
          Serial.println(" ms");
        }
        atomize_a = false;
        break;

      case 'b':
        if (atomize_b) {
          Serial.print("Atomizer B stopped after ");
          Serial.print(millis() - atomization_start_b);
          Serial.println(" ms");
        }
        atomize_b = false;
        break;

      case 'c':
        if (atomize_c) {
          Serial.print("Atomizer C stopped after ");
          Serial.print(millis() - atomization_start_c);
          Serial.println(" ms");
        }
        atomize_c = false;
        break;

      case 'd':
        if (atomize_d) {
          Serial.print("Atomizer D stopped after ");
          Serial.print(millis() - atomization_start_d);
          Serial.println(" ms");
        }
        atomize_d = false;
        break;

      case 'L':
        fanless_mode = true;
        Serial.println("Fanless mode enabled");
        break;

      case 'S':
        if (atomize_a) {Serial.print("Atomizer A stopped after "); Serial.print(millis() - atomization_start_a); Serial.println("ms");}
        if (atomize_b) {Serial.print("Atomizer B stopped after "); Serial.print(millis() - atomization_start_b); Serial.println("ms");}
        if (atomize_c) {Serial.print("Atomizer C stopped after "); Serial.print(millis() - atomization_start_c); Serial.println("ms");}
        if (atomize_d) {Serial.print("Atomizer D stopped after "); Serial.print(millis() - atomization_start_d); Serial.println("ms");}

        atomize_a = atomize_b = atomize_c = atomize_d = false;
        fanless_mode = false;
        break;

      case 'E':
        if (sscanf(args.c_str(), "%d;%d", &atomization_period_a, &atomization_duty_cycle_a) == 2) {
          Serial.print("Atomizer A config: period = "); Serial.print(atomization_period_a);
          Serial.print(" ms, duty = "); Serial.print(atomization_duty_cycle_a); Serial.println(" %");
        } else Serial.println("Invalid Atomizer A config");
        break;

      case 'F':
        if (sscanf(args.c_str(), "%d;%d", &atomization_period_b, &atomization_duty_cycle_b) == 2) {
          Serial.print("Atomizer B config: period = "); Serial.print(atomization_period_b);
          Serial.print(" ms, duty = "); Serial.print(atomization_duty_cycle_b); Serial.println(" %");
        } else Serial.println("Invalid Atomizer B config");
        break;

      case 'G':
        if (sscanf(args.c_str(), "%d;%d", &atomization_period_c, &atomization_duty_cycle_c) == 2) {
          Serial.print("Atomizer C config: period = "); Serial.print(atomization_period_c);
          Serial.print(" ms, duty = "); Serial.print(atomization_duty_cycle_c); Serial.println(" %");
        } else Serial.println("Invalid Atomizer C config");
        break;

      case 'H':
        if (sscanf(args.c_str(), "%d;%d", &atomization_period_d, &atomization_duty_cycle_d) == 2) {
          Serial.print("Atomizer D config: period = "); Serial.print(atomization_period_d);
          Serial.print(" ms, duty = "); Serial.print(atomization_duty_cycle_d); Serial.println(" %");
        } else Serial.println("Invalid Atomizer D config");
        break;

      case '?':
        Serial.print("Diffusing: "); Serial.println(is_diffusing ? "YES" : "NO");
        break;

      default:
        Serial.println("Unknown command");
        break;
    }
  }

  // --- Update atomizations ---
  if (atomize_a) update_atomization(atomization_sq_sig_a, last_atomization_update_a, atomization_period_a, atomization_duty_cycle_a);
  if (atomize_b) update_atomization(atomization_sq_sig_b, last_atomization_update_b, atomization_period_b, atomization_duty_cycle_b);
  if (atomize_c) update_atomization(atomization_sq_sig_c, last_atomization_update_c, atomization_period_c, atomization_duty_cycle_c);
  if (atomize_d) update_atomization(atomization_sq_sig_d, last_atomization_update_d, atomization_period_d, atomization_duty_cycle_d);

  is_diffusing = atomize_a || atomize_b || atomize_c || atomize_d;

  // --- Atomizer fan ---
  analogWrite(ATOMIZER_FAN_PIN, fanless_mode ? 0 : ATOMIZATION_FAN_SPEED);

  // --- Atomizer outputs ---
  digitalWrite(ATOMIZER_A_PIN, atomize_a && atomization_sq_sig_a ? HIGH : LOW);
  digitalWrite(ATOMIZER_B_PIN, atomize_b && atomization_sq_sig_b ? HIGH : LOW);
  digitalWrite(ATOMIZER_C_PIN, atomize_c && atomization_sq_sig_c ? HIGH : LOW);
  digitalWrite(ATOMIZER_D_PIN, atomize_d && atomization_sq_sig_d ? HIGH : LOW);

  // --- Extraction fan ---
  if (!is_diffusing && was_diffusing) extraction_fan_start_time = millis();
  was_diffusing = is_diffusing;

  if (extraction_fan_start_time != -1) {
    if (millis() - extraction_fan_start_time <= EXTRACTION_DURATION) {
      analogWrite(EXTRACTION_FAN_PIN, EXTRACTION_FAN_SPEED);
    } else {
      analogWrite(EXTRACTION_FAN_PIN, 0);
      extraction_fan_start_time = -1;
    }
  } else {
    analogWrite(EXTRACTION_FAN_PIN, 0); // Ensure fan is off by default
  }
}