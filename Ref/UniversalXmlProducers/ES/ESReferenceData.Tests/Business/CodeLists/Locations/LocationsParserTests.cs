using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class LocationsParserTests : TestCase
{
	LocationsParser ParserToRun => new (DateProvider.Object);

	[Test]
	public void CreateRefCusCodeListXMLTest()
	{
		DateProvider.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 6, 19));
		var locationsToExport = new List<ILocationsItem>
		{
			new LocationsItem("ES00010100DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "22-02-2019", "31-12-2099"),
			new LocationsItem("ES00010101DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "24-11-2016", "25-04-2019"),
			new LocationsItem("ES00010101DECO", "AEROPUERTO VIT ALMACÉN DECOEXA", "25-09-1996", "23-11-2016"),
			new LocationsItem("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT(DHL)", "24-04-2019", "31-12-2099"),
			new LocationsItem("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT(DHL)", "17-11-2016", "23-04-2019"),
			new LocationsItem("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT(DHL)", "15-11-2016", "16-11-2016"),
			new LocationsItem("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT(DHL)", "26-05-2016", "14-11-2016"),
			new LocationsItem("ES00010101EAT", "AEROPUERTO VIT ALMACÉN DE EAT(DHL)", "24-11-1998", "25-05-2016"),
			new LocationsItem("ES00010101GENE", "AEROPUERTO MERCANCÍAS RAMPA-ZONA RESTRI", "24-11-2016", "31-12-2099"),
			new LocationsItem("ES00010101GENE", "AEROPUERTO MERCANCÍAS RAMPA-ZONA RESTRI", "11-10-1996", "23-11-2016"),
			new LocationsItem("ES00010101IBER", "AEROPUERTO VIT ALMACÉN DE IBERIA", "29-11-2016", "30-11-2016"),
			new LocationsItem("ES00010101IBER", "AEROPUERTO VIT ALMACÉN DE IBERIA", "24-11-1998", "28-11-2016"),
			new LocationsItem("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "24-04-2019", "31-12-2099"),
			new LocationsItem("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "15-11-2016", "23-04-2019"),
			new LocationsItem("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "04-06-2016", "14-11-2016"),
			new LocationsItem("ES00010101TNT", "AEROPUERTO VIT ALMACÉN TNT", "04-02-1997", "03-06-2016"),
			new LocationsItem("ES00010101UPS", "UNITED PARCEL SERVICES", "05-06-2017", "31-12-2099"),
			new LocationsItem("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "24-11-2016", "31-12-2099"),
			new LocationsItem("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "24-11-2016", "31-12-2090"),
			new LocationsItem("ES00010101VIAS", "AEROPUERTO VIT ALMACÉN VIAS", "25-11-2003", "23-11-2016"),
			new LocationsItem("ES000141IA1001", "DEPOSITO DISTINTO DEL ADUANERO ALDITRANS JUNDIZ S.L", "24-11-2016", "28-09-2018")
		};

		var errors = ParserToRun.ConvertRecordsToXMLFile(locationsToExport, outputFile);
		Assert.That(errors, Is.Empty);
		var expectedXML = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.ESReferenceData.Tests.Business.CodeLists.Locations.TestFiles.Output.RefCusCodeListZZ_ES_LOCATIONS_CODES.xml");
		var xml = File.ReadAllText(outputFile);
		Assert.That(xml, Is.EqualTo(expectedXML));
	}

	[Test]
	public void NoRecordsToProcess()
	{
		var errors = ParserToRun.ConvertRecordsToXMLFile(Enumerable.Empty<ILocationsItem>(), outputFile);
		Assert.That(errors, Does.Contain(UnableToLocateRecordsMessage));
	}

	[Test]
	public void EmptyDescriptionWithEndDateInThePastNoErrorMessageTest()
	{
		var locationsToExport = new List<ILocationsItem>
		{
			new LocationsItem("ES00010101DECO", "", "24-11-2016", "25-04-2019")
		};

		var errors = ParserToRun.ConvertRecordsToXMLFile(locationsToExport, outputFile);
		Assert.That(errors, Does.Not.Contain(UnableToImportLocationMessage));
	}


	[Test]
	[TestCase("Missing location", "", "AEROPUERTO VIT ALMACEN DECOEXA", "22-02-2019", "31-12-2099")]
	[TestCase("Missing description", "ES00010101DECO", "", "24-11-2016", "25-04-2059")]
	[TestCase("Missing start date", "ES00010101DECO", "AEROPUERTO VIT ALMACEN DECOEXA", "", "23-11-2016")]
	[TestCase("Invalid start date", "ES00010101DECO", "AEROPUERTO VIT ALMACEN DECOEXA", "11-23-2016", "23-11-2016")]
	[TestCase("Missing end date", "ES00010101EAT", "AEROPUERTO VIT ALMACEN DE EAT(DHL)", "24-04-2019", "")]
	[TestCase("Invalid end date", "ES00010101EAT", "AEROPUERTO VIT ALMACEN DE EAT(DHL)", "24-04-2019", "04-24-2019")]
	public void InvalidRecordMessageTest(string testCaseDescription, string location, string name, string startDate, string endDate)
	{
		var locationsToExport = new List<ILocationsItem>
		{
			new LocationsItem(location, name, startDate, endDate)
		};

		var errors = ParserToRun.ConvertRecordsToXMLFile(locationsToExport, outputFile);
		Assert.That(
			errors,
			Does.Contain(
				string.Join(Environment.NewLine,
					[
						UnableToImportLocationMessage + ". Details:",
						$"Location: {location}",
						$"Name: {name}",
						$"Start Date: {startDate}",
						$"End Date: {endDate}",
					])),
			testCaseDescription);
	}

	const string UnableToLocateRecordsMessage = "Unable to locate any records for Locations. The provider may not be returning data to process.";
	const string UnableToImportLocationMessage = "Unable to import Location as there is a missing or wrong attribute 'location', 'name', 'start date' or 'end date'";

	public override void OneTimeSetUp()
	{
		base.OneTimeSetUp();
		outputFile = Path.Combine(FileHelper.OutputFolder, "RefCusCodeListZZ_ES_LOCATIONS_CODES.xml");
	}
	string outputFile;

	protected override string TestClassName => nameof(LocationsParserTests);
}
