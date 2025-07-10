using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing
{
	public class AutoratingForwardingConsolIntegrationTest : BaseRatingIntegrationTest
	{
		[GuiTest]
		[TestDate(2020, 01, 01)]
		public void TestAutorateDateFilter_CarrierContract()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var org = Helper.NewOrgHeader();
			var ratingContract = Helper.NewRatingContract(org, "C1234", RatingContractTypes.Provider, transportMode: TransportModes.Sea);
			ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			var ratingDateConfig = Factory.New<RatingDateConfig>();
			ratingDateConfig.RDT_ParentTableCode = RatingContractSchema.Constants.Prefix;
			ratingDateConfig.RDT_ParentID = ratingContract.PK;
			ratingDateConfig.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig.RDT_Direction = FreightShipmentDirection.Code.All;
			ratingDateConfig.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.DepartureDate;
			ratingDateConfig.RDT_ContainerMode = ContainerModes.All;

			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var costing = Helper.NewCosting(null);
			var reateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, "", "FRT", 100m);
			reateEntry.TI_ContractNumber = "C1234";
			var bafLine = reateEntry.AddFlatRateLine("BAF", 200m);
			bafLine.TL_RateStartDate = new ZDate(2020, 2, 11);

			Helper.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "C1234";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = origin;
			transport1.JW_RL_NKDiscPort = destination;
			transport1.JW_Vessel = "Vess1";
			transport1.JW_ATD = new ZDateTime(2020, 2, 10);
			transport1.JW_ETA = new ZDateTime(2020, 2, 10);

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: origin, destination: destination);
			consol.Shipments.Add(shipment);

			var expectedCosts = new[]
			{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 100m,
					}
			};

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("GIVEN Consol Carrier Contract Then should use Consol's Departure Date", null, expectedCosts, consol, false);
				AssertAutoratingAuditLogNoteContainsLines(consol, "Should remove BAF charge",
				"Information: RateLine Filtered BAF-FLT-Standard Costs (TACT/General Rates)	reason:	Departure Date (10-Feb-20) is outside of the date range of this rate (11-Feb-20 to 01-Jul-20)");
			}
		}

		[GuiTest]
		[TestDate(2020, 01, 01)]
		public void TestAutorateDateFilter_Organisation_SystemLevel()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "NEW";
			var otherBranch = otherCompany.Branches.AddNew();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedOrgHeader = Factory.Load<OrgHeader>(TransportProvider1.PK);
				loadedOrgHeader.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;
				newFactory.Save();
			}

			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			rateEntry.TI_RateStartDate = new ZDate(2020, 2, 9);
			rateEntry.TI_RateEndDate = new ZDate(2020, 2, 11);

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "USLAX");
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 10);

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				AutoCostAndAssert
				(
					"GIVEN autorating-date-filtering is arrival THEN should use arrival-date",
					expectedInvoicingCharges: new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
					{
						{ shipment, new[] { new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 10m }, } }
					},
					expectedCosts: Array.Empty<AssertionCost>(),
					consol
				);
			}
		}

		[GuiTest]
		[TestDate(2020, 01, 01)]
		public void TestAutorateDateFilter_Organisation()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			rateEntry.TI_RateStartDate = new ZDate(2020, 2, 4);
			rateEntry.TI_RateEndDate = new ZDate(2020, 2, 6);

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "USLAX");
			shipment.JS_E_DEP = new ZDateTime(2020, 2, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 2, 10);

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			var ratingDateConfig = Factory.New<RatingDateConfig>();
			ratingDateConfig.RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			ratingDateConfig.RDT_ParentID = TransportProvider1.PK;
			ratingDateConfig.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig.RDT_Direction = FreightShipmentDirection.Code.All;
			ratingDateConfig.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.DepartureDate;
			ratingDateConfig.RDT_ContainerMode = ContainerModes.All;
			ratingDateConfig.RDT_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				AutoCostAndAssert
				(
					"GIVEN AutoratingDate is departure-date THEN should use departure-date",
					expectedInvoicingCharges: new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
					{
						{ shipment, new[] { new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 10m }, } }
					},
					expectedCosts: Array.Empty<AssertionCost>(),
					consol
				);
			}
		}

		[GuiTest]
		[TestDate(2020, 01, 01)]
		public void TestAutorateDateFilter()
		{
			var costing = Helper.NewCosting(null);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			rateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			rateEntry1.TI_RateEndDate = new ZDate(2020, 2, 16);

			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "CAF", 20m, currency: "USD");
			rateEntry2.TI_RateStartDate = new ZDate(2020, 2, 19);
			rateEntry2.TI_RateEndDate = new ZDate(2020, 2, 21);

			var shipment1 = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "USLAX");
			shipment1.JS_E_DEP = new ZDateTime(2020, 2, 5);
			shipment1.JS_E_ARV = new ZDateTime(2020, 2, 10);

			var consol1 = CreateConsol();
			consol1.Shipments.Add(shipment1);

			AddContainer(consol1, "20GP", "CONT004", packLines: new[] { shipment1.AddPackLine(weight: 10) });
			AddContainer(consol1, "20GP", "CONT005", packLines: new[] { shipment1.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 15));
			AddContainer(consol1, "20GP", "CONT006", packLines: new[] { shipment1.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 20));

			AssertAutorateDateFilter
			(
				consol1,
				shipment1,
				JobInvoicingConsumerTypes.ShipmentCode,
				TransportModes.Sea,
				ChargeCodeGroupList.Codes.Freight,
				JobDateTypes.Codes.FirstContainerGateInDate,
				expectedCharges: new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ shipment1, new[] { new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 10m }, } }
				},
				message: "GIVEN FirstContainerGateInDate THEN should use the first container-gate-in-date"
			);

			AssertAutorateDateFilter
			(
				consol1,
				shipment1,
				JobInvoicingConsumerTypes.ShipmentCode,
				TransportModes.Sea,
				ChargeCodeGroupList.Codes.Freight,
				JobDateTypes.Codes.LastContainerGateInDate,
				expectedCharges: new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ shipment1, new[] { new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m }, } }
				},
				message: "GIVEN LastContainerGateInDate THEN should use the last container-gate-in-date"
			);

			CommonContainer AddContainer(ForwardingConsol consol, string containerType, string containerNumber, IEnumerable<PackLine> packLines = null, ZDateTime fclWharfGateIn = default)
			{
				var container = consol.AddContainer(containerType, packLines: packLines);
				container.JC_ContainerNum = containerNumber;
				container.JC_FCLWharfGateIn = fclWharfGateIn;
				return container;
			}

			void AssertAutorateDateFilter(ForwardingConsol consol, ForwardingShipment shipment, string jobType, string mode, string chargeGroup, string dateType, Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>> expectedCharges, string message = default)
			{
				var configuration = new AutoRateDateByChargeGroupConfiguration { FilterType = RatingDateFilterTypes.Codes.Custom, };
				var autoRateDate = new AutoRateDate { JobType = jobType, Mode = mode, DirectionCode = FreightShipmentDirection.Code.All, DateType = dateType };
				var autoRateDateByChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().Single(item => item.ChargeGroup == chargeGroup);
				autoRateDateByChargeGroup.ChargeGroupSettings.Add(autoRateDate);
				AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);

				using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					AutoCostAndAssert(message, expectedCharges, Array.Empty<AssertionCost>(), consol);
				}
			}
		}

		#region AutoRating ULD Consol with LSE rates to rate lines not packed in any container

		[GuiTest]
		public void TestUldConsol_PackedAndUnpackedLines_UseLseRatesForUnpackedLines()
		{
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DCART"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DSEC"].AC_IsGroupageCharge = true;

			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			var consol1 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol1.Shipments.Add(shipment);

			var consol2 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol2.Shipments.Add(shipment);

			var packLines = new[]
			{
				shipment.AddPackLine(weight: 100),
				shipment.AddPackLine(weight: 200),
				shipment.AddPackLine(weight: 300),
				shipment.AddPackLine(weight: 400),
				shipment.AddPackLine(weight: 500),
				shipment.AddPackLine(weight: 600),
			};

			consol1.AddContainer(containerType: "LD-1", count: 2, packLines: new[] { packLines[0], packLines[1] });
			consol1.AddContainer(containerType: "LD-2", count: 1, packLines: new[] { packLines[4] });
			consol1.AddContainer(containerType: "LD-3", count: 1, packLines: new[] { packLines[5] });

			consol2.AddContainer(containerType: "LD-1", count: 4, packLines: new[] { packLines[1], packLines[2] });
			consol2.AddContainer(containerType: "LD-2", count: 5, packLines: new[] { packLines[3] });

			// Rates Setup
			var ld1Rate = CreateCostRate(category: RatingConstants.RateCategory.AIR, mode: RateMode.ULD, container: "LD-1");
			ld1Rate.AddUnitRateLine("FRT", 10, "CN", "UAH");
			ld1Rate.AddUnitRateLine("BAF", 1, "KG", "UAH");

			var ld2Rate = CreateCostRate(category: RatingConstants.RateCategory.AIR, mode: RateMode.ULD, container: "LD-2");
			ld2Rate.AddUnitRateLine("FRT", 20, "CN", "UAH");
			ld2Rate.AddUnitRateLine("BAF", 2, "KG", "UAH");

			var lseRate = CreateCostRate(category: RatingConstants.RateCategory.AIR, mode: RateMode.LSE, container: null);
			lseRate.AddUnitRateLine("FRT", 30, "CN", "UAH");
			lseRate.AddUnitRateLine("BAF", 3, "KG", "UAH");

			var destinationRate = CreateCostRate(category: RatingConstants.RateCategory.DST, mode: RateMode.AIR, container: null);
			destinationRate.AddUnitRateLine("DDOC", 40, "CN", "UAH");
			destinationRate.AddUnitRateLine("DCART", 4, "KG", "UAH");
			destinationRate.AddFlatRateLine("DSEC", 100, "UAH");

			Factory.Save();

			// Consol Setup
			//
			//	Consol 1:
			//		Allocated:
			//			2 x LD-1: PackLine[0] (100 KG), PackLine[1] (200 KG)
			//			1 x LD-2: PackLine[4] (500 KG)
			//			1 x LD-3: PackLine[5] (600 KG)
			//		Unallocated:
			//			4 x PackLine[2] (300 KG)
			//			5 x PackLine[3] (400 KG)
			//	Consol 2:
			//		Allocated:
			//			4 x LD-1: PackLine[1] (200 KG), PackLine[2] (300 KG)
			//			5 x LD-2: PackLine[3] (400 KG)
			//		Unallocated:
			//			PackLine[0] (100 KG)
			//			PackLine[4] (500 KG)
			//			PackLine[5] (600 KG)
			//
			//	Rates Setup
			//
			//		ULD	LD-1	FRT		$10 per CN
			//		ULD	LD-1	BAF		$1 per KG
			//		ULD	LD-2	FRT		$20 per CN
			//		ULD LD-2	BAF		$2 per KG
			//		LSE			FRT		$30 per CN
			//		LSE			BAF		$3 per KG
			//		AIR			DDOC	$40 per CN
			//		AIR			DCART	$4 per KG
			//		AIR			DSEC	$100

			var expectedCosts = new[]
			{
				// LD-1
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 20,
					CostCalculationDescription = "FRT: 2 LD-1 Container(s) @ UAH 10.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 300,
					CostCalculationDescription = "BAF: 300 Kilogram(s) @ UAH 1.00/KG"
				},

				// LD-2
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 20,
					CostCalculationDescription = "FRT: 1 LD-2 Container(s) @ UAH 20.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 1000,
					CostCalculationDescription = "BAF: 500 Kilogram(s) @ UAH 2.00/KG"
				},

				// LSE (300 KG + 400 KG unallocated lines)
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 2100,
					CostCalculationDescription = "BAF: 700 Kilogram(s) @ UAH 3.00/KG"
				},

				// Destination
				new AssertionCost
				{
					ChargeCode = "DDOC",
					E6_OSCostAmount = 160,
					CostCalculationDescription = "DDOC: 4 Container(s) @ UAH 40.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "DCART",
					E6_OSCostAmount = 5600,
					CostCalculationDescription = "DCART: 1400 Kilogram(s) @ UAH 4.00/KG"	// Allocated
				},
				new AssertionCost
				{
					ChargeCode = "DCART",
					E6_OSCostAmount = 2800,
					CostCalculationDescription = "DCART: 700 Kilogram(s) @ UAH 4.00/KG"		// Unallocated
				},
				new AssertionCost
				{
					ChargeCode = "DSEC",
					E6_OSCostAmount = 100,
					CostCalculationDescription = "DSEC: Base Rate UAH 100.00"
				},
			};

			AutoCostAndAssert("AutoRate with ULD and LSE rates", null, expectedCosts, consol1, false);
		}

		#endregion

		[GuiTest]
		public void TestAutorateConsolCosting_ShouldApplyCommodityCorrect()
		{
			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			var consol = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol.Shipments.Add(shipment);

			var packLines = new[]
			{
				shipment.AddPackLine(weight: 100),
				shipment.AddPackLine(weight: 200),
			};

			var container_LD_1 = consol.AddContainer(containerType: "LD-1", commodity: "HAZ", count: 1, packLines: new[] { packLines[0] });
			var container_LD_2 = consol.AddContainer(containerType: "LD-2", commodity: "SALT", count: 1, packLines: new[] { packLines[1] });

			// Consol Setup
			//
			//	Consol 1:
			//		Allocated:
			//			1 x LD-1: PackLine[0] (100 KG) 
			//			1 x LD-2: PackLine[1] (200 KG)
			//
			//	Rates Setup
			//
			//		ULD	LD-1	FRT		$10 per CN		HAZ
			//		ULD	LD-1	BAF		$1 per KG		HAZ
			//		ULD	LD-1	FRT		$20 per CN		GEN
			//		ULD LD-1	BAF		$2 per KG		GEN
			//		ULD	LD-1	FRT		$30 per CN		SALT
			//		ULD LD-1	BAF		$3 per KG		SALT
			//		ULD	LD-2	FRT		$40 per CN		HAZ
			//		ULD	LD-2	BAF		$4 per KG		HAZ
			//		ULD	LD-2	FRT		$50 per CN		GEN
			//		ULD LD-2	BAF		$5 per KG		GEN
			//		ULD	LD-2	FRT		$60 per CN		SALT
			//		ULD LD-2	BAF		$6 per KG		SALT

			// LD-1 rates
			CreateAndAddUnitRateLines("LD-1", "HAZ", 10, 1);
			CreateAndAddUnitRateLines("LD-1", "GEN", 20, 2);
			CreateAndAddUnitRateLines("LD-1", "SALT", 30, 3);

			// LD-2 rates
			CreateAndAddUnitRateLines("LD-2", "HAZ", 40, 4);
			CreateAndAddUnitRateLines("LD-2", "GEN", 50, 5);
			CreateAndAddUnitRateLines("LD-2", "SALT", 60, 6);

			Factory.Save();

			// Senario 1: Consol > Pre-allocation > Commodity has value
			consol.JK_RH_NKConsolCommodity = "HAZ";
			container_LD_1.JC_RH_NKRatingCommodityCode = "SALT";
			container_LD_1.JC_RH_NKContainerCommodityCode = "GEN";
			container_LD_2.JC_RH_NKRatingCommodityCode = "SALT";
			container_LD_2.JC_RH_NKContainerCommodityCode = "GEN";

			var expectedCosts = new[]
			{
				// LD-1
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 10,
					CostCalculationDescription = "FRT: 1 LD-1 Container(s) @ UAH 10.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 100,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ UAH 1.00/KG"
				},

				// LD-2
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 40,
					CostCalculationDescription = "FRT: 1 LD-2 Container(s) @ UAH 40.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 800,
					CostCalculationDescription = "BAF: 200 Kilogram(s) @ UAH 4.00/KG"
				}
			};

			AutoCostAndAssert("AutoRate with Consol > Pre-allocation > Commodity", null, expectedCosts, consol, false);

			// Scenario 2: Container > Freight Rates > Commodity has value
			consol.JK_RH_NKConsolCommodity = "";
			container_LD_1.JC_RH_NKRatingCommodityCode = "SALT";
			container_LD_1.JC_RH_NKContainerCommodityCode = "GEN";
			container_LD_2.JC_RH_NKRatingCommodityCode = "SALT";
			container_LD_2.JC_RH_NKContainerCommodityCode = "GEN";

			expectedCosts = new[]
			{
				// LD-1
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 30,
					CostCalculationDescription = "FRT: 1 LD-1 Container(s) @ UAH 30.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 300,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ UAH 3.00/KG"
				},

				// LD-2
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 60,
					CostCalculationDescription = "FRT: 1 LD-2 Container(s) @ UAH 60.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 1200,
					CostCalculationDescription = "BAF: 200 Kilogram(s) @ UAH 6.00/KG"
				},
			};

			AutoCostAndAssert("AutoRate with Container > Freight Rates > Commodity", null, expectedCosts, consol, false);
			
			// Scenario 3: Container > Commodity has value.
			consol.JK_RH_NKConsolCommodity = "";
			container_LD_1.JC_RH_NKRatingCommodityCode = "";
			container_LD_1.JC_RH_NKContainerCommodityCode = "HAZ";
			container_LD_2.JC_RH_NKRatingCommodityCode = "";
			container_LD_2.JC_RH_NKContainerCommodityCode = "HAZ";

			expectedCosts = new[]
			{
				// LD-1
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 10,
					CostCalculationDescription = "FRT: 1 LD-1 Container(s) @ UAH 10.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 100,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ UAH 1.00/KG"
				},

				// LD-2
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 40,
					CostCalculationDescription = "FRT: 1 LD-2 Container(s) @ UAH 40.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 800,
					CostCalculationDescription = "BAF: 200 Kilogram(s) @ UAH 4.00/KG"
				},
			};

			AutoCostAndAssert("AutoRate with Container > Commodity", null, expectedCosts, consol, false);

			// Scenario 4: Container Commodity Code is empty.
			consol.JK_RH_NKConsolCommodity = "";
			container_LD_1.JC_RH_NKRatingCommodityCode = "";
			container_LD_1.JC_RH_NKContainerCommodityCode = "";
			container_LD_2.JC_RH_NKRatingCommodityCode = "";
			container_LD_2.JC_RH_NKContainerCommodityCode = "";
			packLines[0].JL_RH_NKCommodityCode = "HAZ";
			packLines[1].JL_RH_NKCommodityCode = "GEN";
			expectedCosts = new[]
			{
				// LD-1
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 20,
					CostCalculationDescription = "FRT: 1 LD-1 Container(s) @ UAH 20.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 100,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ UAH 1.00/KG"
				},

				// LD-2
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 50,
					CostCalculationDescription = "FRT: 1 LD-2 Container(s) @ UAH 50.00/Container"
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 1000,
					CostCalculationDescription = "BAF: 200 Kilogram(s) @ UAH 5.00/KG"
				},
			};

			AutoCostAndAssert("When Container Commodity is not specified, similar rates with non-CN unit will be decided by packline commodity", null, expectedCosts, consol, false);
		}

		void CreateAndAddUnitRateLines(string container, string commodity, int frtRate, int bafRate)
		{
			var rate = CreateCostRate(category: RatingConstants.RateCategory.AIR, mode: RateMode.ULD, container: container, commodity: commodity);
			rate.AddUnitRateLine("FRT", frtRate, "CN", "UAH");
			rate.AddUnitRateLine("BAF", bafRate, "KG", "UAH");
		}

		#region ContainerGrossWeightOverridden

		[GuiTest]
		public void TestContainerGrossWeightOverridden()
		{
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DCART"].AC_IsGroupageCharge = true;

			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			var consol = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol.Shipments.Add(shipment);

			var container1 = consol.AddContainer(containerType: "LD-1", count: 2, packLines: new[]
			{
				shipment.AddPackLine(weight: 100, count: 5),
				shipment.AddPackLine(weight: 300, count: 20),
			});
			consol.AddContainer(containerType: "LD-2", count: 2, packLines: new[]
			{
				shipment.AddPackLine(weight: 50, count: 2),
				shipment.AddPackLine(weight: 150, count: 4),
			});

			container1.JC_IsGrossWeightOverridden = true;
			container1.JC_GrossWeightUQ = Weight.Kilograms;
			container1.JC_GrossWeight = 200;

			var rate = CreateCostRate(category: RatingConstants.RateCategory.DST, mode: RateMode.AIR, container: null);
			rate.AddUnitRateLine("DDOC", 100, "PK", "UAH");
			rate.AddUnitRateLine("DCART", 3, "KG", "UAH");

			Factory.Save();

			var expectedCosts = new[]
			{
				// 5 PK LD-1 + 20 PK LD-1 + 2 PK LD-1 + 4PK LD-1
				new AssertionCost
				{
					ChargeCode = "DDOC",
					E6_OSCostAmount = 3100,
					CostCalculationDescription = "DDOC: 31 Package(s) @ UAH 100.00/Package"
				},
				// 200KG LD-1 overriden + 50KG LD-2 + 150KG LD-2
				new AssertionCost
				{
					ChargeCode = "DCART",
					E6_OSCostAmount = 1200,
					CostCalculationDescription = "DCART: 400 Kilogram(s) @ UAH 3.00/KG"
				}
			};

			AutoCostAndAssert("AutoRate consol with a container with overriden weight", null, expectedCosts, consol, false);
		}

		#endregion

		#region Matching Locations

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_AllCorrectMatchingLocations_WhenMultiRouteAutoratingDisabled()
			=> AssertAutoratingWithMatchingLocations_AllCorrectMatchingLocations(false);

		public void AssertAutoratingWithMatchingLocations_AllCorrectMatchingLocations(bool multiRouteAutoratingEnabled)
		{
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, multiRouteAutoratingEnabled))
			{
				AssertAutoratingWithMatchingLocations
				(
					"Entry1 must takes priority because of all correct matching locations",
					locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
					{
						{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
						{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
						{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
						{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
					},
					locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
					expectedEntry1: true
				);
			}
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongFirstLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong First Load Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "SGSIN" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: First Load didn't match job AUBNE,AU,AUEC."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongLastDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong Last Discharge Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "SGSIN" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: Last Discharge didn't match job DEBER,DE."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongFirstRouteSetLoadPort()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong First Route Set Load Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "SGSIN" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: First Route Set Load Port didn't match job AUSYD,AU,AUEC."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongLastRouteSetDischargePort()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong Last Route Set Discharge Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "SGSIN" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: Last Route Set Discharge Port didn't match job DEHAM,DE."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_LastDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_LastDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_FirstRouteSetLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_FirstRouteSetLoad",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_LastDischarge_TakesPriorityOver_FirstRouteSetLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"LastDischarge_TakesPriorityOver_FirstRouteSetLoad",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_LastDischargeLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_LastDischarge_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"LastDischarge_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_LastDischargeLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstRouteSetLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstRouteSetLoad_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstRouteSetLoadPortLRC comparer"
			);
		}

		void AssertAutoratingWithMatchingLocations
		(
			string testFailMessage,
			Dictionary<SchemaStringColumn, ZString> locationFieldsForEntry1,
			Dictionary<SchemaStringColumn, ZString> locationFieldsForEntry2,
			bool expectedEntry1,
			string expectedLogMessage = null
		)
		{
			var origin = "AUBNE";
			var destination = "DEBER";

			var costing = Helper.NewCosting(null);

			var entry1 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", origin, destination, "BAF", 10m, "USD", "20GP");
			foreach (var locationField in locationFieldsForEntry1)
			{
				entry1[locationField.Key] = locationField.Value;
			}

			var entry2 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", origin, destination, "BAF", 20m, "USD", "20GP");
			foreach (var locationField in locationFieldsForEntry2)
			{
				entry2[locationField.Key] = locationField.Value;
			}

			var consol = CreateConsol("SEA", "FCL", origin, destination);
			consol.AddContainer("20GP");
			SetTransport("ROA", "PRE", "AUBNE", "AUSYD", consol.Transports[0]);
			SetTransport("SEA", "OTH", "AUSYD", "HKHKG", consol.Transports.AddNew());
			SetTransport("RAI", "OTH", "HKHKG", "SGSIN", consol.Transports.AddNew());
			SetTransport("SEA", "ONF", "SGSIN", "DEHAM", consol.Transports.AddNew());
			SetTransport("ROA", "ONF", "DEHAM", "DEBER", consol.Transports.AddNew());

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: origin, destination: destination);
			consol.Shipments.Add(shipment);

			var expectedCosts = new[] { new AssertionCost { ChargeCode = "BAF", E6_OSCostAmount = expectedEntry1 ? 10m : 20M } };
			AutoCostAndAssert(testFailMessage, null, expectedCosts, consol, autorateRevenue: false, autorateCosts: true);

			if (!string.IsNullOrEmpty(expectedLogMessage))
			{
				AssertAutoratingAuditLogNoteContainsLines(consol, testFailMessage, expectedLogMessage);
			}

			void SetTransport(string mode, string type, string loadPort, string dischargePort, Transport transport)
			{
				transport.JW_IsLinked = false;
				transport.JW_TransportMode = mode;
				transport.JW_TransportType = type;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}
		}

		#endregion

		#region Multi-routes

		ForwardingConsol CreateForwardingConsolWithGatewayAgents(string origin, string destination, OrgHeader receivingAgent)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

			consol.JK_AgentType = AgentType.Agent;
			return consol;
		}

		void SetUpAppointedGatewayAgentPorts(OrgAddress gatewayAgentAddress, ZString location, string handlingType)
		{
			var gatewayAgent = gatewayAgentAddress.Header;
			var agentPorts = gatewayAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = gatewayAgentAddress.PK;
			agentPorts.O5_PortOrCountry = location;
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_SeaAgentStatus = handlingType;
			agentPorts.O5_AirAgentStatus = handlingType;
			agentPorts.O5_RailAgentStatus = handlingType;
			agentPorts.O5_RoadAgentStatus = handlingType;
		}

		public void TestMultiRouteMode_SameOriginChargesFoundForRoutesAndGatewayConsol()
		{
			var origin = "AUSYD";
			var destination = "SGSIN";
			var intermediatePort = "HKHKG";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewCosting(receivingAgent).AddRateEntryWithFlatRateLine("AIR", "LSE", origin, destination, "BAF", 15);

			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;

			var consolDepartureCFS = Helper.NewOrgHeader("CFS");
			var routeDepartureCTO1 = Helper.NewOrgHeader("DEP1");
			var routeDepartureCTO2 = Helper.NewOrgHeader("DEP2");

			Helper.NewCosting(consolDepartureCFS)
				.AddRateEntry(category: "ORG", mode: "ALL", origin, destination)					// Should match consol, but still filtered out if AllowChargesWithSameChargeCodeForDifferentProvider is false
					.AddFlatCharge("ODOC", 1)														// as consol CFS has lower priority than route CTO
				.AddRateEntry(category: "ORG", mode: "ALL", origin, destination: intermediatePort)	// Should not match as destination doesn't match consol destination
					.AddFlatCharge("ODOC", 2);

			Helper.NewCosting(routeDepartureCTO1)
				.AddRateEntry(category: "ORG", mode: "ALL", origin, destination)					// Should not match as destination doesn't match route 1 destination
					.AddFlatCharge("ODOC", 3)
				.AddRateEntry(category: "ORG", mode: "ALL", origin, destination: intermediatePort)	// Should match route 1
					.AddFlatCharge("ODOC", 4);

			Helper.NewCosting(routeDepartureCTO2)
				.AddRateEntry(category: "ORG", mode: "ALL", origin, destination)					// Should not match as origin doesn't match route 2 origin
					.AddFlatCharge("ODOC", 5)
				.AddRateEntry(category: "ORG", mode: "ALL", origin: intermediatePort, destination)	// Should match route 2
					.AddFlatCharge("ODOC", 6);

			// The above costings must be in DB.
			Factory.Save();

			//         Consol
			//     Route 1  Route 2
			// AUSYD -> HKHKG -> SGSIN

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, receivingAgent: receivingAgent);
			consol.JK_OA_PackDepotAddress = consolDepartureCFS.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;
			consol.Transports[0].JW_OA_CreditorAddress = receivingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			CreateJob(shipment, shipment.JS_UniqueConsignRef);

			// Routes setup
			consol.AddTransport(origin, intermediatePort, routeDepartureCTO1, null, "Route Set 1");
			consol.AddTransport(intermediatePort, destination, routeDepartureCTO2, null, "Route Set 2");

			// Note the job type must be GCN
			using (AllowChargesWithSameChargeCodeForDifferentProvider(true, JobInvoicingConsumerTypes.GatewayConsolCode, "AIR"))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 15 },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 1 },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 4 },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 6 },
				};

				AutorateAndAssert(expected, consol, null, autorateRevenue: false, autorateCosts: true);
			}

			using (AllowChargesWithSameChargeCodeForDifferentProvider(false, JobInvoicingConsumerTypes.GatewayConsolCode, "AIR"))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 15 },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 4 },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 6 },
				};

				AutorateAndAssert(expected, consol, null, autorateRevenue: false, autorateCosts: true);
			}
		}

		[GuiTest]
		public void TestMultiRouteMode_SameDestinationChargesFoundForRoutesAndConsol()
		{
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var consolArrivalCFS = Helper.NewOrgHeader("CFS");
			var routeArrivalCTO1 = Helper.NewOrgHeader("DEP1");
			var routeArrivalCTO2 = Helper.NewOrgHeader("DEP2");

			//         Consol
			//     Route 1  Route 2
			// SGSIN -> HKHKG -> AUSYD

			var shipment = CreateShipment();
			var consol = CreateConsol(origin: "SGSIN", destination: "AUSYD");
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.Shipments.Add(shipment);
			consol.JK_OA_UnpackDepotAddress = consolArrivalCFS.MainAddress.PK;
			consol.AddTransport("SGSIN", "HKHKG", null, routeArrivalCTO1, "Route Set 1");
			consol.AddTransport("HKHKG", "AUSYD", null, routeArrivalCTO2, "Route Set 2");

			Helper.NewCosting(consolArrivalCFS)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "AUSYD")	// Should match consol, but still filtered out if AllowChargesWithSameChargeCodeForDifferentProvider is false
					.AddFlatCharge("DDOC", 1)														// as consol CFS has lower priority than route CTO
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "HKHKG")	// Should not match as destination doesn't match consol destination
					.AddFlatCharge("DDOC", 2);

			Helper.NewCosting(routeArrivalCTO1)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "AUSYD")	// Should not match as destination doesn't match route 1 destination
					.AddFlatCharge("DDOC", 3)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "HKHKG")	// Should match route 1
					.AddFlatCharge("DDOC", 4);

			Helper.NewCosting(routeArrivalCTO2)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "AUSYD")	// Should not match as origin doesn't match route 2 origin
					.AddFlatCharge("DDOC", 5)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "HKHKG", destination: "AUSYD")	// Should match route 2
					.AddFlatCharge("DDOC", 6);

			Factory.Save();

			using (AllowChargesWithSameChargeCodeForDifferentProvider(true))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Charges from routes and consol come through as we allow multiple providers for the same charge code
				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 1 },
					new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 4 },
					new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 6 }
				};

				AutoCostAndAssert("Charges from consol and route should come through", null, expectedCosts, consol, autorateRevenue: false);
			}

			using (AllowChargesWithSameChargeCodeForDifferentProvider(false))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// In this case the consol DDOC found for CFS is overriden by routes DDOC found for CTOs as consol CFS org has lower
				// creditors priority than route CTOs
				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 4 },
					new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 6 }
				};

				AutoCostAndAssert("Only charges from route should come through as they override the ones on consol", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		[GuiTest]
		public void TestMultiRouteMode_StandardCostingForConsolExists_OnlyOriginAndDestinationChargesFromStandardCostingShouldComeThough()
		{
			Helper.ChargeCodes["DCART"].AC_IsGroupageCharge = true;

			var carrier = Helper.NewOrgHeader("QAN");
			var consolArrivalCFS = Helper.NewOrgHeader("CFS");
			consolArrivalCFS.OH_IsShippingLine = true;
			consolArrivalCFS.OH_IsShippingProvider = true;
			var routeArrivalCTO1 = Helper.NewOrgHeader("DEP1");
			routeArrivalCTO1.OH_IsShippingLine = true;
			routeArrivalCTO1.OH_IsShippingProvider = true;
			routeArrivalCTO1.OH_IsCreditor = true;
			var routeArrivalCTO2 = Helper.NewOrgHeader("DEP2");
			routeArrivalCTO2.OH_IsShippingLine = true;
			routeArrivalCTO2.OH_IsShippingProvider = true;
			routeArrivalCTO2.OH_IsCreditor = true;

			//         Consol
			//     Route 1  Route 2
			// SGSIN -> HKHKG -> AUSYD

			var shipment = CreateShipment();
			var consol = CreateConsol(origin: "SGSIN", destination: "AUSYD");
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.Shipments.Add(shipment);
			consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = consolArrivalCFS.PK; // CFS
			consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK = routeArrivalCTO1.PK; // CTO
			consol.JK_OA_DeparturePackCFSTransportAddress_ZAddress.OrgPK = routeArrivalCTO2.PK; //Port Transport
			consol.JK_OA_UnpackDepotAddress = consolArrivalCFS.MainAddress.PK;

			consol.Transports.DeleteAll(); //There should be no transport route for this scenario for First Load and Last Discharge
			consol.AddTransport("SGSIN", "HKHKG", null, null, "Route Set 1", carrier, routeArrivalCTO1);
			consol.AddTransport("HKHKG", "AUSYD", null, null, "Route Set 2", carrier, routeArrivalCTO2);

			Helper.NewCosting(null)
				.AddRateEntry(category: RatingConstants.RateCategory.FCL, mode: RateMode.SEA, origin: "SGSIN", destination: "HKHKG")
					.AddFlatCharge("FRT", 5)
				.AddRateEntry(category: RatingConstants.RateCategory.FCL, mode: RateMode.SEA, origin: "HKHKG", destination: "AUSYD")
					.AddFlatCharge("FRT", 6)
				.AddRateEntry(category: "DST", mode: "ALL", origin: "SGSIN", destination: "AUSYD")
					.AddFlatCharge("DCART", 7)
				.AddRateEntry(category: RatingConstants.RateCategory.FCL, mode: RateMode.SEA, origin: "SGSIN", destination: "AUSYD")
					.AddFlatCharge("FRT", 9);

			Factory.Save();

			using (AllowChargesWithSameChargeCodeForDifferentProvider(true))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Charges from routes and consol come through as we allow multiple providers for the same charge code
				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "FRT", E6_OSCostAmount = 5 },
					new AssertionCost { ChargeCode = "FRT", E6_OSCostAmount = 6 },
					new AssertionCost { ChargeCode = "DCART", E6_OSCostAmount = 7 }
				};

				AutoCostAndAssert("Charges from consol and route should come through", null, expectedCosts, consol, autorateRevenue: false);
			}
		}

		IDisposable AllowChargesWithSameChargeCodeForDifferentProvider(bool allow, string jobType = JobInvoicingConsumerTypes.ForwardingConsolCode, string transportMode = "SEA")
		{
			var registrySetup = new SameChargeCodeDifferentProviderCollection();
			var rule = registrySetup.AddNew();
			rule.JobType = jobType;
			rule.TransportMode = transportMode;
			rule.Direction = "ALL";
			rule.IsEnabled = allow;

			return DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetup);
		}

		#endregion

		#region AutoRating Match For SCN Mode

		[GuiTest]
		public void TestGivenEntryForSCN_WhenAutoRatingForSCNConsol_ThenEntryForSCNShouldBeMatched()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "FRT", 10m, currency: "USD");
			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.SCN, "AUSYD", "USLAX", "FRT", 20m, currency: "USD");
			var rateEntry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "FRT", 30m, currency: "USD");

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 20m
					},
				};
				AutoCostAndAssert
				(
					"Given entry for SCN, when auto rating for SCN consol, then entry for SCN should be matched",
					expectedInvoicingCharges: null,
					expectedCosts: expectedCosts,
					consol,
					autorateRevenue: false
				);
			}
		}

		[GuiTest]
		public void TestGivenEntryWithoutSCN_WhenAutoRatingForSCNConsol_ThenEntryForAllShouldBeMatchedBecauseOfFallback()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "FRT", 10m, currency: "USD");
			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.BCN, "AUSYD", "USLAX", "FRT", 20m, currency: "USD");
			var rateEntry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "FRT", 30m, currency: "USD");

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 30m
					},
				};
				AutoCostAndAssert
				(
					"Given entry without SCN, when auto rating for SCN consol, then entry for all should be matched because of fallback",
					expectedInvoicingCharges: null,
					expectedCosts: expectedCosts,
					consol,
					autorateRevenue: false
				);
			}
		}

		[GuiTest]
		public void TestGivenMultipleEntriesForSCN_WhenAutoRatingForSCNConsol_ThenAllEntriesShouldBeMatched()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SCN, "AUSYD", "USLAX", "FRT", 10m, currency: "USD");
			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.SCN, "AUSYD", "USLAX", "FRT", 20m, currency: "USD");
			var rateEntry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.SCN, "AUSYD", "USLAX", "FRT", 30m, currency: "USD");

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 10m
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 20m
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 30m
					}
				};
				AutoCostAndAssert
				(
					"Given multiple entries for SCN, when auto rating for SCN consol, then all entries should be matched",
					expectedInvoicingCharges: null,
					expectedCosts: expectedCosts,
					consol,
					autorateRevenue: false
				);
			}
		}

		[GuiTest]
		public void TestGivenMultipleEntriesInFCL_WhenAutoRatingForSCNConsol_ThenOnlySCNEntriesShouldBeMatched()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SCN, "AUSYD", "USLAX", "FRT", 10m, currency: "USD");
			var rateEntry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 20m, currency: "USD");
			var rateEntry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUSYD", "USLAX", "FRT", 30m, currency: "USD");
			var rateEntry4 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.RAI, "AUSYD", "USLAX", "FRT", 40m, currency: "USD");

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol(TransportModes.Sea, ContainerModes.ShippersConsol, origin: "AUSYD", destination: "USLAX");
			consol.Shipments.Add(shipment);

			var container = consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 10) });
			container.JC_ContainerNum = "ABC";

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 10m
					}
				};
				AutoCostAndAssert
				(
					"Given multiple entries in FCL Freight, when auto rating for SCN consol, then only SCN mode entries should be matched",
					expectedInvoicingCharges: null,
					expectedCosts: expectedCosts,
					consol,
					autorateRevenue: false
				);
			}
		}

		#endregion

		#region Helpers

		RateEntry CreateCostRate(
			string category = "FCL",
			string mode = "SEA",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = "20GP",
			string commodity = "GEN")
		{
			var entry = Costing.AddRateEntry(category, mode, origin, destination, "STD", container);
			entry.TI_RH_NKCommodityCode = commodity;
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}

		RatingHeader Costing
		{
			get
			{
				if (costing == null)
				{
					costing = Helper.NewCosting(TransportProvider1);
				}

				return costing;
			}
		}

		RatingHeader costing;

		#endregion
	}
}
