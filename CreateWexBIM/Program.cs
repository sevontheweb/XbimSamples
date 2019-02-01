using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;

namespace CreateWexBIM
{
    class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .WriteTo.ColoredConsole()
               .CreateLogger();

            var lf = new LoggerFactory().AddSerilog();
            var log = lf.CreateLogger("WexbimCreation");
            log.LogInformation("Creating wexBIM file from IFC model.");

            // set up xBIM logging. It will use your providers.
            XbimLogging.LoggerFactory = lf;

            //const string fileName = @"SampleHouse.ifc";
            string fileName = GetInputFilePath(args);
            if (fileName == null)
            {
                log.LogError("You must specify an input file.");
                return -1;
            }

            log.LogInformation($"File size: {new FileInfo(fileName).Length / 1e6}MB");
            IfcStore.ModelProviderFactory.UseHeuristicModelProvider();
            using (var model = IfcStore.Open(fileName, null, -1))
            {
                var context = new Xbim3DModelContext(model);
                context.CreateContext();

                var wexBimFilename = Path.ChangeExtension(fileName, "wexbim");
                using (var wexBimFile = File.Create(wexBimFilename))
                {
                    using (var wexBimBinaryWriter = new BinaryWriter(wexBimFile))
                    {
                        model.SaveAsWexBim(wexBimBinaryWriter);
                        wexBimBinaryWriter.Close();
                    }
                    wexBimFile.Close();
                }
            }
            return 0;
        }

        private static String GetInputFilePath(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args[0];
            }
            return null;
        }
    }
}
