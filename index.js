var admin = require("firebase-admin");

admin.initializeApp ({
 credentials: admin.credential.applicationDefault()
});

var db = admin.firestore();

exports.helloPubSub = (event, context) => {
  
  const pubsubMessage = event;
  var data = Buffer.from(pubsubMessage.data, 'base64').toString(); //Will make the pubsubMessage a String.
  var deviceId = pubsubMessage.attributes.device_id; 
  var timeSubmit = pubsubMessage.attributes.published_at;
  var dataObject = JSON.parse(data); //This will parse the data into Json 
  
  var docRef = db.collection('Environmental Data'); //This will create the collection if not created. Then it will create a new document to the collection. 
  //This will assign the data to different fields within the document.
  var setData = docRef.add({
    temperature:dataObject.temp,
    humidity:dataObject.humidity,
    dB:dataObject.dB,
    longitude:dataObject.GPS_Long,
    latitude:dataObject.GPS_Lat,
    fuel:dataObject.Fuel,
    device:deviceId,
    time:timeSubmit,
    timestamp: Date.now()
  });
};
