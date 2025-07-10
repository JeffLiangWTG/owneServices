using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class CommonWorkSheetIntegrationTest : BaseRatingIntegrationTest
	{
		#region TestCommonWorkSheet_Container

		public void TestCommonWorkSheet_Container_ByWeight()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var move = cartage.ContainerBookedMoves.AddNew();
			PopulateContainer(move.Container, refContainer, containerNumber: "#1");
			var leg = move.CartageLegs.AddNew();
			leg.JU_RunSheetSequence = 1;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			var rateLine = CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Weight.Kilograms, 1m);

			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job.",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					E6_OSCostAmount = 2500m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription = "2500 Kilogram(s) @ AUD 1.00/KG"
				}
			};
			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of consol charge code level and rate line on CartageLeg should be calculated", null, expectedCosts, workSheet, false);

			chargeCode.AC_IsGroupageCharge = true;
			Factory.Save();
			rateLine.TL_WeightVolume = QuantityUnit.CN;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 500m;
			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					E6_OSCostAmount = 500m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription = "#1 (20GP) - 1 20GP Container(s) @ AUD 500.00/Container"
				}
			};
			AutoCostAndAssert("Rate line on WorkSheet should be calculated and rate line on CartageLeg should be filtered because of consol charge code level", null, expectedCosts, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected line",
				"Information: RateLine Filtered CC01-UNT-CN-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");
		}

		public void TestCommonWorkSheet_Container_ByVolume()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#1").PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			var rateLine = CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Volume.CubicMetres, 100m);

			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-M3-20GP-Costing TC01	reason:	no Volume measure on the job.",
				"Information: RateLine Filtered CC01-UNT-M3-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			// Containerised cartage doesn't have a volume.
			// Rating by volume will use the container gross weight of 2500kg and conversion factor so 2500 KG ~ 7.508 M3
			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					E6_OSCostAmount = 750.8m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription = "7.508 Cubic Meter(s) @ AUD 100.00/M3"
				}
			};
			AutoCostAndAssert("Runsheet should be autorated with container volume", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Container_ByHour()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_JC_Container = CreateCommonContainer(refContainer).PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_StartTime = DateTime.Today + TimeSpan.FromHours(9);
			workSheet.EY_EndTime = DateTime.Today + TimeSpan.FromHours(15);

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Time.Hours, 100m);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 600m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};

			AutoCostAndAssert("", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Container_ByDay()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_JC_Container = CreateCommonContainer(refContainer).PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_StartTime = DateTime.Today;
			workSheet.EY_EndTime = DateTime.Today + TimeSpan.FromDays(3);

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Time.Days, 1000m);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 3000m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};

			AutoCostAndAssert("", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Container_ByWeightAndServiceLevel()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_JC_Container = CreateCommonContainer(refContainer, grossWeight: 1000, containerNumber: "#1").PK;

			var transportCo = Helper.CreateCreditor("TC01");
			transportCo.OH_IsShippingProvider = true;
			transportCo.OH_IsLocalTransport = true;
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "NUU";
			serviceLevel.PL_CarrierServiceLevelDescription = "New Service Level";

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";
			new JobHeader.Loader(leg.Cartage).TryLoadOrCreate();

			var chargeCode = CreateChargeCode("CC01");

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.FRO, "AU", ZString.Empty);
			entry.TI_RC = refContainer.PK;
			entry.TI_PL_NKCarrierServiceLevel = "NUU";

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<UnitCalculator>().PerUnit = 2.5m;

			Factory.Save();

			AutoCostAndAssert("As there is no way of setting the service level currently, no match should be found.", null, null, workSheet, false);

			entry.TI_PL_NKCarrierServiceLevel = "";
			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected line",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job.",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					E6_OSCostAmount = 2500m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription = "1000 Kilogram(s) @ AUD 2.50/KG"
				}
			};
			AutoCostAndAssert("Runsheet should be autorated with container weight", null, expectedCosts, workSheet, false);
			chargeCode.AC_IsGroupageCharge = true;
			Factory.Save();
			line.TL_WeightVolume = QuantityUnit.CN;
			line.GetCalculator<UnitCalculator>().PerUnit = 500m;
			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 500m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};
			AutoCostAndAssert("Rate line on WorkSheet should be calculated and rate line on CartageLeg should be filtered because of consol charge code level", null, expectedCosts, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected line",
				"Information: RateLine Filtered CC01-UNT-CN-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");
		}

		#endregion

		#region TestCommonWorkSheet_Loose

		public void TestCommonWorkSheet_Loose_ByWeight()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_BookedWeight = 2500m;
			move.EW_WeightUQ = "KG";

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateCostWithUnitCalculator(costing, chargeCode, Constants.Weight.Kilograms, 1m);

			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-KG-Costing TC01	reason:	no Weight measure on the job.",
				"Information: RateLine Filtered CC01-UNT-KG-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 2500m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};
			AutoCostAndAssert("RunSheet should be autorated with Weight measure.", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Loose_ByVolume()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			move.EW_BookedVolume = 1000m;
			move.EW_VolumeUQ = "M3";

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateCostWithUnitCalculator(costing, chargeCode, Constants.Volume.CubicMetres, 1m);

			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-M3-Costing TC01	reason:	no Volume measure on the job.",
				"Information: RateLine Filtered CC01-UNT-M3-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 1000m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};
			AutoCostAndAssert("RunSheet should be autorated with Volume measure.", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Loose_ByHour()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_StartTime = DateTime.Today + TimeSpan.FromHours(9);
			workSheet.EY_EndTime = DateTime.Today + TimeSpan.FromHours(15);

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateCostWithUnitCalculator(costing, chargeCode, Constants.Time.Hours, 100m);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 600m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};

			AutoCostAndAssert("RunSheet should be autorated with Time measure.", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Loose_ByDay()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = Factory.New<CommonCartageLeg>();
			var move = Factory.New<CommonBookedCtgMove>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_StartTime = DateTime.Today;
			workSheet.EY_EndTime = DateTime.Today + TimeSpan.FromDays(3);

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateCostWithUnitCalculator(costing, chargeCode, Constants.Time.Days, 1000m);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 3000m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};

			AutoCostAndAssert("RunSheet should be autorated with Time measure.", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_Loose_ByAdHocJobServices()
		{
			const string registryCode = "NJS";

			var registryDefinedJobServices = FreightDataRegistry.Instance.JobServices.Value;
			registryDefinedJobServices.Add(registryCode, (NoResString)"Registry", false);

			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDefinedJobServices))
			{
				var fumChargeCode = CreateChargeCode("FUMTRN", Constants.FreightServiceType.Codes.Fumigation, true);
				var steChargeCode = CreateChargeCode("STETRN", Constants.FreightServiceType.Codes.SteamCleaning, true);
				var njsChargeCode = CreateChargeCode("NJSTRN", registryCode, true);

				AssertNoExceptionThrown("There should be no Ad Hoc service charge codes existing in clean db", () => Factory.Save());

				var move = Factory.NewWithValidTestData<CommonBookedCtgMove>();
				var cartage = Factory.NewWithValidTestData<CommonCartage>();
				new JobHeader.Loader(cartage).TryLoadOrCreate();
				move.EW_JJ = cartage.PK;
				move.EW_BookedWeight = 1500m;
				move.EW_BookedPackCount = 1;
				move.EW_WeightUQ = Constants.Weight.Kilograms;

				var leg = Factory.NewWithValidTestData<CommonCartageLeg>();
				leg.JU_RunSheetSequence = 1;
				leg.JU_EW = move.PK;

				var transportCo = Helper.CreateCreditor("TC01").PK;
				var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
				workSheet.CartageLegs.Add(leg);
				workSheet.EY_OH_TransportCo = transportCo;
				workSheet.EY_StartTime = DateTime.Today.AddDays(-4);
				workSheet.EY_EndTime = DateTime.Today.AddHours(4);

				Helper.CreateAdHocJobService(move, Constants.FreightServiceType.Codes.Fumigation, 100m, JobServiceInfo.Constants.Codes.ServiceOccurrence, transportCo);
				Helper.CreateAdHocJobService(move, Constants.FreightServiceType.Codes.SteamCleaning, 110m, JobServiceInfo.Constants.Codes.ServiceOccurrence, transportCo);
				Helper.CreateAdHocJobService(move, registryCode, 150m, JobServiceInfo.Constants.Codes.ServiceOccurrence, transportCo);

				Factory.Save();

				AutoCostAndAssert("Rate lines on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
				AssertAutoratingAuditLogNoteContainsLines(workSheet,
					"Log should contain expected lines",
					"Information: RateLine Filtered FUMTRN-Job Fumigation Service	reason:	FUMTRN charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001001.",
					"Information: RateLine Filtered NJSTRN-Job Registry Service	reason:	NJSTRN charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001001.",
					"Information: RateLine Filtered STETRN-Job Steam Cleaning Service	reason:	STETRN charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001001.");

				fumChargeCode.AC_IsGroupageCharge = false;
				steChargeCode.AC_IsGroupageCharge = false;
				njsChargeCode.AC_IsGroupageCharge = false;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = fumChargeCode.AC_Code,
						E6_OSCostAmount = 100m,
						CostCalculationDescription = "1 Port Transport Charges Fumigation @ AUD 100.00/Port Transport Charges Fumigation"
					},
					new AssertionCost
					{
						ChargeCode = steChargeCode.AC_Code,
						E6_OSCostAmount = 110m,
						CostCalculationDescription = "1 Port Transport Charges Steam Cleaning @ AUD 110.00/Port Transport Charges Steam Cleaning"
					},
					new AssertionCost
					{
						ChargeCode = njsChargeCode.AC_Code,
						E6_OSCostAmount = 150m,
						CostCalculationDescription = "1 Port Transport Charges Registry @ AUD 150.00/Port Transport Charges Registry"
					}
				};
				AutoCostAndAssert("Rate line on WorkSheet should be filtered because of consol charge code level and rate line on CartageLeg should be calculated", null, expectedCosts, workSheet, false);
			}
		}

		struct TestChargeCodes
		{
			public TestChargeCodes(AccChargeCode fumChargeCode, AccChargeCode dmeChargeCode, bool autorateConsolLevel)
			{
				this.FUMChargeCode = fumChargeCode;
				this.DMEChargeCode = dmeChargeCode;
				this.FUMChargeCode.AC_IsGroupageCharge = autorateConsolLevel;
				this.DMEChargeCode.AC_IsGroupageCharge = autorateConsolLevel;
			}

			public AccChargeCode FUMChargeCode { get; private set; }
			public AccChargeCode DMEChargeCode { get; private set; }
		}

		CommonWorkSheet SetupTestCommonWorkSheet_Loose_ByStandardAndHiddenJobServices(bool autorateConsolLevel, out TestChargeCodes testChargeCodes)
		{
			testChargeCodes = new TestChargeCodes(
				CreateChargeCode("FUMTRN", Constants.FreightServiceType.Codes.Fumigation),
				CreateChargeCode("DMETRN", ChargeCodeSubGroupList.CartageDemurrageTotal),
				autorateConsolLevel
			);

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var move = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			move.EW_BookedWeight = 1500m;
			move.EW_BookedPackCount = 1;
			move.EW_WeightUQ = Constants.Weight.Kilograms;

			var leg = Factory.NewWithValidTestData<CommonCartageLeg>();
			leg.JU_RunSheetSequence = 1;
			leg.JU_EW = move.PK;
			leg.JU_CartageDeliveryDemurrage = new ZDateTime(ZDateTime.Now.Year, 1, 1).AddHours(2);

			var transportCo = Helper.CreateCreditor("TC01");
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg);
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_StartTime = DateTime.Today.AddDays(-4);
			workSheet.EY_EndTime = DateTime.Today.AddHours(4);

			Helper.CreateAdHocJobService(move, Constants.FreightServiceType.Codes.Fumigation, 0, "", transportCo.PK);

			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.ALL, "AU", "");

			var line1 = entry.AddRateLine(testChargeCodes.FUMChargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var line2 = entry.AddRateLine(testChargeCodes.DMEChargeCode, UnitCalculator.Code, QuantityUnit.SV);
			line2.GetCalculator<UnitCalculator>().PerUnit = 200m;

			Factory.Save();

			return workSheet;
		}

		public void TestCommonWorkSheet_Loose_ByStandardAndHiddenJobServices()
		{
			var workSheet = SetupTestCommonWorkSheet_Loose_ByStandardAndHiddenJobServices(false, out TestChargeCodes testChargeCodes);
			workSheet.EY_RunSheetNumber = "McLaren";

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = testChargeCodes.FUMChargeCode.AC_Code,
					E6_OSCostAmount = 100m
				},
				new AssertionCost
				{
					ChargeCode = testChargeCodes.DMEChargeCode.AC_Code,
					E6_OSCostAmount = 200m
				}
			};

			AutoCostAndAssert("Service costs should be calculated from CartageMove (and CartageLegs)", null, expectedCosts, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Rate lines on worksheet should be filtered because of no service support of worksheet rating adapter",
				"Information: RateLine Filtered DMETRN-UNT-SV-Costing TC01	reason:	DME Service was not present",
				"Information: RateLine Filtered FUMTRN-UNT-SV-Costing TC01	reason:	FUM Service was not present");
		}

		#endregion

		#region TestCommonWorkSheet_MultipleCartageLegs

		public void TestCommonWorkSheet_MultipleCartageLegs()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer1 = Helper.Containers["20GP"];
			var refContainer2 = Helper.Containers["20HC"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();

			var leg1 = Factory.New<CommonCartageLeg>();
			var move1 = Factory.New<CommonBookedCtgMove>();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;
			move1.EW_JJ = cartage.PK;
			move1.EW_JC_Container = CreateCommonContainer(refContainer1, grossWeight: 3000).PK;

			var leg2 = Factory.New<CommonCartageLeg>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;
			move2.EW_JJ = cartage.PK;
			move2.EW_JC_Container = CreateCommonContainer(refContainer2, grossWeight: 2800).PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg2 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode1 = CreateChargeCode("CC01");
			var chargeCode2 = CreateChargeCode("CC02");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode1, refContainer1, Constants.Weight.Kilograms, 2m);
			CreateRateWithUnitCalculator(costing, chargeCode2, refContainer2, Constants.Weight.Kilograms, 1m);
			Factory.Save();

			AutoCostAndAssert("Rate lines on WorkSheet should be filtered because of measure type and rate lines on CartageLegs should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected line",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job.",
				"Information: RateLine Filtered CC02-UNT-KG-20HC-Costing TC01	reason:	no Weight measure on the job.",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.",
				"Information: RateLine Filtered CC02-UNT-KG-20HC-Costing TC01	reason:	CC02 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode1.AC_IsGroupageCharge = false;
			chargeCode2.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CC01", E6_OSCostAmount = 2m * 3000m, E6_ApportionmentMethod = AllocationMethod.Shipment },
				new AssertionCost { ChargeCode = "CC02", E6_OSCostAmount = 1m * 2800m, E6_ApportionmentMethod = AllocationMethod.Shipment }
			};
			AutoCostAndAssert("WorkSheet should be autorated with container weights.", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_MultipleCartageLegs_WithSameContainerType()
		{
			var refContainer = Helper.Containers["20GP"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();

			var leg1 = Factory.New<CommonCartageLeg>();
			var move1 = Factory.New<CommonBookedCtgMove>();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;
			move1.EW_JJ = cartage.PK;
			move1.EW_JC_Container = CreateCommonContainer(refContainer, grossWeight: 3000, containerNumber: "#1").PK;

			var leg2 = Factory.New<CommonCartageLeg>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;
			move2.EW_JJ = cartage.PK;
			move2.EW_JC_Container = CreateCommonContainer(refContainer, grossWeight: 4000, containerNumber: "#2").PK;

			var leg3 = Factory.New<CommonCartageLeg>();
			var move3 = Factory.New<CommonBookedCtgMove>();
			leg3.JU_RunSheetSequence = 3;
			leg3.JU_EW = move3.PK;
			move3.EW_JJ = cartage.PK;
			move3.EW_JC_Container = CreateCommonContainer(refContainer, grossWeight: 5000, containerNumber: "#3").PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg2, leg3 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Weight.Kilograms, 1.5m);

			Factory.Save();

			AutoCostAndAssert("Rate lines on WorkSheet should be filtered because of measure type and rate lines on CartageLegs should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					// 12000kg = 3000 + 4000 + 5000
					E6_OSCostAmount = 12000m * 1.5m,
					CostCalculationDescription =
@"3000 Kilogram(s) @ AUD 1.50/KG
4000 Kilogram(s) @ AUD 1.50/KG
5000 Kilogram(s) @ AUD 1.50/KG"
				}
			};
			AutoCostAndAssert("Runsheet should be autorated with sums of container weights", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbers_NonConsolLevelCharge() =>
			AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbers(false);

		public void TestCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbers_ConsolLevelCharge() =>
			AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbers(true);

		void AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbers(bool isConsolLevelCharge)
		{
			#region Setup

			var deliverToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverToOrg.MainAddress.OA_City = "Deliv";
			deliverToOrg.MainAddress.OA_Address1 = "1 Delivery St";

			var refContainer = Helper.Containers["20GP"];

			var cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage1).TryLoadOrCreate();
			cartage1.LocalClientAddressPK = NewClient.MainAddress.PK;
			var deliverDocAddress1 = cartage1.DocAddresses.AddNew(deliverToOrg.MainAddress);
			var leg1 = Factory.New<CommonCartageLeg>();
			var move1 = Factory.New<CommonBookedCtgMove>();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;
			leg1.JU_E2DeliveryAddressID = deliverDocAddress1.PK;
			move1.EW_JJ = cartage1.PK;
			move1.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#1").PK;
			move1.EW_E2PickupAddressID = cartage1.FirstDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage1.SecondDocAddress.PK;

			var cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage2).TryLoadOrCreate();
			cartage2.LocalClientAddressPK = NewClient.MainAddress.PK;
			var deliverDocAddress2 = cartage2.DocAddresses.AddNew(deliverToOrg.MainAddress);
			var leg2 = Factory.New<CommonCartageLeg>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;
			leg2.JU_E2DeliveryAddressID = deliverDocAddress2.PK;
			move2.EW_JJ = cartage2.PK;
			move2.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#2").PK;
			move2.EW_E2PickupAddressID = cartage2.FirstDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage2.SecondDocAddress.PK;

			var transportCo = Helper.CreateCreditor();
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg2 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("TCC");
			chargeCode.AC_IsGroupageCharge = isConsolLevelCharge;

			var costing = Helper.NewCosting(transportCo);
			var costLine = CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.BusinessQuantityUnit.Container, 100);
			costLine.Parent.TI_OA_CartageDeliveryAddressOverride = deliverToOrg.MainAddress.PK;

			var clientRate = Helper.NewClientRate(NewClient);
			CreateRateWithUnitCalculator(clientRate, chargeCode, refContainer, Constants.BusinessQuantityUnit.Container, 150);

			Factory.Save();

			#endregion

			#region Part 1 - Cost charge and its apportioned charges

			if (isConsolLevelCharge)
			{
				AutoCostAndAssert("Rate line on WorkSheet should be filtered because of delivery address and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
				AssertAutoratingAuditLogNoteContainsLines(workSheet,
					"Log should contain expected lines",
					"Delivery/Consignee Address didn't match job",
					"TCC charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000",
					"TCC charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001001");

				return;
			}

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "TCC",
					E6_LocalCostAmount = 200,
					CostCalculationDescription = @"
#1 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container"
				}
			};
			AutoCostAndAssert("Should create cost charges for 2 legs and merge them to 1", null, expectedCosts, workSheet, autorateRevenue: false);

			Factory.Save();

			var expectedCharges1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 100,
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol McLaren
#1 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container"
				}
			};
			var job1 = cartage1.Job as Job;
			AssertCharges(expectedCharges1, job1);
			AssertEquals("Expected correct container number for job1's charge", "#1", ((Charge)job1.Charges.Single()).JobChargeAttrib_ContainerNumber);

			var expectedCharges2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 100,
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol McLaren
#1 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container"
				}
			};
			var job2 = cartage2.Job as Job;
			AssertCharges(expectedCharges2, job2);
			AssertEquals("Expected correct container number for job2's charge", "#2", ((Charge)job2.Charges.Single()).JobChargeAttrib_ContainerNumber);

			#endregion

			#region Part 2 - New revenue charges merge to existing apportioned charges

			Factory.Save();

			job1 = cartage1.Job as Job;
			expectedCharges1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 150,
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol McLaren
#1 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container",
					RevenueCalculationDescription = "#1 (20GP) - 1 20GP Container(s) @ AUD 150.00/Container"
				}
			};
			AutorateAndAssert(expectedCharges1, cartage1, NewClient, job: job1, autorateCosts: false);

			job2 = cartage2.Job as Job;
			expectedCharges2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 150,
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol McLaren
#1 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 100.00/Container",
					RevenueCalculationDescription = "#2 (20GP) - 1 20GP Container(s) @ AUD 150.00/Container"
				}
			};
			AutorateAndAssert(expectedCharges2, cartage2, NewClient, job: job2, autorateCosts: false);

			#endregion
		}

		public void TestCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbersInTheSameCartage_NonConsolLevelCharges() =>
			AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbersInTheSameCartage(false);

		public void TestCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbersInTheSameCartage_ConsolLevelCharges() =>
			AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbersInTheSameCartage(true);

		void AssertCommonWorkSheet_MultipleCartageLegs_WithSameContainerType_DifferentContainerNumbersInTheSameCartage(bool isConsolLevelCharge)
		{
			#region Setup

			var deliverToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverToOrg.MainAddress.OA_City = "Deliv";
			deliverToOrg.MainAddress.OA_Address1 = "1 Delivery St";

			var refContainer = Helper.Containers["20GP"];

			var cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage1).TryLoadOrCreate();
			cartage1.LocalClientAddressPK = NewClient.MainAddress.PK;
			var deliverDocAddress1 = cartage1.DocAddresses.AddNew(deliverToOrg.MainAddress);
			var leg1 = Factory.New<CommonCartageLeg>();
			var move1 = Factory.New<CommonBookedCtgMove>();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;
			leg1.JU_E2DeliveryAddressID = deliverDocAddress1.PK;
			move1.EW_JJ = cartage1.PK;
			move1.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#1").PK;
			move1.EW_E2PickupAddressID = cartage1.FirstDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage1.SecondDocAddress.PK;

			var cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage2).TryLoadOrCreate();
			cartage2.LocalClientAddressPK = NewClient.MainAddress.PK;
			var deliverDocAddress2 = cartage2.DocAddresses.AddNew(deliverToOrg.MainAddress);
			var leg21 = Factory.New<CommonCartageLeg>();
			var move21 = Factory.New<CommonBookedCtgMove>();
			leg21.JU_RunSheetSequence = 2;
			leg21.JU_EW = move21.PK;
			leg21.JU_E2DeliveryAddressID = deliverDocAddress2.PK;
			move21.EW_JJ = cartage2.PK;
			move21.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#2").PK;
			move21.EW_E2PickupAddressID = cartage2.FirstDocAddress.PK;
			move21.EW_E2DeliveryAddressID = cartage2.SecondDocAddress.PK;
			var leg22 = Factory.New<CommonCartageLeg>();
			var move22 = Factory.New<CommonBookedCtgMove>();
			leg22.JU_RunSheetSequence = 2;
			leg22.JU_EW = move22.PK;
			leg22.JU_E2DeliveryAddressID = deliverDocAddress2.PK;
			move22.EW_JJ = cartage2.PK;
			move22.EW_JC_Container = CreateCommonContainer(refContainer, containerNumber: "#3").PK;
			move22.EW_E2PickupAddressID = cartage2.FirstDocAddress.PK;
			move22.EW_E2DeliveryAddressID = cartage2.SecondDocAddress.PK;

			var transportCo = Helper.CreateCreditor();
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg21, leg22 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("TCC");
			chargeCode.AC_IsGroupageCharge = isConsolLevelCharge;

			var costing = Helper.NewCosting(transportCo);
			var costLine = CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.BusinessQuantityUnit.Container, 80);
			costLine.Parent.TI_OA_CartageDeliveryAddressOverride = deliverToOrg.MainAddress.PK;
			Factory.Save();

			#endregion

			#region Part 1 - Cost charge and its apportioned charges

			if (isConsolLevelCharge)
			{
				AutoCostAndAssert("Rate line on WorkSheet should be filtered because of delivery address and rate line on CartageLeg should be filtered because of consol charge code level", null, null, workSheet, false);
				AssertAutoratingAuditLogNoteContainsLines(workSheet,
					"Log should contain expected lines",
					"Delivery/Consignee Address didn't match job",
					"TCC charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000",
					"TCC charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001001");

				return;
			}

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "TCC",
					E6_LocalCostAmount = 240,
					CostCalculationDescription = @"
