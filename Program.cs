using System;
using System.Management;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            ManagementScope scope = new ManagementScope(@"\\.\root\wmi");

            // Retrieve full charged capacity
            ObjectQuery fullChargeQuery = new ObjectQuery("Select * from BatteryFullChargedCapacity");
            ManagementObjectSearcher fullChargeSearcher = new ManagementObjectSearcher(scope, fullChargeQuery);

            // Retrieve designed capacity
            ObjectQuery designedCapacityQuery = new ObjectQuery("Select * from BatteryStaticData");
            ManagementObjectSearcher designedCapacitySearcher = new ManagementObjectSearcher(scope, designedCapacityQuery);

            // Retrieve battery status
            ObjectQuery statusQuery = new ObjectQuery("Select * from BatteryStatus where Voltage > 0");
            ManagementObjectSearcher statusSearcher = new ManagementObjectSearcher(scope, statusQuery);

            // Retrieve cycle count
            ObjectQuery cycleCountQuery = new ObjectQuery("Select * from BatteryCycleCount");
            ManagementObjectSearcher cycleCountSearcher = new ManagementObjectSearcher(scope, cycleCountQuery);

            var fullChargedCapacities = new Dictionary<int, uint>();
            var designedCapacities = new Dictionary<int, uint>();
            var cycleCounts = new Dictionary<int, uint>();

            // Some host info
            Console.WriteLine($"Machine Name: {Environment.MachineName}");

            /// motherboardinfo
            /// 
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("Motherboard: {0}", queryObj["Product"]);
                    Console.WriteLine("Manufacturer: {0}", queryObj["Manufacturer"]);
                    Console.WriteLine("Serial: {0}", queryObj["SerialNumber"]); 
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for motherboard WMI data: " + e.Message);
            }

            //Curremt time
            Console.WriteLine($"Current Time: {DateTime.Now}");
            Console.WriteLine();
            Console.WriteLine("Battery Information =====================");
            int i = 0;
            try
            {
                
                foreach (ManagementObject battery in fullChargeSearcher.Get())
                {
                    uint fullChargedCapacity = (uint)battery["FullChargedCapacity"];
                    fullChargedCapacities[i] = fullChargedCapacity;
                    Console.WriteLine($"Battery {i} Fully Charged Capacity: {fullChargedCapacity} mWh");
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while querying for battery WMI data: " + e.Message);
            }

            i = 0;
            try
            {
                foreach (ManagementObject battery in designedCapacitySearcher.Get())
                {
                    uint designedCapacity = (uint)battery["DesignedCapacity"];
                    designedCapacities[i] = designedCapacity;
                    Console.WriteLine($"Battery {i} Designed Capacity: {designedCapacity} mWh");

                    if (fullChargedCapacities.ContainsKey(i))
                    {
                        uint fullChargedCapacity = fullChargedCapacities[i];
                        Console.WriteLine($"Battery {i} Health: {100 * fullChargedCapacity / designedCapacity}%");
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while querying for battery WMI data: " + e.Message);
            }
            try
            {
                i = 0;
                foreach (ManagementObject battery in cycleCountSearcher.Get())
                {
                    uint cycleCount = (uint)battery["CycleCount"];
                    cycleCounts[i] = cycleCount;
                    Console.WriteLine($"Battery {i} Cycle Count: {cycleCount}");
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while querying for battery WMI data: " + e.Message);
            }
            try
            {

                // Retrieve and display additional battery status information
                i = 0;
                foreach (ManagementObject battery in statusSearcher.Get())
                {
                    Console.WriteLine($"\nBattery {i} ***************");
                    Console.WriteLine($"Tag:               {battery["Tag"]}");
                    Console.WriteLine($"Name:              {battery["InstanceName"]}");
                    Console.WriteLine($"PowerOnline:       {battery["PowerOnline"]}");
                    Console.WriteLine($"Discharging:       {battery["Discharging"]}");
                    Console.WriteLine($"Charging:          {battery["Charging"]}");
                    Console.WriteLine($"Voltage:           {battery["Voltage"]}");
                    Console.WriteLine($"DischargeRate:     {battery["DischargeRate"]}");
                    Console.WriteLine($"ChargeRate:        {battery["ChargeRate"]}");
                    Console.WriteLine($"RemainingCapacity: {battery["RemainingCapacity"]}");
                    Console.WriteLine($"Active:            {battery["Active"]}");
                    Console.WriteLine($"Critical:          {battery["Critical"]}");
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while querying for battery WMI data: " + e.Message);
            }

            Console.WriteLine("\nPress any key to re-read the data or Ctrl+C to exit...");
            Console.ReadLine();
        }
    }
}
