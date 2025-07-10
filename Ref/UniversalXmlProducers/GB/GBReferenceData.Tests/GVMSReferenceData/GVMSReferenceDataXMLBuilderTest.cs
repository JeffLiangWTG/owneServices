using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business;
using CargoWise.RefDbRepo.GBReferenceData.Business.GvmsReferenceData;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData
{
	[TestFixture]
	class GVMSReferenceDataXMLBuilderTest
	{
		[Test]
		public void ConvertCarrierToRefCarrierModelTest()
		{
			List<RefCarrierCode> refCarrierCodes = ReferenceData.Carriers.Select(x => GvmsReferenceDataConverter.ConvertCarrier(x)).ToList();

			Assert.That(refCarrierCodes.Count == 2);
			Assert.That(refCarrierCodes[0].RefCarrierCodeAttributes.Length == 1);
			Assert.That(refCarrierCodes[0].ZZ4_Code == "1");
			Assert.That(refCarrierCodes[1].ZZ4_Code == "2");
			Assert.That(refCarrierCodes[0].ZZ4_Description == "Stena Line");
			Assert.That(refCarrierCodes[0].RefCarrierCodeAttributes[0].ZZG_Value == "SE");
		}

		[Test]
		public void ConvertPortToRefCusCodeListModelTest()
		{
			List<RefCusCodeList> cdsRefCarrierCodes = ReferenceData.Ports.Where(port => GvmsReferenceDataConverter.IsCdsPort(port)).Select(port => GvmsReferenceDataConverter.ConvertCDSPort(port)).ToList();
			List<RefCusCodeList> chiefRefCarrierCodes = ReferenceData.Ports.Where(port => GvmsReferenceDataConverter.IsChiefPort(port)).Select(port => GvmsReferenceDataConverter.ConvertChiefPort(port)).ToList();

			Assert.That(cdsRefCarrierCodes.Count == 2);
			Assert.That(cdsRefCarrierCodes[0].ZZD_Code == "ABDABDABD");
			Assert.That(cdsRefCarrierCodes[0].ZZD_ZZK_NKCodeType == "PORT");
			Assert.That(cdsRefCarrierCodes[0].RefCusCodeListAttributes.Length == 1);
			Assert.That(cdsRefCarrierCodes[0].RefCusCodeListAttributes[0].ZZE_Value == "7054");

			Assert.That(cdsRefCarrierCodes[1].ZZD_Code == "FXTFXTFXTGVM");
			Assert.That(cdsRefCarrierCodes[1].ZZD_ZZZ_NKDataGrouping == "CDS");
			Assert.That(cdsRefCarrierCodes[1].RefCusCodeListAttributes.Length == 2);
			Assert.That(cdsRefCarrierCodes[1].RefCusCodeListAttributes[0].ZZE_ZXE_NKName == "GvmsPortId");
			Assert.That(cdsRefCarrierCodes[1].RefCusCodeListAttributes[0].ZZE_Value == "1733");
			Assert.That(cdsRefCarrierCodes[1].RefCusCodeListAttributes[1].ZZE_ZXE_NKName == "FACTY");
			Assert.That(cdsRefCarrierCodes[1].RefCusCodeListAttributes[1].ZZE_Value == "AU");

			Assert.That(chiefRefCarrierCodes.Count == 2);
			Assert.That(chiefRefCarrierCodes[0].ZZD_Code == "FXT");
			Assert.That(chiefRefCarrierCodes[1].ZZD_Code == "HEY");
			Assert.That(chiefRefCarrierCodes[0].ZZD_ZZK_NKCodeType == "PORT");
			Assert.That(chiefRefCarrierCodes[1].ZZD_ZZZ_NKDataGrouping == "GB");
			Assert.That(chiefRefCarrierCodes[0].RefCusCodeListAttributes.Length == 1);
			Assert.That(chiefRefCarrierCodes[0].RefCusCodeListAttributes[0].ZZE_ZXE_NKName == "GvmsPortId");
			Assert.That(chiefRefCarrierCodes[0].RefCusCodeListAttributes[0].ZZE_Value == "1733");
		}

		[Test]
		public void ConvertPortToRefLocoMapModelTest()
		{
			List<RefLocoMap> refLocoMaps = new List<RefLocoMap>();
			var ports = ReferenceData.Ports.Where(port => GvmsReferenceDataConverter.IsForeignPort(port));
			foreach (var port in ports)
			{
				refLocoMaps.AddRange(GvmsReferenceDataConverter.ConvertForeignPort(port));
			}

			Assert.That(refLocoMaps.Count == 3);
			Assert.That(refLocoMaps[0].RY_LocalPortCode == "7062");
			Assert.That(refLocoMaps[0].RY_RL_NKLocoPort == "GBCTM");
			Assert.That(refLocoMaps[1].RY_LocalPortCode == "7062");
			Assert.That(refLocoMaps[1].RY_RL_NKLocoPort == "GBGIL");
			Assert.That(refLocoMaps[2].RY_LocalPortCode == "7062");
			Assert.That(refLocoMaps[2].RY_RL_NKLocoPort == "GBSHS");
		}

		[Test]
		public void ConvertRoutesToRefCusCodeListModelTest()
		{
			List<RefCusCodeList> refCusCodeLists = ReferenceData.Routes.Select(x => GvmsReferenceDataConverter.ConvertRoute(x, ReferenceData.Ports, ReferenceData.Carriers)).ToList();

			Assert.That(refCusCodeLists.Count == 2);
			Assert.That(refCusCodeLists[0].ZZD_Code == "102");
			Assert.That(refCusCodeLists[0].ZZD_ZZK_NKCodeType == "GvmRt");
			Assert.That(refCusCodeLists[0].RefCusCodeListAttributes.Length == 4);
			Assert.That(refCusCodeLists[0].RefCusCodeListAttributes[0].ZZE_ZXE_NKName == "ArrivalPortId");
			Assert.That(refCusCodeLists[0].RefCusCodeListAttributes[0].ZZE_Value == "69");
			Assert.That(refCusCodeLists[0].ZZD_Description == "Route #102 from Aberdeen (7054) to Fake Port (69) via Stena Line (1)");
			Assert.That(refCusCodeLists[1].ZZD_Description == "Route #103 from Medway (7062) to Unknown (1401) via DFDS Seaways (2)");
		}

		[Test]
		public void ConvertErrorCodesToRefCusCodeListModelTest()
		{
			List<RefCusCodeList> ruleFailures = ReferenceData.RuleFailures.Select(x => GvmsReferenceDataConverter.ConvertRuleFailure(x)).ToList();

			Assert.That(ruleFailures.Count == 2);
			Assert.That(ruleFailures[0].ZZD_Code == "BR005");
			Assert.That(ruleFailures[0].ZZD_Description == "A GMR can only be used in one crossing between two customs territiories");
			Assert.That(ruleFailures[0].ZZD_ZZK_NKCodeType == "ERRCD");
			Assert.That(ruleFailures[0].ZZD_ZZZ_NKDataGrouping == "GB");
		}

		[Test]
		public void ConvertInspectionLocationsToRefCusCodeListModelTest()
		{
			var locationRefCodes = ReferenceData.InspectionLocations.Select(location => GvmsReferenceDataConverter.ConvertInspectionLocation(location)).ToList();
			AssertConvertInspectionLocationsToRefCusCodeListModel(locationRefCodes[0], "Location 1", "Location Description 1", "Line 1, Line 3, Town 1, Postcode 1", "Inland Border Facility");
			AssertConvertInspectionLocationsToRefCusCodeListModel(locationRefCodes[1], "Location 2", "Location Description 2", "Line 4, Line 5, Town 2, Postcode 2", "A facility that carries out all inspection types");
		}

		void AssertConvertInspectionLocationsToRefCusCodeListModel(RefCusCodeList refCusCode, string expectedCode, string expectedDescription, string expectedAddress, string expectedType)
		{
			Assert.That(refCusCode.ZZD_Code == expectedCode);
			Assert.That(refCusCode.ZZD_Description == expectedDescription);
			Assert.That(refCusCode.ZZD_ZZK_NKCodeType == Constants.GvmsDefaults.Codes.GVMSIL);
			Assert.That(refCusCode.ZZD_ZZZ_NKDataGrouping == Constants.DefaultValues.GBDataGrouping);
			Assert.That(refCusCode.ZZD_StartDate == new DateTime(2023, 05, 22));
			Assert.That(refCusCode.ZZD_EndDate == new DateTime(2023, 05, 22, 23, 59, 0));

			Assert.That(refCusCode.RefCusCodeListAttributes.Length == 2);
			Assert.That(refCusCode.RefCusCodeListAttributes[0].ZZE_ZXE_NKName == Constants.AttributeNames.GvmsAddress);
			Assert.That(refCusCode.RefCusCodeListAttributes[0].ZZE_Value == expectedAddress);
			Assert.That(refCusCode.RefCusCodeListAttributes[1].ZZE_ZXE_NKName == Constants.AttributeNames.GvmsType);
			Assert.That(refCusCode.RefCusCodeListAttributes[1].ZZE_Value == expectedType);
		}

		[Test]
		public void ConvertInspectionTypesToRefCusCodeListModelTest()
		{
			var locationTypesRefCodes = ReferenceData.InspectionTypes.Select(inspectionType => GvmsReferenceDataConverter.ConvertInspectionType(inspectionType)).ToList();
			AssertConvertInspectionTypesToRefCusCodeListModel(locationTypesRefCodes[0], "Inspection Type 1", "Description 1");
			AssertConvertInspectionTypesToRefCusCodeListModel(locationTypesRefCodes[1], "Inspection Type 2", "Description 2");
		}

		void AssertConvertInspectionTypesToRefCusCodeListModel(RefCusCodeList refCusCode, string expectedTypeId, string expectedDescription)
		{
			Assert.That(refCusCode.ZZD_Code == expectedTypeId);
			Assert.That(refCusCode.ZZD_Description == expectedDescription);
			Assert.That(refCusCode.ZZD_ZZK_NKCodeType == Constants.GvmsDefaults.Codes.GVMSIT);
			Assert.That(refCusCode.ZZD_ZZZ_NKDataGrouping == Constants.DefaultValues.GBDataGrouping);
			Assert.That(refCusCode.ZZD_StartDate == DefaultValues.MinimumDateTime);
			Assert.That(refCusCode.ZZD_EndDate == DefaultValues.MaximumDateTime);
		}

		[Test]
		public void TestBuildXML()
		{
			var xmlBuilder = new GvmsReferenceDataBuilder();
			var testDate = new DateTime(2020, 10, 14, 22, 22, 22);
			xmlBuilder.BuildXml(TempFolder, ReferenceData, testDate);

			var files = Directory.GetFiles(TempFolder, "*.xml", SearchOption.TopDirectoryOnly);
			Assert.That(files.Length == 6);

			var result = files.ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => File.ReadAllText(f));
			foreach (var file in result)
			{
				var filename = file.Key.Replace(testDate.ToString("yyyyMMdd", CultureInfo.CurrentCulture), string.Empty);
				var expected = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData.TestFiles.Output.{filename}TEST.xml");
				Assert.That(expected == file.Value);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new ReferenceData();

			// Carriers
			ReferenceData.Carriers = new List<Carrier>
			{
				new Carrier() { CarrierId = "1", CarrierName = "Stena Line", CountryCode = "SE" },
				new Carrier() { CarrierId = "2", CarrierName = "DFDS Seaways", CountryCode = "DK" }
			};

			// Routes
			ReferenceData.Routes = new List<Route>
			{
				new Route()
				{
					RouteId = "102",
					RouteEffectiveFrom = new DateTime(2020, 1, 1),
					RouteEffectiveTo = new DateTime(2020, 1, 2),
					DeparturePortId = "7054",
					RouteDirection = "UK_OUTBOUND",
					ArrivalPortId = "69",
					CarrierId = "1"
				},
				new Route()
				{
					RouteId = "103",
					RouteEffectiveFrom = new DateTime(2020, 1, 1),
					DeparturePortId = "7062",
					RouteDirection = "UK_OUTBOUND",
					ArrivalPortId = "1401",
					CarrierId = "2"
				}
			};

			//Ports
			ReferenceData.Ports = new List<Port>
			{
				new Port()
				{
					PortId = "7054",
					PortRegion = "Scotland",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Aberdeen",
					ChiefPortCode = "",
					CdsPortCode = "ABDABDABD",
					OfficeOfTransitCustomsOfficeCode = "UNKNOWN?",
					TimezoneId = "Europe/London"
				},
				new Port()
				{
					PortId = "69",
					PortRegion = "Fake Town",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Fake Port",
					ChiefPortCode = "",
					CdsPortCode = "",
					OfficeOfTransitCustomsOfficeCode = "UNKNOWN?",
					TimezoneId = "Europe/London"
				},
				new Port()
				{
					PortId = "7062",
					PortRegion = "London",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Medway",
					ChiefPortCode = "",
					CdsPortCode = "",
					OfficeOfTransitCustomsOfficeCode = "UNKNOWN?",
					TimezoneId = "Europe/London"
				},
				new Port()
				{
					PortId = "1733",
					PortRegion = "Anglia",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Felixstowe",
					ChiefPortCode = "FXT",
					CdsPortCode = "GBAUFXTFXTFXTGVM",
					OfficeOfTransitCustomsOfficeCode = "GB000051",
					TimezoneId = "Europe/London"
				},
				new Port()
				{
					PortId = "1849",
					PortRegion = "Northwest",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Heysham",
					ChiefPortCode = "HEY",
					CdsPortCode = "",
					OfficeOfTransitCustomsOfficeCode = "GB005210",
					TimezoneId = "Europe/London"
				}/*,
				new Port()
				{
					PortId = "1755",
					PortRegion = "Central",
					PortEffectiveFrom = "2000-01-01",
					PortDescription = "Tilbury",
					ChiefPortCode = "LON",
					CdsPortCode = "GBAUTILLONTILGVM",
					OfficeOfTransitCustomsOfficeCode = "GB000093",
					TimezoneId = "Europe/London"
				}*/
			};

			// Rule Failures
			ReferenceData.RuleFailures = new List<RuleFailure>
			{
				new RuleFailure()
				{
					RuleId = "BR005",
					RuleDescription = "A GMR can only be used in one crossing between two customs territiories"
				},
				new RuleFailure()
				{
					RuleId = "BR011",
					RuleDescription = "This Customs declaration cannot be in more than one GMR"
				}
			};

			// Inspection Locations
			ReferenceData.InspectionLocations = new List<InspectionLocation>
			{
				new InspectionLocation()
				{
					LocationId = "Location 1",
					LocationDescription = "Location Description 1",
					Address = new InspectionLocation.InspectionLocationAddress() { Lines = new List<string>() { "Line 1", "", "Line 3" }, Town = "Town 1", Postcode = "Postcode 1" },
					LocationType = "IBF",
					SupportedDirections = new List<string>() { "UK_INBOUND" },
					LocationEffectiveFrom = new DateTime(2023, 05, 22),
					LocationEffectiveTo = new DateTime(2023, 05, 23),
					SupportedInspectionTypeIds = new List<string>() {"Port 1", "Port 2" },
					RequiredInspectionLocations = new List<string>() {"Location 1", "Location 2" },
				},
				new InspectionLocation()
				{
					LocationId = "Location 2",
					LocationDescription = "Location Description 2",
					Address = new InspectionLocation.InspectionLocationAddress() { Lines = new List<string>() { "", "Line 4", "Line 5" }, Town = "Town 2", Postcode = "Postcode 2" },
					LocationType = "ALL",
					SupportedDirections = new List<string>() { "UK_INBOUND", "GB_TO_NI" },
					LocationEffectiveFrom = new DateTime(2023, 05, 22),
					LocationEffectiveTo = new DateTime(2023, 05, 23),
					SupportedInspectionTypeIds = new List<string>() {"Port 3", "Port 4" },
					RequiredInspectionLocations = new List<string>() {"Location 3", "Location 4" },
				},
			};

			//Inspection Types
			ReferenceData.InspectionTypes = new List<InspectionType>
			{
				new InspectionType()
				{
					InspectionTypeId = "Inspection Type 1",
					Description = "Description 1",
				},
				new InspectionType()
				{
					InspectionTypeId = "Inspection Type 2",
					Description = "Description 2",
				},
			};

			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		string TempFolder;
		ReferenceData ReferenceData;
	}
}
