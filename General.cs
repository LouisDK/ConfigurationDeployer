using ConfigurationDeployerRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConfigurationDeployerRunner
{
    public class ConfigDeployerLogic
    {
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Loads the Master Config XML file into an ConfigMasterParsed Object
        /// </summary>
        /// <param name="ConfigMasterFilename"></param>
        /// <returns></returns>
        public static ConfigMasterParsed LoadConfigMasterFile(string ConfigMasterFilename)
        {
            var CMO = new ConfigMasterParsed();

            var XMasterFile = XDocument.Load(ConfigMasterFilename);

            //// Load registered Environments
            //foreach (var env in XMasterFile.Root.Element("Environments").Elements("Environment"))
            //{
            //    CMO.ListOfEnvironments.Add(new Models.Environment()
            //    {
            //        Title = env.Attribute("Title").Value,
            //        Description = env.Attribute("Description").Value
            //    });
            //}

            //// Load registered Servers
            //foreach (var env in XMasterFile.Root.Element("Servers").Elements("Server"))
            //{
            //    CMO.ListOfServers.Add(new Models.Computer()
            //    {
            //        ComputerName = env.Attribute("ComputerName").Value,
            //        Description = env.Attribute("Description").Value
            //    });
            //}


            //// Load registered Components
            //foreach (var env in XMasterFile.Root.Element("Components").Elements("Component"))
            //{
            //    CMO.ListOfComponents.Add(new Models.Component()
            //    {
            //        Title = env.Attribute("Title").Value,
            //        Description = env.Attribute("Description").Value
            //    });
            //}

            // Load Recipe
            CMO.GlobalRecipe = new ConfigFileInstances();

            foreach (var env in XMasterFile.Root.Element("Recipes").Elements("Environment"))
            {

                foreach (var comp in env.Elements("Component"))
                {
                    
                    foreach (var svr in comp.Elements("Server"))
                    {

                        var newSvr = new ConfigFileInstances.ConfigFileInstance()
                        {
                            Environment = env.Attribute("Title").Value,
                            Component = comp.Attribute("Title").Value,
                            ComputerName = svr.Attribute("ComputerName").Value,
                            ConfigFilePath = svr.Attribute("ConfigFilePath").Value
                        };

                        CMO.GlobalRecipe.ListOfRecipeConfigFiles.Add(newSvr);
                        
                    }
                     
                }
 
            }

            // Load Settings
            CMO.GlobalSettings = new Settings();
            foreach (var sett in XMasterFile.Root.Element("Settings").Elements("Setting"))
            {
                var newSetting = new Settings.Setting();
                newSetting.Name = sett.Attribute("Name").Value;
                newSetting.Key = sett.Attribute("Key").Value;
                newSetting.GlobalDefaultValue = sett.Attribute("GlobalDefaultValue").Value;

                foreach (var applicComp in sett.Element("ApplicableComponents").Elements("Component"))
                {
                    newSetting.ListOfApplicableComponents.Add(applicComp.Attribute("Title").Value);
                }

                foreach (var overrideSetting in sett.Elements("Override"))
                {
                    var overridSet = new Settings.Setting.Overrides()
                    {
                        OverrideValue = overrideSetting.Attribute("OverrideValue").Value,
                        Component = overrideSetting?.Element("Component")?.Attribute("Title")?.Value,
                        Environment = overrideSetting?.Element("Environment")?.Attribute("Title")?.Value,
                        Server = overrideSetting?.Element("Server")?.Attribute("ComputerName")?.Value,
                    };

                    newSetting.ListOfOverrides.Add(overridSet);
                }

                CMO.GlobalSettings.ListOfSettings.Add(newSetting);
            }


            return CMO;
        }

        public static List<Settings.EffectiveSetting> CalculateThisFile(ConfigMasterParsed configMaster, string configFilename)
        {
            var Result = new List<Settings.EffectiveSetting>();

            var configFileInfo = configMaster.GlobalRecipe.ListOfRecipeConfigFiles.FirstOrDefault(q => q.ConfigFilePath == configFilename);
            if (configFileInfo != null)
            {
                var CurrentEnvironment = configFileInfo.Environment;
                var CurrentServer = configFileInfo.ComputerName;
                var CurrentComponent = configFileInfo.Component;

                var ApplicableSettings = configMaster.GlobalSettings.ListOfSettings.Where(q => q.ListOfApplicableComponents.Contains(CurrentComponent));

                foreach (var sett in ApplicableSettings)
                {
                    var newSetting = new Settings.EffectiveSetting()
                    {
                        Key = sett.Key,
                        Name = sett.Name,
                        DesiredValue = sett.GlobalDefaultValue
                    };

                    // See if we should override the value
                    var ComponentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component?.ToLower() == CurrentComponent?.ToLower() && y.Environment == null && y.Server == null);
                    if (ComponentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = ComponentOverriders?.OverrideValue;
                    }


                    var EnvironmentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component == null && y.Environment?.ToLower() == CurrentEnvironment?.ToLower() && y.Server == null);
                    if (EnvironmentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = EnvironmentOverriders?.OverrideValue;
                    }


                    var ServerOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component == null && y.Environment == null && y.Server?.ToLower() == CurrentServer?.ToLower());
                    if (ServerOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = ServerOverriders?.OverrideValue;
                    }


                    // More specific: Component and Environment
                    var ComponentEnvironmentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component?.ToLower() == CurrentComponent?.ToLower() && y.Environment?.ToLower() == CurrentEnvironment?.ToLower() && y.Server == null);
                    if (ComponentEnvironmentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = ComponentEnvironmentOverriders?.OverrideValue;
                    }


                    var ServerEnvironmentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component == null && y.Environment?.ToLower() == CurrentEnvironment?.ToLower() && y.Server?.ToLower() == CurrentServer?.ToLower());
                    if (ServerEnvironmentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = ServerEnvironmentOverriders?.OverrideValue;
                    }


                    var ServerComponentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component?.ToLower() == CurrentComponent?.ToLower() && y.Environment == null && y.Server?.ToLower() == CurrentServer?.ToLower());
                    if (ServerComponentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = ServerComponentOverriders?.OverrideValue;
                    }


                    // Most Specific:  Component and Environment and Server
                    var EnvironmentServerComponentOverriders = sett.ListOfOverrides.FirstOrDefault(y => y.Component?.ToLower() == CurrentComponent?.ToLower() && y.Environment?.ToLower() == CurrentEnvironment?.ToLower() && y.Server?.ToLower() == CurrentServer?.ToLower());
                    if (EnvironmentServerComponentOverriders?.OverrideValue != null)
                    {
                        newSetting.DesiredValue = EnvironmentServerComponentOverriders?.OverrideValue;
                    }


                    Result.Add(newSetting);
                }
            }

            return Result;
        }

        public static List<CalculatedFile> CalculateFiles(ConfigMasterParsed configMaster)
        {
            var Result = new List<CalculatedFile>();

            foreach (var item in configMaster.GlobalRecipe.ListOfRecipeConfigFiles)
            {
                var newCalcFile = new CalculatedFile()
                {
                    Component = item.Component,
                    ComputerName = item.ComputerName,
                    Environment = item.Environment,
                    ConfigFilename = item.ConfigFilePath
                };

                newCalcFile.ListOfEffectiveSettings = CalculateThisFile(configMaster, newCalcFile.ConfigFilename);

                Result.Add(newCalcFile);
            }

            return Result;
        }

        public static List<CalculatedFile> VerifyActuals(List<CalculatedFile> Calculated, string Environment, string Component, string Server)
        {

            List<CalculatedFile> filteredListOfFiles = Calculated;

            filteredListOfFiles = FilterFiles(Environment, Component, Server, filteredListOfFiles);

            foreach (var item in filteredListOfFiles)
            {
                VerifyActualFile(Calculated, item.ConfigFilename);
            }
            return Calculated;
        }

        private static List<CalculatedFile> FilterFiles(string Environment, string Component, string Server, List<CalculatedFile> filteredListOfFiles)
        {
            if (!string.IsNullOrEmpty(Environment))
            {
                if (Environment?.ToLower() != "all")
                {
                    filteredListOfFiles = filteredListOfFiles.Where(q => q.Environment?.ToLower() == Environment?.ToLower()).ToList();
                }
                
            }

            if (!string.IsNullOrEmpty(Component))
            {
                if (Component?.ToLower() != "all")
                {
                    filteredListOfFiles = filteredListOfFiles.Where(q => q.Component?.ToLower() == Component?.ToLower()).ToList();
                }

            }

            if (!string.IsNullOrEmpty(Server))
            {
                if (Server?.ToLower() != "all")
                {
                    filteredListOfFiles = filteredListOfFiles.Where(q => q.ComputerName?.ToLower() == Server?.ToLower()).ToList();
                }

            }

            return filteredListOfFiles;
        }

        public static List<CalculatedFile> VerifyActualFile(List<CalculatedFile> Calculated, string configFilename)
        {
            var calculatedF = Calculated.First(q => q.ConfigFilename == configFilename);
            calculatedF.FileVerified = true;
            try
            {
                var FI = new System.IO.FileInfo(configFilename);
                calculatedF.FileFound = FI.Exists;

                var configFileDocXML = XDocument.Load(configFilename);

                foreach (var sett in calculatedF.ListOfEffectiveSettings)
                {
                    var FileSetting = configFileDocXML.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(k => k.Attribute("key").Value == sett.Key);

                    if (FileSetting != null)
                    {
                        sett.EffectiveValue = FileSetting.Attribute("value").Value;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }


            return Calculated;
        }


        public static List<CalculatedFile> ApplySettingsToFiles(List<CalculatedFile> Calculated, string Environment, string Component, string Server)
        {

            List<CalculatedFile> filteredListOfFiles = Calculated;

            filteredListOfFiles = FilterFiles(Environment, Component, Server, filteredListOfFiles);

            foreach (var item in filteredListOfFiles)
            {
                ApplySettingsToActualFile(Calculated, item.ConfigFilename);
            }
            return Calculated;
        }

        private static List<CalculatedFile> ApplySettingsToActualFile(List<CalculatedFile> Calculated, string configFilename)
        {
            var calculatedF = Calculated.First(q => q.ConfigFilename == configFilename);

            try
            {
                var configFileDocXML = XDocument.Load(configFilename);

                foreach (var sett in calculatedF.ListOfEffectiveSettings)
                {
                    var FileSetting = configFileDocXML.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(k => k.Attribute("key").Value == sett.Key);

                    if (FileSetting != null)
                    {
                        FileSetting.Attribute("value").Value = sett.DesiredValue;
                        sett.EffectiveValue = FileSetting.Attribute("value").Value;
                    }
                    else
                    {
                        var newElement = new XElement("add") { Name = "add" };
                        newElement.SetAttributeValue("key", sett.Key);
                        newElement.SetAttributeValue("value", sett.DesiredValue);
                        configFileDocXML.Element("configuration").Element("appSettings").Add(newElement);
                        //configFileDocXML.Element("configuration").Element("appSettings").Add(new XElement("add") { Name = sett.Key, Value = sett.DesiredValue });
                        var FileSetting2 = configFileDocXML.Element("configuration").Element("appSettings").Elements("add").FirstOrDefault(k => k.Attribute("key").Value == sett.Key);
                        sett.EffectiveValue = FileSetting2.Attribute("value").Value;
                    }
                }

                configFileDocXML.Save(configFilename);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }


            return Calculated;
        }


        public static ConfigMasterParsed CalculateListsOfEntities(ConfigMasterParsed CMP)
        {
            CMP.ListOfComponents.Clear();
            CMP.ListOfEnvironments.Clear();
            CMP.ListOfServers.Clear();

            // TODO: Populate these lists from the Recipe and Settings.
            // TODO: Indicate which problems or inconsistencies there are.

            return CMP;
        }

        /// <summary>
        /// Print entire configuration to screen
        /// </summary>
        /// <param name="Calculated">The List of all calculated files</param>
        /// <param name="OnlyPrintDiscrepancies">Should we print to screen only the descrepancies, or all settings, even if the effective value is the same as the desired value</param>
        public static void PrintFiles(List<CalculatedFile> Calculated, string Environment, string Component, string Server, bool OnlyPrintDiscrepancies = false)
        {

            List<CalculatedFile> filteredListOfFiles = Calculated;

            filteredListOfFiles = FilterFiles(Environment, Component, Server, filteredListOfFiles);

            foreach (var item in filteredListOfFiles.OrderBy(q => q.Environment).ThenBy(y => y.Component).ThenBy(w => w.ComputerName))
            {
                PrintConfigurationFile(Calculated, item.ConfigFilename, OnlyPrintDiscrepancies);
            }
        }

        public static void PrintConfigurationFile(List<CalculatedFile> Calculated, string ConfigMasterFilename, bool OnlyPrintDiscrepancies = false)
        {
            var cnfFile = Calculated.FirstOrDefault(q => q.ConfigFilename == ConfigMasterFilename);
            if (cnfFile == null)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("__________________________________________________________________");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            
            Console.WriteLine("Configuration:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(cnfFile.ConfigFilename);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   Environment:     " + cnfFile.Environment);
            Console.WriteLine("   Component  :     " + cnfFile.Component);
            Console.WriteLine("   Computer   :     " + cnfFile.ComputerName);
            Console.WriteLine();

            if (cnfFile.FileFound == false && cnfFile.FileVerified)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  *** FILE NOT FOUND ! ***");
                Console.WriteLine();
            }
            var AnyOutOfSync = false;

            foreach (var item in cnfFile.ListOfEffectiveSettings)
            {

                if (item.DesiredValue == item.EffectiveValue)
                {
                    if (OnlyPrintDiscrepancies)
                    {
                        continue;
                    }
                }
                else
                {
                    AnyOutOfSync = true;
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Setting: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(item.Name.PadRight(30));

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" Key: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(item.Key.PadRight(40));
                Console.WriteLine();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" Desired Value  : ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(item.DesiredValue);
                Console.WriteLine();

                if (cnfFile.FileVerified)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" Effective Value: ");

                    if (item.EffectiveValue == item.DesiredValue)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.EffectiveValue))
                        {
                            item.EffectiveValue = "<nothing>";
                            Console.ForegroundColor = ConsoleColor.Magenta;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                    }

                    Console.WriteLine(item.EffectiveValue);

                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            if (!AnyOutOfSync)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("   All settings in SYNC!  :-)");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            
        }


    }
}
