using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.CmdLine;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using static CargoWise.RefDbRepo.NZReferenceData.Tests.SupplierListFileDownloderTest;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	class SupplierListProgramTest
	{
		[Test]
		public void TestDownloadNewData()
		{
			var filesToDelete = new List<string>();
			filesToDelete.Add(SupplierParser.DataFilePath);
			try
			{
				var resourcePath = Assembly.GetExecutingAssembly().GetName().Name + ".Supplier.TestFiles.Input.Suppliercodesupdates_20211001.gz";
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, "https://www.customs.govt.nz/api/datafiles/Supplier").Respond("application/zip", Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath));
					using (var client = mockHttp.ToHttpClient())
					{
						var donwloader = new SupplierListFileDownloderForTest("Suppliercodesupdates_20211001.gz");
						SupplierListProgram.RunCore(client, donwloader, "https://www.customs.govt.nz/api/datafiles/Supplier", new Logger());
					}

					Assert.True(File.Exists(SupplierParser.DataFilePath), "First time produce");

					File.Delete(SupplierParser.DataFilePath);

					var resourcePathNew = Assembly.GetExecutingAssembly().GetName().Name + ".Supplier.TestFiles.Input.Suppliercodesupdates_20211001.gz.new";
					mockHttp.When(HttpMethod.Get, "https://www.customs.govt.nz/api/datafiles/Supplier").Respond("application/zip", Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePathNew));
					using (var client = mockHttp.ToHttpClient())
					{
						var donwloader = new SupplierListFileDownloderForTest("Suppliercodesupdates_20211001.gz.new");
						SupplierListProgram.RunCore(client, donwloader, "https://www.customs.govt.nz/api/datafiles/Supplier", new Logger());
					}
					Assert.True(File.Exists(SupplierParser.DataFilePath), "Second time produce");
				}
			}
			finally
			{
				foreach (var file in filesToDelete)
				{
					File.Delete(file);
				}
			}
		}
	}
}
