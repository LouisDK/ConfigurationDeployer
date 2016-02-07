using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationDeployerRunner.Parameters
{

    [Verb("show", HelpText = "Calculate the Desired Values and print them to screen")]
    class ShowOptions
    {

        [Option(Default = "All", HelpText = "Which Environment to parse and display", Required = false)]
        public string Environment { get; set; }

        [Option(Default = "All", HelpText = "Which Component's configuration to parse and display", Required = false)]
        public string Component { get; set; }

        [Option(Default = "All", HelpText = "Which Computer to parse and display", Required = false)]
        public string Computer { get; set; }

        [Option(HelpText = "Filename of the Master Config XML file.", Required = true)]
        public string MasterConfigFile { get; set; }
    }


    [Verb("verify", HelpText = "Verify if the effective values in current Config Files match the desired values.")]
    class VerifyOptions
    {

        [Option(Default = "All", HelpText = "Which Environment to parse and verify", Required = false)]
        public string Environment { get; set; }

        [Option(Default = "All", HelpText = "Which Component's configuration to parse and verify", Required = false)]
        public string Component { get; set; }

        [Option(Default = "All", HelpText = "Which Computer to parse and verify", Required = false)]
        public string Computer { get; set; }

        [Option(HelpText = "Show only configuration discrenpancies", Required = false)]
        public bool OnlyBad { get; set; }


        [Option(HelpText = "Filename of the Master Config XML file.", Required = true)]
        public string MasterConfigFile { get; set; }
    }

    [Verb("apply", HelpText = "Apply the Desired values to the Config files.")]
    class ApplyOptions
    {

        [Option(Default = "Dev", HelpText = "Which Environment to apply the configuration to", Required = false)]
        public string Environment { get; set; }

        [Option(Default = "All", HelpText = "Which Component's configuration to apply", Required = false)]
        public string Component { get; set; }


        [Option(Default = "All", HelpText = "Which Computer's configuration to apply the configuration to", Required = false)]
        public string Computer { get; set; }

        [Option(HelpText = "Filename of the Master Config XML file.", Required = true)]
        public string MasterConfigFile { get; set; }
    }


}
