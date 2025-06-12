using System.Text.RegularExpressions;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;

namespace Hawking.Xsltc
{
    [ArgumentGroupCertification("d", EArgumentGroupCondition.ExactlyOneUsed)]
    internal class XsltCompilerArgs
    {
        string rootDirectory;

        [ValueArgument(typeof(string), 'd', "dir", Description = "Specify the root directory of your xsl files to be compiled")]
        public string RootDirectory
        {
            get => rootDirectory;
            set => rootDirectory = value.EscapeQuoteQuote();
        }
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
