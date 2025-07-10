using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Mapping;
using CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	public class UNLOCOFixtures
	{
		[Test]
		public void Schema()
		{
			var currentSchema = new UNLOCO();
			var expectedProperties = new string[] {
				"Country",
				"Location",
				"Name",
				"NameWoDiacritics",
				"Subdivision",
				"Status",
				"Function",
				"Date",
				"IATA",
				"Coordinates",
				"Remarks",
				"IATARegionCode",
				"UniqueIdentifier"
			};
			var properties = currentSchema.GetType().GetProperties();

			for (int x = 0; x < properties.Length; x++)
			{
				Assert.That(properties[x].Name == expectedProperties[x]);
			}
		}

		[Test]
		public void MappingSubdivisionsBasedOnMappedCountry()
		{
			var mappedSubdivisions = new List<MappedSubdivisions>()
			{
				new MappedSubdivisions() { OldSubdivisionCode = "OLD1", CurrentSubdivisionCode = "NEW1" },
				new MappedSubdivisions() { OldSubdivisionCode = "OLD2", CurrentSubdivisionCode = "NEW2" }
			};

			var mappedCountries = new List<MappedCountry>()
			{
				new MappedCountry("TEST", mappedSubdivisions)
			};

			var unloco = new UNLOCO()
			{
				Name = "Test",
				Country = "TEST",
				Location = "ANY",
				Subdivision = "OLD1",
				NameWoDiacritics = "ANY"
			};

			var result = unloco.ConvertToRefUNLOCO(mappedCountries);
			Assert.True(result.RL_RW_NKCode == "NEW1");

			var unloco2 = new UNLOCO()
			{
				Name = "Test",
				Country = "TEST",
				Location = "ANY",
				Subdivision = "OLD2",
				NameWoDiacritics = "ANY"
			};

			var result2 = unloco2.ConvertToRefUNLOCO(mappedCountries);
			Assert.True(result2.RL_RW_NKCode == "NEW2");
		}

		[Test]
		public void CoOrdinateConversionTest()
		{
			var point = UNLOCOExtension.GetGeoLocationStringFromCoOrdinates("");
			Assert.That(point.SRID, Is.EqualTo(4326));
			Assert.That(point.AsText(), Is.EqualTo("POINT EMPTY"));

			point = UNLOCOExtension.GetGeoLocationStringFromCoOrdinates("1954S 14805W");
			Assert.That(point.AsText(), Is.EqualTo("POINT (-148.083 -19.9)"));
			point = UNLOCOExtension.GetGeoLocationStringFromCoOrdinates("1954N 14805E");
			Assert.That(point.AsText(), Is.EqualTo("POINT (148.083 19.9)"));
			point = UNLOCOExtension.GetGeoLocationStringFromCoOrdinates("1954S 14805E");
			Assert.That(point.AsText(), Is.EqualTo("POINT (148.083 -19.9)"));
			point = UNLOCOExtension.GetGeoLocationStringFromCoOrdinates("1954N 14805W");
			Assert.That(point.AsText(), Is.EqualTo("POINT (-148.083 19.9)"));
		}
	}
}
