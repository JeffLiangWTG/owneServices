using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Blazor.Common;
using NUnit.Framework;

namespace TestFailureListGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			AssemblyResolver.Setup();

			if (args[0].Equals("/UpdateAll", StringComparison.OrdinalIgnoreCase))
			{
				UpdateAll(userTestPk: Guid.Parse(args[1]), sourcePath: args[2]);
			}
			else if (args[0].Equals("/AppendAllExplicit", StringComparison.OrdinalIgnoreCase))
			{
				AppendAllExplicit(userTestPk: Guid.Parse(args[1]), sourcePath: args[2]);
			}
			else
			{
				throw new InvalidOperationException("Unknown command line argument");
			}
		}

		static void UpdateAll(Guid userTestPk, string sourcePath)
		{
			var testRunFetcher = new UserSubmittedTestRunDATFetcher();
			var failingTests = testRunFetcher.GetFailingTestNames(userTestPk)
				.Where(t => t.AssemblyName.StartsWith(@"winzor\", StringComparison.OrdinalIgnoreCase))
				.Select(t => t.ClassName.Substring("vstest:".Length) + "." + t.MethodName)
				.Append("WinzorTestAdapter.TestClasses.TestClass.TestExplicit")
				.Append("WinzorTestAdapter.TestClasses.TestClass.TestExplicitWithWhiteSpace\t")
				.ToHashSet();
			var explicitFile = Path.Combine(sourcePath, "Winzor", "WinzorTestAdapter", "Explicit.txt");
			var lines = new List<string>();
			foreach (var line in File.ReadAllLines(explicitFile))
			{
				if (failingTests.Contains(line))
				{
					lines.Add(line);
				}
			}
			if (lines.Count == 0)
			{
				File.Delete(explicitFile);
			}
			else
			{
				File.WriteAllLines(explicitFile, lines);
			}
		}

		static void AppendAllExplicit(Guid userTestPk, string sourcePath)
		{
			var testRunFetcher = new UserSubmittedTestRunDATFetcher();
			var explicitFile = Path.Combine(sourcePath, "Winzor", "WinzorTestAdapter", "Explicit.txt");
			var list = new List<string>();
			list.AddRange(File.ReadAllLines(explicitFile));
			list.AddRange(testRunFetcher.GetFailingTestNames(userTestPk).Where(t => t.AssemblyName.StartsWith(@"winzor\")).Select(t => t.ClassName.Substring("vstest:".Length) + "." + t.MethodName));
			list.Sort();
			File.WriteAllLines(explicitFile, list.ToArray());
		}
	}
}
