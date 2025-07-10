using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContractAllocationHelperTest : TestCaseWithFactory
	{
		public void TestGetCarrierContract()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "BLAHAJ";
			contract.RCT_OH = carrier.PK;
			contract.RCT_ContractType = "PRO";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			AssertNotNull("Contract should be found", ContractAllocationHelper.GetCarrierContract(otherFactory, "BLAHAJ", carrier.PK));
		}

		public void TestCalculateOutstandingUtilisation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "SHREK2ONDVD";
			contract.RCT_OH = carrier.PK;

			var allocationLine = contract.Allocations.AddNew();
			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
			allocationLine.RCA_AllocatedQuantity = 20;

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_Code = "SHREK1";
			refContainer1.RC_TEU = 3;

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_Code = "SHREK2";
			refContainer2.RC_TEU = 7;

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol1.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol1.JK_RCA_AllocationLine = allocationLine.PK;

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = refContainer1.PK;

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerCount = 5;
			container2.JC_RC = refContainer2.PK;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol2.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = refContainer1.PK;
			container3.JC_RCA_AllocationLine = allocationLine.PK;

			var container4 = consol2.Containers.AddNew();
			container4.JC_ContainerCount = 2;
			container4.JC_RC = refContainer1.PK;
			container4.JC_RCA_AllocationLine = allocationLine.PK;

			var container5 = consol2.Containers.AddNew();
			container5.JC_ContainerCount = 1;
			container5.JC_RC = refContainer2.PK;

			ZDecimal expectedResult = 20 - ((3 + 5 * 7) + (3 + 2 * 3));
			AssertEquals(
				"Outstanding utilisation should be permitted TEU minus sum of TEU of all assigned containers and all containers on assigned consols.",
				expectedResult,
				ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationLine));

			allocationLine.RCA_BookingVariance = -50;
			AssertEquals(
				"Outstanding utilisation should be permitted TEU minus sum of TEU of all assigned containers and all containers on assigned consols.",
				expectedResult,
				ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationLine));

			allocationLine.RCA_BookingVariance = 0;
			allocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			expectedResult = 20 - (1 + 5 + 1 + 2);
			AssertEquals(
				"Outstanding utilisation should be permitted containers minus count of all assigned containers and all containers on assigend consols.",
				expectedResult,
				ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationLine));

			allocationLine.RCA_BookingVariance = 25;
			expectedResult = 20 * 1.25 - (1 + 5 + 1 + 2);
			AssertEquals(
				"Outstanding utilisation should be permitted containers minus count of all assigned containers and all containers on assigend consols.",
				expectedResult,
				ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationLine));

			allocationLine.RCA_BookingVariance = -50;
			expectedResult = 20 - (1 + 5 + 1 + 2);
			AssertEquals(
				"Outstanding utilisation should be permitted containers minus count of all assigned containers and all containers on assigend consols.",
				expectedResult,
				ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationLine));
		}

		public void TestCalculateOutstandingUtilisationForBlankQuantity()
		{
			using (FreightConfigurationRegistry.Instance.EnableBlankQuantityForSubAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();

				var ratingContract = Factory.New<RatingContract>();
				ratingContract.RCT_ContractNumber = "BLANK";
				ratingContract.RCT_OH = carrier.PK;

				var parentAllocationLine = ratingContract.Allocations.AddNew();
				parentAllocationLine.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
				parentAllocationLine.RCA_AllocatedQuantity = 100;

				var childAllocationLine1 = ratingContract.Allocations.AddNew();
				childAllocationLine1.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
				childAllocationLine1.RCA_AllocatedQuantity = 30;
				childAllocationLine1.RCA_RCA_ParentAllocationRoute = parentAllocationLine.PK;

				var childAllocationLine2 = ratingContract.Allocations.AddNew();
				childAllocationLine2.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
				childAllocationLine2.RCA_AllocatedQuantity = 30;
				childAllocationLine2.RCA_RCA_ParentAllocationRoute = parentAllocationLine.PK;

				var childAllocationLine3 = ratingContract.Allocations.AddNew();
				childAllocationLine3.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
				childAllocationLine3.RCA_AllocatedQuantity = 0;
				childAllocationLine3.RCA_RCA_ParentAllocationRoute = parentAllocationLine.PK;

				var refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.RC_Code = "BlankRef";
				refContainer.RC_TEU = 1;

				var consolidation = Factory.New<ForwardingConsol>();
				consolidation.JK_CarrierContractNumber = contract.RCT_ContractNumber;
				consolidation.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consolidation.JK_RCA_AllocationLine = childAllocationLine3.PK;

				var container = consolidation.Containers.AddNew();
				container.JC_ContainerCount = 20;
				container.JC_RC = refContainer.PK;

				var childAllocationLine4 = ratingContract.Allocations.AddNew();
				childAllocationLine4.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
				childAllocationLine4.RCA_AllocatedQuantity = 0;
				childAllocationLine4.RCA_RCA_ParentAllocationRoute = parentAllocationLine.PK;

				ZDecimal expectedResult = 100 - (30 + 30) - (1 * 20);

				AssertEquals(
					"Outstanding utilisation of blank quantity route should be parent's allocated quantity minus sum of all other lines quantity and containers/TEUs of other blank quantities",
					expectedResult,
					ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, childAllocationLine4));
			}
		}

		public void TestCheckNamedAccountValidation()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var namedAccounts = new[] { org1, org2 };

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.ConsigneePK = org1.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.ConsignorPK = org2.PK;

			var shipment3 = Factory.New<ForwardingShipment>();
			new JobHeader.Loader(shipment3).TryLoadOrCreate();
			shipment3.ShipmentJobHeader.LocalChargesPK = org2.PK;

			var shipments = new[] { shipment1, shipment2, shipment3 };

			Assert("All shipments have matching orgs, so validation should pass.", ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));

			shipment1.ConsigneePK = ZGuid.Empty;
			Assert("Shipment 1's Consignee no longer matches, so validation should fail.", !ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));

			shipment1.ConsigneePK = org1.PK;
			shipment2.ConsignorPK = ZGuid.Empty;
			Assert("Shipment 2's Consignor no longer matches, so validation should fail.", !ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));

			shipment2.ConsignorPK = org2.PK;
			shipment3.ShipmentJobHeader.LocalChargesPK = ZGuid.Empty;
			Assert("Shipment 3's Local Client no longer matches, so validation should fail.", !ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));

			shipment3.ShipmentJobHeader.LocalChargesPK = org2.PK;
			Assert("All shipments have matching orgs, so validation should pass.", ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));

			var emptyNamedAccounts = new List<OrgHeader>();
			Assert("No Named Accounts, so should pass.", ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, emptyNamedAccounts));
		}

		public void TestCheckNamedAccountValidation_ControllingCustomerIsValidated()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ORG";
			shipment.ControllingCustomerAddress.OrganisationPK = organisation.PK;

			AssertNull(shipment.ShipmentJobHeader);
			AssertNotNull(shipment.ControllingCustomer);

			var namedAccounts = new[] { organisation };
			var shipments = new[] { shipment };

			Assert("Shipment have matching org, so validation should pass.", ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));
		}

		public void TestCheckNamedAccountValidation_CheckNullValues()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			AssertNull(shipment.ControllingCustomer);
			AssertNull(shipment.ShipmentJobHeader);

			var namedAccounts = new[] { organisation };
			var shipments = new[] { shipment };

			AssertEquals(
				"No exception shall be thrown and the result shall still be false",
				false,
				ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts));
		}

		public void TestIsPortCoveredByLocation()
		{
			var nz = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Contract;
			intZone.FZ_Code = "DNKY";
			intZone.FZ_Description = "Donkeys half of the swamp";
			intZone.UNLOCOs.Add(aubne);
			intZone.Countries.Add(nz);

			var relatedPortAUSYD = Factory.New<RefUNLOCORelatedPort>();
			relatedPortAUSYD.RLR_GroupNumber = 1;
			relatedPortAUSYD.RLR_RL_NKRelatedPort = "AUSYD";

			var relatedPortCNSHA = Factory.New<RefUNLOCORelatedPort>();
			relatedPortCNSHA.RLR_GroupNumber = 1;
			relatedPortCNSHA.RLR_RL_NKRelatedPort = "CNSHA";

			Factory.Save();

			Assert("Port does not match country, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "NZ", false));
			Assert("Port matches country, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "AU", false));

			Assert("Port does not match unloco, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "AUPER", false));
			Assert("Port matches unloco, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "AUSYD", false));

			Assert("Port is related to unloco, but related ports not allowed, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "CNSHA", "AUSYD", false));
			Assert("Port is not related to unloco, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "USLAX", "AUSYD", true));
			Assert("Port is related to unloco, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "CNSHA", "AUSYD", true));

			Assert("Port does not match unlocos or countries in zone, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "DNKY", false));
			Assert("Port matches unloco in zone, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUBNE", "DNKY", false));
			Assert("Port matches country in zone, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "NZAKL", "DNKY", false));

			Assert("Route location is empty, so should not check and is valid.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "", false));
		}

		public void TestIsPortCoveredByLocation_ZoneType_Is_All()
		{
			var nz = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			intZone.FZ_Code = "SHRK";
			intZone.UNLOCOs.Add(aubne);
			intZone.Countries.Add(nz);

			Assert("Port does not match unlocos or countries, so should not be covered.", !ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUSYD", "SHRK", false));
			Assert("Port matches unloco, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "AUBNE", "SHRK", false));
			Assert("Port matches country, so should be covered.", ContractAllocationHelper.IsPortCoveredByLocation(Factory, "NZAKL", "SHRK", false));
		}

		public void TestIsPortCoveredByLocation_Zone_WithAllowRelatedPorts()
		{
			var relatedPortCNSHA = Factory.New<RefUNLOCORelatedPort>();
			relatedPortCNSHA.RLR_GroupNumber = 1;
			relatedPortCNSHA.RLR_RL_NKRelatedPort = "CNSHA";

			var relatedPortCNTAC = Factory.New<RefUNLOCORelatedPort>();
			relatedPortCNTAC.RLR_GroupNumber = 1;
			relatedPortCNTAC.RLR_RL_NKRelatedPort = "CNTAC";

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			intZone.FZ_Code = "ZSHA";
			intZone.FZ_Description = "Zone with CNSHA";
			intZone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "CNSHA"));

			Factory.Save();

			AssertEquals("CNTAC is related to CNSHA, which is in zone ZSHA, so it should be covered for allowRelatedPorts = true.", true, ContractAllocationHelper.IsPortCoveredByLocation(Factory, "CNTAC", "ZSHA", true));
			AssertEquals("It should not be covered for allowRelatedPorts = false.", false, ContractAllocationHelper.IsPortCoveredByLocation(Factory, "CNTAC", "ZSHA", false));
		}

		public void TestDoesContainerTypeMatchAllocation()
		{
			var container = Factory.New<ForwardingContainer>();
			var refContainer = Factory.New<RefContainer>();

			container.JC_RC = refContainer.PK;
			var allocationRoute = Factory.New<RatingContractAllocationLine>();
			allocationRoute.RCA_RC_ContainerType = refContainer.PK;

			Assert("Container matches allocation", ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute));

			container.JC_RC = Factory.New<RefContainer>().PK;

			Assert("Container does not matches allocation", !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute));
		}

		public void TestConsolWillHaveLoadDetailsDefaulted_FalseForNonUNLOCORouteLoad()
		{
			consol.JK_RL_NKLoadPort = ZString.Empty;
			allocationRoute.RCA_LoadLocation = "AUSYD";

			var result = ContractAllocationHelper.ConsolWillHaveLoadDetailsDefaulted(consol, allocationRoute);
			Assert(result);

			allocationRoute.RCA_LoadLocation = "AU";
			result = ContractAllocationHelper.ConsolWillHaveLoadDetailsDefaulted(consol, allocationRoute);
			Assert("Consol load details can only be defaulted to UNLOCO values", !result);
		}

		public void TestConsolWillHaveDischargeDetailsDefaulted_FalseForNonUNLOCORouteDisc()
		{
			consol.JK_RL_NKDischargePort = ZString.Empty;
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var result = ContractAllocationHelper.ConsolWillHaveDischargeDetailsDefaulted(consol, allocationRoute);
			Assert(result);

			allocationRoute.RCA_DischargeLocation = "AU";
			result = ContractAllocationHelper.ConsolWillHaveDischargeDetailsDefaulted(consol, allocationRoute);
			Assert("Consol discharge details can only be defaulted to UNLOCO values", !result);
		}

		public void TestSingleLegLoadWillBeDefaulted_FalseForNonUNLOCORouteLoad()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RL_NKLoadPort = ZString.Empty;
			consol.Transports[0].JW_TransportMode = contract.RCT_TransportMode;

			allocationRoute.RCA_LoadLocation = "AUSYD";

			var result = ContractAllocationHelper.SingleLegLoadWillBeDefaulted(consol, allocationRoute);
			Assert(result);

			allocationRoute.RCA_LoadLocation = "AU";
			result = ContractAllocationHelper.SingleLegLoadWillBeDefaulted(consol, allocationRoute);
			Assert("Consol single leg load can only be defaulted to UNLOCO values", !result);
		}

		public void TestSingleLegDischargeWillBeDefaulted_FalseForNonUNLOCORouteDisc()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RL_NKDischargePort = ZString.Empty;
			consol.Transports[0].JW_TransportMode = contract.RCT_TransportMode;

			allocationRoute.RCA_DischargeLocation = "AUSYD";

			var result = ContractAllocationHelper.SingleLegDischargeWillBeDefaulted(consol, allocationRoute);
			Assert(result);

			allocationRoute.RCA_DischargeLocation = "AU";
			result = ContractAllocationHelper.SingleLegDischargeWillBeDefaulted(consol, allocationRoute);
			Assert("Consol single leg discharge can only be defaulted to UNLOCO values", !result);
		}

		protected override void SetUp()
		{
			base.SetUp();

			contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "R2D2";
			allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_AllocationLineID = "STUPEFY";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "EXPELLIARMUS";
		}

		RatingContract contract;
		RatingContractAllocationLine allocationRoute;
		ForwardingConsol consol;
	}
}
