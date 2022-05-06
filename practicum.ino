/**** 
Name: Practicum
Author: Ryan Schauer
Description: This program will enable your device to collect data from the environment.
Note: This was made in the particle web IDE. You must download the libraries Adafruit_DHT and Adafruit_GPS.
****/ 

// This #include statement was automatically added by the Particle IDE.
#include <Adafruit_DHT.h>

// This #include statement was automatically added by the Particle IDE.
#include <Adafruit_GPS.h>

#include <math.h>

#define GPSSerial Serial1
Adafruit_GPS GPS(&GPSSerial);

#define DHTPIN 2
#define DHTTYPE DHT22
DHT dht(DHTPIN, DHTTYPE);

int getNoiseLevel = A5; //This will ensure we can read voltage to collect dB information
int collectDataPause = 15000; //This is the pause after we collect information
//These will be the time values in which the GPS loop will be able to collect location.
int gpsRegularCollectTimer = 3000; //This for for a normal operation.
int gpsConnectBeforeStart = 90000; //This will give the GPS 90 seconds to connect before we collect info.
bool isGPSConnected = false; //This will be used to send one text message saying the GPS is connected.
//These are global variables used to format everything into on publish 
double tempF = 0;
double humidity = 0;
double finalDB = 0;
String latGPS = "";
String longGPS = "";
FuelGauge fuel;

void setup() {
  Particle.publish("Your Device is now online");
  //This code will setup the GPS portion of the project
  GPS.begin(9600);
  GPS.sendCommand(PMTK_SET_NMEA_OUTPUT_RMCGGA);
  GPS.sendCommand(PMTK_SET_NMEA_UPDATE_1HZ); 
  GPS.sendCommand(PGCMD_ANTENNA);
  delay(1000);
  //This is to read from the DHT device
  dht.begin();
  //This will read from the microphone
  pinMode(getNoiseLevel, AN_INPUT);
  Serial.begin(9600);
  getGPSLocation(gpsConnectBeforeStart);
}

void loop() {
    getEnviromental();
    getVolume();
    getGPSLocation(gpsRegularCollectTimer);
    String data = String::format("{\"temp\":%.1f, \"humidity\":%.1f, \"dB\":%.1f, \"GPS_Lat\":\"%s\", \"GPS_Long\":\"%s\", \"Fuel\":\"%.1f\"}", tempF, humidity, finalDB, latGPS.c_str(), longGPS.c_str(), fuel.getSoC());
    Particle.publish("Data: ", data);
    delay(collectDataPause); //This will save data and publish operations.
}
//This method will publish the current temperature and humidity.
void getEnviromental() {
    humidity = dht.getHumidity();
    tempF = dht.getTempFarenheit();
    double overLimit = 100.00; //This double is used to ensure a valid humidity.
    double absoluteZero = -469.67; //This is to make sure there isn't a impossible value to be published. It is an error checker
    int errorWait = 2500; //I will give the device 2.5 seconds to allow it self to recover.
    //This will act as the first layer of defense against incorrect data.
    //This will not work 100% of the time so there is more layers of testing before publishing
    if (isnan(humidity) || isnan(tempF)) {
		delay(errorWait);
		getEnviromental();
		return;
    }
	
    //Will publish a valid humidity level
    //Its is possible for a 0.00 humidity level, but 0.00 always occurs when there is an error with the device.
    //This acts like a second level of defense against invalid data
    if(humidity <= 0.000000 || humidity > overLimit) {
        delay(errorWait);
	    getEnviromental();
	    return;
    }
    //Will publish a valid temperature in fahrenheit
    if(tempF < absoluteZero) {
        delay(errorWait);
		getEnviromental();
		return;
    }
}
//This method will publish the volume in dB.
void getVolume() {
    int max = 0; //This is the loudest recorded value
    int min = 4095; //This is the quietest recorded value
    int sampleCollectTime = 10000; //This will ensure that the while loop runs for ten seconds.
    int timer = millis();
    int peakToPeakVoltage = 0;
    double rmsVoltage = 0.707;
    int sample = analogRead(getNoiseLevel);
   //This while loop will collect data. It will record the loudest and quietest instance in a ten second period. 
    while(millis() - timer < sampleCollectTime) { //Ten second timer 
        sample = analogRead(getNoiseLevel);
        if(sample < 4096) {
            if (sample > max) {
                max = sample;  //This will get us our max value for peak to peak 
            } else if (sample < min){
                min = sample;  //This will get us our min value for peak to peak 
            }
       }
    }
    //This is used to calculate dB from the voltage/
    peakToPeakVoltage = max - min;
    double voltage = ((peakToPeakVoltage * 3.3) / 4095) * rmsVoltage; //It will change the analog readings to valid RMS voltage.
    double calculateRawDb = log10(voltage/0.00631) * 20; //This will take in the raw voltage and process it.
    finalDB = calculateRawDb + 94 - 44 - 25; //The 75 value is calculated from the gain and other factors.
    //Particle.publish("dB: ", String(finalDB));
} 
//This method will return the current GPS location if there is a fix.
void getGPSLocation(int sampleCollectTime) {
    bool isParsed = false;
    int timer = millis();
    
    //This allow the Boron to get the coordinates from the GPS. It needs many tries. It will either publish the required data or inform the user of an issue.
    //It usually takes 3 minutes on campus for the GPS to get a fix. This code may change in order to save data. 
    while (millis() - timer < sampleCollectTime) {
        GPS.read(); //This will Boron to read from the GPS 
        bool isParsed = false;
        //This will check if we can pasrse the incoming data from the GPS and set the received flag to false;
        if (GPS.newNMEAreceived()) {
            if (!GPS.parse(GPS.lastNMEA())) {
                isParsed = false;
            } else {
                isParsed = true;
            }
        }
        // Check if the GPS has a fix
        if (GPS.fix && isParsed == true) {
            if(isGPSConnected == false) {
                Particle.publish("GPS Connected"); //This is for an IFTTT to send me a text when the GPS is connected
                isGPSConnected = true;
            }
            
            latGPS = String(GPS.latitudeDegrees);
            longGPS = String(GPS.longitudeDegrees);
            return;                                                                                        
        }
    }
    latGPS = "No Fix";
    longGPS = "No Fix";
}
