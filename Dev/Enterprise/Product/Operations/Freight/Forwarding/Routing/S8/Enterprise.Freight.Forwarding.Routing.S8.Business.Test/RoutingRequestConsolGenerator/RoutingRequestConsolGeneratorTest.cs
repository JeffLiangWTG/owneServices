using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class RoutingRequestConsolGeneratorTest : TestCaseWithFactory
	{
		public void TestRoutingRequestConsolGenerator_SetDefaultValues()
		{
			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			var generator = new RoutingRequestConsolGenerator(MultiDaysSelectionForTest);
			AssertEquals("WeightUnit", Constants.Weight.Kilograms, generator.WeightUnit);
			AssertEquals("VolumeUnit", Constants.Volume.CubicMetres, generator.VolumeUnit);
		}

		public void TestGenerateConsols_SetConsolFields()
		{
			var multiDaysSelection = MultiDaysSelectionForTest;

			ZShort consolsPerFlight = 1;
			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				Weight = 200m,
				WeightUnit = Constants.Weight.Kilograms,
				Volume = 0m,
				VolumeUnit = Constants.Volume.CubicMetres,
				Shipments = 10,
				Chargeable = 800m,
				AirlinePrefix = "QF",
				AllocateNeutralMaster = true,
				ConsolsPerFlight = consolsPerFlight
			};

			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			AssertEquals("Precondition - Expected no consols to be created.", 0, multiDaysSelection.CreatedConsols.Count);

			var header = CreateHeader();
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);

			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());

			var consolsExpected = consolsPerFlight * departureDates.Count;
			AssertEquals("Precondition - incorrect number of consols created.", consolsExpected, multiDaysSelection.CreatedConsols.Count);
			var airlinePrefixExpected = RefAirline.LoadFromAirline2LetterCode(Factory, generator.AirlinePrefix).RM_EagleAddedAirlinePrefixOrAccountingCode;

			var createdConsol = multiDaysSelection.CreatedConsols[0];
			AssertEquals(generator.WeightUnit, createdConsol.JK_TotalShipmentChargeableUnit);
			AssertEquals(generator.Weight, createdConsol.JK_TotalShipmentActWeightCheck);
			AssertEquals(generator.Volume, createdConsol.JK_TotalShipmentActVolumeCheck);
			AssertEquals(generator.Shipments, createdConsol.JK_TotalShipmentCountCheck);
			AssertEquals(generator.Chargeable, createdConsol.JK_TotalShipmentChargableCheck);
			AssertEquals(airlinePrefixExpected, createdConsol.JK_MasterBillNum);
			AssertEquals(Constants.TransportModes.Air, createdConsol.JK_TransportMode);
			AssertEquals(generator.AllocateNeutralMaster, createdConsol.JK_IsNeutralMaster);
		}

		public void TestGenerateConsols_CreateTransports()
		{
			var header = CreateHeader();
			ZShort consolsPerFlight = 1;

			var multiDaysSelection = MultiDaysSelectionForTest;

			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = consolsPerFlight
			};

			AssertEquals("Precondition - should be no consols created so far.", 0, multiDaysSelection.CreatedConsols.Count);

			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);
			var consolsExpected = consolsPerFlight * sailingList.Count;
			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());
			AssertEquals("Precondition - incorrect quantity of consols created.", consolsExpected, multiDaysSelection.CreatedConsols.Count);

			var consol1 = multiDaysSelection.CreatedConsols[0];
			var sailingsConsol1 = sailingList[0] as JobSailingCollection;
			AssertEquals("Consol contains incorrect number of transport lines.", sailingsConsol1.Count, consol1.Transports.Count);
			AssertEquals("Consol contains incorrect Load Port.", "AU" + header.Origin, consol1.JK_RL_NKLoadPort);
			AssertEquals("Consol contains incorrect Discharge Port.", "AU" + header.Destination, consol1.JK_RL_NKDischargePort);

			AssertEquals("Trasport line PK should correspond to its sailing.", sailingsConsol1[0].PK, consol1.Transports[0].JW_JX);
			Assert(consol1.Transports[0].JW_JX_IsPublished);
			AssertEquals("Trasport line PK should correspond to its sailing.", sailingsConsol1[1].PK, consol1.Transports[1].JW_JX);
			Assert(consol1.Transports[1].JW_JX_IsPublished);

			var consol2 = multiDaysSelection.CreatedConsols[1];
			var sailingsConsol2 = sailingList[1] as JobSailingCollection;
			AssertEquals("Consol contains incorrect number of transport lines.", sailingList[0].Count, consol2.Transports.Count);
			AssertEquals("Consol contains incorrect Load Port.", "AU" + header.Origin, consol2.JK_RL_NKLoadPort);
			AssertEquals("Consol contains incorrect Discharge Port.", "AU" + header.Destination, consol2.JK_RL_NKDischargePort);

			AssertEquals("Trasport line PK should correspond to its sailing.", sailingsConsol2[0].PK, consol2.Transports[0].JW_JX);
			Assert(consol2.Transports[0].JW_JX_IsPublished);
			AssertEquals("Trasport line PK should correspond to its sailing.", sailingsConsol2[1].PK, consol2.Transports[1].JW_JX);
			Assert(consol2.Transports[1].JW_JX_IsPublished);
		}

		public void TestGenerateConsols_NumberOfConsolsPerHeader()
		{
			ZShort consolsPerFlight = 5;

			var multiDaysSelection = MultiDaysSelectionForTest;

			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = consolsPerFlight
			};

			AssertEquals("Precondition - should be no consols created so far.", 0, multiDaysSelection.CreatedConsols.Count);

			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1),
				requestedDate.AddDays(2)
			};

			var header = CreateHeader();
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);

			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());

			var expectedConsols = consolsPerFlight * departureDates.Count;
			AssertEquals("Consols quantity is incorrect.", expectedConsols, multiDaysSelection.CreatedConsols.Count);
		}

		public void TestGenerateConsols_SetTransportTerminalCutOff()
		{
			var multiDaysSelection = MultiDaysSelectionForTest;
			var header = CreateHeader();
			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);

			var startOfYear = new ZDateTime(2018, 1, 1);
			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = 1,
				CTOCutOff = new ZDateTime(2018, 1, 1, 2, 30, 0)
			};
			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());
			AssertEquals("Precondition - incorrect number of consols created.", 2, multiDaysSelection.CreatedConsols.Count);

			var consol1 = multiDaysSelection.CreatedConsols[0];
			var consol1Sailings = sailingList[0] as JobSailingCollection;
			AssertTransportCutOffFields(consol1, consol1Sailings[0].Origin.JA_E_DEP.AddMinutes(-150), ZDateTime.Empty);
			var consol2 = multiDaysSelection.CreatedConsols[1];
			var consol2Sailings = sailingList[1] as JobSailingCollection;
			AssertTransportCutOffFields(consol2, consol2Sailings[0].Origin.JA_E_DEP.AddMinutes(-150), ZDateTime.Empty);
		}

		public void TestGenerateConsols_SetTransportDepotCutOff()
		{
			var multiDaysSelection = MultiDaysSelectionForTest;
			var header = CreateHeader();
			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);

			var startOfYear = new ZDateTime(2018, 1, 1);
			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = 1,
				CFSCutOff = new ZDateTime(2018, 1, 1, 2, 30, 0)
			};
			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());
			AssertEquals("Precondition - incorrect number of consols created.", 2, multiDaysSelection.CreatedConsols.Count);

			var consol1 = multiDaysSelection.CreatedConsols[0];
			var consol1Sailings = sailingList[0] as JobSailingCollection;
			AssertTransportCutOffFields(consol1, ZDateTime.Empty, consol1Sailings[0].Origin.JA_E_DEP.AddMinutes(-150));
			var consol2 = multiDaysSelection.CreatedConsols[1];
			var consol2Sailings = sailingList[1] as JobSailingCollection;
			AssertTransportCutOffFields(consol2, ZDateTime.Empty, consol2Sailings[0].Origin.JA_E_DEP.AddMinutes(-150));
		}

		public void TestNeutralAirWaybillServiceLevelList()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "MU").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			var multiDaysSelection = MultiDaysSelectionForTest;
			var header = CreateHeader();
			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = 1,
				CFSCutOff = new ZDateTime(2018, 1, 1, 2, 30, 0)
			};

			AssertEquals("XXX, EXP and STD", 3, generator.NeutralAirWaybillServiceLevelList.Count);
			var serviceLevels = generator.NeutralAirWaybillServiceLevelList.Cast<OrgCarrierServiceLevel>();
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "XXX"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == "EXP"));
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == OrgCarrierServiceLevel.StandardCode));

			generator.AirlinePrefix = "NZ";
			AssertEquals("STD", 1, generator.NeutralAirWaybillServiceLevelList.Count);
			serviceLevels = generator.NeutralAirWaybillServiceLevelList.Cast<OrgCarrierServiceLevel>();
			AssertNotNull(serviceLevels.FirstOrDefault(s => s.PL_Code == OrgCarrierServiceLevel.StandardCode));
		}

		public void TestConsolServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "MU").PK;

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "XXX";
			serviceLevel1.PL_CarrierServiceLevelDescription = "XXX Description";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "EXP";
			serviceLevel2.PL_CarrierServiceLevelDescription = "EXP Description";

			Factory.Save();

			var multiDaysSelection = MultiDaysSelectionForTest;
			var header = CreateHeader();
			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);
			var generator = new RoutingRequestConsolGenerator(multiDaysSelection)
			{
				ConsolsPerFlight = 1,
				CFSCutOff = new ZDateTime(2018, 1, 1, 2, 30, 0),
				ServiceLevel = "EXP"
			};

			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());
			AssertEquals(2, multiDaysSelection.CreatedConsols.Count);

			var consol1 = multiDaysSelection.CreatedConsols[0];
			AssertEquals("EXP", consol1.JK_AWBServiceLevel);
			var consol2 = multiDaysSelection.CreatedConsols[1];
			AssertEquals("EXP", consol2.JK_AWBServiceLevel);
		}

		#region Implementation

		protected RoutingResponseHeader CreateHeader()
		{
			var messageLine = "100 SYD BNE 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 BNE 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(messageLine, Factory);
			Factory.Save();
			return header;
		}

		RoutingMultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var requestDate = new ZDateTime(2018, 7, 6);
				var testMessageLine1 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
				var testMessageLine2 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
				var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
				{
					header1,
					header2
				};

				return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
			}
		}

		void AssertTransportCutOffFields(CommonConsol consol, ZDateTime expectedTerminalCutOff, ZDateTime expectedDepotCutOff)
		{
			AssertEquals($"Precondition - consol contains incorrect number of transport lines.", 2, consol.Transports.Count);
			var terminalCutOffMessage = expectedTerminalCutOff == ZDateTime.Empty ?
				"Terminal Cut Off should not be set for first transport line as CTOCutOff is not set" :
				"Terminal Cut Off should be set to (Departure Time - CTOCutOff) for first transport line";
			var depotCutOffMessage = expectedDepotCutOff == ZDateTime.Empty ?
				"Depot Cut Off should not be set for first transport line as CFSCutOff is not set" :
				"Depot Cut Off should be set to (Departure Time - CFSCutOff) for first transport line";
			CombineAssertions(() =>
			{
				AssertEquals(terminalCutOffMessage, expectedTerminalCutOff, consol.Transports[0].JW_TerminalCutOff);
				AssertEquals(depotCutOffMessage, expectedDepotCutOff, consol.Transports[0].JW_DepotCutOff);
				AssertEquals("Terminal Cut Off should not be set for other transport lines", ZDateTime.Empty, consol.Transports[1].JW_TerminalCutOff);
				AssertEquals("Depot Cut Off should not be set for other transport lines", ZDateTime.Empty, consol.Transports[1].JW_DepotCutOff);
			});
		}

		#endregion
	}
}
