#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <math.h>

// TankManagement: telemetría y comandos por USB/serial a la API.
// Telemetría (cada API_INTERVAL_MS): <tanqueId>;<distanciaCm>  -> 1;25.50
// Comando API: CMD;<tanqueId>;FILL80
// Confirmación: ACK;<tanqueId>;FILL80;<estado>[;<detalle>]

const int TANQUE_ID = 3;
const int TRIG_PIN = 5;
const int ECHO_PIN = 18;
const int RELAY_PIN = 19;

// Parámetros físicos estáticos del tanque. El sensor se considera montado en el borde superior.
const float ALTURA_TANQUE_CM = 17.0f;
const float DIAMETRO_TANQUE_CM = 10.0f;
const float NIVEL_INICIO_AUTO_PCT = 10.0f;
const float NIVEL_OBJETIVO_PCT = 80.0f;
const bool RELE_ACTIVO_EN_HIGH = true;

const unsigned long SENSOR_INTERVAL_MS = 500;
const unsigned long API_INTERVAL_MS = 5000;
const unsigned long LCD_INTERVAL_MS = 1000;

LiquidCrystal_I2C lcd(0x27, 16, 2);
unsigned long lastSensor = 0;
unsigned long lastApi = 0;
unsigned long lastLcd = 0;
float distanciaCm = -1.0f;
float porcentajeLlenado = 0.0f;
float volumenActualCm3 = 0.0f;
bool valvulaAbierta = false;
bool llenadoHasta80Activo = false;

float calcularPorcentaje(float distancia) {
  if (distancia < 0.0f) return -1.0f;
  float alturaAgua = constrain(ALTURA_TANQUE_CM - distancia, 0.0f, ALTURA_TANQUE_CM);
  return (alturaAgua / ALTURA_TANQUE_CM) * 100.0f;
}

float calcularVolumenCm3(float porcentaje) {
  if (porcentaje < 0.0f) return -1.0f;
  const float radioCm = DIAMETRO_TANQUE_CM / 2.0f;
  const float volumenTotal = PI * radioCm * radioCm * ALTURA_TANQUE_CM;
  return volumenTotal * constrain(porcentaje, 0.0f, 100.0f) / 100.0f;
}

float leerDistancia() {
  const int muestras = 5;
  float suma = 0.0f;
  int validas = 0;

  for (int i = 0; i < muestras; i++) {
    digitalWrite(TRIG_PIN, LOW);
    delayMicroseconds(2);
    digitalWrite(TRIG_PIN, HIGH);
    delayMicroseconds(10);
    digitalWrite(TRIG_PIN, LOW);

    unsigned long duracion = pulseIn(ECHO_PIN, HIGH, 30000);
    if (duracion > 0) {
      float d = duracion * 0.0343f / 2.0f;
      if (d >= 2.0f && d <= 400.0f) {
        suma += d;
        validas++;
      }
    }
    delay(10);
  }

  return validas ? suma / validas : -1.0f;
}

void setValvula(bool abierta) {
  valvulaAbierta = abierta;
  bool salida = RELE_ACTIVO_EN_HIGH ? abierta : !abierta;
  digitalWrite(RELAY_PIN, salida ? HIGH : LOW);
}

// Tanto la solicitud de la API como el modo automático usan esta misma función.
bool iniciarLlenadoHasta80() {
  if (distanciaCm < 0.0f || porcentajeLlenado < 0.0f) {
    setValvula(false);
    llenadoHasta80Activo = false;
    return false;
  }

  if (porcentajeLlenado >= NIVEL_OBJETIVO_PCT) {
    setValvula(false);
    llenadoHasta80Activo = false;
    return true;  // Ya estaba en el nivel objetivo.
  }

  llenadoHasta80Activo = true;
  setValvula(true);
  return true;
}

void enviarAck(int requestedTankId, const char* estado, const char* detalle = nullptr) {
  Serial.print("ACK;");
  Serial.print(requestedTankId);
  Serial.print(";FILL80;");
  Serial.print(estado);
  if (detalle != nullptr) {
    Serial.print(";");
    Serial.print(detalle);
  }
  Serial.println();
}

