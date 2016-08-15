using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAppInventoryPkgStatus
{
    class Options
    {
        [Option('a', "appCIID", Required = true, HelpText = "AppCIID for the application package that is being updated. Should be an integer value")]
        public int AppCIID { get; set; }

        [Option('s', "status", Required = true, HelpText = @"Packaging status for the app. Discovery/Packaging/QA/UAT/Production/Cancelled/On Hold")]
        public string Status { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
