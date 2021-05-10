/*
 * Pin definition
 * Pin 30 and 31 for left motor
 * Pin 32 and 33 for right motor
 * Pin 10 for PWM to control the motor speed  
 * LED pin is 13 to indicate system status
 */
#define LED 13
#define Speed 10
#define leftForward 4  
#define leftBackward 5
#define rightForward 6
#define rightBackward 7
#define RX 0
#define TX 1

/*
 * Variable definition
 */
char data;//for bluetooth data

/*
 * Setup code
 * 
 */
void setup() {
  pinMode(LED, OUTPUT);
  pinMode(Speed, OUTPUT);
  pinMode(leftForward, OUTPUT);
  pinMode(leftBackward, OUTPUT);
  pinMode(rightForward, OUTPUT);
  pinMode(rightBackward, OUTPUT);
  analogWrite(Speed, 200); // speed of DC motor
  Serial.begin(38400);
  Serial.println("Bluetooth started on bot.");
}

/*
 * Function to move bot in FORWARD motion
 */
void leftMotorForward(){
  digitalWrite(leftForward, HIGH);
  digitalWrite(leftBackward, LOW);
}
void rightMotorForward(){
  digitalWrite(rightForward, HIGH);
  digitalWrite(rightBackward, LOW);
}

/*
 * Function to move bot in Backward motion
 */
void leftMotorBackward(){
  digitalWrite(leftForward, LOW);
  digitalWrite(leftBackward, HIGH);
}
void rightMotorBackward(){
  digitalWrite(rightForward, LOW);
  digitalWrite(rightBackward, HIGH);
}

/*
 * to Stop the bot
 */
void stopBot(){
  digitalWrite(rightForward, LOW);
  digitalWrite(rightBackward, LOW);
  digitalWrite(leftForward, LOW);
  digitalWrite(leftBackward, LOW);
}

/*
 * to move left
 */
void moveLeft(){
  digitalWrite(leftForward, LOW);
  digitalWrite(leftBackward, LOW);
  digitalWrite(rightBackward,LOW);
  digitalWrite(rightForward, HIGH);
}

/*
 * to move right
 */
void moveRight(){
  digitalWrite(leftBackward, LOW);
  digitalWrite(rightBackward,LOW);
  digitalWrite(rightForward, LOW);
  digitalWrite(leftForward, HIGH);
}

void loop() {
  if(Serial.available() > 0){ 
    data = Serial.read(); // Reads the character data from the serial port
    switch(data){
      //to move left
      case '1': moveLeft();
                Serial.println("Left");
                break;
      //to move right
      case '2': moveRight();
                Serial.println("Right");
                break;
      //to move forward
      case '3': leftMotorForward();
                rightMotorForward();
                Serial.println("Forward");
                break;
      //to move backward
      case '4': leftMotorBackward();
                rightMotorBackward();
                Serial.println("Backward");
                break;
      //to stop bot movement
      case '5': stopBot();
                Serial.println("Stop");
                break;
      default:  Serial.print("Incorrect data:- ");
                Serial.println(data);
                break;  
    }
  }
}
