using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	[TestFixture]
	public class TradeGroupLoaderTest
	{
		[Test]
		public void Load()
		{
			var tradeGroups = TradeGroupLoader.Load(DataFileName);
			Assert.AreEqual(40, tradeGroups.Count());
			Assert.AreEqual(683, tradeGroups.SelectMany(tradeGroup => tradeGroup.Countries).Count());

			var lastTradeGroup = tradeGroups.Last();
			Assert.AreEqual("IS", lastTradeGroup.Code);
			Assert.AreEqual("İzlanda", lastTradeGroup.Description);
			Assert.AreEqual(new DateTime(2024, 03, 06, 00, 00, 00), lastTradeGroup.StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 59), lastTradeGroup.EndDate);

			var lastCountry = lastTradeGroup.Countries.Last();
			Assert.AreEqual("IS", lastCountry.Code);
			Assert.AreEqual("Iceland", lastCountry.Description);
			Assert.AreEqual(new DateTime(2024, 03, 06, 00, 00, 00), lastCountry.StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 59), lastCountry.EndDate);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.TradeGroups.TestFiles.Input.TradeGroupData.xlsx");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;

		string DataFileName => Path.Combine(tempFolder, "TradeGroupData.xlsx");
	}
}
