using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRelatedPartyCompanySpecificCollection))]
	sealed class OrgRelatedPartyCompanySpecificCollectionTest : OrgRelatedPartyCollectionTest
	{
		public void TestRemoveAndDelete()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Delivery);

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

			Factory.Save();

			AssertEquals(1, mainOrg.AllRelatedParties.Count);
			mainOrg.AllRelatedParties.RemoveAndDelete(mainOrg.AllRelatedParties[0]);
			AssertEquals(1, mainOrg.AllRelatedParties.Count);

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = true;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = false;

			AssertEquals(1, mainOrg.AllRelatedParties.Count);
			mainOrg.AllRelatedParties.RemoveAndDelete(mainOrg.AllRelatedParties[0]);
			AssertEquals(0, mainOrg.AllRelatedParties.Count);

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery);

			Factory.Save();

			AssertEquals(1, mainOrg.AllRelatedParties.Count);
			mainOrg.AllRelatedParties.RemoveAndDelete(mainOrg.AllRelatedParties[0]);
			AssertEquals(1, mainOrg.AllRelatedParties.Count);

			Env.Security.OrgDetailsModifyFinancialRelatedParties.IsAllowed = false;
			Env.Security.OrgDetailsModifyNonFinancialRelatedParties.IsAllowed = true;

			AssertEquals(1, mainOrg.AllRelatedParties.Count);
			mainOrg.AllRelatedParties.RemoveAndDelete(mainOrg.AllRelatedParties[0]);
			AssertEquals(0, mainOrg.AllRelatedParties.Count);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenMasterIsNull()
		{
			OrgRelatedPartyCompanySpecificCollectionForTest col = new OrgRelatedPartyCompanySpecificCollectionForTest(null, Factory);
			ZQuery query = col.CreateRelationshipFilter_Exposed();
			Assert("No exception should be thrown on prev line", true);
		}

		public void TestGetRelatedPartyWithOrgAddress()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery);
			OrgRelatedParty relatedParty = mainOrg.AllRelatedParties[0];
			relatedParty.PR_OA = mainOrg.MainAddress.PK;

			AssertEquals(relatedParty, mainOrg.AllRelatedParties.GetRelatedParty(mainOrg.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty));
		}

		public void TestRemoveItemRefreshesParentsProxyProperties()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales);
			AssertEquals(anotherOrg.PK, (ZGuid)mainOrg.SourceOfLeadPKInfo.Value);

			mainOrg.AllRelatedParties.RemoveRelatedParty(RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyDirectionList.Codes.Sales);
			AssertEquals(ZGuid.Empty, (ZGuid)mainOrg.SourceOfLeadPKInfo.Value);
		}

		public void TestLoadsOnlyRecordsForCurrentCompanyOrNull()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			OrgRelatedParty decoyParty = GetNewPartyRecord(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, ZGuid.NewZGuid());
			decoyParty.PR_OH_Parent = mainOrg.PK;

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup);
			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);

			OrgRelatedPartyCompanySpecificCollection allParties = mainOrg.AllRelatedParties;
			AssertEquals(2, allParties.Count);
			Assert(!allParties.Contains(decoyParty.PK));
		}

		public void TestGetRelatedPartyCompanySpecificOnly()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader aPSettlementOrg = GetOrganisation("Alex");
			OrgHeader decoyOrg = GetOrganisation("Sam");

			OrgRelatedParty decoyPartyRecord = GetNewPartyRecord(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP, ZString.Empty, ZString.Empty, ZGuid.Empty);
			decoyPartyRecord.PR_OH_Parent = mainOrg.PK;
			decoyPartyRecord.PR_OH_RelatedParty = decoyOrg.PK;

			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			AssertEquals("Should have one record", 1, parties.Count);

			OrgRelatedParty partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			AssertNull(partyRecord);

			mainOrg.SetRelatedParty(aPSettlementOrg, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			AssertNotNull(partyRecord);
			AssertEquals(GlbCompany.CurrentCompany.PK, partyRecord.PR_GC);
			AssertEquals(aPSettlementOrg.PK, partyRecord.RelatedParty.PK);
		}

		public void TestGetRelatedPartyWithFallbackForEnterpriseLevel()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader relatedPartyOrg = GetOrganisation("Peter");

			OrgRelatedParty partyRecord = GetNewPartyRecord(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty, ZGuid.Empty);
			partyRecord.PR_OH_Parent = mainOrg.PK;
			partyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;

			OrgRelatedParty cartagePartyRecord = mainOrg.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			AssertNotNull(cartagePartyRecord);
			AssertEquals("Company PK empty - Enterprise level", ZGuid.Empty, cartagePartyRecord.PR_GC);
			AssertEquals(relatedPartyOrg.PK, cartagePartyRecord.RelatedParty.PK);
		}

		public void TestGetRelatedPartyWithFallBackOnMode()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader aLLorg = GetOrganisation("Alex");
			OrgHeader seaOrg = GetOrganisation("Sam");
			OrgHeader fCLorg = GetOrganisation("Peter");

			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			OrgHeader lCLCartage = mainOrg.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertNull("No LCLCartage found", lCLCartage);

			mainOrg.SetRelatedParty(aLLorg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			lCLCartage = mainOrg.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(aLLorg.PK, lCLCartage.PK);

			mainOrg.SetRelatedParty(seaOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			lCLCartage = mainOrg.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(seaOrg.PK, lCLCartage.PK);

			mainOrg.SetRelatedParty(fCLorg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			lCLCartage = mainOrg.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(fCLorg.PK, lCLCartage.PK);
		}

		public void TestGetRelatedParty_WhenQueryTransportModeIsFASAndFSA()
		{
			//Delivery
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Delivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Delivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Delivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Delivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: true);

			//Pickup
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Pickup,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Pickup,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Pickup,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.Pickup,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: false);

			//PickupOrDelivery
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Delivery,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: false);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Air,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.SeaAir,
				shouldHaveRelatedParty: true);
			AssertHasRelatedParty(
				relatedPartyDirection: RelatedPartyDirectionList.Codes.PickupAndDelivery,
				relatedPartyTransportMode: Constants.TransportModes.Sea,
				queryDirection: RelatedPartyDirectionList.Codes.Pickup,
				queryTransportMode: Constants.TransportModes.AirSea,
				shouldHaveRelatedParty: false);
		}

		void AssertHasRelatedParty(ZString relatedPartyDirection, ZString relatedPartyTransportMode, ZString queryDirection, ZString queryTransportMode, bool shouldHaveRelatedParty)
		{
			var mainOrg = GetOrganisation("Main");
			var relatedPartyOrg = GetOrganisation("Peter");

			var partyRecord = GetNewPartyRecord(RelatedPartyTypeList.Codes.LocalTransport, relatedPartyDirection, relatedPartyTransportMode, ZString.Empty, ZGuid.Empty);
			partyRecord.PR_OH_Parent = mainOrg.PK;
			partyRecord.PR_OH_RelatedParty = relatedPartyOrg.PK;

			var cartagePartyRecord = mainOrg.AllRelatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, queryDirection, queryTransportMode, ZString.Empty);

			if (shouldHaveRelatedParty)
			{
				AssertNotNull(cartagePartyRecord);
				AssertEquals("Company PK empty - Enterprise level", ZGuid.Empty, cartagePartyRecord.PR_GC);
				AssertEquals(relatedPartyOrg.PK, cartagePartyRecord.RelatedParty.PK);
			}
			else
			{
				AssertNull(cartagePartyRecord);
			}
		}

		public void TestFallbackToPickupAndDelivery()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader pICDLVorg = GetOrganisation("Alex");

			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			mainOrg.SetRelatedParty(pICDLVorg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, ZString.Empty);

			OrgRelatedParty partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			OrgHeader deliverySeaCustomsBroker = partyRecord.Parent.DeliverySeaCustomsBroker;
			AssertNotNull(deliverySeaCustomsBroker);
			AssertEquals(deliverySeaCustomsBroker.PK, pICDLVorg.PK);
		}

		public void TestFallbackToLocation()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader melOrg = GetOrganisation("MEL");
			OrgHeader auOrg = GetOrganisation("AU");
			OrgHeader fallbackOrg = GetOrganisation("Fallback");

			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			mainOrg.SetRelatedParty(melOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, "AUMEL");
			mainOrg.SetRelatedParty(auOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, "AU");
			mainOrg.SetRelatedParty(fallbackOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty, "");

			OrgRelatedParty partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty, "AUMEL");
			AssertEquals("MEL", partyRecord.RelatedPartyName);

			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty, "AUSYD");
			AssertEquals("AU", partyRecord.RelatedPartyName);

			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty, "USCHI");
			AssertEquals("Fallback", partyRecord.RelatedPartyName);
		}

		public void TestOrderOfFallback()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader r1Org = GetOrganisation("R1");
			OrgHeader r2Org = GetOrganisation("R2");
			OrgHeader r3Org = GetOrganisation("R3");
			OrgHeader r4Org = GetOrganisation("R4");
			OrgHeader r5Org = GetOrganisation("R5");
			OrgHeader r6Org = GetOrganisation("R6");
			OrgHeader r7Org = GetOrganisation("R7");

			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			mainOrg.SetRelatedParty(r1Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			mainOrg.SetRelatedParty(r2Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			mainOrg.SetRelatedParty(r3Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, "AUMEL");
			mainOrg.SetRelatedParty(r4Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty, "AUMEL");
			mainOrg.SetRelatedParty(r5Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AU");
			mainOrg.SetRelatedParty(r6Org, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			var companySpecificParty = GetNewPartyRecord(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, GlbCompany.CurrentCompany.PK, "AUMEL");
			companySpecificParty.PR_OH_Parent = mainOrg.PK;
			companySpecificParty.PR_OH_RelatedParty = r7Org.PK;
			parties.Add(companySpecificParty);

			AssertEquals(7, parties.Count);

			OrgRelatedParty partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R7", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R1", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R2", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R3", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R4", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R5", partyRecord.RelatedPartyName);

			parties.Remove(partyRecord);
			partyRecord = parties.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			AssertEquals("R6", partyRecord.RelatedPartyName);
		}

		public void TestRemoveOrgRelatedParty()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, ZString.Empty);
			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			OrgRelatedPartyCompanySpecificCollection parties = mainOrg.AllRelatedParties;
			parties.Load();
			AssertEquals("There should be 2 related parties", 2, parties.Count);

			parties.RemoveRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Air, ZString.Empty);
			AssertEquals("No record should have been deleted", 2, parties.Count);

			parties.RemoveRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, ZString.Empty);
			AssertEquals("Only 1 record in the collection", 1, parties.Count);
		}

		public void TestSetRelatedParty()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");
			OrgHeader anotherOrg2 = GetOrganisation("Mike");

			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			mainOrg.SetRelatedParty(anotherOrg, RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);

			OrgRelatedPartyCompanySpecificCollection allParties = mainOrg.AllRelatedParties;
			AssertEquals(2, allParties.Count);

			OrgRelatedParty cartageParty = allParties.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			AssertNotNull(cartageParty);
			AssertEquals(ZGuid.Empty, cartageParty.PR_GC);
			AssertEquals(anotherOrg.PK, cartageParty.RelatedParty.PK);

			mainOrg.SetRelatedParty(anotherOrg2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			cartageParty = allParties.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			AssertEquals(anotherOrg2.PK, cartageParty.RelatedParty.PK);

			OrgRelatedParty apSettlementParty = allParties.GetRelatedParty(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyDirectionList.Codes.AP);
			AssertNotNull(apSettlementParty);
			AssertEquals(GlbCompany.CurrentCompany.PK, apSettlementParty.PR_GC);
		}

		public void TestSetRelatedParty_WithAddress()
		{
			OrgHeader mainOrg = GetOrganisation("Main");
			OrgHeader anotherOrg = GetOrganisation("Alex");

			mainOrg.AllRelatedParties.SetRelatedParty(mainOrg.MainAddress.PK, anotherOrg, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty);

			OrgRelatedPartyCompanySpecificCollection allParties = mainOrg.AllRelatedParties;
			AssertEquals(1, allParties.Count);

			OrgRelatedParty localTransportParty = allParties.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery);
			AssertEquals(mainOrg.MainAddress.PK, localTransportParty.PR_OA);
		}

		public void TestGetRelatedPartyWithNullAddress()
		{
			var mainOrg = GetOrganisation("Main");

			var orgWithAddress = GetOrganisation("Related party with address");
			var orgWithoutAddress = GetOrganisation("Related party without address");

			var addressPK = ZGuid.NewZGuid();
			mainOrg.SetRelatedParty(addressPK, orgWithAddress, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			mainOrg.SetRelatedParty(orgWithoutAddress, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var parties = mainOrg.AllRelatedParties;

			AssertEquals(2, parties.Count);

			var relatedPartyRecord = parties.GetRelatedParty(addressPK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);
			AssertEquals("When an address is provided, only parties with addresses should be considered.", orgWithAddress.PK, relatedPartyRecord.RelatedParty.PK);

			relatedPartyRecord = parties.GetRelatedParty(null, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);
			AssertEquals("When a null address is provided, only parties without addresses should be considered, even if a party with an address is a better match.", orgWithoutAddress.PK, relatedPartyRecord.RelatedParty.PK);
		}

		public void TestShouldBeCompanySpecific()
		{
			CombineAssertions(() =>
			{
				var codes = (new RelatedPartyTypeList()).GetAllCodes();
				foreach (var code in codes)
				{
					AssertEquals(code, code == RelatedPartyTypeList.Codes.APSettlementGroup || code == RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartyCompanySpecificCollection.ShouldBeCompanySpecific(code));
				}
			});
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgRelatedPartyCompanySpecificCollection(GetOrganisation("John"), Factory);
		}

		class OrgRelatedPartyCompanySpecificCollectionForTest : OrgRelatedPartyCompanySpecificCollection
		{
			public OrgRelatedPartyCompanySpecificCollectionForTest(OrgHeader parentOrganisation, BusinessObjectFactory factory)
				: base(parentOrganisation, factory)
			{
			}

			public ZQuery CreateRelationshipFilter_Exposed()
			{
				return base.CreateRelationshipFilter();
			}
		}

		OrgRelatedParty GetNewPartyRecord(ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZGuid companyPK)
		{
			return GetNewPartyRecord(partyType, direction, transportMode, containerMode, companyPK, ZString.Empty);
		}

		OrgRelatedParty GetNewPartyRecord(ZString partyType, ZString direction, ZString transportMode, ZString containerMode, ZGuid companyPK, ZString location)
		{
			OrgRelatedParty result = Factory.New<OrgRelatedParty>();
			result.PR_PartyType = partyType;
			result.PR_FreightDirection = direction;
			result.PR_Location = location;
			result.PR_FreightTransportMode = transportMode;
			result.PR_FreightContainerMode = containerMode;
			result.PR_GC = companyPK;
			return result;
		}

		#endregion
	}
}