#1 (20GP) - 1 20GP Container(s) @ AUD 80.00/Container
#2 (20GP) - 1 20GP Container(s) @ AUD 80.00/Container
#3 (20GP) - 1 20GP Container(s) @ AUD 80.00/Container"
				}
			};
			AutoCostAndAssert("Should create cost charge for RunSheet", null, expectedCosts, workSheet, autorateRevenue: false);

			Factory.Save();

			var expectedCharges1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 120,
					JR_LocalSellAmt = 120,
					RevenueCalculationDescription = ""
				}
			};
			var job1 = cartage1.Job as Job;
			AssertCharges("Charges are split in half because of 2 cartages", expectedCharges1, job1);

			var charge1 = (Charge)job1.Charges.Single();
			Assert("The charge should be apportioned from cost charge and carry CostCalculationDescription over", !charge1.CostCalculationDescription.IsEmpty);
			AssertEquals("The JobChargeAttrib_ContainerNumber is incorrect", "#1", charge1.JobChargeAttrib_ContainerNumber);

			var expectedCharges2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 120,
					JR_LocalSellAmt = 120,
					RevenueCalculationDescription = ""
				},
			};
			var job2 = cartage2.Job as Job;
			AssertCharges("Charges are split in half because of 2 cartages", expectedCharges2, job2);

			var charge2 = (Charge)job2.Charges.Single();
			Assert("The charge should be apportioned from cost charge and carry CostCalculationDescription over", !charge2.CostCalculationDescription.IsEmpty);
			AssertEquals("Leg21 is currently has advantage over Leg22 because of their orders", "#2", charge2.JobChargeAttrib_ContainerNumber);

			#endregion

			#region Part 2 - New revenue charges merge to existing apportioned charges

			var clientRate = Helper.NewClientRate(NewClient);
			CreateRateWithUnitCalculator(clientRate, chargeCode, refContainer, Constants.BusinessQuantityUnit.Container, 150);

			Factory.Save();

			expectedCharges1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 120,
					JR_LocalSellAmt = 150,
					RevenueCalculationDescription = "TCC: #1 (20GP) - 1 20GP Container(s) @ AUD 150.00/Container"
				}
			};
			AutorateAndAssert("Exiting charge should be updated with new RevenueCalculationDescription", expectedCharges1, cartage1, NewClient, job: job1, autorateCosts: false);
			AssertEquals("#1", ((Charge)job1.Charges.Single()).JobChargeAttrib_ContainerNumber.ToString());

			expectedCharges2 = new[]
			{
				// updated charge
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 120,
					JR_LocalSellAmt = 150,
					RevenueCalculationDescription = "TCC: #2 (20GP) - 1 20GP Container(s) @ AUD 150.00/Container"
				},
				// new charge
				new AssertionCharge
				{
					ChargeCode = "TCC",
					JR_LocalCostAmt = 150,
					JR_LocalSellAmt = 150,
					CostCalculationDescription = "",
					RevenueCalculationDescription = "TCC: #3 (20GP) - 1 20GP Container(s) @ AUD 150.00/Container"
				}
			};
			AutorateAndAssert(expectedCharges2, cartage2, NewClient, job: job2, autorateCosts: false);

			// The 2 charges should each have a container number as an identification factor.
			AssertContainsExactElementsInAnyOrder(
				"Each charge should have a unique container number as an identification factor",
				new[] { "#2", "#3" },
				job2.Charges.Select(x => x.JobChargeAttrib_ContainerNumber.ToString())
			);

			#endregion
		}

		public void TestCommonWorkSheet_MultipleCartageLegs_WithDifferentContainerTypes()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer1 = Helper.Containers["20GP"];
			var refContainer2 = Helper.Containers["20RE"];

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();

			var leg1 = Factory.New<CommonCartageLeg>();
			var move1 = Factory.New<CommonBookedCtgMove>();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;
			move1.EW_JJ = cartage.PK;
			move1.EW_JC_Container = CreateCommonContainer(refContainer1, grossWeight: 3000, containerNumber: "#1").PK;

			var leg2 = Factory.New<CommonCartageLeg>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;
			move2.EW_JJ = cartage.PK;
			move2.EW_JC_Container = CreateCommonContainer(refContainer1, grossWeight: 4000, containerNumber: "#2").PK;

			var leg3 = Factory.New<CommonCartageLeg>();
			var move3 = Factory.New<CommonBookedCtgMove>();
			leg3.JU_RunSheetSequence = 3;
			leg3.JU_EW = move3.PK;
			move3.EW_JJ = cartage.PK;
			move3.EW_JC_Container = CreateCommonContainer(refContainer2, grossWeight: 5000, containerNumber: "#3").PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg1, leg2, leg3 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode1 = CreateChargeCode("CC01");
			var chargeCode2 = CreateChargeCode("CC02");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode1, refContainer1, Constants.Weight.Kilograms, 1.5m);
			CreateRateWithUnitCalculator(costing, chargeCode2, refContainer2, Constants.Weight.Kilograms, 2m);

			Factory.Save();

			AutoCostAndAssert("Rate lines on WorkSheet should be filtered because of measure type and rate lines on CartageLegs should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job",
				"Information: RateLine Filtered CC02-UNT-KG-20RE-Costing TC01	reason:	no Weight measure on the job",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.",
				"Information: RateLine Filtered CC02-UNT-KG-20RE-Costing TC01	reason:	CC02 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode1.AC_IsGroupageCharge = false;
			chargeCode2.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost {
					ChargeCode = "CC01",
					// leg1 3000kg + leg2 4000kg = 7000kg
					E6_OSCostAmount = 7000m * 1.5m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription =
@"3000 Kilogram(s) @ AUD 1.50/KG
4000 Kilogram(s) @ AUD 1.50/KG"
				},
				new AssertionCost
				{
					ChargeCode = "CC02",
					E6_OSCostAmount = 10000m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription = "5000 Kilogram(s) @ AUD 2.00/KG"
				}
			};
			AutoCostAndAssert("Runsheet should be autorated with sums of container weights", null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_MultipleCartageLegs_FromDifferentCartages()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var refContainer = Helper.Containers["20GP"];

			var cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage1).TryLoadOrCreate();

			var leg11 = Factory.New<CommonCartageLeg>();
			var move11 = Factory.New<CommonBookedCtgMove>();
			leg11.JU_RunSheetSequence = 1;
			leg11.JU_EW = move11.PK;
			move11.EW_JJ = cartage1.PK;
			move11.EW_JC_Container = CreateCommonContainer(refContainer, 3000, "#1").PK;

			var leg12 = Factory.New<CommonCartageLeg>();
			var move12 = Factory.New<CommonBookedCtgMove>();
			leg12.JU_RunSheetSequence = 2;
			leg12.JU_EW = move12.PK;
			move12.EW_JJ = cartage1.PK;
			move12.EW_JC_Container = CreateCommonContainer(refContainer, 4000, "#2").PK;

			var cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			new JobHeader.Loader(cartage2).TryLoadOrCreate();

			var leg21 = Factory.New<CommonCartageLeg>();
			var move21 = Factory.New<CommonBookedCtgMove>();
			leg21.JU_RunSheetSequence = 1;
			leg21.JU_EW = move21.PK;
			move21.EW_JJ = cartage2.PK;
			move21.EW_JC_Container = CreateCommonContainer(refContainer, 5000, "#3").PK;

			var leg22 = Factory.New<CommonCartageLeg>();
			var move22 = Factory.New<CommonBookedCtgMove>();
			leg22.JU_RunSheetSequence = 2;
			leg22.JU_EW = move22.PK;
			move22.EW_JJ = cartage2.PK;
			move22.EW_JC_Container = CreateCommonContainer(refContainer, 6000, "#4").PK;

			var transportCo = Helper.CreateCreditor("TC01");

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { leg11, leg12, leg21, leg22 });
			workSheet.EY_OH_TransportCo = transportCo.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var chargeCode = CreateChargeCode("CC01");
			var costing = Helper.NewCosting(transportCo);
			CreateRateWithUnitCalculator(costing, chargeCode, refContainer, Constants.Weight.Kilograms, 2m);

			Factory.Save();

			AutoCostAndAssert("Rate line on WorkSheet should be filtered because of measure type and rate lines on CartageLegs should be filtered because of consol charge code level", null, null, workSheet, false);
			AssertAutoratingAuditLogNoteContainsLines(workSheet,
				"Log should contain expected lines",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	no Weight measure on the job",
				"Information: RateLine Filtered CC01-UNT-KG-20GP-Costing TC01	reason:	CC01 charge code is flagged as Consol Level. Consol Level Costs don't apply to T00001000.");

			chargeCode.AC_IsGroupageCharge = false;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "CC01",
					// leg11 3000kg + leg12 4000kg + leg21 5000kg + leg22 6000kg = 18000kg
					E6_OSCostAmount = 18000m * 2m,
					E6_ApportionmentMethod = AllocationMethod.Shipment,
					CostCalculationDescription =
