using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.eServices.TestHelpers.Database.Common;
using NDbUnit.Core;
using NDbUnit.Core.SqlClient;

namespace CargoWise.eServices.USCustoms.IntegrationTests
{
	public class TestDataController
	{
		public TestDataController(string commonTestDataLocation, string schemaLocation, string testClassName, string connectionStringPattern, string testClassLocationPattern)
		{
			if (string.IsNullOrEmpty(commonTestDataLocation))
			{
				throw new Exception("Common test data location cannot be empty.");
			}
			if (string.IsNullOrEmpty(schemaLocation))
			{
				throw new Exception("Test schema location cannot be empty.");
			}
			this.commonTestDataLocation = commonTestDataLocation;
			this.schemaLocation = schemaLocation;
			this.testClassLocationPattern = testClassLocationPattern;
			this.connectionStringPattern = connectionStringPattern;
			this.testClassName = testClassName;
		}

		public void LoadTestData()
		{
			var prefix = assemblyName + testClassLocationPattern + testClassName + "Data.";
			var testDataResourceNames = GetResourcesByNamePattern(assemblyName + commonTestDataLocation);
			testDataResourceNames = testDataResourceNames.Concat(GetResourcesByNamePattern(prefix));
			foreach (var testDataResourceName in testDataResourceNames)
			{
				var name = GetSchemaName(testDataResourceName);
				var database = new SqlDbUnitTest(SqlServerHelper.GetAdminConnectionString(name.Replace("_", ".")));
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + schemaLocation + name + ".xsd"))
				{
					database.ReadXmlSchema(stream);
				}
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(testDataResourceName))
				{
					database.ReadXml(stream);
				}
				testDBCollection.Add(database);
			}
			InsertTestDataIntoDatabase();
		}

		void InsertTestDataIntoDatabase()
		{
			foreach (var database in testDBCollection)
			{
				database.PerformDbOperation(DbOperationFlag.Insert);
			}
		}

		public void RestoreDatabase()
		{
			foreach (var database in testDBCollection)
			{
				database.PerformDbOperation(DbOperationFlag.DeleteAll);
			}
		}

		string GetSchemaName(string resourceName)
		{
			var name = resourceName.Split('.');
			return name[name.Length - 2];
		}

		IEnumerable<string> GetResourcesByNamePattern(string pattern)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => new Regex(pattern).IsMatch(name));
		}

		readonly List<INDbUnitTest> testDBCollection = new List<INDbUnitTest>();
		readonly string commonTestDataLocation;
		readonly string connectionStringPattern;
		readonly string schemaLocation;
		readonly string testClassLocationPattern;
		readonly string testClassName;
		readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
	}
}
