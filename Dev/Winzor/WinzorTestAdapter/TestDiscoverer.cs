using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Blazor.Common;
using CWNUnit.TestAdapter;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace WinzorTestAdapter
{
	[FileExtension(".dll")]
	[DefaultExecutorUri(TestExecutor.ExecutorUriString)]
	public class TestDiscoverer : ITestDiscoverer
	{
		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
		{
			CecilExtensions.ClearCache();

			var testDiscovery = new TestDiscovery(new TestAdapterContext());

			var executorUri = new Uri(TestExecutor.ExecutorUriString);
			foreach (var source in sources)
			{
				logger?.SendMessage(TestMessageLevel.Informational, "WinzorTestAdapter.DiscoverTests " + source);

				// only try to load the explicit list if there is a test case discovered
				HashSet<string> explicitList = null;

				foreach (var testCase in CWNUnit.TestAdapter.TestDiscoverer.GetTestCases(source, testDiscovery, executorUri, AddDatCapabilityRequirements))
				{
					if (explicitList == null)
					{
						explicitList = LoadExplicitList(source);
					}

					if (explicitList.Contains(testCase.FullyQualifiedName))
					{
						testCase.Traits.Add("Explicit", "");
						TestUtilities.SetTestPropertyValue(testCase, "Explicit", "");
					}

					discoverySink.SendTestCase(testCase);
				}
			}

			CecilExtensions.ClearCache();
		}

		HashSet<string> LoadExplicitList(string source)
		{
			var explicitList = new HashSet<string>();
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinzorTestAdapter.Explicit.txt"))
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					explicitList.Add(line.Trim());
				}
			}
			return explicitList;
		}

		static void AddDatCapabilityRequirements(TestCase testCase, TestDescriptor testDescriptor)
		{
			if (testDescriptor.CapabilityRequirements != null && testDescriptor.CapabilityRequirements.Length > 0)
			{
				testCase.Traits.Add("DAT:CapabilityRequirements", string.Join(",", testDescriptor.CapabilityRequirements.Where(req => req != "NET48" && req != "GUI")));
			}
		}

		static TestDiscoverer()
		{
			AssemblyResolver.Setup();
		}
	}
}
