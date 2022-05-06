using System;
using System.Runtime.Serialization;
using System.Windows;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace Data
{
    [FirestoreData]
    [Serializable()]
    public class deviceEnvironmentalData : ISerializable
    {
        [FirestoreProperty]
        public String device { get; set; }
        [FirestoreProperty]
        public String time { get; set; }
        [FirestoreProperty]
        public String latitude { get; set; }
        [FirestoreProperty]
        public String longitude { get; set; }
        [FirestoreProperty]
        public double temperature { get; set; }
        [FirestoreProperty]
        public double dB { get; set; }
        [FirestoreProperty]
        public double humidity { get; set; }
        [FirestoreProperty]
        public String fuel { get; set; }
        [FirestoreProperty]
        public long timestamp { get; set; }
        
        /// <summary>
        /// This will return a string of data from the the most recent reading from the device.. 
        /// </summary>
        /// <returns></returns>
        public String printData()
        {
            String printAllData;
            printAllData = "Device: " + device + "\n" + "Time: " + time + "\n" + "Latitude: " + latitude + "\n";
            printAllData += "Longitude: " + longitude + "\n" + "Temperature: " + temperature.ToString() + "\n";
            printAllData += "dB: " + dB.ToString() + "\n" + "Humidity: " + humidity.ToString() + "\n" + "Battery: " + fuel;
            return printAllData;
        }
        /// <summary>
        /// This is return the device ID.
        /// </summary>
        /// <returns></returns>
        public String getDeviceId()
        {
            return device;
        }
        /// <summary>
        /// This will return the timestamp.
        /// </summary>
        /// <returns></returns>
        public long getTimeStamp()
        {
            return timestamp;
        }
        /// <summary>
        /// This will return the entries temperature. 
        /// </summary>
        /// <returns></returns>
        public double getTemp()
        {
            return temperature;
        }
        /// <summary>
        /// This will return the entries decibel level.
        /// </summary>
        /// <returns></returns>
        public double getDecibels()
        {
            return dB;
        }
        /// <summary>
        /// This will return the entries humidity percentage.
        /// </summary>
        /// <returns></returns>
        public double getHumidity()
        {
            return humidity;
        }
        /// <summary>
        /// This method will enable us to write this object as JSON to a text file. 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("device", device);
            info.AddValue("time", time);
            info.AddValue("latitude", latitude);
            info.AddValue("longitude", longitude);
            info.AddValue("temperature", temperature);
            info.AddValue("dB", dB);
            info.AddValue("humidity", humidity);
            info.AddValue("timestamp", timestamp);
        }
    }
}
