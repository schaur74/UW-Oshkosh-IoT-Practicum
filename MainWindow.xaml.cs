using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using Data;

namespace Practicum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Timer update = new Timer(5000); //This will update the program information every 5 seconds.
        DatabaseNoSQL noSql = new DatabaseNoSQL(); //This is creating the database class.
        Map currentMap = new Map();//This is a WPF window that has the map.
        List<String> allDevices = new List<String>();//This list of Strings will store all the device IDs 
        String selectedDevice = null; //This will store the selected device in a String.
        
        public MainWindow()
        {
            InitializeComponent();
            update.Elapsed += TimerUpdate;
            update.Enabled = true;
            update.AutoReset = true;
            update.Start();
            currentMap.Show();
            getAllDeviceIdsTimer();
        }
        /// <summary>
        /// If the main window is closed then program will terminate. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }
        /// <summary>
        /// Once a device is selected the text boxes will populate with relevant data. 
        /// </summary>
        private async void updateTextBoxes()
        {
            double[] allDeviceAverages = await noSql.queryForDeviceEntriesAverage(selectedDevice);
            if (selectedDevice != null)
            {
                decibels.Text = allDeviceAverages[0].ToString() + " dB";
                temp.Text = string.Format("{0}° F", allDeviceAverages[1]);
                humidity.Text = allDeviceAverages[2].ToString() + "% Humidity";
                numberOfEntries.Text = allDeviceAverages[3].ToString() + " Entries";
            }
        }

        /// <summary>
        ///This will update the the information on the map of the currently selected device.
        ///It will keep the list box up to date. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerUpdate(object sender, ElapsedEventArgs e)
        {
            getAllDeviceIdsTimer();
           
            this.Dispatcher.Invoke(() => //This is needed due to the timer.
            {
                if (selectedDevice != null)
                {
                    currentMap.addDataAndUpdate(selectedDevice);
                    updateTextBoxes();
                }
            });
        }
        /// <summary>
        /// This will create a flat file if a device is selected. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void creatFlatFile(object sender, RoutedEventArgs e)
        {
            if (selectedDevice != null)
            {
                noSql.flatFilePrint(selectedDevice);
            }
        }
        
        /// <summary>
        /// Due to this being called from a timer thread the this.Dispatcher.Invoke becomes necessary.
        /// This will keep the list box up to date. 
        /// </summary>
        private void getAllDeviceIdsTimer()
        {
            allDevices = noSql.returnAllDeviceIds();
            this.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < allDevices.Count(); i++)
                {
                    if (!devices.Items.Contains(allDevices[i])) //This will ensure that only new devices are added when the program is running.
                    {
                        devices.Items.Add(allDevices[i]);
                    }
                }
            });
        }
        /// <summary>
        /// Once a device is selected from the list it will call addDataAndUpdateHelper in the map class.
        /// This will also select the currently selected 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void devicesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentMap.addDataAndUpdate(devices.SelectedItem.ToString());
            selectedDevice = devices.SelectedItem.ToString();
            updateTextBoxes();
            createFlatFile.IsEnabled = true;
        }
    }
}
