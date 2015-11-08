﻿using System;
using System.Collections.Generic;
using System.Reflection;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using System.Management;

namespace Org.BeyondComputing.NewRelic.HyperV
{
    class PluginAgent : Agent
    {
        // Name of Agent
        private string name;

        // Provides logging for Plugin
        private Logger log = Logger.GetLogger(typeof(PluginAgent).Name);

        /// <summary>
        /// Constructor for Agent Class
        /// Accepts name and other parameters from plugin.json file
        /// </summary>
        /// <param name="name"></param>
        public PluginAgent(string name)
        {
            this.name = name;
        }

        #region "NewRelic Methods"
        /// <summary>
        /// Provides the GUID which New Relic uses to distiguish plugins from one another
        /// Must be unique per plugin
        /// </summary>
        public override string Guid
        {
            get
            {
                return "org.beyondcomputing.newrelic.hyperv";
            }
        }

        /// <summary>
        /// Provides the version information to New Relic.
        /// Uses the 
        /// </summary>
        public override string Version
        {
            get
            {
                return typeof(PluginAgent).Assembly.GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Returns a human-readable string to differentiate different hosts/entities in the New Relic UI
        /// </summary>
        public override string GetAgentName()
        {
            return this.name;
        }

        /// <summary>
        /// This is where logic for fetching and reporting metrics should exist.  
        /// Call off to a REST head, SQL DB, virtually anything you can programmatically 
        /// get metrics from and then call ReportMetric.
        /// </summary>
        public override void PollCycle()
        {
            try
            {
                log.Info($"Collecting information from server: {this.name}");
                ManagementObjectCollection vmDetails = hyperv.GetVMDetails(this.name);

                foreach (ManagementObject vm in vmDetails)
                {
                    // Exclude host from metrics - just want VM's
                    if (vm["Description"].ToString() != "Microsoft Hosting Computer System")
                    {
                        // Report Metrics to New Relic
                        ReportVMMetrics(vm);
                        ReportVMUptime(vm);
                        ReportReplicationHealth(vm);
                        ReportVMHealth(vm);
                    }
                }
            }
            catch
            {
                log.Error("Unable to fetch information from the Hyper-v host '{0}'", this.name);
            }
        }

        #endregion

        private void ReportVMMetrics(ManagementObject vm)
        {
            // Name of VM
            string name = vm["ElementName"].ToString();

            // Create Settings
            ManagementObject vmSettings = hyperv.GetVirtualSystemSettingData(this.name, vm["__PATH"].ToString());
            ManagementObject vsms = hyperv.GetVSMS(this.name);

            // Get VM Settings
            ManagementBaseObject inParams = vsms.GetMethodParameters("GetSummaryInformation");
            inParams["SettingData"] = new string[1] { vmSettings["__PATH"].ToString() };

            // Request CPU Information
            inParams["RequestedInformation"] = new int[4] { 4, 100, 101, 103};

            ManagementBaseObject outParams = vsms.InvokeMethod("GetSummaryInformation", inParams, null);
            ManagementBaseObject[] value = (ManagementBaseObject[])outParams.GetPropertyValue("SummaryInformation");

            UInt16 state = (UInt16)value[0].GetPropertyValue("EnabledState");
            ReportMetric($"vms/{name}/numberofprocessors", "procs", (UInt16)value[0].GetPropertyValue("NumberOfProcessors"));

            if (state == 2)
            {
                ReportMetric($"vms/{name}/processorload", "percent", (UInt16)value[0].GetPropertyValue("ProcessorLoad"));
                ReportMetric($"vms/{name}/memoryused", "mibibytes", (UInt64)value[0].GetPropertyValue("MemoryUsage"));
            }

            // Cleanup
            inParams.Dispose();
            outParams.Dispose();
            vmSettings.Dispose();
            vsms.Dispose();
        }

        private void ReportVMUptime(ManagementObject vm)
        {
            // Name of VM
            string name = vm["ElementName"].ToString();

            // Uptime of VM
            UInt64 UpTimeDays = UInt64.Parse(vm["OnTimeInMilliseconds"].ToString()) / (1000 * 60 * 60 * 24);
            UInt64 UpTimeHours = UInt64.Parse(vm["OnTimeInMilliseconds"].ToString()) / (1000 * 60 * 60);
            UInt64 UpTimeMinutes = UInt64.Parse(vm["OnTimeInMilliseconds"].ToString()) / (1000 * 60);

            ReportMetric($"vms/{name}/UpTimeInDays", "Days", UpTimeDays);
            ReportMetric($"vms/{name}/UpTimeInHours", "Hours", UpTimeHours);
            ReportMetric($"vms/{name}/UpTimeInMinutes", "Minutes", UpTimeMinutes);
        }

        private void ReportVMHealth(ManagementObject vm)
        {
            // Name of VM
            string name = vm["ElementName"].ToString();

            // Health of VM
            hyperv.NewRelicMetric HealthMetric;
            HealthMetric = hyperv.GetHealthState(vm);
            ReportMetric($"vms/{name}/health", "errors", HealthMetric.Metric);
        }

        private void ReportReplicationHealth(ManagementObject vm)
        {
            // Name of VM
            string name = vm["ElementName"].ToString();

            // Get Replication Mode
            hyperv.NewRelicMetric ReplicationMode;
            ReplicationMode = hyperv.GetReplicationMode(vm);

            // Get Replication Health Status
            hyperv.NewRelicMetric ReplicationHealth;
            ReplicationHealth = hyperv.GetReplicationHealth(vm);

            if (ReplicationMode.Metric == 1)
            {
                // Primary Replication Node
                log.Info($"Primary Node {name} - Health: {ReplicationHealth.Description}");
                ReportMetric($"replication/primary/{name}/health", "errors", ReplicationHealth.Metric);
            }
            else if (ReplicationMode.Metric != 0)
            {
                // Secondary Replication Node
                log.Info($"Secondary Node {name} - Health: {ReplicationHealth.Description}");
                ReportMetric($"replication/secondary/{name}/health", "errors", ReplicationHealth.Metric);
            }

        }
    }
}
