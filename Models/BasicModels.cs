using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationDeployerRunner.Models
{
    public class Environment
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class Computer
    {
        public string ComputerName { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return this.ComputerName;
        }
    }

    public class Component
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// A Recipe is the 
    /// </summary>
    public class ConfigFileInstances
    {
        public ConfigFileInstances()
        {
            this.ListOfRecipeConfigFiles = new List<ConfigFileInstance>();
        }
        public List<ConfigFileInstance> ListOfRecipeConfigFiles { get; set; }


        public class ConfigFileInstance
        {
            public string Component { get; set; }
            public string Environment { get; set; }
            public string ComputerName { get; set; }
            public string ConfigFilePath { get; set; }

            public override string ToString()
            {
                return string.Format("Environment: {0}  Component: {1}  Server: {2}  File: {3}", Environment, Component, ComputerName, ConfigFilePath ); 
            }
        }

    }

    public class Settings
    {
        public Settings()
        {
            this.ListOfSettings = new List<Setting>();
        }
        public List<Setting> ListOfSettings { get; set; }


        public class Setting
        {
            public Setting()
            {
                this.ListOfApplicableComponents = new List<string>();
                this.ListOfOverrides = new List<Overrides>();
            }

            public string Name { get; set; }
            public string Key { get; set; }

            public string GlobalDefaultValue { get; set; }

            public List<String> ListOfApplicableComponents { get; set; }

            public List<Overrides> ListOfOverrides { get; set; }

            public class Overrides
            {
                public string Component { get; set; }
                public string Environment { get; set; }
                public string Server { get; set; }

                public string OverrideValue { get; set; }
            }

            public override string ToString()
            {
                return this.Name + " (" + this.Key + ") : " + this.GlobalDefaultValue;
            }
        }

        public class EffectiveSetting
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public string DesiredValue { get; set; }

            /// <summary>
            /// This is the value currently detected in the config file. Retrieved by running VerifyActual
            /// </summary>
            public string EffectiveValue { get; set; }
        }
    }


    /// <summary>
    /// The object where the Master Configuration is kept. Typically the Master Config XML file is parsed into this object
    /// </summary>
    public class ConfigMasterParsed
    {
        public ConfigMasterParsed()
        {
            this.ListOfComponents = new List<Component>();
            this.ListOfEnvironments = new List<Environment>();
            this.ListOfServers = new List<Computer>();
        }
        public List<Environment> ListOfEnvironments { get; set; }
        public List<Computer> ListOfServers { get; set; }
        public List<Component> ListOfComponents { get; set; }

        /// <summary>
        /// This is where each config file is associated with a Computer, a Component and an Environment
        /// </summary>
        public ConfigFileInstances GlobalRecipe { get; set; }

        /// <summary>
        /// This is where Settings are defined
        /// Each Setting can be applied to one or more components
        /// A Setting can be global (applicable to all config files of that component)
        /// A Setting can also contain overrides, which allows unique values for a specific Server and/or Environment
        /// </summary>
        public Settings GlobalSettings { get; set; }

    }

    public class CalculatedFile
    {
        public List<Settings.EffectiveSetting> ListOfEffectiveSettings { get; set; }

        public string Environment { get; set; }
        public string ComputerName { get; set; }
        public string Component { get; set; }
        public string ConfigFilename { get; set; }

        public bool FileVerified { get; set; }
        public bool FileFound { get; set; }
    }

}