void procesarComandosSerial() {
  while (Serial.available() > 0) {
    String linea = Serial.readStringUntil('\n');
    linea.trim();
    if (linea.length() == 0) continue;

    if (!linea.startsWith("CMD;")) continue;

    int separador = linea.indexOf(';', 4);
    if (separador < 0) continue;

    int requestedTankId = linea.substring(4, separador).toInt();
    String comando = linea.substring(separador + 1);
    comando.trim();

    if (comando != "FILL80") continue;
    if (requestedTankId != TANQUE_ID) {
      enviarAck(requestedTankId, "ERROR", "ID_MISMATCH");
      continue;
    }

    if (!iniciarLlenadoHasta80()) {
      enviarAck(requestedTankId, "ERROR", "SENSOR_INVALIDO_VALVULA_CERRADA");
    } else if (porcentajeLlenado >= NIVEL_OBJETIVO_PCT) {
      enviarAck(requestedTankId, "ALREADY_AT_TARGET");
    } else {
      enviarAck(requestedTankId, "STARTED");
    }
  }
}

void actualizarControlLlenado() {
  if (distanciaCm < 0.0f || porcentajeLlenado < 0.0f) {
    // Fail-safe: ante pérdida de lectura se corta cualquier llenado en curso.
    if (llenadoHasta80Activo || valvulaAbierta) {
      setValvula(false);
      llenadoHasta80Activo = false;
    }
    return;
  }

  if (llenadoHasta80Activo) {
    if (porcentajeLlenado >= NIVEL_OBJETIVO_PCT) {
      setValvula(false);
      llenadoHasta80Activo = false;
    } else {
      setValvula(true);
    }
    return;
  }

  // Inicio automático al llegar al 10 % o menos; la misma rutina cierra al alcanzar el 80 %.
  if (porcentajeLlenado <= NIVEL_INICIO_AUTO_PCT) {
    iniciarLlenadoHasta80();
  } else {
    setValvula(false);
  }
}

void enviarSerial() {
  if (distanciaCm < 0.0f) return;
  Serial.print(TANQUE_ID);
  Serial.print(';');
  Serial.println(distanciaCm, 2);
}

void mostrarLCD() {
  lcd.setCursor(0, 0);
  lcd.print("                ");
  lcd.setCursor(0, 0);
  if (distanciaCm < 0.0f) {
    lcd.print("Dist: ERROR");
  } else {
    lcd.print("Dist:");
    lcd.print(distanciaCm, 1);
    lcd.print("cm");
  }

  lcd.setCursor(0, 1);
  lcd.print("                ");
  lcd.setCursor(0, 1);
  if (porcentajeLlenado < 0.0f) {
    lcd.print("Nivel: ERROR");
  } else {
    lcd.print("Niv:");
    lcd.print((int)round(porcentajeLlenado));
    lcd.print("% V:");
    lcd.print(valvulaAbierta ? "ON" : "OFF");
  }
}

void setup() {
  Serial.begin(115200);
  Serial.setTimeout(20);
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);
  pinMode(RELAY_PIN, OUTPUT);
  setValvula(false);

  lcd.init();
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print("Tank Management");
  lcd.setCursor(0, 1);
  lcd.print("Inicializando");
}

void loop() {
  procesarComandosSerial();
  const unsigned long now = millis();

  if (now - lastSensor >= SENSOR_INTERVAL_MS) {
    lastSensor = now;
    distanciaCm = leerDistancia();
    porcentajeLlenado = calcularPorcentaje(distanciaCm);
    volumenActualCm3 = calcularVolumenCm3(porcentajeLlenado);
    actualizarControlLlenado();
  }

  if (now - lastApi >= API_INTERVAL_MS) {
    lastApi = now;
    enviarSerial();
  }

  if (now - lastLcd >= LCD_INTERVAL_MS) {
    lastLcd = now;
    mostrarLCD();
  }
}
