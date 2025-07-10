using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	public class RFVersionSupporterTest : TestCaseWithFactory
	{
		public void TestGetAndroidWebServiceVersion()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var filePath = resourceRetriever.SaveResourceToFile("Enterprise.Warehouse.Web.Testing.WebService.TestFiles.SampleAndroidVersion.txt");

				var rfSupporter = new RFVersionSupporter();
				AssertEquals("Android Version number shoud match with the Sample File Version number",
					"123.456.123.456",
					rfSupporter.GetAndroidWebServiceVersion(filePath));
			}
		}

		public void TestGetAndroidWebServiceVersion_BlankFile()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var filePath = resourceRetriever.SaveResourceToFile("Enterprise.Warehouse.Web.Testing.WebService.TestFiles.BlankAndroidVersion.txt");

				var rfSupporter = new RFVersionSupporter();
				AssertExceptionThrown<InvalidOperationException>("Should throw an invalid operation exception for empty files.", () => rfSupporter.GetAndroidWebServiceVersion(filePath));
			}
		}

		public void TestGetAndroidWebServiceVersion_FileDoesntExist()
		{
			var rfSupporter = new RFVersionSupporter();
			AssertExceptionThrown<InvalidOperationException>("Should throw an invalid operation exception if file not found.", () => rfSupporter.GetAndroidWebServiceVersion("ABC"));
		}
	}
}
