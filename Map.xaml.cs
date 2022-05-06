using Data;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Practicum
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : Window
    {
        deviceEnvironmentalData selectedData = new deviceEnvironmentalData();
        DatabaseNoSQL noSql = new DatabaseNoSQL();
        public Map()
        {
            InitializeComponent();
        }

        /// <summary>
        /// If the map window is closed then the program will terminate. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// This is the method that the Main Window calls to get the selected data.
        /// </summary>
        /// <param name="selectedUnit"></param>
        public void addDataAndUpdate(String selectedUnit)
        {
            Task task = addDataAndUpateHelper(selectedUnit);
        }

        /// <summary>
        /// This will update the data on the Map class if there is new data. 
        /// </summary>
        /// <param name="selectedUnit"></param>
        /// <returns></returns>
        private async Task addDataAndUpateHelper(String selectedUnit)
        {
            
            double unitLatitude;
            double unitLongitude;
            bool validGPSLocation; 

            selectedData = await noSql.queryForLatestEntry(selectedUnit); //This will call the database class for the newest entry.
            currentDeviceData.Text = selectedData.printData();//This will print the data in a text box on the bottom right corner of the map window.
            //This will test if these are valid GPS locations.
            //The device will still report data even if the GPS location is not determined. 
            validGPSLocation = double.TryParse(selectedData.latitude, out unitLatitude); 
            double.TryParse(selectedData.longitude, out unitLongitude);
            //This will get rid of any pins on the map for an update.
            clearMap();
            //This will end the method early if no location is found.
            if (!validGPSLocation)
            {
                return;
            }
            //This will put a pin on the map if there is a valid location.
            Pushpin pin = new Pushpin();
            pin.Location = new Location(unitLatitude, unitLongitude);
            myMap.Children.Add(pin);
        }

        /// <summary>
        /// This will get rid of all the pins on the map.
        /// </summary>
        public void clearMap()
        {
            myMap.Children.Clear();
        } 
    }
}
