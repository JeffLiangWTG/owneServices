using NDbUnit.Core;
using NDbUnit.Core.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.eServices.TestHelpers.Database.Common;
using System.Drawing;
using CargoWise.eServices.TestHelpers.Database.Deployment;

namespace CargoWise.eHub.Selenium.IntegrationTests.Core
{
	public class TestDataController
	{
		public TestDataController(Assembly testAssembly, string commonTestDataLocation, string schemaLocation, string testClassName, string connectionStringPattern, string testClassLocationPattern)
		{
			if (string.IsNullOrEmpty(commonTestDataLocation))
			{
				throw new Exception("Common test data location cannot be empty.");
			}
			if (string.IsNullOrEmpty(schemaLocation))
			{
				throw new Exception("Test schema location cannot be empty.");
			}
			this.CommonTestDataLocation = commonTestDataLocation;
			this.SchemaLocation = schemaLocation;
			this.TestClassLocationPattern = testClassLocationPattern;
			this.ConnectionStringPattern = connectionStringPattern;
			this.TestClassName = testClassName;
			this.TestAssembly = testAssembly;
		}

		public void LoadTestData()
		{
			var prefix = AssemblyName + TestClassLocationPattern + TestClassName + "Data.";
			var testDataResourceNames = GetResourcesByNamePattern(AssemblyName + CommonTestDataLocation);
			testDataResourceNames = testDataResourceNames.Concat(GetResourcesByNamePattern(prefix));
			if (testDataResourceNames.Count() > 0)
			{
				foreach (var testDataName in testDataResourceNames)
				{
					var name = GetSchemaName(testDataName);
					var database = new SqlDbUnitTest(SqlServerHelper.GetAdminConnectionString(name.Replace("_", ".")));
					using (var stream = TestAssembly.GetManifestResourceStream(AssemblyName + SchemaLocation + name + ".xsd"))
					{
						database.ReadXmlSchema(stream);
						if (!Deployment.IsRunningInDAT())
							database.PerformDbOperation(DbOperationFlag.DeleteAll);
					}
					using (var stream = TestAssembly.GetManifestResourceStream(testDataName))
					{
						database.ReadXml(stream);
					}
					TestDBCollection.Add(database);
				}
			}
			InsertTestDataIntoDatabase();
		}

		private void InsertTestDataIntoDatabase()
		{
			foreach (var database in TestDBCollection)
			{
				database.PerformDbOperation(DbOperationFlag.Insert);
			}
		}

		public void RestoreDatabase()
		{
			foreach (var database in TestDBCollection)
			{
				database.PerformDbOperation(DbOperationFlag.DeleteAll);
			}
		}

		private string GetSchemaName(string resourceName)
		{
			var name = resourceName.Split('.');
			return name[name.Length - 2];
		}

		private IEnumerable<string> GetResourcesByNamePattern(string pattern)
		{
			return TestAssembly.GetManifestResourceNames().Where(name => new Regex(pattern).IsMatch(name));
		}

		private List<INDbUnitTest> TestDBCollection = new List<INDbUnitTest>();
		private string CommonTestDataLocation;
		private string ConnectionStringPattern;
		private string SchemaLocation;
		private string TestClassLocationPattern;
		private string TestClassName;
		private string AssemblyName => TestAssembly.GetName().Name;
		private Assembly TestAssembly;
	}
}
