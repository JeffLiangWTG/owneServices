using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	public class HVLVDeveloperTest : TestCase
	{
		string[] ProductionAssemblies
		{
			get
			{
				return new[]
				{
					"Enterprise.eTail.Business",
					"Enterprise.eTail.DataTransfer",
					"Enterprise.eTail.GUI",
					"Enterprise.eTail.Module",
					"Enterprise.eTail.Integration",
					"Enterprise.eTail.ServiceTasks"
				};
			}
		}

		string[] TestingAssemblies
		{
			get
			{
				return new[]
				{
					"Enterprise.eTail.Business.Testing",
					"Enterprise.eTail.DataTransfer.Testing",
					"Enterprise.eTail.GUI.Testing",
					"Enterprise.eTail.Module.Testing",
					"Enterprise.eTail.ServiceTasks.Testing"
				};
			}
		}

		[DeveloperOnlyTest]
		public void TestNoTestingClassesInAllProductionAssemblies()
		{
			var subClassRetriever = new SubClassRetriever(ProductionAssemblies, typeof(TestCase));
			var errors = new List<string>();
			foreach (var testCaseGotLost in subClassRetriever.Retrieve())
			{
				errors.Add(testCaseGotLost.FullName);
			}

			Assert("Below testing class(es) should be moved to testing project:\r\n\r\n\t-\t" + string.Join("\r\n\t-\t", errors), !errors.Any());
		}

		[DeveloperOnlyTest]
		public void TestAllTestingClassesHaveProperNamespaces()
		{
			var subClassRetriever = new SubClassRetriever(TestingAssemblies, typeof(TestCase));
			var errors = new List<string>();
			foreach (var testCase in subClassRetriever.Retrieve())
			{
				var expectedNamespace = System.Reflection.AssemblyName.GetAssemblyName(testCase.Assembly.Location).Name;
				if (!testCase.Namespace.Equals(expectedNamespace))
				{
					errors.Add(string.Format("The namespace of class: [{0}] should be changed to [{1}].", testCase.FullName, expectedNamespace));
				}
			}

			Assert(string.Join("\r\n", errors), !errors.Any());
		}
	}
}
