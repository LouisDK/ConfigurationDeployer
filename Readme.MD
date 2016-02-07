#Configuration Deployer

The Configuration Deployer manages a collection of .NET config files (app.config or web.config).
At its heart is the Master Configuration file - an XML file where all settings are defined.
It can apply these settings to those configuration files.
It can also later verify that all the settings in those configuration files are still correct, as per the Master Configuration File.

### Use case
Sometime a system consist of many different components, each with its own .config file.
Imagine a system with 10 components, that is typically deployed across 3-5 computers.
Due to budget constraints there are 3 computers to test the system on in the QA environment, and 5 computers to run the system on in Production.
In our environment this translated to 30-40 different configuration files, each with its own unique set of settings...
**Everytime we need to deploy, someone needed to ensure that each file had the correct settings!**

>If you can store your settings in a central repository (like a database), please do so!
This Configuration Deployer project was conceived for those types of settings that you do not want to, or cannot, store in a database (e.g. a database ConnectionString)

##### Distributing and Overriding Settings
1. Some settings are "global" - their values stay the same accross all environments, all components, all computers
2. Some settings are almost global - their values stays the same everywhere, but the setting is not present in all configuration files
3. Some settings are almost unique - their values are unique per environment (e.g. a database connection string), or even per computer (e.g. QueueNames )

#### Configuration Deployer allows you to:

- Define a "Setting" with a default value
- Define where this Setting applies (which configuration files it should appear in)
- Define override values for a setting (in which environment, component, computer the setting's value should change from the default)
- Apply a "blessed" set of settings to many configuration files across many computers
- Detect and fix "configuration drift" (imagine someone who changes a setting in 1 of the 40 configuration files, thereby introducing a problem that is hard to pinpoint)

>To understand how the environments, components and computers relates to settings, and how settings can be overriden, look at the example Master Configuration XML file.

### Compatibility:

  - .NET Framework 4.0+

### Current Release:

  - This is a **pre-release**
  - It has only been tested on Windows Server 2008, Windows Server 2008 R2 and Windows 10.

# To install:

Copy these files to its own directory:

- CDR.exe
- CommandLine.dll
- NLog.dll
- NLog.config

# To Configure
Define your own Master Configuration XML file:

```csharp
<ConfigurationMaster>
  <Recipes>
    <Environment Title="QA">
      <Component Title="ComponentA">
        <Server ComputerName="svrA" ConfigFilePath="\\localhost\temp\sampleConfigs\QA-CompA-SvrA-MicroService.exe.config" />
      </Component>

      <Component Title="ComponentB">
        <Server ComputerName="svrA" ConfigFilePath="\\localhost\temp\sampleConfigs\QA-CompB-SvrA-MicroService.exe.config" />
      </Component>
    </Environment>

    <Environment Title="Prod">
      <Component Title="ComponentA">
        <Server ComputerName="svrA" ConfigFilePath="\\localhost\temp\sampleConfigs\QA-CompA-SvrA-MicroService.exe.config" />
      </Component>

      <Component Title="ComponentB">
        <Server ComputerName="svrA" ConfigFilePath="\\localhost\temp\sampleConfigs\QA-CompB-SvrA-MicroService.exe.config" />
        <Server ComputerName="svrB" ConfigFilePath="\\localhost\temp\sampleConfigs\QA-CompB-SvrB-MicroService.exe.config" />
      </Component>
    </Environment>
  </Recipes>

  <Settings>
    <Setting Name="POPIKey" Key="POPIKey" GlobalDefaultValue="$ss$d%037!">
      <ApplicableComponents>
        <Component Title="ComponentA" />
        <Component Title="ComponentB" />
      </ApplicableComponents>
    </Setting>

    <Setting Name="ConnectionString" Key="ConnectionString" GlobalDefaultValue="Data Source=.;Initial Catalog=PIRSDB;Integrated Security=True;MultipleActiveResultSets=True">
      <ApplicableComponents>
        <Component Title="ComponentA" />
        <Component Title="ComponentB" />
      </ApplicableComponents>

      <Override OverrideValue="ConnectionString" GlobalDefaultValue="Data Source=ProdDBServer;Initial Catalog=PIRSDBProd;Integrated Security=True;MultipleActiveResultSets=True">
        <Environment Title="Prod" />
      </Override>
    </Setting>

    <Setting Name="Rabbit MQ Unique Client Name" Key="RabbitMQClientName" GlobalDefaultValue="QTest">
      <ApplicableComponents>
        <Component Title="ComponentA" />
        <Component Title="ComponentB" />
      </ApplicableComponents>

      <Override OverrideValue="webA">
        <Component Title="ComponentA" />
        <Server ComputerName="svrA"/>
      </Override>

      <Override OverrideValue="webB">
        <Component Title="ComponentA" />
        <Server ComputerName="svrB"/>
      </Override>

      <Override OverrideValue="msB-svrA">
        <Component Title="ComponentB" />
        <Server ComputerName="svrA"/>
      </Override>

      <Override OverrideValue="msB-svrB">
        <Component Title="ComponentB" />
        <Server ComputerName="svrB"/>
      </Override>
    </Setting>
  </Settings>
</ConfigurationMaster>
```

# To run:


CDR.exe **action** --**parameter name** *parameter value*

Example: 
```
CDR.exe show --masterconfigfile mymasterconfig.xml --environment QA
```

##Actions
### Show
This parses the Master Configuration XML file and shows the desired values for each configuration file, per Environment, per computer
##### Parameters for the Show action:

- **MasterConfigFile** `filename` (required) _filename and path of Master Configuration XML file_
- **Environment** `EnvironmentName` (optional) _filter by environment_
- **Component** `ComponentName` (optional) _filter by Component_
- **Computer** `ComputerName` (optional) _filter by Computer_
 

Example: 
```csharp
// Shows all desired configuration settings for all environments, all components, all computers
CDR.exe show --masterconfigfile mymasterconfig.xml 

// Shows the desired configuration settings for the QA environment, all components, all computers
CDR.exe show --masterconfigfile mymasterconfig.xml --environment QA 

// Shows the desired configuration settings for the QA environment, the MicroServiceX component, all computers
CDR.exe show --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX

// Shows the desired configuration settings for the QA environment, the MicroServiceX component, the SvrJhb01 computer
CDR.exe show --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX --computer svrjhb01
```

### Verify
This parses the Master Configuration XML file and then compares the desired settings with the actual settings found in the various configuration files out there.
##### Parameters for the Verify action:

- **MasterConfigFile** `filename` (required) _filename and path of Master Configuration XML file_
- **Environment** `EnvironmentName` (optional) _filter by environment_
- **Component** `ComponentName` (optional) _filter by Component_
- **Computer** `ComputerName` (optional) _filter by Computer_
- **OnlyBad** (optional) _shows only the settings where the effective value is not the same as the desired value_

Example: 
```csharp
// Verifies all desired configuration settings for all environments, all components, all computers
CDR.exe verify --masterconfigfile mymasterconfig.xml 

// Verifies all desired configuration settings for all environments, all components, all computers. Only settings where the desired value != effective value will be shown.
CDR.exe verify --masterconfigfile mymasterconfig.xml --onlybad

// Verifies the desired configuration settings for the QA environment, all components, all computers
CDR.exe verify --masterconfigfile mymasterconfig.xml --environment QA 

// Verifies the desired configuration settings for the QA environment, the MicroServiceX component, all computers
CDR.exe verify --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX

// Verifies the desired configuration settings for the QA environment, the MicroServiceX component, the SvrJhb01 computer
CDR.exe verify --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX --computer svrjhb01
```


### Apply
This parses the Master Configuration XML file and then **applies** the desired settings to the various configuration files out there.
##### Parameters for the apply action:

- **MasterConfigFile** `filename` (required) _filename and path of Master Configuration XML file_
- **Environment** `EnvironmentName` (optional) _filter by environment_
- **Component** `ComponentName` (optional) _filter by Component_
- **Computer** `ComputerName` (optional) _filter by Computer_

Example: 
```csharp
// Applies all desired configuration settings for all environments, all components, all computers
CDR.exe apply --masterconfigfile mymasterconfig.xml 

// Applies the desired configuration settings for the QA environment, all components, all computers
CDR.exe apply --masterconfigfile mymasterconfig.xml --environment QA 

// Applies the desired configuration settings for the QA environment, the MicroServiceX component, all computers
CDR.exe apply --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX

// Applies the desired configuration settings for the QA environment, all components, just for the SvrJhb01 computer
CDR.exe apply --masterconfigfile mymasterconfig.xml --environment QA --computer svrjhb01

// Applies the desired configuration settings for the QA environment, the MicroServiceX component, the SvrJhb01 computer
CDR.exe apply --masterconfigfile mymasterconfig.xml --environment QA --component microserviceX --computer svrjhb01
```

Acknowledgements:
---
This project was developed using:

- Visual Studio 2015
- C#

This project uses:

- [CommandLineParser](https://github.com/cosmo0/commandline)
- [NLog](https://github.com/NLog)