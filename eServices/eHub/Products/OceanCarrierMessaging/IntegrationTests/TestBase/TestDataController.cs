using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDbUnit.Core;
using NDbUnit.Core.SqlClient;
using CargoWise.eServices.TestHelpers.Database.Common;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	class TestDataController
	{
		public TestDataController(string commonTestDataLocation, string schemaLocation, string testClassName,
			string connectionStringPattern, string testClassLocationPattern)
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
		}

		public void LoadTestData()
		{
			var prefix = AssembleyName + TestClassLocationPattern + TestClassName + "Data.";
			var testDataResourceNames = GetResourcesByNamePattern(AssembleyName + CommonTestDataLocation);
			testDataResourceNames = testDataResourceNames.Concat(GetResourcesByNamePattern(prefix));
			if (testDataResourceNames.Count() > 0)
			{
				testDataResourceNames = testDataResourceNames.Where(x => x.Contains("_Infrastructure")).Concat(testDataResourceNames.Where(x => !x.Contains("_Infrastructure")));
				foreach (var testDataName in testDataResourceNames)
				{
					var name = GetSchemaName(testDataName);
					var database = new SqlDbUnitTest(SqlServerHelper.GetAdminConnectionString(name));
					using (var stream = Assembly.GetExecutingAssembly()
						.GetManifestResourceStream(AssembleyName + SchemaLocation + name + ".xsd"))
					{
						database.ReadXmlSchema(stream);
					}
					using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(testDataName))
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
			return name[name.Length - 2].Replace("_Infrastructure", "");
		}

		private IEnumerable<string> GetResourcesByNamePattern(string pattern)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Where(name => new Regex(pattern).IsMatch(name));
		}

		private List<INDbUnitTest> TestDBCollection = new List<INDbUnitTest>();
		private string CommonTestDataLocation;
		private string ConnectionStringPattern;
		private string SchemaLocation;
		private string TestClassLocationPattern;
		private string TestClassName;
		private string AssembleyName = Assembly.GetExecutingAssembly().GetName().Name;
	}
}