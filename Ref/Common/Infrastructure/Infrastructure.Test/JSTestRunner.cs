using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class JSTestRunner
	{
		static string SourceFolder => TestSourcePathHelper.DATTestSourcePath;

		public static IEnumerable<TestCaseData> GetJsFiles(string relativeFolderToRoot)
		{
			var sourceFolder = SourceFolder;
			var path = Path.Combine(sourceFolder, relativeFolderToRoot);
			var allJSFiles = Directory.GetFiles(path, "*fixture.js", SearchOption.AllDirectories).Select(x => x.Substring(0, x.Length - 3));
			var allTSFiles = Directory.GetFiles(path, "*fixture.ts", SearchOption.AllDirectories).Select(x => x.Substring(0, x.Length - 3));
			var allFiles = allJSFiles.Union(allTSFiles);

			return allFiles.Select(x => new TestCaseData(GetRelativePath(sourceFolder, x)).SetName("JS Test - " + GetRelativePath(sourceFolder, x)));
		}

		public static void Run(string jsScriptPath)
		{
			var contentFolder = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var fullScriptPath = Path.Combine(contentFolder, jsScriptPath);
			fullScriptPath += File.Exists(fullScriptPath + ".ts") ? ".ts" : ".js";
			var config = Path.Combine(Path.GetDirectoryName(fullScriptPath), "chutzpah.json");
			if (!File.Exists(config))
			{
				Assert.Fail("No config file. {0}", config);
			}
			Assert.That(File.Exists(fullScriptPath), Is.True);
			var binFolder = GetBinFolder();
			var chutzpahRelativePath = @"..\..\Testing\chutzpah\chutzpah.console.exe";
			var chutzpahPath = Path.Combine(binFolder, chutzpahRelativePath);
			var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var junitFilePath = Path.Combine(Path.GetTempPath(), $"Chutzpah_{Guid.NewGuid()}.junit.xml");

			var args = new[]
			{
				"/silent",
				"/failOnError",
				"/failonscripterror",
				"/timeoutmilliseconds 15000",
				$@"/junit ""{junitFilePath}"""
			};

			var chutzpahArgs = $@"""{fullScriptPath}"" {string.Join(" ", args)}";
			var workingDirectory = Path.GetDirectoryName(chutzpahPath);
			var startInfo = new ProcessStartInfo(chutzpahPath, chutzpahArgs)
			{
				WorkingDirectory = workingDirectory,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var outputText = ProcessRunner.RunProcess(startInfo, true);
			Assert.That(File.Exists(junitFilePath), outputText);
			var junit = XDocument.Load(junitFilePath);

			var suites = junit.XPathSelectElements("//testsuite");
			if (!suites.Any())
			{
				Assert.Fail("No test suites were found. {0}, {1}, {2}, {3}", outputText, junitFilePath, junit.ToString(), fullScriptPath);
			}

			var noTests = junit.XPathSelectElements("//testsuite[@tests = 0]").ToArray();
			if (noTests.Any())
			{
				Assert.Fail("No tests found in {0}{1}{1}{2}", string.Join(", ", noTests.Select(x => x.Attribute("name").Value)), Environment.NewLine, outputText);
			}

			var failures = junit.XPathSelectElements("//testsuite[not(@failures = 0)]").ToArray();
			if (failures.Length > 0)
			{
				Assert.Fail(GetFailureText(failures));
			}
		}

		static string GetFailureText(IEnumerable<XElement> failures)
		{
			var sb = new StringBuilder();

			foreach (var testSuite in failures)
			{
				var suiteName = Path.GetFileName(testSuite.Attribute("name").Value);

				foreach (var testCase in testSuite.Elements("testcase"))
				{
					var failure = testCase.Element("failure");
					if (failure != null)
					{
						sb.Append(CultureInfo.InvariantCulture, $"Failed: {suiteName}");
						sb.AppendLine();
						sb.Append(CultureInfo.InvariantCulture, $"Case: {testCase.Attribute("name").Value}");
						sb.AppendLine();
						sb.Append(CultureInfo.InvariantCulture, $"Reason: {failure.Attribute("message").Value}");
						sb.AppendLine();
					}
				}
			}

			return sb.ToString();
		}

		static string GetRelativePath(string root, string path)
		{
			var pathUri = new Uri(path);
			if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.InvariantCultureIgnoreCase))
			{
				root += Path.DirectorySeparatorChar;
			}
			var rootUri = new Uri(root);
			return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		static string GetBinFolder()
		{
			var codeBase = Assembly.GetExecutingAssembly().Location;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}
