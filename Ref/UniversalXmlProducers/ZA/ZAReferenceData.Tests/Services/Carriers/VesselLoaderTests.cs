using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestClasses;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers
{
	[TestFixture]
	class VesselLoaderTests
	{
		[Test]
		public void LoadVessels()
		{
			var fileDate = new DateTime(2022, 6, 11, 12, 13, 14);
			var loader = new VesselLoaderForTest("baseUrl/", new List<LoaderConfig> { new LoaderConfig("transportsea.csv", "CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Carriers.TestFiles.Vessels1.txt", fileDate) });

			var vesselData = loader.LoadData();

			Assert.That(vesselData, Is.Not.Null);
			Assert.That(vesselData.PublicationDate, Is.EqualTo(fileDate));
			Assert.That(vesselData.Data, Is.Not.Null);
			Assert.That(vesselData.Data.Count, Is.EqualTo(4));

			Assert.That(vesselData.Data[0].RadioCallSign, Is.EqualTo("R0001"));
			Assert.That(vesselData.Data[0].VesselName, Is.EqualTo("Vessel 1"));
			Assert.That(vesselData.Data[0].Carrier, Is.Not.Null);
			Assert.That(vesselData.Data[0].Carrier.CarrierCode, Is.EqualTo("40000001"));
			Assert.That(vesselData.Data[0].Carrier.CarrierName, Is.EqualTo("Carrier Eight"));
			Assert.That(vesselData.Data[0].Carrier.Attributes, Is.Not.Null);
			Assert.That(vesselData.Data[0].Carrier.Attributes.Count, Is.EqualTo(1));

			Assert.That(vesselData.Data[1].RadioCallSign, Is.EqualTo("R0002"));
			Assert.That(vesselData.Data[1].VesselName, Is.EqualTo("Vessel 2"));
		}

		[Test]
		public void RemoveSpecialChars()
		{
			var loader = new VesselLoaderForTest("", null);

			var result = loader.RemoveSpecialCharactersExposed("QqWwEeRrTtYyUuIiOoPp`~1!2@3#4$5%6^7&8*9(0)-_=+[{]}\\|;:'\",<.>/?");

			Assert.That(result, Is.EqualTo("QqWwEeRrTtYyUuIiOoPp  1 2 3 4 5 6 7&8 9(0)-_            , . / "));
		}
	}
}
