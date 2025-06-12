using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Build.Utilities;

namespace BizTalk.Utilities.MSBuild
{
	public class ExtractXslContentTask : Task
	{
		const string BtmCodeBehindFileExtension = "*.btm.cs";
		const string XslFileExtension = ".xsl";
		const string ExtXmlFileExtension = "_extml.xml";

		public override bool Execute()
		{
			var result = false;

			try
			{
				var codeBehindFiles = Directory.GetFiles(BaseDirectory, BtmCodeBehindFileExtension, SearchOption.TopDirectoryOnly);
				if (codeBehindFiles.Any())
				{
					foreach (var csFile in codeBehindFiles)
					{
						var csFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(csFile));
						var xslFilePath = Path.Combine(BaseDirectory, csFileName + XslFileExtension);
						var extFilePath = Path.Combine(BaseDirectory, csFileName + ExtXmlFileExtension);

						var lines = new List<string>(File.ReadAllLines(csFile, Encoding.UTF8));

						ExtractContent(lines, @"^\s+private const string _strMap.*(\<.*>).*$", @"^.*(\</xsl\:stylesheet>).*$", xslFilePath);
						ExtractContent(lines, @"^.*(<ExtensionObjects>).*$", @"^.*(</ExtensionObjects>).*$", extFilePath);
					}
				}

				result = true;

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			return result;
		}

		void ExtractContent(List<string> lines, string startLinePattern, string endLinePattern, string outputPath)
		{
			var foundStartLine = false;
			var foundEndLine = false;
			var stringBuilder = new StringBuilder();

			while (lines.Any())
			{
				var line = lines[0];
				lines.RemoveAt(0);

				if (!foundStartLine)
				{
					var match = Regex.Match(line, startLinePattern);
					if (match.Success)
					{
						foundStartLine = true;
						stringBuilder.AppendLine(match.Groups[1].Value.ReplaceDoubleQuotes().HtmlDecode());
					}
				}
				else
				{
					var matchEnd = Regex.Match(line, endLinePattern);
					if (matchEnd.Success)
					{
						foundEndLine = true;
						stringBuilder.AppendLine(matchEnd.Groups[1].Value.ReplaceDoubleQuotes().HtmlDecode());

						break;
					}

					stringBuilder.AppendLine(line.ReplaceDoubleQuotes().HtmlDecode());
				}
			}

			if (foundStartLine && foundEndLine)
			{
				if (File.Exists(outputPath))
				{
					File.Delete(outputPath);
				}

				File.WriteAllText(outputPath, stringBuilder.ToString());
			}
		}

		#region MSBuild Properties

		[Required]
		public string BaseDirectory { get; set; }

		#endregion
	}

	static class Helper
	{
		public static string ReplaceDoubleQuotes(this string input)
		{
			return input.Replace("\"\"", "\"");
		}

		public static string HtmlDecode(this string input)
		{
			var output = input.Replace("&quot;", "'");
			return HttpUtility.HtmlDecode(output);
		}

	}
}
