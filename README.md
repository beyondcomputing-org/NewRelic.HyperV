# Org.BeyondComputing.NewRelic.HyperV
New Relic plugin for Microsoft HyperV.  Currently monitors: CPU, memory, VM health, replication health and allows alerting based on replication issues or VM health issues.  The plugin is a work in progress so please let me know what features are most important or if you have any issues.

# Requirements
1. Plugin .Net 4.5.2 or 4.6
2. New Relic NPI installer requires .Net 2.0/3.5
3. Microsoft HyperV

# Known Issues

# Configuration

# Installation
1. Download release and unzip on machine to handle monitoring.
2. Edit Config Files
    rename newrelic.template.json to newrelic.json
    Rename plugin.template.json to plugin.json
    Update settings in both config files for your environment
3. Run plugin.exe from Command line (Run as Administrator)

Use NPI to install the plugin to register as a service

1. Run Command as admin: npi install org.beyondcomputing.newrelic.hyperv
2. Follow on screen prompts
