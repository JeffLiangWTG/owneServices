using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers
{
	[TestFixture]
	class CarrierLoaderTests
	{
		[Test]
		public void LoadCarriers()
		{
			var fileDate = new DateTime(2022, 6, 11, 12, 13, 14);
			var loader = new CarrierLoaderForTest("baseUrl/", new List<LoaderConfig> { new LoaderConfig("CargoCarrierAir.csv", "CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestFiles.Carriers1.txt", fileDate) });

			var carrierData = loader.LoadData();

			Assert.That(carrierData, Is.Not.Null);
			Assert.That(carrierData.PublicationDate, Is.EqualTo(fileDate));
			Assert.That(carrierData.Data, Is.Not.Null);
			Assert.That(carrierData.Data.Count, Is.EqualTo(4));

			Assert.That(carrierData.Data[0].CarrierCode, Is.EqualTo("10000001"));
			Assert.That(carrierData.Data[0].CarrierName, Is.EqualTo("Carrier 1"));
			Assert.That(carrierData.Data[0].Attributes, Is.Not.Null);
			Assert.That(carrierData.Data[0].Attributes.Count, Is.EqualTo(1));

			Assert.That(carrierData.Data[1].CarrierCode, Is.EqualTo("10000002"));
			Assert.That(carrierData.Data[1].CarrierName, Is.EqualTo("Carrier 2"));

			Assert.That(carrierData.Data[2].CarrierCode, Is.EqualTo("10000003"));
			Assert.That(carrierData.Data[2].CarrierName, Is.EqualTo("Carrier with an unusually long name compared to all the others in this file"));
		}

		[Test]
		public void MultipleUrls()
		{
			var fileDate1 = new DateTime(2022, 6, 11, 12, 13, 14);
			var fileDate2 = new DateTime(2022, 7, 3, 4, 5, 6);
			var fileDate3 = new DateTime(2022, 6, 28, 9, 10, 11);
			var loader = new CarrierLoaderForTest("baseUrl/", new List<LoaderConfig>
			{
				new LoaderConfig("CargoCarrierAir.csv", "CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestFiles.Carriers1.txt", fileDate1),
				new LoaderConfig("CargoCarrierSea.csv", "CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestFiles.Carriers2.txt", fileDate2),
				new LoaderConfig("MasterCargoCarrierSea.csv", "CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestFiles.Carriers3.txt", fileDate3)
			});

			var carrierData = loader.LoadData();

			Assert.That(carrierData, Is.Not.Null);
			Assert.That(carrierData.PublicationDate, Is.EqualTo(fileDate2));
			Assert.That(carrierData.Data, Is.Not.Null);
			Assert.That(carrierData.Data.Count, Is.EqualTo(8));

			var combinedCarrier = carrierData.Data.FirstOrDefault(x => x.CarrierCode == "X0000008");
			Assert.That(combinedCarrier, Is.Not.Null);
			Assert.That(combinedCarrier.Attributes.Count, Is.EqualTo(2));
		}

		[TestCase("CargoCarrierAir.csv", true, false, Constants.Attributes.CargoCarrier)]
		[TestCase("CargoCarrierSea.csv", false, true, Constants.Attributes.CargoCarrier)]
		[TestCase("MasterCargoCarrierSea.csv", false, true, Constants.Attributes.Master)]
		public void FileMappings(string file, bool expectedIsAir, bool expectedIsSea, string expectedAttribute)
		{
			var loader = new CarrierLoaderForTest("baseUrl/", new List<LoaderConfig>());

			var mappings = loader.GetFileMappingsExposed();

			var found = mappings.TryGetValue(file, out var mapping);

			Assert.That(found, Is.EqualTo(true), $"Mapping for {file} not found");
			Assert.That(expectedIsAir, Is.EqualTo(mapping.IsAir), $"IsAir mapping for {file}");
			Assert.That(expectedIsSea, Is.EqualTo(mapping.IsSea), $"IsSea mapping for {file}");

			var expectedAttributes = (new [] { expectedAttribute }).ToList();
			Assert.That(mapping.Attributes?.Count ?? 0, Is.EqualTo(expectedAttributes.Count));

			foreach (var attribute in mapping.Attributes)
			{
				Assert.That(expectedAttributes.Contains(attribute), Is.EqualTo(true), $"Attribute {attribute} not found for {file}");
			}
		}
	}
}
