using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.RefZoneHeaderLookups;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadList))]
	public class HVLVOriginLoadListTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsFunctionalTesting()
		{
			Assert("IsFunctionalTesting should be false by default", !HVLVOriginLoadList.IsFunctionalTesting);
			using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("IsFunctionalTesting should be true when registry setting is On", HVLVOriginLoadList.IsFunctionalTesting);
			}
		}

		public void TestNoExceptionThrow_WhenCanNotGetCarrierParties()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Courier;
			loadList.HVL_OH_Carrier = carrier.PK;

			var carrierParties = CTOFromCarrierDefaulter.GetCarrierParties(loadList.HVL_TransportMode, carrier);
			AssertNull("precondition:CarrierParties is null", carrierParties);

			AssertNoExceptionThrown(() =>
			{
				var originCTO = loadList.OriginCTO;
			});
		}

		public void TestHVL_Status_ReadOnly()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			Assert("Load List Status should be read only by default", loadList.HVL_StatusInfo.ReadOnly);
			using (HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Load List Status should be editable when testing mode is On", !loadList.HVL_StatusInfo.ReadOnly);
			}
		}

		public void TestGivenOriginLoadListLodgedOrConsolidated_MarkAllFieldsReadOnly()
		{
			var loadListOpen = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListOpen.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;

			var loadListPending = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListPending.HVL_Status = HVLVOriginLoadListStatus.Codes.Pending;

			var loadListClosed = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListClosed.HVL_Status = HVLVOriginLoadListStatus.Codes.Closed;

			var loadListFailed = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListFailed.HVL_Status = HVLVOriginLoadListStatus.Codes.Failed;

			var loadListLodged = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListLodged.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;

			var loadListConsolidated = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadListConsolidated.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newOpenList = newFactory.Load<HVLVOriginLoadList>(loadListOpen.PK);
			var newPendingList = newFactory.Load<HVLVOriginLoadList>(loadListPending.PK);
			var newClosedList = newFactory.Load<HVLVOriginLoadList>(loadListClosed.PK);
			var newFailedList = newFactory.Load<HVLVOriginLoadList>(loadListFailed.PK);
			var newLodgedList = newFactory.Load<HVLVOriginLoadList>(loadListLodged.PK);
			var newConsList = newFactory.Load<HVLVOriginLoadList>(loadListConsolidated.PK);

			AssertEquals("Consolidated load list is readonly", true, newConsList.ReadOnly);
			AssertEquals("Lodged load list is readonly", true, newLodgedList.ReadOnly);
			AssertEquals("Closed load list is not readonly", false, newClosedList.ReadOnly);
			AssertEquals("Failed load list is not readonly", false, newFailedList.ReadOnly);
			AssertEquals("Open load list is not readonly", false, newOpenList.ReadOnly);
			AssertEquals("Pending load list is not readonly", false, newPendingList.ReadOnly);
		}

		public void TestHumanReadableName()
		{
			var list = Factory.New<HVLVOriginLoadList>();
			AssertEquals("HVLVOriginLoadList without HVL_Unique reference", "HVLV Origin Load List", list.HumanReadableName);
			list.HVL_UniqueReference = "uniqueReference";
			AssertEquals("HVLVOriginLoadList with HVL_Unique reference", "HVLV Origin Load List uniqueReference", list.HumanReadableName);
		}

		public void TestSetLoadedOnConsolForOuterPackages()
		{
			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();

			outerPackage1.HVO_HVL_LoadList = loadList.PK;
			outerPackage2.HVO_HVL_LoadList = loadList.PK;
			loadList.SetLoadedOnConsolForOuterPackages(forwardingConsol);

			CombineAssertions(() =>
			{
				AssertEquals(outerPackage1.HVO_JK_LoadedOnConsol, forwardingConsol.PK);
				AssertEquals(outerPackage2.HVO_JK_LoadedOnConsol, forwardingConsol.PK);
			});
		}

		public void TestSetsHVLVOriginLoadListReferenceOnSaving()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_Status = "OPN";
			AssertEquals("Precondition: UniqueReference should be empty", "", loadList.HVL_UniqueReference);

			var uniqueReference = Env.NumberFountains.HVLVOriginLoadListReference.PeekPreliminaryFormatted(Factory);
			Factory.Save();

			AssertEquals(uniqueReference, loadList.HVL_UniqueReference);
		}

		public void TestOriginCTO_MappedToTransportMode()
		{
			var org1 = Factory.New<OrgHeader>();
			var address1 = org1.Addresses.AddNew();

			var org2 = Factory.New<OrgHeader>();
			var address2 = org2.Addresses.AddNew();

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;

			var originDepot = Factory.New<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUBNE";
			loadList.HVL_OA_OriginDepot = originDepot.PK;

			var carrier = Factory.New<OrgHeader>();
			loadList.HVL_OH_Carrier = carrier.PK;

			var airCTO = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			airCTO.O5_PortOrCountry = "AUBNE";
			airCTO.O5_OA_AgentOfficeAddress = address1.PK;

			var seaCTO = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO.O5_PortOrCountry = "AUBNE";
			seaCTO.O5_OA_AgentOfficeAddress = address2.PK;

			AssertEquals("Origin CTO mapped to transport mode", address1.PK, loadList.OriginCTO);
		}

		public void TestOriginCTO_MappedToFirstPortCTO()
		{
			var org1 = Factory.New<OrgHeader>();
			var address1 = org1.Addresses.AddNew();

			var org2 = Factory.New<OrgHeader>();
			var address2 = org2.Addresses.AddNew();

			var org3 = Factory.New<OrgHeader>();
			var address3 = org3.Addresses.AddNew();

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Sea;

			var originDepot = Factory.New<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUBNE";
			loadList.HVL_OA_OriginDepot = originDepot.PK;

			var carrier = Factory.New<OrgHeader>();
			loadList.HVL_OH_Carrier = carrier.PK;

			var seaCTO1 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO1.O5_PortOrCountry = "USLAX";
			seaCTO1.O5_OA_AgentOfficeAddress = address1.PK;

			var seaCTO2 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO2.O5_PortOrCountry = "AUBNE";
			seaCTO2.O5_OA_AgentOfficeAddress = address2.PK;

			var seaCTO3 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO3.O5_PortOrCountry = "AUBNE";
			seaCTO3.O5_OA_AgentOfficeAddress = address3.PK;

			AssertEquals("Origin CTO mapped to first cto", address2.PK, loadList.OriginCTO);
		}

		public void TestGivenLoadListThatHaveItemsAndOuterPackages_WhenDeletingLoadList_ThenRemoveAllFKReferencesToLoadListInItemsAndOuterPackages()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;
			item1.HVI_HVO_OuterPackage = outerPackage.PK;
			item1.HVI_HVL_LoadList = loadList.PK;

			item2.HVI_HVL_LoadList = loadList.PK;
			item3.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			AssertEquals("Precondition", loadList.PK, outerPackage.HVO_HVL_LoadList);
			AssertEquals("Precondition", loadList.PK, item1.HVI_HVL_LoadList);
			AssertEquals("Precondition", loadList.PK, item2.HVI_HVL_LoadList);
			AssertEquals("Precondition", loadList.PK, item3.HVI_HVL_LoadList);

			loadList.Delete();

			Factory.Save();

			AssertNotNull("Expected outer package not to be deleted when we delete load list that it FK references", outerPackage);
			AssertNotNull("Expected item1 not to be deleted when we delete load list that it FK references", item1);
			AssertNotNull("Expected item2 not to be deleted when we delete load list that it FK references", item2);
			AssertNotNull("Expected item3 not to be deleted when we delete load list that it FK references", item3);
			AssertEquals("Expected outer package FK to load list to be set to empty when load list is deleted", ZGuid.Empty, outerPackage.HVO_HVL_LoadList);
			AssertEquals("Expected item1 FK to load list to be set to empty when load list is deleted", ZGuid.Empty, item1.HVI_HVL_LoadList);
			AssertEquals("Expected item2 FK to load list to be set to empty when load list is deleted", ZGuid.Empty, item2.HVI_HVL_LoadList);
			AssertEquals("Expected item3 FK to load list to be set to empty when load list is deleted", ZGuid.Empty, item3.HVI_HVL_LoadList);
		}

		public void TestLoadListActiveItems_ReturnsOnlyActiveItems()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var consignment = bookingHeader.Consignments.AddNew();

			var activeItem = consignment.Items.AddNew();
			activeItem.HVI_IsActive = true;
			activeItem.HVI_HVL_LoadList = loadList.PK;

			var inactiveItem = consignment.Items.AddNew();
			inactiveItem.HVI_IsActive = false;
			inactiveItem.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			var activeItemOnLoadList = loadList.ActiveItems.Single();
			AssertEquals(activeItem.PK, activeItemOnLoadList.PK);
		}

		public void TestOriginDepot()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "17 Park Avenue North";
			address.OA_RL_NKRelatedPortCode = "USNYC";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = address.PK;

			AssertEquals("HVL_RL_NKOrigin property should be USNYC", "USNYC", loadList.HVL_RL_NKOrigin);

			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_OA_OriginDepot = address.PK;
			AssertEquals("HVL_RL_NKOrigin property is not updated by origin depot", "AUSYD", loadList.HVL_RL_NKOrigin);
		}

		public void TestDestinationDepot()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "17 Park Avenue North";
			address.OA_RL_NKRelatedPortCode = "USNYC";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = address.PK;

			AssertEquals("HVL_RL_NKDestination property should be USNYC", "USNYC", loadList.HVL_RL_NKDestination);

			loadList.HVL_RL_NKDestination = "AUSYD";
			loadList.HVL_OA_DestinationDepot = address.PK;
			AssertEquals("HVL_RL_NKDestination property is not updated by origin depot", "AUSYD", loadList.HVL_RL_NKDestination);
		}

		public void TestIsContainerInfoSpecified()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			AssertEquals("Pre-condition", false, loadList.IsContainerInfoSpecified);

			loadList.HVL_ContainerNumber = "TESTCONTAINER";
			AssertEquals("IsContainerInfoSpecified should be true when HVL_ContainerNumber is not empty", true, loadList.IsContainerInfoSpecified);
		}

		public void TestSplitSubLoadList()
		{
			var destinationDepot1 = Factory.New<OrgAddress>();
			destinationDepot1.OA_RN_NKCountryCode = "AU";
			var destinationDepot2 = Factory.New<OrgAddress>();
			destinationDepot2.OA_RN_NKCountryCode = "NZ";
			var billToParty = Factory.New<OrgAddress>();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader1.HVH_OA_BillToParty = billToParty.PK;

			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_OA_DestinationDepot = destinationDepot1.PK;
			var item1 = consignment1.Items.AddNew();
			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = loadList.PK;
			item1.HVI_HVO_OuterPackage = outerPackage1.PK;
			item1.HVI_HVL_LoadList = loadList.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader2.HVH_OA_BillToParty = billToParty.PK;

			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_OA_DestinationDepot = destinationDepot2.PK;
			var item2 = consignment2.Items.AddNew();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage2.HVO_HVL_LoadList = loadList.PK;
			item2.HVI_HVO_OuterPackage = outerPackage2.PK;
			item2.HVI_HVL_LoadList = loadList.PK;

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_RS_NKBookingServiceLevel = "LST";
			bookingHeader3.HVH_OA_BillToParty = billToParty.PK;

			var consignment3 = bookingHeader3.Consignments.AddNew();
			consignment3.HVC_OA_DestinationDepot = destinationDepot1.PK;
			var item3 = consignment3.Items.AddNew();
			var outerPackage3 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage3.HVO_HVL_LoadList = loadList.PK;
			item3.HVI_HVO_OuterPackage = outerPackage3.PK;
			item3.HVI_HVL_LoadList = loadList.PK;

			var subLoadListsActionDisposer = loadList.Split();

			AssertEquals("Should have 2 sub load lists", 2, loadList.SubLoadLists.Count);

			subLoadListsActionDisposer.Dispose();
			AssertEquals("SubLoadList collection should be cleared", 0, loadList.SubLoadLists.Count);

			loadList.HVL_IsMasterHouse = true;
			loadList.Split();

			AssertEquals("Should have 3 sub load lists", 3, loadList.SubLoadLists.Count);
		}

		public void TestGetPortUNLOCOFromDestinationCountry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DMY";

			var destination = Factory.NewWithValidTestData<OrgAddress>();
			destination.OA_Address1 = "dummy";
			destination.OA_OH = org.PK;

			LoadList.HVL_OA_DestinationDepot = destination.PK;

			AssertEquals("Cannot find UNLOCO when the destionation depot org has no HVLV gateway defined", ZString.Empty, LoadList.TryGetPortUNLOCOFromDestinationCountry("AU"));

			var hvlvZone = Factory.NewWithValidTestData<RefZoneHeader>();
			hvlvZone.FZ_ZoneType = ZoneTypeCodes.HVLVGateway;
			hvlvZone.FZ_OH_RelatedParty = org.PK;

			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco1.RL_Code = "AUAAA";
			unloco1.RL_RN_NKCountryCode = "AU";

			Factory.Save();

			AssertEquals("Cannot find UNLOCO when the HVLV Gateway has no UNLOCO defined", ZString.Empty, LoadList.TryGetPortUNLOCOFromDestinationCountry("AU"));

			var zonePivot1 = Factory.NewWithValidTestData<RefZonePivot>();
			zonePivot1.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;
			zonePivot1.F2_ParentID = unloco1.PK;
			zonePivot1.F2_FZ = hvlvZone.PK;

			Factory.Save();

			AssertEquals("Cannot find UNLOCO when the HVLV Gateway for the wrong country", ZString.Empty, LoadList.TryGetPortUNLOCOFromDestinationCountry("NZ"));
			AssertEquals("Can find UNLOCO when the destination depot org has HVLV gateway defined", "AUAAA", LoadList.TryGetPortUNLOCOFromDestinationCountry("AU"));

			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.RL_Code = "AUBBB";
			unloco2.RL_RN_NKCountryCode = "AU";

			var zonePivot2 = Factory.NewWithValidTestData<RefZonePivot>();
			zonePivot2.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;
			zonePivot2.F2_ParentID = unloco2.PK;
			zonePivot2.F2_FZ = hvlvZone.PK;

			Factory.Save();

			AssertEquals("Cannot find unloco when there are more than 1 matches", ZString.Empty, LoadList.TryGetPortUNLOCOFromDestinationCountry("AU"));
		}

		public void TestContainerMode()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();

			loadList.HVL_TransportMode = TransportModes.Sea;
			AssertEquals(ContainerModes.LCL, loadList.CalculateContainerMode());

			loadList.HVL_TransportMode = TransportModes.Rail;
			AssertEquals(ContainerModes.LCL, loadList.CalculateContainerMode());

			loadList.HVL_TransportMode = TransportModes.Road;
			AssertEquals(ContainerModes.LTL, loadList.CalculateContainerMode());

			loadList.HVL_TransportMode = TransportModes.Air;
			AssertEquals(ContainerModes.Loose, loadList.CalculateContainerMode());
			loadList.HVL_ContainerNumber = "AA";
			AssertEquals(ContainerModes.ULD, loadList.CalculateContainerMode());
			loadList.HVL_ContainerNumber = string.Empty;
			loadList.HVL_RC_ContainerType = ZGuid.NewZGuid();
			AssertEquals(ContainerModes.ULD, loadList.CalculateContainerMode());
		}

		public void TestNoAuditLog()
		{
			var newFactoryForLoading = new BusinessObjectFactory();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var query = new ZQuery(StmALogSchema.SL_Parent, loadList.PK);
			loadList.HVL_ContainerNumber = "1";
			Factory.Save();
			Assert("Not expecting Add event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			loadList.HVL_ContainerNumber = "2";
			Factory.Save();
			Assert("Not expecting Edit event.", !newFactoryForLoading.Exists(typeof(StmALog), query));

			loadList.Delete();
			Factory.Save();
			Assert("Not expecting Delete event.", !newFactoryForLoading.Exists(typeof(StmALog), query));
		}

		#region Is Neutral Master

		public void TestIsNeutralMasterReadonlyDisabled()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var mawbStock = Factory.New<JobMawb>();
			mawbStock.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_TransportMode = ContainerModes.AIR;

			AssertEquals(false, loadList.HVL_IsNeutralMaster_ReadOnly);
		}

		public void TestIsNeutralMasterReadonlyEnabled()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var loadList = Factory.New<HVLVOriginLoadList>();

			LoadList.HVL_RL_NKOrigin = string.Empty;
			AssertNull(LoadList.Origin);
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_TransportMode = ContainerModes.AIR;
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			var mawbStock = Factory.New<JobMawb>();
			mawbStock.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			mawbStock.JM_ParentID = Guid.NewGuid();
			mawbStock.JM_ParentTableCode = "JK";
			Factory.Save();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RL_NKOrigin = "CNSHA";
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);

			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_TransportMode = ContainerModes.LCL;
			AssertEquals(true, loadList.HVL_IsNeutralMaster_ReadOnly);
		}

		public void TestHVL_IsNeutralMasterSetDefaultFalse()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_IsNeutralMaster = true;

			loadList.HVL_TransportMode = ContainerModes.LCL;
			AssertEquals(false, loadList.HVL_IsNeutralMaster);

			loadList.HVL_IsNeutralMaster = true;
			loadList.HVL_TransportMode = ContainerModes.AIR;
			loadList.HVL_RL_NKOrigin = string.Empty;
			AssertEquals(false, loadList.HVL_IsNeutralMaster);

			loadList.HVL_IsNeutralMaster = true;
			loadList.HVL_TransportMode = ContainerModes.AIR;
			loadList.HVL_RL_NKOrigin = "CNSHA";
			AssertEquals(false, loadList.HVL_IsNeutralMaster);
		}

		public void TestIsNeutralMasterShouldBeFalseWhenRepenFormWithoutMAWBStock()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var mawbStock = Factory.New<JobMawb>();
			mawbStock.JM_GB = GlbBranch.CurrentBranch.PK;
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_TransportMode = ContainerModes.AIR;
			loadList.HVL_IsNeutralMaster = true;
			Factory.Save();

			AssertEquals(false, loadList.HVL_IsNeutralMaster_ReadOnly);
			AssertEquals(true, loadList.HVL_IsNeutralMaster);

			mawbStock.JM_ParentID = Guid.NewGuid();
			mawbStock.JM_ParentTableCode = "JK";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var reloadedList = factory.Load<HVLVOriginLoadList>(loadList.PK);

			AssertEquals(true, reloadedList.HVL_IsNeutralMaster_ReadOnly);
			AssertEquals(false, reloadedList.HVL_IsNeutralMaster);
		}

		public void TestMasterBillNumberShouldBeResetWhenReadOnly()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var mawbStock = Factory.New<JobMawb>();
			mawbStock.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_MasterBillNumber = "033";
			loadList.HVL_TransportMode = ContainerModes.AIR;

			AssertEquals(false, loadList.HVL_MasterBillNumber_ReadOnly);

			loadList.HVL_IsNeutralMaster = true;

			AssertEquals(true, loadList.HVL_MasterBillNumber_ReadOnly);
			AssertEquals(string.Empty, loadList.HVL_MasterBillNumber);
		}

		#endregion

		HVLVOriginLoadList LoadList
		{
			get { return (HVLVOriginLoadList)BusinessObject; }
		}

		#region IDtbBookingParent Tests

		public void TestIDtbBookingParentMembers()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			var dtbBookingParent = loadList as IDtbBookingParent;

			AssertNotNull("Booking header should implement IDtbBookingParent", dtbBookingParent);

			CombineAssertions("IDtbBookingParent members should be set correctly", () =>
			{
				AssertEquals("Controller ID", ControllerIDs.HVLVOriginLoadList, dtbBookingParent.ControllerID);
				AssertEquals("Job Number", loadList.HVL_UniqueReference, dtbBookingParent.JobNumber);
				AssertEquals("Job Status", "OPN", dtbBookingParent.JobStatus);
				AssertEquals("Job Description", "HVLV Origin Load List", dtbBookingParent.JobDescription);
				AssertContainsExactElementsInAnyOrder(new[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV }, dtbBookingParent.GetSupportedDirections());
			});
		}

		public void TestCanCreateTransportBooking()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			var dtbBookingParent = loadList as IDtbBookingParent;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}
		public void TestBookingParentPK()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			var dtbBookingParent = loadList as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Load List PK.", loadList.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			var dtbBookingParent = loadList as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Load List table prefix.", loadList.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			var dtbBookingParent = loadList as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		[TestedType(typeof(HVLVOriginLoadList))]
		public class HVLVOriginLoadListIDtbBookingParentTestCase : IDtbBookingParentTestCase<HVLVOriginLoadList>
		{
			protected override bool CanHaveDirectCartageChild => false;

			protected override HVLVOriginLoadList GetNewParent() => Factory.New<HVLVOriginLoadList>();

			protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking() => true;
		}

		#endregion

		#region Workflow Tests

		public void TestGetWorkflowInformationProvider()
		{
			AssertNull(((IWorkflowProvider)LoadList).GetWorkflowInformationProvider());
		}

		public void TestWorkflowItems()
		{
			var provider = LoadList as IWorkflowProvider;
			AssertNotNull(provider);
			AssertNotNull(provider.WorkflowItems);
			Assert("IsRegisteredEditableChildObject", LoadList.IsRegisteredEditableChildObject(provider.WorkflowItems));
		}

		public void TestGetTemplateSelectionCriteria()
		{
			AssertNotNull(((IWorkflowProvider)LoadList).GetTemplateSelectionCriteria());
			Assert("is ColumnValueRanker", ((IWorkflowProvider)LoadList).GetTemplateSelectionCriteria() is ColumnValueRanker);
		}

		public void TestWorkflowType()
		{
			Assert(LoadList.GetType().ToString() + " must support Workflow", LoadList.WorkflowType.Equals(WorkflowDescriptors.HVLVOriginLoadListWorkflowDescriptorCode));
		}

		#endregion

		#region WorkflowProviderTest

		[TestedType(typeof(HVLVOriginLoadList))]
		public class HVLVOriginLoadListWorkflowProviderTest : WorkflowProviderTest<HVLVOriginLoadList, HVLVOriginLoadListProcessTaskCollection>
		{
			protected override ZString ExpectedWorkflowType => WorkflowDescriptors.HVLVOriginLoadListWorkflowDescriptorCode;
		}

		#endregion WorkflowProviderTest

		#region HVL_StatusUpdateTests

		public void TestShouldCreateSTULogEntryWhenStatusIsChanged()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			Factory.Save();

			var createdLog = loadList.Logs.GetAllLogs().First(note => ((StmALog)note).SL_SE_NKEvent.Equals(AutoEvents.StatusUpdated.Code)) as StmALog;
			CombineAssertions("Asserting log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, createdLog.SL_SE_NKEvent);
				AssertEquals("|OLD=OPN|NEW=LDG", createdLog.SL_Reference);
			});
		}

		public void TestShouldCreateSTULogEntryForEachChangeWhenStatusIsChanged()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Pending;
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			Factory.Save();

			var createdLogs = loadList.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent.Equals(AutoEvents.StatusUpdated.Code)).OrderBy(log => log.SL_EventTime);
			AssertEquals(2, createdLogs.Count());

			var enumerator = createdLogs.GetEnumerator();
			enumerator.MoveNext();

			CombineAssertions("Asserting first log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, enumerator.Current.SL_SE_NKEvent);
				AssertEquals("|OLD=OPN|NEW=PEN", enumerator.Current.SL_Reference);
			});

			enumerator.MoveNext();
			CombineAssertions("Asserting second log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, enumerator.Current.SL_SE_NKEvent);
				AssertEquals("|OLD=PEN|NEW=LDG", enumerator.Current.SL_Reference);
			});
		}

		public void TestShouldCreateSTULogEntryForEachChangeWhenStatusIsChangedAndReverted()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Pending;
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			Factory.Save();

			var createdLogs = loadList.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent.Equals(AutoEvents.StatusUpdated.Code)).OrderBy(log => log.SL_EventTime);
			AssertEquals(2, createdLogs.Count());

			var enumerator = createdLogs.GetEnumerator();
			enumerator.MoveNext();
			CombineAssertions("Asserting first log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, enumerator.Current.SL_SE_NKEvent);
				AssertEquals("|OLD=OPN|NEW=PEN", enumerator.Current.SL_Reference);
			});

			enumerator.MoveNext();
			CombineAssertions("Asserting second log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, enumerator.Current.SL_SE_NKEvent);
				AssertEquals("|OLD=PEN|NEW=OPN", enumerator.Current.SL_Reference);
			});
		}

		public void TestShouldCreateSTULogEntryWithFailureReasonWhenStatusIsChangedToFailed()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.ReportProcessingErrorAndUpdateStatus("test reason");
			Factory.Save();

			var createdLog = loadList.Logs.GetAllLogs().First(note => ((StmALog)note).SL_SE_NKEvent.Equals(AutoEvents.StatusUpdated.Code)) as StmALog;
			CombineAssertions("Asserting log event code and reference are correct", () =>
			{
				AssertEquals(AutoEvents.StatusUpdated.Code, createdLog.SL_SE_NKEvent);
				AssertEquals("|OLD=OPN|NEW=FAL|RES=test reason", createdLog.SL_Reference);
			});
		}

		public void TestShouldUpdateLoadListItemsStatusWhenStatusIsChangedToFailed()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = loadList.PK;
			item1.HVI_HVO_OuterPackage = outerPackage.PK;
			item1.HVI_HVL_LoadList = loadList.PK;

			item2.HVI_HVL_LoadList = loadList.PK;
			item3.HVI_HVL_LoadList = loadList.PK;

			item1.HVI_Status = "LDG";
			item1.HVI_Status = "LDG";
			item2.HVI_Status = "LDG";

			loadList.ReportProcessingErrorAndUpdateStatus("test reason");
			Factory.Save();

			Assert("Set HVI_Status of Items to be 'LLA'", loadList.Items.Cast<HVLVItem>().All(item => item.HVI_Status == "LLA"));
		}

		public void TestShouldNotCreateLogIfTheValuePassedToTheSetterIsTheCurrentValue()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			Factory.Save();

			var createdLog = loadList.Logs.GetAllLogs().Where(log => log.SL_SE_NKEvent.Equals(AutoEvents.StatusUpdated.Code));
			Assert("Created log collection should be empty", createdLog.IsNullOrEmpty());
		}

		#endregion
	}
}
