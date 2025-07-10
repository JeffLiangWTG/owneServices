using System;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Business.Cus;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Cus
{
	[TestFixture]
	sealed class CusExchangeRateTest : CusExchangeRate
	{
		[Test]
		public void TestGetFullXmlUrl()
		{
			var xmlFilename = "a141z200722";
			var expected = new Uri($"{CusConstants.NbpXmlUrl}{xmlFilename}.xml");
			var result = GetFullXmlUrl(xmlFilename);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestGetListOfFilenamesFromReadDirTxt()
		{
			var dataToTest = $"a141z200722\nb029z200722";

			var result = GetListOfFilenamesFromReadDirTxt(new StringReader(dataToTest));

			if (result.Count != 2)
			{
				Assert.Fail($"Expected 2 lines but got {result.Count}");
			}

			Assert.AreEqual(result[0], "a141z200722");
			Assert.AreEqual(result[1], "b029z200722");
		}

		[Test]
		public void TestGetSpecifiedFilenameWithDateX()
		{
			var dirTxtFileContent = new StringReader(TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.PLReferenceData.Tests.Cus.TestFiles.Input.dir.txt"));

			if (dirTxtFileContent == null)
			{
				Assert.Fail("Could not read from dir.txt test file.");
			}

			var result = GetSpecifiedFilenameWithDateX(GetListOfFilenamesFromReadDirTxt(dirTxtFileContent), "200722");

			if (result == null)
			{
				Assert.Fail("Not received Tuple with table 'a' and table 'b' filenames.");
			}

			Assert.AreEqual("a141z200722", result.Item1);
			Assert.AreEqual("b029z200722", result.Item2);
		}

		[TestCase(23, 12, 2020)]
		[TestCase(18, 11, 2020)]
		[TestCase(21, 10, 2020)]
		[TestCase(23, 09, 2020)]
		[TestCase(19, 08, 2020)]
		[TestCase(22, 07, 2020)]
		[TestCase(17, 06, 2020)]
		[TestCase(20, 05, 2020)]
		[TestCase(22, 04, 2020)]
		[TestCase(18, 03, 2020)]
		[TestCase(19, 02, 2020)]
		[TestCase(22, 01, 2020)]
		public void TestGetPenultimateWednesday(int day, int month, int year)
		{
			var expectedPenultimateWednesday = new DateTime(year,month,day);
			var result = GetPenultimateWednesday(expectedPenultimateWednesday);

			Assert.AreEqual(expectedPenultimateWednesday, result);
		}

		[TestCase(24, 12, 2020)]
		[TestCase(22, 12, 2020)]
		public void FailingTestGetPenultimateWednesday(int day, int month, int year)
		{
			var expectedPenultimateWednesday = new DateTime(year, month, day);
			var result = GetPenultimateWednesday(expectedPenultimateWednesday);

			Assert.AreNotEqual(expectedPenultimateWednesday, result);
		}

		[Test]
		public void TestGetPenultimateWednesdayBasedOnConfigrationData()
		{
			var expectedPenultimateWednesday = new DateTime(Configuration.Cus.Year, Configuration.Cus.Month, Configuration.Cus.Day);
			var result = GetPenultimateWednesday(expectedPenultimateWednesday);

			Assert.AreEqual(expectedPenultimateWednesday, result);
		}
	}
}
