using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Business.ManualProcessor;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.ManualProcessor
{
	[TestFixture]
	public class CACDocumentTypesLoaderTest
	{
		[Test]
		public void TestCADocumentTypesList()
		{
			var list = CsvLoader.GetCADocumentTypesList(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\ManualProcessor\CACDocumentTypes.csv"));
			Assert.AreEqual(34, list.Count);
			CollectionAssert.AreEquivalent(list.Select(x => x.GovAgencyIDCode), ExpectedIDCodeList);
			CollectionAssert.AreEquivalent(list.Select(x => x.Code), ExpectedCodeList);
			CollectionAssert.AreEquivalent(list.Select(x => x.Desc), ExpectedDescriptionList);
			CollectionAssert.AreEquivalent(list.Select(x => x.IsDefaultRefNum), ExpectedIsDefaultRefNumList);
		}

		IEnumerable<string> ExpectedIDCodeList => new string[]
		{
			"GAC","GAC","GAC","GAC","GAC","GAC",
			"NRCan","NRCan","NRCan","NRCan",
			"TC","TC","TC","TC","TC","TC",
			"HC","HC","HC","HC","HC",
			"DFO","DFO","DFO","DFO","DFO",
			"ECCC","ECCC",
			"PHAC",
			"CNSC","CNSC",
			"GAC","GAC","GAC"
		};

		IEnumerable<string> ExpectedCodeList => new string[]
		{
			"2001","2003","2004","2005","2006","2007",
			"3001","3002","3003","3004",
			"4001","4002","4003","4004","4005","4006",
			"5001","5002","5003","5004","5005",
			"6000","6001","6002","6003","6004",
			"8000","8001",
			"5503",
			"7000","7001",
			"80","81","83"
		};

		IEnumerable<string> ExpectedDescriptionList => new string[]
		{
			"Clothing and Textiles - Clothing and Textiles Shipment Specific Permit",
			"Agriculture Products - Agriculture Products - Shipment-Specific Permit",
			"Agriculture Products - Agriculture Products General Import Permit (GIP)",
			"Generic - Documented Alternative Quantity",
			"Steel - General Import Permit",
			"Foreign Export Licence",

			"Explosives - Annual Explosives Permit ( Type A)",
			"Explosives - Single Use Explosives Permit (Type G)",
			"Explosives - Tour Event or International Competition Explosives Permit",
			"Rough Diamonds - Kimberley Process Certificate",

			"Generic - Other Document",
			"Vehicles - Case-by-Case Authorization",
			"Generic Manufacturer Letter of Compliance",
			"Generic - Vehicle Title",
			"Vehicles - Manufacturer Certificate of Origin",
			"Generic - Racing Sanctioning Body Letter",

			"Active Pharmaceutical Ingredients - Establishment Licence (EL)",
			"Blood and Blood Components - Establishment Licence (EL)",
			"Blood and Blood Components - Proof of Prescription",
			"Cells  Tissues and Organs - Importer Establishment Registration",
			"Cells  Tissues and Organs - Exporter Establishment Registration",

			"Aquatic Biotechnology - New Substances Notification",
			"Invasive Species - Federal Release/Transfer Licence",
			"Invasive Species - Alberta Fisheries Licence",
			"Trade Tracking - Atlantic Bluefin Tuna Catch Document (ICCAT)",
			"Trade Tracking - Patagonian Toothfish (Dissostichus) Catch Document (CCAMLR)",

			"Waste Reduction and Management - Movement Type",
			"Waste Reduction and Management - Hazardous Waste/Hazardous Recyclable (HW/HRM) Permit",

			"Pathogen and Toxin Licence",

			"Import Program - Licence",
			"Import Program - Device Certificate",

			"Carbon steel",
			"Specialty Steel Products",
			"Aluminum Products"
		};


		IEnumerable<string> ExpectedIsDefaultRefNumList => new string[]
		{
			"N","N","N","N","N","N",
			"N","N","N","N",
			"Y","N","Y","Y","Y","Y",
			"N","N","N","N","N",
			"N","N","N","N","N",
			"N","N",
			"N",
			"N","N",
			"N","N","N"
		};
	}
}
