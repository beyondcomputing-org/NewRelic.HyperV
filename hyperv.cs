using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;

namespace Org.BeyondComputing.NewRelic.HyperV
{
    class hyperv
    {
        public struct NewRelicMetric
        {
            public string Description;
            public float Metric;
        }

        public static ManagementObjectCollection GetVMDetails(string Server)
        {
            // Connect to the Hyper-V namespace
            ManagementScope manScope = new ManagementScope($@"\\{Server}\root\virtualization\v2");

            // Grab all VM info from msvm_ComputerSystem
            ObjectQuery queryObj = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem");
            ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(manScope, queryObj);
            ManagementObjectCollection vmCollection = vmSearcher.Get();

            // Return Collection
            return vmCollection;
        }

        public static ManagementObject GetVSMS(string Server)
        {
            ManagementScope scope = new ManagementScope($@"\\{Server}\root\virtualization\v2");
            scope.Connect();

            ManagementPath wmiPath = new ManagementPath("Msvm_VirtualSystemManagementService");
            ManagementClass serviceClass = new ManagementClass(scope, wmiPath, null);
            ManagementObjectCollection services = serviceClass.GetInstances();

            ManagementObject serviceObject = null;

            foreach (ManagementObject service in services)
            {
                serviceObject = service;
            }
            return serviceObject;
        }

        public static ManagementObject GetVirtualSystemSettingData(string Server, string VMpath)
        {
            // Connect to the Hyper-V namespace
            ManagementScope manScope = new ManagementScope($@"\\{Server}\root\virtualization\v2");

            // Grab settings for the specified VM
            ObjectQuery queryObj = new ObjectQuery($"ASSOCIATORS OF {{{VMpath}}} WHERE resultClass = Msvm_VirtualSystemsettingData");
            ManagementObjectSearcher vmSearcher = new ManagementObjectSearcher(manScope, queryObj);
            ManagementObjectCollection vmCollection = vmSearcher.Get();
            ManagementObject mo = vmCollection.OfType<ManagementObject>().FirstOrDefault();

            return mo;
        }

        public static NewRelicMetric GetHealthState(ManagementObject VM)
        {
            NewRelicMetric Metric = new NewRelicMetric();
            int HealthCode = Convert.ToInt32(VM["HealthState"]);

            if(HealthCode == 5)
            {
                Metric.Metric = 0;
                Metric.Description = "Healthy";
            }
            else if(HealthCode == 20)
            {
                Metric.Metric = 1;
                Metric.Description = "Major Failure";
            }
            else
            {
                Metric.Metric = 2;
                Metric.Description = "Critical Failure";
            }

            return Metric;
        }

        public static NewRelicMetric GetReplicationHealth(ManagementObject VM)
        {
            NewRelicMetric Metric = new NewRelicMetric();
            int HealthCode = Convert.ToInt32(VM["ReplicationHealth"]);

            if (HealthCode == 0)
            {
                Metric.Metric = 0;
                Metric.Description = "Not applicable";
            }
            else if (HealthCode == 1)
            {
                Metric.Metric = 0;
                Metric.Description = "Ok";
            }
            else if (HealthCode == 2)
            {
                Metric.Metric = 1;
                Metric.Description = "Warning";
            }
            else
            {
                Metric.Metric = 2;
                Metric.Description = "Critical";
            }

            return Metric;
        }

        public static NewRelicMetric GetReplicationMode(ManagementObject VM)
        {
            NewRelicMetric Metric = new NewRelicMetric();
            int HealthCode = Convert.ToInt32(VM["ReplicationMode"]);

            if (HealthCode == 0)
            {
                Metric.Metric = 0;
                Metric.Description = "None";
            }
            else if (HealthCode == 1)
            {
                Metric.Metric = 1;
                Metric.Description = "Primary";
            }
            else if (HealthCode == 2)
            {
                Metric.Metric = 2;
                Metric.Description = "Recovery";
            }
            else if (HealthCode == 3)
            {
                Metric.Metric = 3;
                Metric.Description = "Replica";
            }
            else if (HealthCode == 4)
            {
                Metric.Metric = 4;
                Metric.Description = "Extended replica";
            }

            return Metric;
        }

    }
}
