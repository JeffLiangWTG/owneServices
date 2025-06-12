using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.AlertService.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.AlertService.Tests
{
	[TestClass]
	public class TransformationTests
	{
		MapTester _mapTester;
		Assembly _testFixture;

		[TestInitialize]
		public void TestSetup()
		{
			_testFixture = Assembly.GetExecutingAssembly();
			_mapTester = new MapTester(_testFixture, Comparer);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_dbo_SelectErrorToAlert2AlertData()
		{
			string source = "TestFiles.dbo_SelectErrorToAlert2AlertData_Source.xml";
			string expected = "TestFiles.dbo_SelectErrorToAlert2AlertData_Expected.xml";
			_mapTester.Execute<dbo_SelectErrorToAlert2AlertData>(source, expected);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_dbo_SelectErrorToAlert2AlertData_WithSourceField()
		{
			string source = "TestFiles.dbo_SelectErrorToAlert2AlertData_Source_WithSourceField.xml";
			string expected = "TestFiles.dbo_SelectErrorToAlert2AlertData_Expected_WithSourceField.xml";
			_mapTester.Execute<dbo_SelectErrorToAlert2AlertData>(source, expected);
		}

		ICompare Comparer
		{
			get
			{
				if (comparer == null)
				{
					List<string> exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='InsertError']/*[local-name()='CurrentDateTimeUTC']");
					exclusionXpaths.Add("/*[local-name()='InsertError']/*[local-name()='ErrorPK']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;
	}
}
