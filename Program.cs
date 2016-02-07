using CommandLine;
using ConfigurationDeployerRunner.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationDeployerRunner
{
    class Program
    {

        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            PrintLogo();

            var Result = CommandLine.Parser.Default.ParseArguments<ShowOptions, VerifyOptions, ApplyOptions>(args)
              .MapResult(
                (ShowOptions opts) => {
                    var cfgMaster = ConfigDeployerLogic.LoadConfigMasterFile(opts.MasterConfigFile);
                    var Calculated = ConfigDeployerLogic.CalculateFiles(cfgMaster);
                    ConfigDeployerLogic.PrintFiles(Calculated, opts.Environment, opts.Component, opts.Computer, false);
                    return 1;
                },
                (VerifyOptions opts) => {
                    var cfgMaster = ConfigDeployerLogic.LoadConfigMasterFile(opts.MasterConfigFile);
                    var Calculated = ConfigDeployerLogic.CalculateFiles(cfgMaster);
                    Calculated = ConfigDeployerLogic.VerifyActuals(Calculated, opts.Environment, opts.Component, opts.Computer);
                    ConfigDeployerLogic.PrintFiles(Calculated, opts.Environment, opts.Component, opts.Computer, opts.OnlyBad);
                    return 1;
                },
                (ApplyOptions opts) => {
                    var cfgMaster = ConfigDeployerLogic.LoadConfigMasterFile(opts.MasterConfigFile);
                    var Calculated = ConfigDeployerLogic.CalculateFiles(cfgMaster);
                    ConfigDeployerLogic.ApplySettingsToFiles(Calculated, opts.Environment, opts.Component, opts.Computer);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("      Configuration Applied!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();

                    return 1;
                },
                errs => 1);

            return 1;
        }

        public static void PrintLogo()
        {
            Console.SetWindowSize(140, 84);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(@"8888888                         .d8888b.                     .d888 d8b          ");
            Console.WriteLine(@"  888                          d88P  Y88b                   d88P""  Y8P          ");
            Console.WriteLine(@"  888                          888    888                   888                 ");
            Console.WriteLine(@"  888   88888b.   .d88b.       888         .d88b.  88888b.  888888 888  .d88b.  ");
            Console.WriteLine(@"  888   888 ""88b d88""""88b      888        d88""""88b 888 ""88b 888    888 d88P""88b ");
            Console.WriteLine(@"  888   888  888 888  888      888    888 888  888 888  888 888    888 888  888 ");
            Console.WriteLine(@"  888   888  888 Y88..88P      Y88b  d88P Y88..88P 888  888 888    888 Y88b 888 ");
            Console.WriteLine(@"8888888 888  888  ""Y88P""        ""Y8888P""   ""Y88P""  888  888 888    888  ""Y88888 ");
            Console.WriteLine(@"                                                                            888 ");
            Console.WriteLine(@"                                                                       Y8b d88P ");
            Console.WriteLine(@"                                                                        ""Y88P""  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(@"         louis@inobits.com        Inobits Consulting 2016");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }



    }
}
