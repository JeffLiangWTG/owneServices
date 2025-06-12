using System;
using System.IO;
using System.Text.RegularExpressions;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;

namespace Hawking.Xslt.Nupack
{
    [ArgumentGroupCertification("d", EArgumentGroupCondition.ExactlyOneUsed)]
    public class Args
    {
        const string BinFolderName = "bin";

        string rootDirectory;
        string outputDirectory;

        [ValueArgument(typeof(string), 'd', "dir", Description = "Specify the root directory of your xsl files to be compiled")]
        public string RootDirectory
        {
            get => rootDirectory;
            set 
            {
                rootDirectory = value.EscapeQuoteQuote();
                TargetDir = Path.GetFullPath(rootDirectory);
            }
        }

        [ValueArgument(typeof(string), 'o', "out", Description = "Specify the output directory to place the generated output files")]
        public string OutputDirectory
        {
            get => outputDirectory;
            set
            {
                var dir = value.EscapeQuoteQuote();
                outputDirectory = Path.GetFullPath(dir);
            }
        }

        public static Args Parse(string[] commandlineArgs)
        {
            var parser = new CommandLineParser.CommandLineParser();
            var args = new Args();

            try
            {
                parser.ExtractArgumentAttributes(args);
                parser.ParseCommandLine(commandlineArgs);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("For usage:");
                Console.WriteLine(string.Join(Environment.NewLine, parser.ShowUsageCommands));
                Environment.Exit(0);
            }

            return args;
        }

        public string TargetDir { get; private set; }
    }

    static class Helper
    {
        internal static string EscapeQuoteQuote(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var match = Regex.Match(value, "^'(?<value>.*)'$");
            return match.Success ? match.Groups["value"].Value : value;
        }
    }
}