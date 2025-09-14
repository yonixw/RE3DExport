using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE3DExport
{
    class Program
    {
        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (string arg in args)
            {
                var parts = arg.Split(new[] { ':', '=' }, 2);
                if (parts.Length == 2)
                {
                    // Remove leading hyphens or slashes for key
                    var key = parts[0].TrimStart('-', '/');
                    dictionary[key] = parts[1];
                }
                else if (parts.Length == 1)
                {
                    // This handles boolean flags like "--verbose"
                    var key = parts[0].TrimStart('-', '/');
                    dictionary[key] = "true";
                }
            }
            return dictionary;
        }

        static void Main(string[] args)
        {
            var arguments = ParseArgs(args);

            string inFile = "";
            if (File.Exists(args[args.Length - 1]))
            {
                inFile = args[args.Length - 1]; // Support drag that will put filename as last arg
            }

            if (inFile == "")
            {
                if (!arguments.ContainsKey("in") || arguments.ContainsKey("help") || arguments.ContainsKey("h"))
                {
                    Console.WriteLine("HELP:\n .exe -in=[file] [-out=[file]]");
                    return;
                }
                inFile = arguments["in"];
            }

            NVXConverter.Convert(new NVXConverterInput(inFile, arguments.ContainsKey("out") ? arguments["out"] : null));
        }
    }
}
