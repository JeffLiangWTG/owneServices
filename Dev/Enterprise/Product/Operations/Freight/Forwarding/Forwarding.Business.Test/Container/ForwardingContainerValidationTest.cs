using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerValidationTest : BaseFreightTest
	{
		public void TestValidateJC_RH_NKContainerCommodityCode()
		{
			var code1 = Factory.New<RefCommodityCode>();
			code1.RH_Code = "AAA";
			code1.RH_IsForwarding = true;

			var code2 = Factory.New<RefCommodityCode>();
			code2.RH_Code = "BBB";
			code2.RH_IsForwarding = false;

			var container = Factory.New<ForwardingContainer>();

			container.JC_RH_NKContainerCommodityCode = code1.RH_Code;
			AssertNoWarnings(container.JC_RH_NKContainerCommodityCodeInfo);

			container.JC_RH_NKContainerCommodityCode = code2.RH_Code;
			AssertHasError(container.JC_RH_NKContainerCommodityCodeInfo, "Enter a valid Commodity.");

			container.JC_RH_NKContainerCommodityCode = "ZZZ";
			AssertHasError(container.JC_RH_NKContainerCommodityCodeInfo, "Enter a valid Commodity.");
		}

		public void TestValidateJC_IsEmptyContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_IsEmptyContainer = true;
			AssertNoErrors(container.JC_IsEmptyContainerInfo);

			var pack1 = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pack1);
			pack1.JL_PackageCount = 0;
			AssertNoErrors(container.JC_IsEmptyContainerInfo);

			pack1.JL_PackageCount = 1;
			AssertHasError(container.JC_IsEmptyContainerInfo, "The container cannot be empty as it contains a pack line with non-zero pack count.");

			pack1.JL_PackageCount = 0;
			var pack2 = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pack2);
			pack2.JL_PackageCount = 0;
			container.RunPreSaveValidation();
			AssertHasError(container.JC_IsEmptyContainerInfo, "The container cannot be empty as it contains more than one pack line.");
		}

		public void TestValidateJC_TareWeights()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var defaultWeight = container.JC_Calc_TareWeight;
			container.JC_TareWeight = defaultWeight;

			AssertNoWarnings("JC_TareWeight has no warnings", container.JC_TareWeightInfo);

			container.JC_TareWeight = 123.123;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			var newDefaultWeight = container.JC_Calc_TareWeight;

			AssertNotEquals("Precondition: JC_TareWeight is not default weight", newDefaultWeight, container.JC_TareWeight);
			AssertHasWarning("JC_TareWeight has warnings", container.JC_TareWeightInfo, "This field has not been converted when the UOM was changed.");

			container.JC_TareWeight = newDefaultWeight;
			AssertNoWarnings("JC_TareWeight warning disappears", container.JC_TareWeightInfo);
		}

		public void TestValidateJC_DunnageWeight()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_DunnageWeight = 0;

			AssertNoWarnings("JC_DunnageWeight has no warnings", container.JC_DunnageWeightInfo);

			ZDecimal newWeight = 123.123;
			container.JC_DunnageWeight = newWeight;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			AssertEquals("Precondition: JC_DunnageWeight is not changed", newWeight, container.JC_DunnageWeight);
			AssertHasWarning("JC_DunnageWeight has warnings", container.JC_DunnageWeightInfo, "This field has not been converted when the UOM was changed.");

			container.JC_TareWeight = 0;
			AssertNoWarnings("JC_TareWeight warning disappears", container.JC_TareWeightInfo);
		}

		[TestDate(2021, 1, 1)]
		public void TestCheckJC_ArrivalCTOStorageStartDate()
		{
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 0 }))
			{
				var today = ZDateTime.Today;
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(3);
				var strategy = new Moq.Mock<IContainerDefaultingStrategy>();
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (today.AddDays(5), today, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => today);
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				var transport = consol.Transports[0];
				transport.JW_ATA = today.AddDays(10);
				transport.JW_VoyageFlight = "ZZ1234";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "NZAKL";

				using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
				{
					var container = consol.Containers.AddNew();

					container.JC_FCLAvailable = today;
					AssertEquals(today.AddDays(5), container.JC_ArrivalCTOStorageStartDate);
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertNoWarnings(container.JC_ArrivalCTOStorageStartDateInfo);

					container.JC_ArrivalCTOStorageStartDate = today;
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertHasWarning(container.JC_ArrivalCTOStorageStartDateInfo, $"The availability date and applicable storage free days indicate that this should be '{today.AddDays(5).ToShortDateString()}'.");
				}
			}

			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var today = ZDateTime.Today;
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(3);
				var strategy = new Moq.Mock<IContainerDefaultingStrategy>();
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (today.AddDays(6), today, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => today);
				var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as CommonConsol;
				consol.JK_TransportMode = TransportModes.Sea;
				var transport = consol.Transports[0];
				transport.JW_ATA = today.AddDays(10);
				transport.JW_VoyageFlight = "ZZ1234";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_TerminalAvailabilityDateForBinding = today;

				using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
				{
					var container = consol.Containers.AddNew();

					container.JC_FCLWharfGateOut = today.AddDays(11);
					AssertEquals(today.AddDays(6), container.JC_ArrivalCTOStorageStartDate);
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertNoWarnings(container.JC_ArrivalCTOStorageStartDateInfo);

					container.JC_ArrivalCTOStorageStartDate = today;
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertHasWarning(container.JC_ArrivalCTOStorageStartDateInfo, $"The availability date and applicable storage free days indicate that this should be '{today.AddDays(6).ToShortDateString()}'.");
				}
			}

			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 0 }))
			{
				var today = ZDateTime.Today;
				var strategy = new Moq.Mock<IContainerDefaultingStrategy>();
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(() => null);
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (ZDateTime.Empty, today, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => today);
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				var transport = consol.Transports[0];
				transport.JW_ATA = today.AddDays(10);
				transport.JW_VoyageFlight = "ZZ1234";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "NZAKL";

				using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
				{
					var container = consol.Containers.AddNew();

					container.JC_FCLAvailable = today;
					AssertEquals(ZDateTime.Empty, container.JC_ArrivalCTOStorageStartDate);
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertNoWarnings(container.JC_ArrivalCTOStorageStartDateInfo);

					container.JC_ArrivalCTOStorageStartDate = today;
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertNoWarnings(container.JC_ArrivalCTOStorageStartDateInfo);

					container.JC_ArrivalCTOStorageStartDate = today.AddDays(-1);
					container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
					AssertHasWarning(container.JC_ArrivalCTOStorageStartDateInfo, $"The availability date and applicable storage free days indicate that this should be greater than '{today.ToShortDateString()}'.");
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestCheckJC_ArrivalCTOStorageStartDate_TransportStorageSetToBlankFallsBackToRegistry()
		{
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var today = ZDateTime.Today;
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				var transport = consol.Transports[0];
				transport.JW_ATA = today.AddDays(10);
				transport.JW_VoyageFlight = "ZZ1234";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_TerminalAvailabilityDate = today.AddDays(5);
				transport.JW_TerminalStorageDate = today.AddDays(4);

				var container = consol.Containers.AddNew();
				container.JC_OverrideFCLAvailableStorage = false;

				AssertEquals("Pre: It uses the transport storage date", today.AddDays(4), container.JC_ArrivalCTOStorageStartDate);

				transport.JW_TerminalStorageDate = ZDateTime.Empty;
				container.Validation.ValidateJC_ArrivalCTOStorageStartDate();
				AssertEquals("It uses the registry, CTO Available (JW_TerminalAvailabilityDate) + 3 days", transport.JW_TerminalAvailabilityDate.AddDays(3), container.JC_ArrivalCTOStorageStartDate);
				AssertNoWarnings("The warning validation calculation should be consistent", container.JC_ArrivalCTOStorageStartDateInfo);
			}
		}

		#region Test Contract Allocations

		public void TestValidateJC_RCA_AllocationLine()
		{
			var nz = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ");
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Contract;
			intZone.FZ_Code = "SHRK";
			intZone.UNLOCOs.Add(ausyd);
			intZone.Countries.Add(nz);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "SHREKS SHIP";

			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "FIONA";
			refContainer1.RC_FreightRateClass = "XXX";
			refContainer1.RC_StorageClass = "YYY";
			refContainer1.RC_TEU = 1;
			refContainer1.RC_ContainerType = "DRY";

			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "PUSS";
			refContainer2.RC_FreightRateClass = "XXX";
			refContainer2.RC_StorageClass = "YYY";
			refContainer2.RC_TEU = 1;
			refContainer2.RC_ContainerType = "OTH";

			var carrier = Factory.New<OrgHeader>();

			var carrierContract1 = Factory.New<RatingContract>();
			carrierContract1.RCT_ContractNumber = "SHREKTRACT";
			carrierContract1.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract1.RCT_OH = carrier.PK;

			var allocationRoute1 = carrierContract1.Allocations.AddNew();
			allocationRoute1.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute1.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute1.RCA_LoadLocation = "AUSYD";
			allocationRoute1.RCA_DischargeLocation = "NZAKL";
			allocationRoute1.RCA_VoyageNumber = "SHREK123";
			allocationRoute1.RCA_RV_NKVessel = vessel.RV_Code;
			allocationRoute1.RCA_RC_ContainerType = refContainer1.PK;
			allocationRoute1.RCA_AllocatedQuantity = 15;
			allocationRoute1.RCA_AllocatedUQ = AllocationQuantityUnits.TwentyFootUnits;
			allocationRoute1.RCA_BookingVariance = 10;
			allocationRoute1.RCA_AllocationLineID = "0001";
			allocationRoute1.RCA_HasBookingLimit = true;

			var allocationRoute2 = carrierContract1.Allocations.AddNew();
			allocationRoute2.RCA_AllocationLineID = "0002";

			var carrierContract2 = Factory.New<RatingContract>();
			carrierContract2.RCT_ContractNumber = "FARQUAAD";
			carrierContract2.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract2.RCT_OH = carrier.PK;

			var allocationRoute3 = carrierContract2.Allocations.AddNew();
			allocationRoute3.RCA_AllocationLineID = "0003";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_ETD = allocationRoute1.RCA_StartDate;
			transport1.JW_RL_NKLoadPort = allocationRoute1.RCA_LoadLocation;
			transport1.JW_RL_NKDiscPort = "AUPER";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_ETD = allocationRoute1.RCA_ExpiryDate;
			transport2.JW_RL_NKLoadPort = "AUPER";
			transport2.JW_RL_NKDiscPort = allocationRoute1.RCA_DischargeLocation;
			transport2.JW_VoyageFlight = allocationRoute1.RCA_VoyageNumber;
			transport2.JW_Vessel = allocationRoute1.RCA_RV_NKVessel;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "DONKEY6969";
			container1.JC_RC = refContainer1.PK;
			container1.JC_ContainerCount = 7;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainer2.PK;
			container2.JC_ContainerCount = 4;

			consol.JK_RCA_AllocationLine = ZGuid.Empty;
			container1.JC_RCA_AllocationLine = allocationRoute1.PK;
			container2.JC_RCA_AllocationLine = allocationRoute1.PK;
			AssertContainerAllocationLineHasNoErrors(container1);

			container1.JC_RC = refContainer2.PK;
			AssertContainerAllocationLineHasError(container1, $"Container Code ({allocationRoute1.ContainerType.RC_Code}) of Allocation Route {allocationRoute1.RCA_AllocationLineID} does not match the Container Code ({refContainer2.RC_Code}) of this Container. Container on a Consol and on a selected Allocation Route should share the same Container Code.");

			allocationRoute1.RCA_RC_ContainerType = ZGuid.Empty;
			AssertContainerAllocationLineHasNoErrors(container1);

			allocationRoute1.RCA_StorageOrFreightRateClass = "XXX";
			AssertContainerAllocationLineHasNoErrors(container1);

			refContainer2.RC_FreightRateClass = "AAA";
			AssertContainerAllocationLineHasError(container1, $"Container Class ({allocationRoute1.RCA_StorageOrFreightRateClass}) of Allocation Route {allocationRoute1.RCA_AllocationLineID} does not match the Container Class ({refContainer2.RC_StorageClass}/{refContainer2.RC_FreightRateClass}) of this Container. Container on a Consol and on a selected Allocation Route should share the same Container Class.");

			allocationRoute1.RCA_StorageOrFreightRateClass = "YYY";
			AssertContainerAllocationLineHasNoErrors(container1);

			refContainer2.RC_StorageClass = "BBB";
			AssertContainerAllocationLineHasError(container1, $"Container Class ({allocationRoute1.RCA_StorageOrFreightRateClass}) of Allocation Route {allocationRoute1.RCA_AllocationLineID} does not match the Container Class ({refContainer2.RC_StorageClass}/{refContainer2.RC_FreightRateClass}) of this Container. Container on a Consol and on a selected Allocation Route should share the same Container Class.");

			allocationRoute1.RCA_StorageOrFreightRateClass = string.Empty;
			AssertContainerAllocationLineHasNoErrors(container1);

			carrierContract1.RCT_ContainerType = refContainer1.RC_ContainerType;
			AssertContainerAllocationLineHasError(container1, $"Container Type ({carrierContract1.RCT_ContainerType}) of Allocation Route's Parent Contract {carrierContract1.RCT_ContractNumber} does not match the Container Type ({refContainer2.RC_ContainerType}) of this Container. Container(s) on a Consol and on a selected Allocation Route's Parent Contract should share the same Container Type.");

			carrierContract1.RCT_ContainerType = refContainer2.RC_ContainerType;
			AssertContainerAllocationLineHasNoErrors(container1);

			allocationRoute1.RCA_LoadLocation = string.Empty;
			AssertContainerAllocationLineHasNoErrors(container1);

			allocationRoute1.RCA_DischargeLocation = string.Empty;
			AssertContainerAllocationLineHasNoErrors(container1);

			consol.JK_CarrierContractNumber = ZString.Empty;
			allocationRoute1.RCA_RCT_RatingContract = ZGuid.Empty;
			container1.JC_RCA_AllocationLine = allocationRoute1.PK;
			allocationRoute1.RCA_RCT_RatingContract = carrierContract1.PK;
			AssertContainerAllocationLineHasError(container1, "This Consol doesn't have any Carrier Contracts allocated. Allocation Route ID selected for the Consol and/or its Containers should be from the same Carrier Contract that this Consol is allocated to.");

			consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;
			var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
			container1.JC_RCA_AllocationLine = nonExistentAllocationLineGuid;
			AssertContainerAllocationLineHasError(container1, $"Allocation Route ID selected for the Consol and/or its Containers should be valid and from the same Carrier Contract ({carrierContract1.RCT_ContractNumber}) that this Consol is allocated to.");

			container1.JC_RCA_AllocationLine = allocationRoute1.PK;
			allocationRoute1.RCA_RCT_RatingContract = carrierContract2.PK;
			AssertContainerAllocationLineHasError(container1, $"Allocation Route ID selected for the Consol and/or its Containers should be from the same Carrier Contract ({carrierContract1.RCT_ContractNumber}) that this Consol is allocated to.");

			allocationRoute1.RCA_RCT_RatingContract = carrierContract1.PK;
			allocationRoute1.RCA_AllocatedQuantity = 5;
			var exceededAmount = -ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationRoute1);
			AssertContainerAllocationLineHasError(container1, $"Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID} by {exceededAmount} TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.");

			allocationRoute1.RCA_AllocatedUQ = AllocationQuantityUnits.Containers;
			exceededAmount = -ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationRoute1);
			AssertContainerAllocationLineHasError(container1, $"Number of Containers to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID} by {exceededAmount}. Number of Containers should be within the Booking Limit of the selected Allocation Route.");
		}

		public void TestValidateJC_RCA_AllocationLine_LoadPort()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			AssertNoErrorContaining("Consol first load matches", container.JC_RCA_AllocationLineInfo, "The First Load Port of the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");

			consol.JK_RL_NKLoadPort = "AUBNE";
			container.Validation.ValidateJC_RCA_AllocationLine();

			AssertHasErrorContaining("Consol first load doesn't matches", container.JC_RCA_AllocationLineInfo, "The First Load Port of the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has leg that matches load", container.JC_RCA_AllocationLineInfo, "The First Load Port of the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");
		}

		public void TestJC_RCA_AllocationLine_LoadPortWithValidETD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();

			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");

			transport.JW_ETD = ZDateTime.Today.AddDays(6);
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");

			transport.JW_RL_NKLoadPort = "AUBNE";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");

			transport.JW_ETD = ZDateTime.Today;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");

			consol.JK_RL_NKLoadPort = "AUBNE";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");

			transport.JW_RL_NKLoadPort = "AUSYD";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period");
		}

		public void TestJC_RCA_AllocationLine_DischargePort()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var container = consol.Containers.AddNew();

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol final discharge matches", container.JC_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");

			consol.JK_RL_NKDischargePort = "AUBNE";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol final discharge does not match", container.JC_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has leg that matches discharge", container.JC_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");
		}

		public void TestJC_RCA_AllocationLine_GatewayConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "TIRAMISU";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "PUDDING";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_AllowGatewayConsolOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "TIRAMISU";
			consol.JK_SendingForwarderHandlingType = "YAH";
			consol.JK_ReceivingForwarderHandlingType = "AAH";

			var container = consol.Containers.AddNew();

			const string errorMessage = "Allocation Route PUDDING is restricted to Gateway Consols only, but this Consol has no assigned Gateway Agent. Either the Sending or Receiving Agent of this Consol must be assigned as a Gateway Agent.";

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no Gateway Agent assigned.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_SendingForwarderHandlingType = "GTT";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has Gateway Agent assigned.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_SendingForwarderHandlingType = "YAH";
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has Gateway Agent assigned.", container.JC_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestJC_RCA_AllocationLine_ContainerOwner()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DJUNGELSKOG";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "BLAHAJ";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DJUNGELSKOG";

			var container = consol.Containers.AddNew();
			container.JC_IsShipperOwned = false;

			const string shipperErrorMessage = "Only Shipper Owned Containers are eligible to consume Allocation Route BLAHAJ but this container is not Shipper Owned.";
			const string nonShipperErrorMessage = "Only Non-Shipper Owned Containers are eligible to consume Allocation Route BLAHAJ but this container is Shipper Owned.";

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Container is not Shipper Owned.", container.JC_RCA_AllocationLineInfo, shipperErrorMessage);

			container.JC_IsShipperOwned = true;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Container is Shipper Owned.", container.JC_RCA_AllocationLineInfo, shipperErrorMessage);

			allocationRoute.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Container is Shipper Owned.", container.JC_RCA_AllocationLineInfo, nonShipperErrorMessage);

			container.JC_IsShipperOwned = false;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Container is not Shipper Owned.", container.JC_RCA_AllocationLineInfo, nonShipperErrorMessage);
		}

		public void TestJC_RCA_AllocationLine_GroupageContainerMode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "AQ";
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AQ";
			allocationRoute.RCA_AllowGroupageOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "AQ";
			consol.JK_ConsolMode = "FCL";

			var container = consol.Containers.AddNew();

			const string errorMessage = "Allocation Route AQ is restricted to Consols with Groupage container mode. The Consol's current container mode is FCL.";

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Error when Consol's container mode is not GRP.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_ConsolMode = "GRP";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's container mode is GRP.", container.JC_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestJC_RCA_AllocationLine_DischargePortWithValidETD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();

			const string etdDateRangeMessage = "None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period";

			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today.AddDays(6);
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_RL_NKDiscPort = "NZWEL";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);

			consol.JK_RL_NKDischargePort = "NZWEL";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_RL_NKDiscPort = "NZAKL";
			var precedingTransport = consol.Transports.AddNew();
			precedingTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			precedingTransport.JW_ETD = transport.JW_ETD.AddDays(-1);
			precedingTransport.JW_RL_NKLoadPort = "AUPER";
			precedingTransport.JW_RL_NKDiscPort = "NZWEL";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid disc leg that falls within valid ETD dates", container.JC_RCA_AllocationLineInfo, etdDateRangeMessage);
		}

		public void TestJC_RCA_AllocationLine_TransportLegScheduleDetails()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_VoyageNumber = "SORYA";
			allocationRoute.RCA_RV_NKVessel = "DUH";
			allocationRoute.RCA_ServiceLoop = "CD2";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_VoyageFlight = "DF2";

			var container = consol.Containers.AddNew();

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Schedule details are not fully matching", container.JC_RCA_AllocationLineInfo, "that match the Voyage, Vessel, and Service String of Allocation Route");

			transport.JW_VoyageFlight = "SORYA";
			transport.JW_Vessel = "DUH";
			transport.JW_ServiceString = "CD2";
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("Schedule details are fully matching", container.JC_RCA_AllocationLineInfo, "that match the Voyage, Vessel, and Service String of Allocation Route");
		}

		public void TestJC_RCA_AllocationLine_WarningWhenNoMatchingNamedAccounts()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "RivalEvilCo";

			var carrierContract = Factory.New<RatingContract>();
			carrierContract.RCT_ContractNumber = "WorldDominationAttempt03";
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_StartDate = ZDate.Today.AddDays(-5);
			carrierContract.RCT_EndDate = ZDate.Today.AddDays(5);
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_OH = carrier.PK;

			var allocationLine = carrierContract.Allocations.AddNew();
			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			allocationLine.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			allocationLine.RCA_LoadLocation = "AUSYD";
			allocationLine.RCA_DischargeLocation = "NZAKL";
			allocationLine.RCA_AllocationLineID = "007";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = TransportModes.Sea;
			transport1.JW_ETD = carrierContract.RCT_StartDate;
			transport1.JW_RL_NKLoadPort = allocationLine.RCA_LoadLocation;
			transport1.JW_RL_NKDiscPort = "AUPER";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_ETD = carrierContract.RCT_EndDate;
			transport2.JW_RL_NKLoadPort = "AUPER";
			transport2.JW_RL_NKDiscPort = allocationLine.RCA_DischargeLocation;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ControllingCustomerNameOrPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToString();

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationLine.PK;

			container.Validation.ValidateJC_RCA_AllocationLine();
			var message = "At least one of the Container's Consol clients should match a Named Account of Carrier Contract WorldDominationAttempt03 and Allocation Route 007 (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";
			AssertHasWarning("should have warning since there is no matching named account", container.JC_RCA_AllocationLineInfo, message);

			shipment.ConsigneePK = namedAccount.PK;
			AssertContainerAllocationLineHasNoErrors(container);
		}

		public void TestJC_RCA_AllocationLine_Agents()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "CHICKENJOCKEY";
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "CHICKEN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_AllocatedQuantity = 5;
			allocationRoute.RCA_AllocatedUQ = "TU";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "DENNIS";
			var randomOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg1.OH_Code = "STEVE";
			var randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg2.OH_Code = "GARRETT";
			allocationRoute.AgentPivots.AddRelatedIfNotExist(agent);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;

			const string errorMessage = "Neither Sending Agent STEVE nor Receiving Agent GARRETT of the Consol matches with the Agents specified on Allocation Route CHICKEN under Carrier Contract CHICKENJOCKEY.";

			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.GC_OH_OrgProxy = agent.PK;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = branchProxy.MainAddress.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg1.MainAddress.PK;
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", container.JC_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestJC_RCA_AllocationLine_MatchesParentConsolAllocationLine()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "RivalEvilCo";

			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "WorldDominationAttempt03";
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_StartDate = ZDate.Today.AddDays(-5);
			carrierContract.RCT_EndDate = ZDate.Today.AddDays(5);
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_OH = carrier.PK;

			var allocationLine1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine1.RCA_AllocationLineID = "A001";

			var allocationLine2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine2.RCA_AllocationLineID = "A002";

			carrierContract.Allocations.Add(allocationLine1);
			carrierContract.Allocations.Add(allocationLine2);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var voyageTransport = consol.Transports.MostInterestingTransport;
			voyageTransport.JW_ETD = ZDate.Today;
			voyageTransport.JW_RL_NKLoadPort = allocationLine1.RCA_LoadLocation;
			voyageTransport.JW_RL_NKDiscPort = allocationLine1.RCA_DischargeLocation;

			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_RCA_AllocationLine = allocationLine1.PK;

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_RCA_AllocationLine = allocationLine2.PK;

			AssertContainerAllocationLineHasError(consolContainer, "Allocation Route A001 selected for the Consol does not match the Allocation Route A002 selected for the Container.");

			consolContainer.JC_RCA_AllocationLine = allocationLine1.PK;
			AssertContainerAllocationLineHasNoErrors(consolContainer);

			consol.JK_RCA_AllocationLine = Guid.Empty;
			consolContainer.JC_RCA_AllocationLine = allocationLine1.PK;
			AssertContainerAllocationLineHasNoErrors(consolContainer); // container can have allocation line when consol has no allocation line

			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			booking.JS_RCA_BookingAllocationLine = allocationLine1.PK;

			var bookingContainer = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			bookingContainer.JC_RCA_AllocationLine = allocationLine2.PK;

			var expectedQBMessage = "Allocation Route A001 selected for the Booking does not match the Allocation Route A002 selected for the Container.";
			AssertContainerAllocationLineHasError(bookingContainer, expectedQBMessage);

			bookingContainer.JC_RCA_AllocationLine = allocationLine1.PK;
			AssertContainerAllocationLineNoError(bookingContainer, expectedQBMessage);

			booking.JS_RCA_BookingAllocationLine = ZGuid.Empty;
			AssertContainerAllocationLineNoError(bookingContainer, expectedQBMessage);
		}

		public void TestJC_RCA_AllocationLine_CannotBeEmptyWhileParentConsolIsAllocated()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "RivalEvilCo";

			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "WorldDominationAttempt03";
			carrierContract.RCT_ContractType = RatingContractTypes.Provider;
			carrierContract.RCT_StartDate = ZDate.Today.AddDays(-5);
			carrierContract.RCT_EndDate = ZDate.Today.AddDays(5);
			carrierContract.RCT_TransportMode = TransportModes.Sea;
			carrierContract.RCT_OH = carrier.PK;

			var allocationLine1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine1.RCA_AllocationLineID = "A001";

			var allocationLine2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine2.RCA_AllocationLineID = "A002";

			carrierContract.Allocations.Add(allocationLine1);
			carrierContract.Allocations.Add(allocationLine2);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var voyageTransport = consol.Transports.MostInterestingTransport;
			voyageTransport.JW_ETD = ZDate.Today;
			voyageTransport.JW_RL_NKLoadPort = allocationLine1.RCA_LoadLocation;
			voyageTransport.JW_RL_NKDiscPort = allocationLine1.RCA_DischargeLocation;

			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_RCA_AllocationLine = allocationLine1.PK;

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_RCA_AllocationLine = ZGuid.Empty;

			AssertContainerAllocationLineHasError(consolContainer, "Allocation Route cannot be empty on a Container while an Allocation Route is selected for the Consol.");

			consolContainer.JC_RCA_AllocationLine = allocationLine1.PK;
			AssertContainerAllocationLineHasNoErrors(consolContainer);

			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			booking.JS_RCA_BookingAllocationLine = allocationLine1.PK;

			var bookingContainer = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			bookingContainer.JC_RCA_AllocationLine = ZGuid.Empty;

			var expectedQBMessage = "Allocation Route cannot be empty on a Container while an Allocation Route is selected for the Booking.";
			AssertContainerAllocationLineHasError(bookingContainer, expectedQBMessage);

			bookingContainer.JC_RCA_AllocationLine = allocationLine1.PK;
			AssertContainerAllocationLineNoError(bookingContainer, expectedQBMessage);
		}

		public void TestValidateJC_CLH_LoadListPlan_NoError()
		{
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_LoadListId = "CLP101";
			var container = Factory.New<ForwardingContainer>();
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;

			foreach (var validStatus in new[] { ContainerLoadListHeaderStatus.Approved, ContainerLoadListHeaderStatus.Incomplete, ContainerLoadListHeaderStatus.Placed, ContainerLoadListHeaderStatus.Rejected })
			{
				containerLoadPlan.CLH_Status = validStatus;
				AssertNoErrors(container.JC_CLH_LoadListPlanInfo);
			}
		}

		public void TestValidateRelatedContainerLoadPlanPK_ShowErrorWhenStatusOrModeNotValid()
		{
			var errorMsg = "Only incomplete, placed, rejected or approved Container Load Plan can be linked to containers.";
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_LoadListId = "CLP102";
			containerLoadPlan.CLH_LoadMode = ContainerLoadListHeaderLoadMode.ContainerYard;

			var container = Factory.New<ForwardingContainer>();
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			AssertHasError(container.JC_CLH_LoadListPlanInfo, errorMsg);

			containerLoadPlan.CLH_LoadMode = ContainerLoadListHeaderLoadMode.ContainerFreightStation;
			foreach (var invalidStatus in new[] { ContainerLoadListHeaderStatus.Converted, ContainerLoadListHeaderStatus.Cancelled, ContainerLoadListHeaderStatus.Planned, ContainerLoadListHeaderStatus.Shipped })
			{
				containerLoadPlan.CLH_Status = invalidStatus;
				AssertHasError(container.JC_CLH_LoadListPlanInfo, errorMsg);
			}
		}

		public void TestValidateForConsolParentForUnlinkedAllocationRoute_InvalidDatesWhenAllocatedToRoute()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(5);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate;
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today;

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertContainerAllocationLineHasError(consolContainer,
				$"None of the ETDs under the relevant Routing Leg(s) of the Consol for this Container fall within the validity period between the Start Date ({allocationRoute.RCA_StartDate.ToShortDateString()}) and the Expiry Date ({allocationRoute.RCA_ExpiryDate.ToShortDateString()}) of Allocation Route .");
		}

		void AssertContainerAllocationLineHasError(ForwardingContainer container, string expectedMessage)
		{
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertHasError(container.JC_RCA_AllocationLineInfo, expectedMessage);
		}

		void AssertContainerAllocationLineNoError(ForwardingContainer container, string expectedMessage)
		{
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoError(container.JC_RCA_AllocationLineInfo, expectedMessage);
		}

		void AssertContainerAllocationLineHasNoErrors(ForwardingContainer container)
		{
			container.Validation.ValidateJC_RCA_AllocationLine();
			AssertNoErrors(container.JC_RCA_AllocationLineInfo);
		}

		#endregion

		#region Test QuotedBooking Allocations

		public void TestJC_RCA_AllocationLine_CannotBeEmptyWhileParentBookingIsAllocated()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			booking.JS_RCA_BookingAllocationLine = allocationRoute.PK;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RCA_AllocationLine = ZGuid.Empty;

			var expectedMessage = "Allocation Route cannot be empty on a Container while an Allocation Route is selected for the Booking.";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			container.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingAllocationBookingLimit()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_HasBookingLimit = true;
			booking.JS_RCA_BookingAllocationLine = allocationRoute.PK;

			var refcontainer1 = Factory.NewWithValidTestData<RefContainer>();
			refcontainer1.RC_TEU = 1;

			allocationRoute.RCA_AllocatedUQ = AllocationQuantityUnits.Containers;
			var expectedCNMessage = $"Number of Containers to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute.RCA_AllocationLineID} by 1. Number of Containers should be within the Booking Limit of the selected Allocation Route.";
			var expectedTEUMessage = $"Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute.RCA_AllocationLineID} by 1 TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.";

			var container1 = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container1.JC_RC = refcontainer1.PK;
			container1.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertContainerAllocationLineNoError(container1, expectedCNMessage);
			AssertContainerAllocationLineNoError(container1, expectedTEUMessage);

			var container2 = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_RC = refcontainer1.PK;
			container2.JC_RCA_AllocationLine = allocationRoute.PK;
			AssertContainerAllocationLineHasError(container2, expectedCNMessage);

			allocationRoute.RCA_AllocatedUQ = AllocationQuantityUnits.TwentyFootUnits;
			AssertContainerAllocationLineHasError(container2, expectedTEUMessage);
		}

		public void TestValidateContainerBookingAllocationRouteInContract()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			booking.JS_CarrierContractNumber = ZString.Empty;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var invalidAllocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			container.JC_RCA_AllocationLine = invalidAllocationRoute.PK;

			var expectedMessage = "This Booking doesn't have any Carrier Contracts allocated. Allocation Route ID selected for the Booking and/or its Containers should be from the same Carrier Contract that this Booking is allocated to.";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;
			var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
			container.JC_RCA_AllocationLine = nonExistentAllocationLineGuid;

			expectedMessage = $"Allocation Route ID selected for the Booking and/or its Containers should be valid and from the same Carrier Contract ({contract.RCT_ContractNumber}) that this Booking is allocated to.";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			container.JC_RCA_AllocationLine = invalidAllocationRoute.PK;
			expectedMessage = $"Allocation Route ID selected for the Booking and/or its Containers should be from the same Carrier Contract ({booking.JS_CarrierContractNumber}) that this Booking is allocated to.";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			var validAllocation = contract.Allocations.AddNew();
			container.JC_RCA_AllocationLine = validAllocation.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingETD()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();

			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			container.JC_RCA_AllocationLine = allocationRoute.PK;

			booking.JS_E_DEP = ZDate.Today.AddDays(-10);
			var expectedLateMessage = $"Start Date ({allocationRoute.RCA_StartDate.ToString()}) of Allocation Route {allocationRoute.RCA_AllocationLineID} is later than the ETD ({booking.JS_E_DEP.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.";
			AssertContainerAllocationLineHasError(container, expectedLateMessage);

			booking.JS_E_DEP = ZDate.Today.AddDays(10);
			var expectedEarlyMessage = $"Expiry Date ({allocationRoute.RCA_ExpiryDate.ToString()}) of Allocation Route {allocationRoute.RCA_AllocationLineID} is earlier than the ETD ({booking.JS_E_DEP.Date.ToString()}) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.";
			AssertContainerAllocationLineHasError(container, expectedEarlyMessage);

			booking.JS_E_DEP = ZDate.Today.AddDays(-5);
			AssertContainerAllocationLineNoError(container, expectedLateMessage);
			AssertContainerAllocationLineNoError(container, expectedEarlyMessage);

			booking.JS_E_DEP = ZDate.Today;
			AssertContainerAllocationLineNoError(container, expectedLateMessage);
			AssertContainerAllocationLineNoError(container, expectedEarlyMessage);
		}

		public void TestValidateContainerBookingPorts()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			booking.JS_RL_NKLoadPort = "AUSYD";
			allocationRoute.RCA_LoadLocation = "NZAKL";

			var loadMessage = $"Load Port ({allocationRoute.RCA_LoadLocation}) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Load Port ({booking.JS_RL_NKLoadPort}) of this Booking. Booking Load Port should match the selected Allocation Route's Load Port.";
			AssertContainerAllocationLineHasError(container, loadMessage);

			booking.JS_RL_NKDischargePort = "NZAKL";
			allocationRoute.RCA_DischargeLocation = "AUSYD";
			var dischargeMessage = $"Discharge Port ({allocationRoute.RCA_DischargeLocation}) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Discharge Port ({booking.JS_RL_NKDischargePort}) of this Booking. Booking Discharge Port should match the selected Allocation Route's Discharge Port.";
			AssertContainerAllocationLineHasError(container, dischargeMessage);

			booking.JS_RL_NKLoadPort = "NZAKL";
			booking.JS_RL_NKDischargePort = "AUSYD";
			AssertContainerAllocationLineNoError(container, loadMessage);
			AssertContainerAllocationLineNoError(container, dischargeMessage);
		}

		public void TestValidateContainerBookingVoyageNumber()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			allocationRoute.RCA_VoyageNumber = ZString.Empty;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			booking.JS_JX = sailing.PK;

			var testJobVoyage = Factory.New<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "0007";

			var testJobVoyOrigin = Factory.New<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;

			sailing.JX_JA = testJobVoyOrigin.PK;

			var expectedMessage = $"Voyage (SHREK123) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Voyage ({sailing.JX_JV_VoyageFlight}) of this Booking. Booking Voyage should match the selected Allocation Route's Voyage.";
			AssertContainerAllocationLineNoError(container, expectedMessage);

			allocationRoute.RCA_VoyageNumber = "SHREK123";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			allocationRoute.RCA_VoyageNumber = "0007";
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingVessel()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			allocationRoute.RCA_RV_NKVessel = ZString.Empty;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			booking.JS_JX = sailing.PK;
			var testJobVoyage = Factory.New<JobVoyage>();
			testJobVoyage.JV_RV_NKVessel = "GWEN";
			var testJobVoyOrigin = Factory.New<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			sailing.JX_JA = testJobVoyOrigin.PK;

			var expectedMessage = $"Vessel (MILES) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Vessel ({sailing.JX_JV_NKVessel}) of this Booking. Booking Vessel should match the selected Allocation Route's Vessel.";
			AssertContainerAllocationLineNoError(container, expectedMessage);

			allocationRoute.RCA_RV_NKVessel = "MILES";
			AssertContainerAllocationLineHasError(container, expectedMessage);

			allocationRoute.RCA_RV_NKVessel = "GWEN";
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingTypeAndClass()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			SetupBookingContract(booking, contract);

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			container.JC_RCA_AllocationLine = allocationRoute.PK;
			container.JC_ContainerNum = "1";
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_StorageClass = "AAA";
			container.JC_RC = refContainer.PK;
			container.RefContainer.RC_ContainerType = "TOP";

			var expectedTypeMessage = $"Container Type (BOT) of Allocation Route's Parent Contract {allocationRoute.Contract.RCT_ContractNumber} does not match the Container Type ({container.RefContainer.RC_ContainerType}) of this Container. Container(s) on a Booking and on a selected Allocation Route's Parent Contract should share the same Container Type.";
			var expectedClassMessage = $"Container Class (BOT) of Allocation Route {allocationRoute.RCA_AllocationLineID} does not match the Container Class (AAA) of this Container. Container on a Booking and on a selected Allocation Route should share the same Container Class.";

			AssertContainerAllocationLineNoError(container, expectedTypeMessage);
			AssertContainerAllocationLineNoError(container, expectedClassMessage);

			contract.RCT_ContainerType = "BOT";
			AssertContainerAllocationLineHasError(container, expectedTypeMessage);

			contract.RCT_ContainerType = "TOP";
			AssertContainerAllocationLineNoError(container, expectedTypeMessage);

			allocationRoute.RCA_StorageOrFreightRateClass = "BOT";
			AssertContainerAllocationLineHasError(container, expectedClassMessage);

			allocationRoute.RCA_StorageOrFreightRateClass = "TOP";
			AssertContainerAllocationLineNoError(container, expectedClassMessage);
		}

		public void TestValidateContainerBookingNamedAccountWithFallBack_ControllingCustomer()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();

			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			booking.BookedShippingLinePK = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;
			contract.RCT_ContractNumber = "MYCONTRACT";

			contract.RCT_ContractType = RatingContractTypes.Provider;
			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AAAH";
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			// Fallback to contract's named accounts
			var expectedMessage = $"At least one of the Container's Booking clients should match a Named Account of Carrier Contract MYCONTRACT and Allocation Route AAAH (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";

			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddChild(controllingCustomer1);
			booking.ControllingCustomerAddress.OrganisationPK = controllingCustomer1.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);

			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer3 = Factory.NewWithValidTestData<OrgHeader>();
			allocationRoute.NamedAccountPivots.AddChild(controllingCustomer3);

			// Can no longer fallback to contract's named accounts
			AssertContainerAllocationLineHasError(container, expectedMessage);

			booking.ControllingCustomerAddress.OrganisationPK = controllingCustomer2.PK;
			AssertContainerAllocationLineHasError(container, expectedMessage);

			booking.ControllingCustomerAddress.OrganisationPK = controllingCustomer3.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingNamedAccountWithFallBack_Client()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "MYCONTRACT";

			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			booking.BookedShippingLinePK = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;

			contract.RCT_ContractType = RatingContractTypes.Provider;
			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AAAH";
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			// Fallback to contract's named accounts
			var expectedMessage = "At least one of the Container's Booking clients should match a Named Account of Carrier Contract MYCONTRACT and Allocation Route AAAH (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";
			var client1 = Factory.NewWithValidTestData<OrgHeader>();

			var job = new JobHeader.Loader(booking).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = client1.MainAddress.PK;

			contract.NamedAccountPivots.AddChild(client1);
			AssertContainerAllocationLineNoError(container, expectedMessage);

			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var client3 = Factory.NewWithValidTestData<OrgHeader>();
			allocationRoute.NamedAccountPivots.AddChild(client3);

			// Can no longer fallback to contract's named accounts
			AssertContainerAllocationLineHasError(container, expectedMessage);

			job.JH_OA_LocalChargesAddr = client2.MainAddress.PK;
			AssertContainerAllocationLineHasError(container, expectedMessage);

			job.JH_OA_LocalChargesAddr = client3.MainAddress.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingNamedAccountWithFallBack_Consignor()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "MYCONTRACT";

			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			booking.BookedShippingLinePK = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;

			contract.RCT_ContractType = RatingContractTypes.Provider;
			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AAAH";
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			// Fallback to contract's named accounts
			var expectedMessage = "At least one of the Container's Booking clients should match a Named Account of Carrier Contract MYCONTRACT and Allocation Route AAAH (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddChild(consignor1);
			booking.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);

			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor3 = Factory.NewWithValidTestData<OrgHeader>();
			allocationRoute.NamedAccountPivots.AddChild(consignor3);

			// Can no longer fallback to contract's named accounts
			AssertContainerAllocationLineHasError(container, expectedMessage);
			booking.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;

			AssertContainerAllocationLineHasError(container, expectedMessage);

			booking.ConsignorDocumentaryAddress.OrganisationPK = consignor3.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		public void TestValidateContainerBookingNamedAccountWithFallBack_Consignee()
		{
			var quotedBooking = GetNewQuotedBooking();
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "MYCONTRACT";

			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			booking.BookedShippingLinePK = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;

			contract.RCT_ContractType = RatingContractTypes.Provider;
			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;

			var container = (ForwardingContainer)quotedBooking.QuotedBookingContainers.AddNew();
			var allocationRoute = contract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AAAH";
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			// Fallback to contract's named accounts
			var expectedMessage = "At least one of the Container's Booking clients should match a Named Account of Carrier Contract MYCONTRACT and Allocation Route AAAH (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).";
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee1.PK;
			contract.NamedAccountPivots.AddChild(consignee1);
			AssertContainerAllocationLineNoError(container, expectedMessage);

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee3 = Factory.NewWithValidTestData<OrgHeader>();
			allocationRoute.NamedAccountPivots.AddChild(consignee3);

			// Can no longer fallback to contract's named accounts
			AssertContainerAllocationLineHasError(container, expectedMessage);
			booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee2.PK;

			AssertContainerAllocationLineHasError(container, expectedMessage);

			booking.ConsigneeDocumentaryAddress.OrganisationPK = consignee3.PK;
			AssertContainerAllocationLineNoError(container, expectedMessage);
		}

		void SetupBookingContract(ForwardingShipment booking, RatingContract contract)
		{
			var contractServiceProvider = Factory.NewWithValidTestData<OrgHeader>();
			booking.BookedShippingLinePK = contractServiceProvider.PK;
			contract.RCT_OH = contractServiceProvider.PK;

			contract.RCT_ContractType = RatingContractTypes.Provider;
			booking.JS_CarrierContractNumber = contract.RCT_ContractNumber;
		}

		IQuotedBooking GetNewQuotedBooking()
		{
			return ObjectFactory
				.Get<IQuotedBookingBuilder>()
				.CreateNew(QuoteBookingType.QuickBooking, Factory);
		}

		#endregion
	}
}
