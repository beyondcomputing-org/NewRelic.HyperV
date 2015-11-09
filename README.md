# Org.BeyondComputing.NewRelic.HyperV
New Relic plugin for Microsoft HyperV

# Requirements
1. .Net 4.5.2 or 4.6
2. Microsoft HyperV

# Known Issues

# Configuration

# Installation
1. Download release and unzip on machine to handle monitoring.
2. Edit Config Files
    rename newrelic.template.json to newrelic.json
    Rename plugin.template.json to plugin.json
    Update settings in both config files for your environment
3. Run plugin.exe from Command line

Use NPI to install the plugin to register as a service

1. Run Command as admin: npi install org.beyondcomputing.newrelic.hyperv
2. Follow on screen prompts