@"Leg for #1  20GP  T00001000/A
3000 Kilogram(s) @ AUD 2.00/KG
Leg for #2  20GP  T00001000/B
4000 Kilogram(s) @ AUD 2.00/KG
Leg for #3  20GP  T00001001/A
5000 Kilogram(s) @ AUD 2.00/KG
Leg for #4  20GP  T00001001/B
6000 Kilogram(s) @ AUD 2.00/KG"
				}
			};
			AutoCostAndAssert("Runsheet should be autorated with sum of container weights", null, expectedCosts, workSheet, false);
		}

		#endregion

		#region Services

		public void TestCommonWorkSheet_ContainerServices_MultipleCartageLegs_AutorateServiceCost()
		{
			var taxRatePk = Helper.ChargeCodes.CreateTaxRate(10).PK;

			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM20", "Port Transport Fumigation 20GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, Constants.FreightServiceType.Codes.Fumigation);
			chargeCode1.AC_AT_GSTRate = taxRatePk;
			var chargeCode2 = Helper.ChargeCodes.New("PTFUM40", "Port Transport Fumigation 40GP", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, Constants.FreightServiceType.Codes.Fumigation);
			chargeCode2.AC_AT_GSTRate = taxRatePk;

			var costing = Helper.NewCosting(TransportProvider1);
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.ALL, "AU", container: "20GP");
			var line1 = entry1.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, Constants.CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.ALL, "AU", container: "40GP");
			var line2 = entry2.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, Constants.CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_RC = GP20.PK;

			var service1 = container1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_JC_Container = container1.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			// move2 setup
			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_RC = GP40.PK;

			var service2 = container2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCount = 2;

			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_JC_Container = container2.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { cartageLeg11, cartageLeg12, cartageLeg21, cartageLeg22, cartageLeg23 });
			workSheet.EY_OH_TransportCo = TransportProvider1.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "PTFUM20",
					E6_OSCostAmount = 100 // move1 x 1 SV x $100/SV
				},
				new AssertionCost
				{
					ChargeCode = "PTFUM40",
					E6_OSCostAmount = 220 // move2 x 2 SV x $110/SV
				}
			};

			var message = "Since worksheets are collections of legs, each leg should use services from its move.";
			AutoCostAndAssert(message, null, expectedCosts, workSheet, false);
		}

		public void TestCommonWorkSheet_LooseServices_MultipleCartageLegs_AutorateServiceCost()
		{
			var taxRatePk = Helper.ChargeCodes.CreateTaxRate(10).PK;

			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode1 = Helper.ChargeCodes.New("PTFUM", "Port Transport Fumigation", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, Constants.FreightServiceType.Codes.Fumigation);
			chargeCode1.AC_AT_GSTRate = taxRatePk;
			var chargeCode2 = Helper.ChargeCodes.New("PTCLN", "Port Transport Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, Constants.FreightServiceType.Codes.Cleaning);
			chargeCode2.AC_AT_GSTRate = taxRatePk;

			var costing = Helper.NewCosting(TransportProvider1);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.ALL, "AU");
			var line1 = entry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.SV, Constants.CurrencyCodes.Australia);
			line1.GetCalculator<UnitCalculator>().PerUnit = 100;
			var line2 = entry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.SV, Constants.CurrencyCodes.Australia);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110;

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCFS;
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			// move1 setup
			var cartageMove1 = Factory.New<CommonBookedCtgMove>();
			cartageMove1.EW_JJ = cartage.PK;
			cartageMove1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service1 = cartageMove1.Services.AddNew();
			service1.ES_Booked = ZDateTime.Today;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service1.ES_ServiceCount = 1;

			var cartageLeg11 = cartageMove1.CartageLegs.AddNew();
			cartageLeg11.JU_RunSheetSequence = 1;
			cartageLeg11.JU_EW = cartageMove1.PK;

			var cartageLeg12 = cartageMove1.CartageLegs.AddNew();
			cartageLeg12.JU_RunSheetSequence = 2;
			cartageLeg12.JU_EW = cartageMove1.PK;

			// move2 setup
			var cartageMove2 = Factory.New<CommonBookedCtgMove>();
			cartageMove2.EW_JJ = cartage.PK;
			cartageMove2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			cartageMove2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var service2 = cartageMove2.Services.AddNew();
			service2.ES_Booked = ZDateTime.Today;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service2.ES_ServiceCount = 2;

			var cartageLeg21 = cartageMove2.CartageLegs.AddNew();
			cartageLeg21.JU_RunSheetSequence = 1;
			cartageLeg21.JU_EW = cartageMove2.PK;

			var cartageLeg22 = cartageMove2.CartageLegs.AddNew();
			cartageLeg22.JU_RunSheetSequence = 2;
			cartageLeg22.JU_EW = cartageMove2.PK;

			var cartageLeg23 = cartageMove2.CartageLegs.AddNew();
			cartageLeg23.JU_RunSheetSequence = 3;
			cartageLeg23.JU_EW = cartageMove2.PK;

			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			workSheet.CartageLegs.AddRange(new[] { cartageLeg11, cartageLeg12, cartageLeg21, cartageLeg22, cartageLeg23 });
			workSheet.EY_OH_TransportCo = TransportProvider1.PK;
			workSheet.EY_RunSheetNumber = "McLaren";

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "PTFUM",
					E6_OSCostAmount = 100 // move1 x 1 FUM SV x $100/SV
				},
				new AssertionCost
				{
					ChargeCode = "PTCLN",
					E6_OSCostAmount = 220 // move2 x 2 CLN SV x $110/SV
				}
			};

			var message = "Since worksheets are collections of legs, each leg should use services from its move.";
			AutoCostAndAssert(message, null, expectedCosts, workSheet, false);
		}

		#endregion

		#region TestAutorateRevenueAndCostSeparately

		public void TestAutorateRevenueAndCostSeparately_ContainerType()
		{
			var revenue20GP = (ZDecimal)1200;
			var revenue40GP = (ZDecimal)1800;
			var cost20GP = (ZDecimal)1000;
			var cost40GP = (ZDecimal)1500;

			var (runSheet20GP, runSheet40GP, cartage, chargeCode) = SetupTestAutorateRevenueAndCostSeparately_ContainerType(revenue20GP, revenue40GP, cost20GP, cost40GP);

			// Ultimate result: different auto cost/revenue orders but always the same charges.
			var expectedFinalCharges = new[]
			{
				new AssertionCharge { JR_LocalSellAmt = revenue20GP, JR_LocalCostAmt = cost20GP },
				new AssertionCharge { JR_LocalSellAmt = revenue40GP, JR_LocalCostAmt = cost40GP },
			};

			var dataForTest = new DataForTestAutorateRevenueAndCostSeparately(
				revenue20GP, revenue40GP, cost20GP, cost40GP,
				runSheet20GP, runSheet40GP, cartage,
				chargeCode, expectedFinalCharges);

			// individual costs then revenue
			TestAutorateRevenueAndCostSeparately_RunSheet1_RunSheet2_TransportBooking(dataForTest);
			TestAutorateRevenueAndCostSeparately_RunSheet2_RunSheet1_TransportBooking(dataForTest);

			// revenue then individual costs
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet1_RunSheet2(dataForTest);
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet2_RunSheet1(dataForTest);
		}

		(CommonWorkSheet, CommonWorkSheet, CommonCartage, ZString) SetupTestAutorateRevenueAndCostSeparately_ContainerType(ZDecimal revenue20GP, ZDecimal revenue40GP, ZDecimal cost20GP, ZDecimal cost40GP)
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);
			var chargeCode = CreateChargeCode("CC01");

			var costing = Helper.NewCosting(TransportProvider1);
			CreateRateWithUnitCalculator(costing, chargeCode, GP20, "CN", cost20GP);
			CreateRateWithUnitCalculator(costing, chargeCode, GP40, "CN", cost40GP);

			var clientRate = Helper.NewClientRate(NewClient);
			CreateRateWithUnitCalculator(clientRate, chargeCode, GP20, "CN", revenue20GP);
			CreateRateWithUnitCalculator(clientRate, chargeCode, GP40, "CN", revenue40GP);

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			var leg20GP = Factory.New<CommonCartageLeg>();
			var move20GP = Factory.New<CommonBookedCtgMove>();
			leg20GP.JU_RunSheetSequence = 1;
			leg20GP.JU_EW = move20GP.PK;
			move20GP.EW_JJ = cartage.PK;
			move20GP.EW_JC_Container = CreateCommonContainer(GP20).PK;
			move20GP.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move20GP.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg40GP = Factory.New<CommonCartageLeg>();
			var move40GP = Factory.New<CommonBookedCtgMove>();
			leg40GP.JU_RunSheetSequence = 1;
			leg40GP.JU_EW = move40GP.PK;
			move40GP.EW_JJ = cartage.PK;
			move40GP.EW_JC_Container = CreateCommonContainer(GP40).PK;
			move40GP.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move40GP.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var runSheet20GP = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet20GP.CartageLegs.AddRange(new[] { leg20GP });
			runSheet20GP.EY_OH_TransportCo = TransportProvider1.PK;
			runSheet20GP.EY_RunSheetNumber = "Number1";

			var runSheet40GP = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet40GP.CartageLegs.AddRange(new[] { leg40GP });
			runSheet40GP.EY_OH_TransportCo = TransportProvider1.PK;
			runSheet40GP.EY_RunSheetNumber = "Number2";

			return (runSheet20GP, runSheet40GP, cartage, chargeCode.AC_Code);
		}

		public void TestAutorateRevenueAndCostSeparately_ContainerNumber_ExportJob()
		{
			var revenue = (ZDecimal)2500;
			var cost = (ZDecimal)1000;

			var (runSheet20GP1, runSheet20GP2, cartage, chargeCode) = SetupTestAutorateRevenueAndCostSeparately_ContainerNumber(revenue, cost, isExportJob: true);

			var expectedFinalCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = revenue,
					JR_LocalCostAmt = cost * 2,
					RevenueCalculationDescription = "CC01: CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 2500.00/Container",
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol Number1
CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container
CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container"
				},
				new AssertionCharge
				{
					JR_LocalSellAmt = revenue,
					JR_LocalCostAmt = cost * 2,
					RevenueCalculationDescription = "CC01: CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 2500.00/Container",
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol Number2.
CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container
CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container"
				},
			};

			var dataForTest = new DataForTestAutorateRevenueAndCostSeparately(
				revenue, revenue, cost * 2, cost * 2,
				runSheet20GP1, runSheet20GP2, cartage,
				chargeCode, expectedFinalCharges);

			// individual costs then revenue
			TestAutorateRevenueAndCostSeparately_RunSheet1_RunSheet2_TransportBooking(dataForTest);
			TestAutorateRevenueAndCostSeparately_RunSheet2_RunSheet1_TransportBooking(dataForTest);

			// revenue then individual costs
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet1_RunSheet2(dataForTest);
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet2_RunSheet1(dataForTest);
		}

		public void TestAutorateRevenueAndCostSeparately_ContainerNumber_ImportJob()
		{
			var revenue = (ZDecimal)2500;
			var cost = (ZDecimal)1000;

			var (runSheet20GP1, runSheet20GP2, cartage, chargeCode) = SetupTestAutorateRevenueAndCostSeparately_ContainerNumber(revenue, cost, isExportJob: false);

			var expectedFinalCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalSellAmt = revenue,
					JR_LocalCostAmt = cost * 2,
					RevenueCalculationDescription = "CC01: CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 2500.00/Container",
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol Number1
CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container
CONTAINER1 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container"
				},
				new AssertionCharge
				{
					JR_LocalSellAmt = revenue,
					JR_LocalCostAmt = cost * 2,
					RevenueCalculationDescription = "CC01: CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 2500.00/Container",
					CostCalculationDescription = @"This cost was autocosted and apportioned from Consol Number2.
CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container
CONTAINER2 (20GP) - 1 20GP Container(s) @ AUD 1000.00/Container"
				},
			};

			var dataForTest = new DataForTestAutorateRevenueAndCostSeparately(
				revenue, revenue, cost * 2, cost * 2,
				runSheet20GP1, runSheet20GP2, cartage,
				chargeCode, expectedFinalCharges);

			// individual costs then revenue
			TestAutorateRevenueAndCostSeparately_RunSheet1_RunSheet2_TransportBooking(dataForTest);
			TestAutorateRevenueAndCostSeparately_RunSheet2_RunSheet1_TransportBooking(dataForTest);

			// revenue then individual costs
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet1_RunSheet2(dataForTest);
			TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet2_RunSheet1(dataForTest);
		}

		(CommonWorkSheet, CommonWorkSheet, CommonCartage, ZString) SetupTestAutorateRevenueAndCostSeparately_ContainerNumber(ZDecimal revenue, ZDecimal cost, bool isExportJob)
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.Shipment);

			var deliverToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliverToOrg.MainAddress.OA_City = "Deliv";
			deliverToOrg.MainAddress.OA_Address1 = "1 Delivery St";

			// This charge code is not a consol level charge and could match both leg and runsheet adapters.
			// However we want the charge to only match leg adapters.
			// It is to test that CartageLegs can have cost charges and are apportioned correctly to Cartage
			// So we set the To address on the rate and leg, which will prevent matching on the runsheet adapter.
			var taxRatePk = Helper.ChargeCodes.CreateTaxRate(10).PK;
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode = Helper.ChargeCodes["CC01"];
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			chargeCode.AC_AT_GSTRate = taxRatePk;

			var costing = Helper.NewCosting(TransportProvider1);
			var costLine = CreateRateWithUnitCalculator(costing, chargeCode, GP20, "CN", cost);
			costLine.Parent.TI_OA_CartageDeliveryAddressOverride = deliverToOrg.MainAddress.PK;

			var clientRate = Helper.NewClientRate(NewClient);
			CreateRateWithUnitCalculator(clientRate, chargeCode, GP20, "CN", revenue);

			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			// CartageJobType.Export: First Leg is picked for revenue.
			// CartageJobType.Import: Last Leg is picked for revenue.
			cartage.JJ_E3_NKJobType = isExportJob ? Constants.CartageJobType.NEW_FCLExportToSHP : Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			var deliverDocAddress = cartage.DocAddresses.AddNew(deliverToOrg.MainAddress);
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;
			PopulateContainer(move1.Container, GP20, containerNumber: "CONTAINER1");

			// set up 2 legs so that we can test cost charge merging from legs
			// for example
			// - Leg1: 1xTRNCC - 1 20GP CN, Number AAA - $80
			// - Leg2: 1xTRNCC - 1 20GP CN, Number AAA - $80
			// - Result for RunSheet: 1xTRNCC -  $160
			// It's unrealistic to charge both the outward and inward leg the same rate
			// and doubly unrealistic for them to have the same delivery address
			// but doing it this way for historical compatibility so the rates can be merged.
			var leg11 = move1.CartageLegs.AddNew();
			leg11.JU_RunSheetSequence = 1;
			leg11.JU_DisplayOrder = 1;
			leg11.JU_E2DeliveryAddressID = deliverDocAddress.PK;
			var leg12 = move1.CartageLegs.AddNew();
			leg12.JU_RunSheetSequence = 2;
			leg12.JU_DisplayOrder = 2;
			leg12.JU_IsEmptyContainer = ZBool.True;
			leg12.JU_E2DeliveryAddressID = deliverDocAddress.PK;

			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;
			PopulateContainer(move2.Container, GP20, containerNumber: "CONTAINER2");

			var leg21 = move2.CartageLegs.AddNew();
			leg21.JU_RunSheetSequence = 1;
			leg21.JU_DisplayOrder = 1;
			leg21.JU_E2DeliveryAddressID = deliverDocAddress.PK;
			var leg22 = move2.CartageLegs.AddNew();
			leg22.JU_RunSheetSequence = 2;
			leg22.JU_DisplayOrder = 2;
			leg22.JU_IsEmptyContainer = ZBool.True;
			leg22.JU_E2DeliveryAddressID = deliverDocAddress.PK;

			var runSheet1 = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet1.CartageLegs.AddRange(new[] { leg11, leg12 });
			runSheet1.EY_OH_TransportCo = TransportProvider1.PK;
			runSheet1.EY_RunSheetNumber = "Number1";

			var runSheet2 = Factory.NewWithValidTestData<CommonWorkSheet>();
			runSheet2.CartageLegs.AddRange(new[] { leg21, leg22 });
			runSheet2.EY_OH_TransportCo = TransportProvider1.PK;
			runSheet2.EY_RunSheetNumber = "Number2";

			return (runSheet1, runSheet2, cartage, chargeCode.AC_Code);
		}

		readonly struct DataForTestAutorateRevenueAndCostSeparately
		{
			public DataForTestAutorateRevenueAndCostSeparately(
				ZDecimal revenue1, ZDecimal revenue2, ZDecimal cost1, ZDecimal cost2,
				CommonWorkSheet runSheet1,
				CommonWorkSheet runSheet2,
				CommonCartage cartage,
				ZString chargeCode,
				IEnumerable<AssertionCharge> expectedCharges)
			{
				Revenue1 = revenue1;
				Revenue2 = revenue2;
				Cost1 = cost1;
				Cost2 = cost2;
				RunSheet1 = runSheet1;
				RunSheet2 = runSheet2;
				Cartage = cartage;
				ChargeCode = chargeCode;
				ExpectedCharges = expectedCharges;
			}

			public ZDecimal Revenue1 { get; }
			public ZDecimal Revenue2 { get; }
			public ZDecimal Cost1 { get; }
			public ZDecimal Cost2 { get; }
			public CommonWorkSheet RunSheet1 { get; }
			public CommonWorkSheet RunSheet2 { get; }
			public CommonCartage Cartage { get; }
			public ZString ChargeCode { get; }
			public IEnumerable<AssertionCharge> ExpectedCharges { get; }
		}

		void TestAutorateRevenueAndCostSeparately_RunSheet1_RunSheet2_TransportBooking(DataForTestAutorateRevenueAndCostSeparately data)
		{
			var expectedChargesFromFirstAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				// cost and revenue are set to the same value when setting cost amount
				{ data.Cartage, new [] { new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost1, JR_LocalSellAmt = data.Cost1 } } }
			};
			var expectedCostsFromFirstAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost1, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			var expectedChargesFromSecondAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					data.Cartage,
					new[]
					{
						// cost and revenue are set to the same value when setting cost amount
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost1, JR_LocalSellAmt = data.Cost1 },
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost2, JR_LocalSellAmt = data.Cost2  }
					}
				}
			};
			var expectedCostsFromSecondAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost2, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			AutorateCostsThenAutorateRevenue(
				data.RunSheet1, data.RunSheet2, data.Cartage,
				expectedChargesFromFirstAutoCost, expectedCostsFromFirstAutoCost,
				expectedChargesFromSecondAutoCost, expectedCostsFromSecondAutoCost,
				data.ExpectedCharges);
		}

		void TestAutorateRevenueAndCostSeparately_RunSheet2_RunSheet1_TransportBooking(DataForTestAutorateRevenueAndCostSeparately data)
		{
			var expectedChargesFromFirstAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ data.Cartage, new [] { new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost2, JR_LocalSellAmt = data.Cost2 } } }
			};
			var expectedCostsFromFirstAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost2, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			var expectedChargesFromSecondAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					data.Cartage,
					new[]
					{
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost1, JR_LocalSellAmt = data.Cost1 },
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost2, JR_LocalSellAmt = data.Cost2  }
					}
				}
			};
			var expectedCostsFromSecondAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost1, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			AutorateCostsThenAutorateRevenue(
				data.RunSheet2, data.RunSheet1, data.Cartage,
				expectedChargesFromFirstAutoCost, expectedCostsFromFirstAutoCost,
				expectedChargesFromSecondAutoCost, expectedCostsFromSecondAutoCost,
				data.ExpectedCharges);
		}

		void TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet1_RunSheet2(DataForTestAutorateRevenueAndCostSeparately data)
		{
			var expectedChargesFromAutorate = new[]
			{
				// cost and revenue are set to same value when setting sell amounts
				new AssertionCharge { JR_LocalSellAmt = data.Revenue1, JR_LocalCostAmt = data.Revenue1 },
				new AssertionCharge { JR_LocalSellAmt = data.Revenue2, JR_LocalCostAmt = data.Revenue2 },
			};

			var expectedChargesFromFirstAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					data.Cartage,
					new[]
					{
						// changed
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost1, JR_LocalSellAmt = data.Revenue1 },
						// unchanged
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Revenue2, JR_LocalSellAmt = data.Revenue2 }
					}
				}
			};
			var expectedCostsFromFirstAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost1, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			var expectedChargesFromSecondAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> { { data.Cartage, data.ExpectedCharges } };
			var expectedCostsFromSecondAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost2, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			AutorateRevenueThenAutorateCosts(
				data.Cartage, data.RunSheet1, data.RunSheet2,
				expectedChargesFromAutorate,
				expectedChargesFromFirstAutoCost, expectedCostsFromFirstAutoCost,
				expectedChargesFromSecondAutoCost, expectedCostsFromSecondAutoCost);
		}

		void TestAutorateRevenueAndCostSeparately_TransportBooking_RunSheet2_RunSheet1(DataForTestAutorateRevenueAndCostSeparately data)
		{
			var expectedChargesFromAutorate = new[]
			{
				new AssertionCharge { JR_LocalSellAmt = data.Revenue1, JR_LocalCostAmt = data.Revenue1 },
				new AssertionCharge { JR_LocalSellAmt = data.Revenue2, JR_LocalCostAmt = data.Revenue2 },
			};

			var expectedChargesFromFirstAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{
					data.Cartage,
					new[]
					{
						// unchanged
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Revenue1, JR_LocalSellAmt = data.Revenue1 },
						// changed
						new AssertionCharge { ChargeCode = data.ChargeCode, JR_LocalCostAmt = data.Cost2, JR_LocalSellAmt = data.Revenue2 }
					}
				}
			};
			var expectedCostsFromFirstAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost2, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			var expectedChargesFromSecondAutoCost = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> { { data.Cartage, data.ExpectedCharges } };
			var expectedCostsFromSecondAutoCost = new[]
			{
				new AssertionCost { ChargeCode = data.ChargeCode, E6_LocalCostAmount = data.Cost1, E6_ApportionmentMethod = AllocationMethod.Shipment },
			};

			AutorateRevenueThenAutorateCosts(
				data.Cartage, data.RunSheet2, data.RunSheet1,
				expectedChargesFromAutorate,
				expectedChargesFromFirstAutoCost, expectedCostsFromFirstAutoCost,
				expectedChargesFromSecondAutoCost, expectedCostsFromSecondAutoCost);
		}

		void AutorateCostsThenAutorateRevenue(
			CommonWorkSheet runSheet1,
			CommonWorkSheet runSheet2,
			CommonCartage cartage,
			Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedChargesAfterFirstAutoCost,
			IEnumerable<AssertionCost> expectedCostsAfterFirstAutoCost,
			Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedChargesAfterSecondAutoCost,
			IEnumerable<AssertionCost> expectedCostsAfterSecondAutoCost,
			IEnumerable<AssertionCharge> expectedFinalCharges)
		{
			DeleteExistingCosts(runSheet1.PK);
			DeleteExistingCosts(runSheet2.PK);
			var job = (Job)cartage.Job;
			job?.Charges.RemoveAndDeleteAll();

			// individual job costs in order
			// auto cost RunSheet (1) => cost charges are created from legs (11, 12) and then merged
			// the charge then is apportioned to cartage as (C1 , leg 11)
			AutoCostAndAssert("After auto costs first run sheet", expectedChargesAfterFirstAutoCost, expectedCostsAfterFirstAutoCost, runSheet1, false);
			// auto cost RunSheet (2) => cost charges are created from legs (21, 22) and then merged
			// the charge then is apportioned to cartage as (C2, leg 21)
			AutoCostAndAssert("After auto costs second run sheet", expectedChargesAfterSecondAutoCost, expectedCostsAfterSecondAutoCost, runSheet2, false);

			AutorateAndAssert("After auto revenue cartage", expectedFinalCharges, cartage, NewClient, autorateCosts: false, job: job);
		}

		void AutorateRevenueThenAutorateCosts(
			CommonCartage cartage,
			CommonWorkSheet runSheet1,
			CommonWorkSheet runSheet2,
			IEnumerable<AssertionCharge> expectedChargesFromAutorate,
			Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedChargesAfterFirstAutoCost,
			IEnumerable<AssertionCost> expectedCostsAfterFirstAutoCost,
			Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedChargesAfterSecondAutoCost,
			IEnumerable<AssertionCost> expectedCostsAfterSecondAutoCost)
		{
			DeleteExistingCosts(runSheet1.PK);
			DeleteExistingCosts(runSheet2.PK);
			var job = (Job)cartage.Job;
			job?.Charges.RemoveAndDeleteAll();

			AutorateAndAssert("After auto revenue cartage", expectedChargesFromAutorate, cartage, NewClient, autorateCosts: false, job: job);

			// individual job costs in order
			AutoCostAndAssert("After auto costs first run sheet", expectedChargesAfterFirstAutoCost, expectedCostsAfterFirstAutoCost, runSheet1, false);
			AutoCostAndAssert("After auto costs second run sheet", expectedChargesAfterSecondAutoCost, expectedCostsAfterSecondAutoCost, runSheet2, false);

			// do not need to verify the final job.Charge since expectedChargesAfterSecondAutoCost should be it.
		}

		#endregion

		#region Implementation

		RateLine CreateRateWithUnitCalculator(RatingHeader costing, AccChargeCode chargeCode, RefContainer refContainer, string unit, decimal perUnit)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.FRO, "AU", "");
			entry.TI_RC = refContainer.PK;

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return line;
		}

		RateLine CreateCostWithUnitCalculator(Costing costing, AccChargeCode chargeCode, string unit, decimal perUnit)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TRN, Constants.RateMode.LRO, "AU", "");
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return line;
		}

		AccChargeCode CreateChargeCode(string code, string chargeSubGroup = "", bool isAdHoc = false)
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var result = Helper.ChargeCodes.NewConsolChargeCode(code, code + " Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport, chargeSubGroup);
			result.AC_IsAdhocServiceCharge = isAdHoc;

			return result;
		}

		CommonContainer CreateCommonContainer(RefContainer refContainer, decimal grossWeight = 2500m, string containerNumber = null)
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			PopulateContainer(container, refContainer, grossWeight, containerNumber);
			return container;
		}

		void PopulateContainer(CommonContainer container, RefContainer refContainer, decimal grossWeight = 2500m, string containerNumber = null)
		{
			container.JC_RC = refContainer.PK;
			container.JC_GrossWeight = grossWeight;
			container.JC_GrossWeightUQ = QuantityUnit.KG;
			container.JC_ContainerNum = containerNumber;
		}

		#endregion
	}
}
