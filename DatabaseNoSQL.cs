using System;
using Google.Cloud.Firestore;
using DocumentReference = Google.Cloud.Firestore.DocumentReference;
using System.Windows;
using System.Collections.Generic;
using Data;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class DatabaseNoSQL
{
    List<String> distinctIds = new List<String>();
    private String fileName = "Hello";

    /// <summary>
    /// This method will create a list with all the entries from the last 24 hours.
    /// Then it will compute the average temperature, humidity, and decibel level. 
    /// Finally, it will return an array with the computed values and the number of entries.
    /// </summary>
    /// <param name="device"></param>
    public async Task<double[]> queryForDeviceEntriesAverage(String device)
    {
        List<deviceEnvironmentalData> allEntiresFromDevice = new List<deviceEnvironmentalData>();
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        long rangeOfOneDayUpperBounds = currentTime.ToUnixTimeMilliseconds();
        const long MILLISECONDS_IN_A_DAY = 86400000;
        long rangeOfOneDayLowerBounds = rangeOfOneDayUpperBounds - MILLISECONDS_IN_A_DAY;

        double totalDecibels = 0;
        double totalTemp = 0;
        double totalHumidity = 0;

        String path = AppDomain.CurrentDomain.BaseDirectory + @"practicum.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        FirestoreDb database = FirestoreDb.Create("uw-oshkosh-final-practicum");

        Query qref = database.Collection("Environmental Data")
            .OrderByDescending("timestamp"); 
        //This will query the database for all entries wil timestamps ordered from newest to oldest.
        QuerySnapshot snap = await qref.GetSnapshotAsync();
        foreach(DocumentSnapshot docsnap in snap)
        {
            deviceEnvironmentalData currData = docsnap.ConvertTo<deviceEnvironmentalData>();
            if (docsnap.Exists)
            {
                if (currData.getDeviceId() == device && currData.getTimeStamp() > rangeOfOneDayLowerBounds && currData.getTimeStamp() < rangeOfOneDayUpperBounds)
                {
                    allEntiresFromDevice.Add(currData);
                    totalDecibels += currData.getDecibels();
                    totalTemp += currData.getTemp();
                    totalHumidity += currData.getHumidity();
                } 
            }
        }
        //This is where the averages are calculated.
        double averageDecibels = Math.Round(totalDecibels / allEntiresFromDevice.Count, 1);
        double averageTemp = Math.Round(totalTemp / allEntiresFromDevice.Count(), 1);
        double averageHumidity = Math.Round(totalHumidity / allEntiresFromDevice.Count(), 1);
        //This will create the array to be returned and populate it.
        double[] averageData = new double[4];
        averageData[0] = averageDecibels;
        averageData[1] = averageTemp;
        averageData[2] = averageHumidity;
        averageData[3] = allEntiresFromDevice.Count();
        return averageData;
    }

    /// <summary>
    /// This will take in a device ID and return its most recent entry. 
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public async Task<deviceEnvironmentalData> queryForLatestEntry(String device)
    {
        //This will connect to the database.
        String path = AppDomain.CurrentDomain.BaseDirectory + @"practicum.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        FirestoreDb database = FirestoreDb.Create("uw-oshkosh-final-practicum");
        //This will query the NoSQL database for the matching device ID. 
        Query qref = database.Collection("Environmental Data")
            .WhereEqualTo("device", device);
       
        List<deviceEnvironmentalData> findMostRecent = new List<deviceEnvironmentalData>();
        long largestTimeStamp = 0; //Keeps track of the largest timestamp 
        int locationOfNewest = 0; //This will keep track of the location of the most recent entry.
        int index = 0; //I used a index to keep track of the current location in the foreach loop. 
        //This will find the most recent entry added based on timestamp.
        QuerySnapshot snap = await qref.GetSnapshotAsync();
        foreach (DocumentSnapshot docsnap in snap)
        {
            deviceEnvironmentalData currData = docsnap.ConvertTo<deviceEnvironmentalData>();
            if (docsnap.Exists)
            {
                findMostRecent.Add(currData);
                if(largestTimeStamp <= currData.getTimeStamp())
                {
                    largestTimeStamp = currData.getTimeStamp();
                    locationOfNewest = index;
                }
                index++;
            }
        }
        return findMostRecent[locationOfNewest];
    }

    /// <summary>
    /// This will return the list of distinct device IDs.
    /// </summary>
    /// <returns></returns>
    public List<String> returnAllDeviceIds()
    {
        getAllDeivces();
        return distinctIds;
    }

    /// <summary>
    /// This will query the database for all distinct devices. 
    /// </summary>
    private async void getAllDeivces()
    {
        //This will connect to Firestore.
        String path = AppDomain.CurrentDomain.BaseDirectory + @"practicum.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        FirestoreDb database = FirestoreDb.Create("uw-oshkosh-final-practicum");
        //This will query the database for all entires to find all distinct IDs.
        Query qref = database.Collection("Environmental Data");
        QuerySnapshot snap = await qref.GetSnapshotAsync();

        List<String> allDeviceIds = new List<String>();
        //This will put all the device IDs in a list.
        foreach (DocumentSnapshot docsnap in snap)
        {
            deviceEnvironmentalData currData = docsnap.ConvertTo<deviceEnvironmentalData>();
            if (docsnap.Exists)
            {
                allDeviceIds.Add(currData.getDeviceId());
            }
        }
        distinctIds = allDeviceIds.Distinct().ToList();//This will populate the distinctIds List<String>.
    }

    /// <summary>
    /// This method will take in a device id as a string and print data to a text file in JSON. 
    /// </summary>
    /// <param name="device"></param>
    public async void flatFilePrint(String device)
    {
        fileName = device;
        List<deviceEnvironmentalData> allEntiresFromDevice = new List<deviceEnvironmentalData>();
        //This will connect to Firestore.
        String path = AppDomain.CurrentDomain.BaseDirectory + @"practicum.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        FirestoreDb database = FirestoreDb.Create("uw-oshkosh-final-practicum");
        //This will query the database for all entires in the order they were uploaded. 
        Query qref = database.Collection("Environmental Data")
            .OrderByDescending("timestamp"); 
        QuerySnapshot snap = await qref.GetSnapshotAsync();
        //This will add entries to a list where the deivce ids match the input string. 
        foreach (DocumentSnapshot docsnap in snap)
        {
            deviceEnvironmentalData currData = docsnap.ConvertTo<deviceEnvironmentalData>();
            if (docsnap.Exists)
            {
                if (currData.getDeviceId() == device)
                {
                    allEntiresFromDevice.Add(currData);
                   
                }
            }
        }
        
        //This will create a file if it doesn't exist.
        //It will delete the file if it exists and recreates it with current data. 
        if (File.Exists(this.fileName))
        {
            File.Delete(this.fileName);
            var open = File.Create(this.fileName);
            open.Close();
        }
        else
        {
            var open = File.Create(this.fileName);
            open.Close();
        }
        //This will write to the newly created file. 
        var jsonFinalString = JsonConvert.SerializeObject(allEntiresFromDevice, Formatting.Indented);
        File.WriteAllText(this.fileName, jsonFinalString);
    }
}
