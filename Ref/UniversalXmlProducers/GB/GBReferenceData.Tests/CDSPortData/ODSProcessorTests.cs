using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Config;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	public class ODSProcessorTests
	{
		[TestCase("None")]
		[TestCase("BIP")]
		[TestCase("COA")]
		[TestCase("CSE")]
		[TestCase("DEP")]
		[TestCase("DES")]
		[TestCase("ETSF")]
		[TestCase("ITSF")]
		[TestCase("ITFSR")]
		[TestCase("Other")]
		[TestCase("GVMS")]
		[TestCase("Rail")]
		[TestCase("RORO")]
		public void ExtractContent(string name)
		{
			var configProvider = new ConfigProvider("CDSPortConfig.xml");
			var source = ConfigLoader.LoadConfigFile(configProvider).FirstOrDefault(x => x.Name == name);
			Assert.IsNotNull(source, $"Pre-requisite: configuration for {name}");

			var resultsData = TestResults.FirstOrDefault(x => x.name == name);
			Assert.IsNotNull(resultsData, $"Pre-requisite: results data for {name}");

			var content = TestHelper.ReadManifestResourceContentBytes($"CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.{resultsData.filename}");

			var errorCollector = new StringBuilder();
			var data = new ODSProcessor(source, errorCollector, Mocker.GetDownloadManagerForBinary(content)).Extract().ToList();

			Assert.That(data.Count, Is.EqualTo(resultsData.expectedCount));

			Assert.Multiple(() =>
			{
				foreach (var (index, code, description, additionalInfo) in resultsData.Item4)
				{
					Assert.That(data[index].Code, Is.EqualTo(code), $"Item {index} Code");
					Assert.That(data[index].Description, Is.EqualTo(description), $"Item {index} Description");
					Assert.That(data[index].AdditionalInfo, Is.EqualTo(additionalInfo), $"Item {index} AdditionalInfo");
				}
			});

			Assert.That(errorCollector.ToString(), Is.Empty, "ErrorCollector");
		}

		static readonly (string name, string filename, int expectedCount, (int index, string code, string description, string additionalInfo)[])[] TestResults =
		{
			(
				"None", "20250530_CDS_DE_5-23_Appendix16C_Maritimeportsandwharves.ods", 323,
				[
					(0, "GBAUABDABDADP", "Aberdeen - Denholm Port Services Ltd", ""),
					(78, "GBAUFXTFXTFXT", "Felixstowe", ""),
					(322, "GBAUWORWORWOR", "Workington", ""),
				]
			),
			(
				"BIP", "20250131_CDS_DE_5-23_Appendix16G_Borderinspectionposts.ods", 3,
				[
					(0, "GBAUEMAEMABIP", "North West Leicestershire District Council", ""),
					(2, "GBAUSTNLSABIP", "Stansted Airport Border Inspection Post Limited", ""),
				]
			),
			(
				"COA", "20250131_CDS_DE_5-23_Appendix16B_RegulatedAerodromes.ods", 113,
				[
					(0, "GBCUSAWLSASAW", "Audley End Airfield – Saffron Walden", ""),
					(72, "GBCUHMELSAHME", "Peterborough (Conington) Business Airfield — Holme", ""),
					(112, "GBCUYEOYVLYEO", "Yeovil Aerodrome", ""),
				]
			),
			(
				"CSE", "20250131_CDS_DE_5-23_Appendix16L_LocationCodesToDeclareGoodsForExportAtCSE.ods", 66,
				[
					(0, "GBBUABDLEACSE", "Aberdeen, 28 Guild Street, Aberdeen. AB11 6GY", ""),
					(55, "GBBUSWALBCCSE", "Swansea", ""),
					(65, "GBBUYRKLDCCSE", "York", ""),
				]
			),
			(
				"DEP", "20250417_CDS_DE_5-23_Appendix16K_DesignatedExportPlace.ods", 86,
				[
					(0, "GBAULHRLHRXBN", "Air Menzies International Ltd", ""),
					(14, "GBAUCLPTILCTD", "Crown Fine Art Ltd", ""),
					(85, "GBAUASFLHROWD", "X One Wholesale Ltd", ""),
				]
			),
			(
				"DES", "20250131_CDS_DE_5-23_Appendix16A_Locatiocodeforairports.ods", 51,
				[
					(0, "GBAUDYCABZDYC", "Aberdeen Airport - Dyce", ""),
					(32, "GBAULHRLHRLHR", "London Heathrow Airport", ""),
					(50, "GBAUWDDWADWDD", "Waddington RAF Base", ""),
				]
			),
			(
				"ETSF", "20250606_CDS_DE_5-23_Appendix16F_External_Temporary_Storage_Facilities.ods", 690,
				[
					(0, "GBAUSLSSTNTCL1", "3TC Logistics Limited Unit 1, The Ferns, Common Road, Whiteparish, Wiltshire. SP5 2RD", ""),
					(345, "GBAUSTSPTMFRA1", "Jersey Post Global Logistics UK Ltd Unit 1, Quadra Point, Sharps Close, Anchorage Park, Portsmouth. PO3 5PL", ""),
					(689, "GBAULGXLGPZIE1", "Ziegler UK LTD North 4, Channel Close, Stanford-Le-Hope. SS17 9FJ", ""),
				]
			),
			(
				"ITSF", "20250606_CDS_DE_5-23_Appendix16D_Internal_Temporary_Storage_Facilities.ods", 122,
				[
					(0, "GBAUKLNKLNBNT", "Agrilynk Ltd Bentinck Dock, Port of Kings Lynn, Kings Lynn. PE20 2HA", ""),
					(9, "GBAUCCHBOHJWS", "Bournemouth Airport Hangar 401, Bournemouth Airport, Christchurch, Dorset. BH23 6SE", ""),
					(121, "GBAUSOUSTNYUW1", "YA Autotrade Dock Gate 4, Eastern Docks, Southampton. SO14 3AH", ""),
				]
			),
			(
				"ITFSR", "20250131_CDS_DE_5-23_Appendix16E_RemoteInternalTemporaryStorageFacilities.ods", 14,
				[
					(0, "GBAULSALSAEVS", "Aircraft Engineering Services Ltd c/o Ryanair Engineering Hangar 10, Long Border Road, Stansted. CM24 1RE", ""),
					(6, "GBAULHRLHRNCS", "Dnata Limited Unit 3 Northumberland Close, Stanwell, Middlesex. TW19 7LN", ""),
					(13, "GBAUEMAEMAUPS4", "UPS Limited Building 19 (Cargo Terminal 2), East Midlands Airport, Castle Donington. DE74 2SA", ""),
				]
			),
			(
				"Other", "20250131_CDS_DE_5-23_Appendix16J_-OtherLocationCodes.ods", 17,
				[
					(0, "GBDUBIFBIFBGTGAS", "Barrow Gas Terminal - Barrow in Furness", ""),
					(11, "GBDUGTYGTYPGTGAS", "Perenco Gas Terminal - Great Yarmouth", ""),
					(16, "GBDUMTPMTPTGTGAS", "Theddlethorpe Gas Terminal - Mablethorpe", ""),
				]
			),
			(
				"GVMS", "20250411_CDS_DE5-23_Appendix16S_GoodsVehicleMovementServicecodes.ods", 28,
				[
				(0, "GBAUABDABDABDGVM", "Aberdeen GVMS Port", ""),
				(1, "GBAUBELBELGVM", "Belfast GVMS Port", ""),
				(27, "GBAUWPTWPTGVM", "Warrenpoint GVMS Port", "")
				]
			),
			(
				"Rail", "20240131_CDS_DE_F-23_Appendix16M_RailLocationCodes.ods", 15,
				[
				(0, "GBAUSCPSCPBRS", "British Steel The Rail Terminal, Brigg Road, Scunthorpe. DN16 1BP", ""),
				(5, "GBAUASDASDASD", "Eurostar Ashford International.", ""),
				(14, "GBAUKLSDVYWHM", "WHM Daventry WH Malcom Ltd Railport, Daventry International Railport, Railport Approach, Crick, Kilsby, Northampton. NN6 7JZ", ""),
				]
			),
			(
				"RORO", "20250131_CDS_DE_5-23_Appendix16R_RollonRollOffPorts.ods", 24,
				[
				(0, "GBAUCYNAYRCYN", "Cairnryan", "Whole Location"),
				(2, "GBAUDVRDOVDVR", "Dover", "Eastern Docks excluding Dover Cargo Terminal & Eastern Arm"),
				(23, "GBAUTILLONTIL", "Tilbury", "34 Berth")
				]
			),
		};

		[Test]
		public void Extract_Error()
		{
			var source = new CDSPortSource { Code = "ABC", CodeColumn = 2, DescriptionColumns = [ 1 ], ODSDataTag = "This Text Certainly Does Not Appear In The File" };
			var content = TestHelper.ReadManifestResourceContentBytes($"CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData.TestFiles.Input.20250530_CDS_DE_5-23_Appendix16C_Maritimeportsandwharves.ods");
			var errorCollector = new StringBuilder();
			var processor = new ODSProcessor(source, errorCollector, Mocker.GetDownloadManagerForBinary(content));
			var data = processor.Extract().ToList();

			Assert.That(data.Count, Is.EqualTo(0), "Should not return any data");
			Assert.That(errorCollector.ToString(), Contains.Substring("Could not find the data tag in the source file"));
		}
	}
}
