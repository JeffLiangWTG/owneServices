using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class DomesticRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Consignment RunSheet Rating

		#region TestRateRunSheetWithOneConsignment

		public void TestRateRunSheetWithOneConsignment()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_IsCreditor = true;
				transportCo.CompanyData.SetAPTaxApplicable(false);

				var chargeCode = CreateChargeCode("CARTBC", "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
				CreateCostingWithUnitCalculator(transportCo, chargeCode, "AU", PkgUnit.Box, 5m);

				var consignment = CreateConsignment(3, 10m);
				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "CARTBC", E6_OSCostAmount = 15m, }
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 15m } } }
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, runSheet);
			}
		}

		#endregion

		#region TestRateRunSheetWithConsignmentsAndTransportMode

		public void TestRateRunSheetWithConsignmentsAndTransportMode()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_IsCreditor = true;
				transportCo.CompanyData.SetAPTaxApplicable(false);

				var chargeCode = CreateChargeCode("CARTBC", "Cartage Charge 1", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);

				var costing = Helper.NewCosting(transportCo);
				var entryForAll = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
				var lineForAll = entryForAll.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Box);
				lineForAll.GetCalculator<UnitCalculator>().PerUnit = 5m;

				var entryForRAI = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.RAI, "AU", "");
				var lineForRAI = entryForRAI.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Box);
				lineForRAI.GetCalculator<UnitCalculator>().PerUnit = 2m;

				var consignment = CreateConsignment(qty: 3, weight: 10m);
				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);
				runSheet.KG_TransportMode = TransportModes.Rail;
				runSheet.KG_ContainerMode = ContainerModes.FCL;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "CARTBC", E6_OSCostAmount = 6m, }
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 6m } } }
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, runSheet);
			}
		}

		#endregion

		#region TestRateRunSheetWithOneConsignment_OverriddenAddresses

		public void TestRateRunSheetWithOneConsignment_OverriddenAddresses()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_IsCreditor = true;
				transportCo.CompanyData.SetAPTaxApplicable(false);

				var chargeCode = CreateChargeCode("CARTBC", "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
				CreateCostingWithUnitCalculator(transportCo, chargeCode, "NZ", PkgUnit.Box, 5m);

				var consignment = CreateConsignment(3, 10m);
				var pickupAddress = consignment.PickupAddress.Address;
				var deliveryAddress = consignment.DeliveryAddress.Address;
				pickupAddress.E2_AddressOverride = true;
				pickupAddress.E2_RN_NKCountryCode = "NZ";
				deliveryAddress.E2_AddressOverride = true;

				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = "CARTBC", E6_OSCostAmount = 15m, }
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 15m } } }
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, runSheet);
			}
		}

		#endregion

		#region TestRateRunSheetWithOneConsignment_AdHocJobService

		public void TestRateRunSheetWithOneConsignment_AdHocJobService()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var clnChargeCode = Helper.ChargeCodes.New("CLNTBCSRV", "Cleaning Service", "", ChargeCodeGroupList.Codes.TransportBooking, FreightServiceType.Codes.Cleaning);
				var taiChargeCode = Helper.ChargeCodes.New("TAITBCSRV", "Tailgate Service", "", ChargeCodeGroupList.Codes.TransportBooking, FreightServiceType.Codes.Tailgate);
				var qinChargeCode = Helper.ChargeCodes.New("QINTBCSRV", "Quarantine A Service", "", ChargeCodeGroupList.Codes.TransportBooking, FreightServiceType.Codes.QuarantineInspection);
				var qupChargeCode = Helper.ChargeCodes.New("QUPTBCSRV", "Quarantine B Service", "", ChargeCodeGroupList.Codes.TransportBooking, FreightServiceType.Codes.QuarantineUnpack);
				clnChargeCode.AC_IsAdhocServiceCharge = true;
				taiChargeCode.AC_IsAdhocServiceCharge = true;
				qinChargeCode.AC_IsAdhocServiceCharge = true;
				qupChargeCode.AC_IsAdhocServiceCharge = true;

				Factory.Save();

				var consignment = CreateConsignment(3, 10m);
				consignment.LTC_Distance = 36.12m;

				var transportCo = TransportProvider1;
				transportCo.OH_IsCreditor = true;
				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);

				Helper.CreateAdHocJobService(consignment, FreightServiceType.Codes.Cleaning, 1m, JobServiceInfo.Constants.Codes.ServiceOccurrence);
				Helper.CreateAdHocJobService(consignment, FreightServiceType.Codes.Tailgate, 1.5m, JobServiceInfo.Constants.Codes.PickUpDistance);
				Helper.CreateAdHocJobService(consignment, FreightServiceType.Codes.QuarantineInspection, 150m, JobServiceInfo.Constants.Codes.FlatRate);
				Helper.CreateAdHocJobService(consignment, FreightServiceType.Codes.QuarantineUnpack, 100m, JobServiceInfo.Constants.Codes.Container);

				var job = new Job.Loader(consignment).TryLoadOrCreateWithoutMutexForTestOnly();
				job.LocalChargesPK = NewClient.PK;
				Factory.Save();

				var expectedAssertionCharges = new[]
				{
					new AssertionCharge { JR_OSSellAmt = 1m, RevenueCalculationDescription = "CLNTBCSRV: 1 Transport Booking Cleaning @ AUD 1.00/Transport Booking Cleaning", JR_Desc = "Cleaning Service {REFCLN1}" },
					new AssertionCharge { JR_OSSellAmt = 54.18m, RevenueCalculationDescription = "TAITBCSRV: 36.12 Kilometer(s) @ AUD 1.50/Kilometer", JR_Desc = "Tailgate Service {REFTAI1.5}" },
					new AssertionCharge { JR_OSSellAmt = 150m, RevenueCalculationDescription = "QINTBCSRV: Base Rate AUD 150.00", JR_Desc = "Quarantine A Service {REFQIN150}" },
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment, expectedAssertionCharges }
				};

				AutoCostAndAssert("", expectedCharges, Array.Empty<AssertionCost>(), runSheet);
			}
		}

		#endregion

		#region TestRateRunSheetWithThreeConsignments

		public void TestRateRunSheetWithThreeConsignments()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.GrossWeight);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsCreditor = true;
			transportCo.CompanyData.SetAPTaxApplicable(false);

			var chargeCode = CreateChargeCode("CARTBC", "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
			CreateCostingWithUnitCalculator(transportCo, chargeCode, "AU", PkgUnit.Box, 5m);

			var consignment1 = CreateConsignment(3, 30m);
			var consignment2 = CreateConsignment(4, 5m);
			var consignment3 = CreateConsignment(5, 25m);
			var runSheet = CreateRunSheet(transportCo, consignment1.PickupAddress.PickupAction, consignment2.PickupAddress.PickupAction, consignment3.PickupAddress.PickupAction);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CARTBC", E6_OSCostAmount = 60m, }
			};

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ consignment1, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 30m } } },
				{ consignment2, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 5m } } },
				{ consignment3, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 25m } } }
			};

			// cost = 60, apportioned by weight (not boxes)

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				AutoCostAndAssert("", expectedCharges, expectedCosts, runSheet);
			}
		}

		#endregion

		#region TestRateRunSheetWithThreeConsignmentsThenRateConsignments

		public void TestRateRunSheetWithThreeConsignmentsThenRateConsignments()
		{
			SetConsolCostDefaultApportionmentMethodFromCode(AllocationMethod.GrossWeight);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsCreditor = true;
			transportCo.CompanyData.SetAPTaxApplicable(false);

			var chargeCode = CreateChargeCode("CARTBC", "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
			CreateCostingWithUnitCalculator(transportCo, chargeCode, "AU", PkgUnit.Box, 5m);

			var consignment1 = CreateConsignment(3, 30m);
			var consignment2 = CreateConsignment(4, 5m);
			var consignment3 = CreateConsignment(5, 25m);
			var runSheet = CreateRunSheet(transportCo, consignment1.PickupAddress.PickupAction, consignment2.PickupAddress.PickupAction, consignment3.PickupAddress.PickupAction);

			Factory.Save();

			// autorate Run Sheet : cost = 60, apportioned by weight (not boxes)
			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "CARTBC", E6_OSCostAmount = 60m, }
			};

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ consignment1, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 30m } } },
				{ consignment2, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 5m } } },
				{ consignment3, new [] { new AssertionCharge() { ChargeCode = "CARTBC", JR_OSCostAmt = 25m } } }
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				AutoCostAndAssert("", expectedCharges, expectedCosts, runSheet);
			}

			// autorate Consignments : should not rate, instead should keep Run Sheet Apportioned Charges

			AutorateAndAssert(expectedCharges[consignment1], consignment1, NewClient);
			AutorateAndAssert(expectedCharges[consignment2], consignment2, NewClient);
			AutorateAndAssert(expectedCharges[consignment3], consignment3, NewClient);
		}

		#endregion

		#region TestRateThreeConsignmentsThenRateRunSheet

		public void TestRateThreeConsignmentsThenRateRunSheet()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var transportCo = Helper.CreateCreditor();
				var code = "CARTBC";
				var chargeCode = CreateChargeCode(code, "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
				CreateCostingWithUnitCalculator(transportCo, chargeCode, "AU", PkgUnit.Box, 5m);

				var consignment1 = CreateConsignment(3, 30m);
				var consignment2 = CreateConsignment(4, 5m);
				var consignment3 = CreateConsignment(5, 25m);
				var runSheet = CreateRunSheet(transportCo, consignment1.PickupAddress.PickupAction, consignment2.PickupAddress.PickupAction, consignment3.PickupAddress.PickupAction);

				var job1 = (Job)new JobHeader.Loader(consignment1).TryLoadOrCreate();
				var job2 = (Job)new JobHeader.Loader(consignment2).TryLoadOrCreate();
				var job3 = (Job)new JobHeader.Loader(consignment3).TryLoadOrCreate();

				Factory.Save();

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment1, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 15m, CostCalculationDescription = "3 Box(s) @ AUD 5.00/Box" } } },
					{ consignment2, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 20m, CostCalculationDescription = "4 Box(s) @ AUD 5.00/Box" } } },
					{ consignment3, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 25m, CostCalculationDescription = "5 Box(s) @ AUD 5.00/Box" } } }
				};

				AutorateAndAssert(expectedCharges[consignment1], consignment1, NewClient, job: job1);
				AutorateAndAssert(expectedCharges[consignment2], consignment2, NewClient, job: job2);
				AutorateAndAssert(expectedCharges[consignment3], consignment3, NewClient, job: job3);

				var expectedCost = new[] { new AssertionCost { ChargeCode = code, E6_OSCostAmount = 60m, CostCalculationDescription = "12 Box(s) @ AUD 5.00/Box" } };
				expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{ consignment1, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 20m, CostCalculationDescription = "12 Box(s) @ AUD 5.00/Box" } } },
					{ consignment2, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 20m, CostCalculationDescription = "12 Box(s) @ AUD 5.00/Box" } } },
					{ consignment3, new [] { new AssertionCharge { ChargeCode = code, JR_OSCostAmt = 20m, CostCalculationDescription = "12 Box(s) @ AUD 5.00/Box" } } }
				};

				AutoCostAndAssert("", expectedCharges, expectedCost, runSheet, deleteExistingCosts: false);
			}
		}

		#endregion

		#endregion

		#region TestBookingsAppliesClientRatesForConsigneeConsignorAndLocalClient

		public void TestBookingsAppliesClientRatesForConsigneeConsignorAndLocalClient()
		{
			var randomOrg = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCode1 = CreateChargeCode("CARTLC", "Local Client Charge", ChargeCodeGroupList.Codes.TransportBooking);
			var chargeCode2 = CreateChargeCode("CARTNA", "Random Charge", ChargeCodeGroupList.Codes.TransportBooking);
			var chargeCode3 = CreateChargeCode("CARTCNR", "Consignor Charge", ChargeCodeGroupList.Codes.TransportBooking);
			var chargeCode4 = CreateChargeCode("CARTCNE", "Consignee Charge", ChargeCodeGroupList.Codes.TransportBooking);

			CreateClientRateWithUnitCalculator(NewClient, chargeCode1, "AU", PkgUnit.Box, 2m);
			CreateClientRateWithUnitCalculator(randomOrg, chargeCode2, "AU", PkgUnit.Box, 3m);
			CreateClientRateWithUnitCalculator(Consignor, chargeCode3, "AU", PkgUnit.Box, 5m);
			CreateClientRateWithUnitCalculator(Consignee, chargeCode4, "AU", PkgUnit.Box, 7m);

			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_JobID = "TM00000001";
			booking.KM_RatingFreightMode = ContainerModes.Loose;
			AssignLocalClient(booking.DocAddresses);

			Factory.Save();

			var package = CreatePackage(booking, 3, PkgUnit.Box, null, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = Consignor.PK;
			CreateInstructionPkgDivots(fromInstruction, package);

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = Consignee.PK;
			CreateInstructionPkgDivots(toInstruction, package);

			var message = "Should include client rates for the Consignee, Consignor and Local Client Orgs but not the rate from the trandom org";
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 6m,
					RevenueCalculationDescription = "CARTLC: 3 Box(s) @ AUD 2.00/Box",
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 15m,
					RevenueCalculationDescription = "CARTCNR: 3 Box(s) @ AUD 5.00/Box",
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 21m,
					RevenueCalculationDescription = "CARTCNE: 3 Box(s) @ AUD 7.00/Box",
				}
			};

			AutorateAndAssert(message, expectedCharges, booking, NewClient);
		}

		#endregion

		public void TestAutoRatingOperationalAction_DebtorNotSetWhenNotValid()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());

			var provider = TransportProvider1;
			var clientNotDebtor = NewClient;
			clientNotDebtor.OH_IsDebtor = false;

			var chargeCode1 = CreateChargeCode("CARTLC", "Local Client Charge", ChargeCodeGroupList.Codes.TransportBooking);

			CreateCostingWithUnitCalculator(provider, chargeCode1, "AU", PkgUnit.Box, 2m);

			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_JobID = "TM00000001";
			booking.KM_RatingFreightMode = ContainerModes.Loose;
			booking.Address.OrganisationPK = provider.PK;
			AssignLocalClient(booking.DocAddresses);

			Factory.Save();

			var package = CreatePackage(booking, 3, PkgUnit.Box, null, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = Consignor.PK;
			CreateInstructionPkgDivots(fromInstruction, package);

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = Consignee.PK;
			CreateInstructionPkgDivots(toInstruction, package);

			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new AutoRatingActionMethodApplicator(Factory, autorateCosts: true, autorateRevenue: false);
			applicator.InitialiseBeforeIndividiualBatchRun();
			applicator.Apply(actualLog, new[] { booking });

			using (var job = new JobHeader.Loader(booking).Load() as Job)
			{
				AssertEquals("autorating succeeded", 1, job.Charges.Count);
				AssertEquals("Debtor not set on charge", ZGuid.Empty, job.Charges[0].JR_OH_SellAccount);
			}
		}

		#region CreateConsignment

		DtbConsignment CreateConsignment(int qty, decimal weight)
		{
			var jobID = ZString.Format("LTC00{0}", jobNumber);
			var consignment = ConsignmentTestHelper.CreateConsignment(jobID);
			ConsignmentTestHelper.CreateConsignmentAddress(consignment, InstructionTypes.Codes.PickUp, Consignor.MainAddress, ConsignmentAddressStatus.Codes.Allocated, ++jobNumber, DocAddressType.LocalCartageExporter);
			ConsignmentTestHelper.CreateConsignmentAddress(consignment, InstructionTypes.Codes.Delivery, Consignee.MainAddress, ConsignmentAddressStatus.Codes.Allocated, ++jobNumber, DocAddressType.LocalCartageImporter);
			ConsignmentTestHelper.CreateConsignmentAction(consignment.PickupAddress, ActionTypes.Codes.PickUp);
			ConsignmentTestHelper.CreateConsignmentAction(consignment.DeliveryAddress, ActionTypes.Codes.Delivery);
			ConsignmentTestHelper.CreatePackage(consignment, PkgUnit.Box, weight, 1m, qty);
			AssignLocalClient(consignment.DocAddresses);

			return consignment;
		}

		int jobNumber;

		#endregion

		#region CreateRunSheet

		DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCo, params DtbConsignmentAction[] actions)
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet.KG_OH_TransportCo = transportCo.PK;

			ConsignmentTestHelper.CreateRunSheetInstruction(runSheet, actions);

			return runSheet;
		}

		#endregion

		#region CreateClientRateWithUnitCalculator

		ClientRate CreateClientRateWithUnitCalculator(OrgHeader client, AccChargeCode chargeCode, string location, string unit, decimal perUnit)
		{
			var clientRate = Helper.NewClientRate(client);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, location, "");
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return clientRate;
		}

		#endregion

		#region CreateCostingWithUnitCalculator

		Costing CreateCostingWithUnitCalculator(OrgHeader transportCo, AccChargeCode chargeCode, string location, string unit, decimal perUnit)
		{
			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, location, "");
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return costing;
		}

		#endregion

		#region CreateChargeCode

		AccChargeCode CreateChargeCode(string code, string desc, string chargeGroup, bool isGroupage = false)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_Code = code;
			result.AC_Desc = desc;
			result.AC_ChargeGroup = chargeGroup;
			result.AC_IsGroupageCharge = isGroupage;
			result.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
			result.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
			result.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
			result.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;
			result.AC_RateCalculator = UnitCalculator.Code;
			result.AC_ChargeType = ChargeType.Margin;
			result.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			return result;
		}

		#endregion

		#region Assign Local Client

		void AssignLocalClient(JobDocAddressDependentCollection addresses)
		{
			NewClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			var billingAddress = addresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty)
				?? addresses.AddNew(DocAddressType.ClientRequestedBillingParty);

			billingAddress.E2_OA_Address = NewClient.MainAddress.PK;
		}

		#endregion

		Guid GetCartageDepartmentPK()
		{
			return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "TOT")).PK.ToGuid();
		}

		#region Helpers

		TransportConsignmentTestHelper ConsignmentTestHelper
		{
			get { return consignmentTestHelper ?? (consignmentTestHelper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper consignmentTestHelper;

		#endregion
	}
}
