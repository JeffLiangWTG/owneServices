using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgRelatedPartiesModuleFilter))]
	internal class OrgRelatedPartiesModuleFilterTest : ModuleTextFilterTest
	{
		public void TestWithDelegate()
		{
			var callback = new Mock<GetRelatedPartiesQuery>(MockBehavior.Strict);
			ZQuery usedQuery = null;

			callback.Setup(p => p(It.IsAny<ZQuery>())).Returns((ZQuery args) =>
			{
				usedQuery = new ZQuery();
				usedQuery.AddToFilter(args);
				return usedQuery;
			});

			OrgRelatedPartiesModuleFilter filter = new OrgRelatedPartiesModuleFilter("description", callback.Object);
			filter.RelatedParty = ZGuid.NewZGuid();

			ZQuery returnedQuery;

			returnedQuery = filter.Query;

			AssertEquals(usedQuery.LiteralTextSqlFormatted, returnedQuery.LiteralTextSqlFormatted);
			AssertContains(filter.RelatedParty.ToSqlGuid(), returnedQuery.LiteralTextSqlFormatted);
		}

		public void TestFilter_Company()
		{
			GlbCompany comp1 = Factory.New<GlbCompany>();

			RelatedPartyFilter.RelatedParty = party1.PK;
			filteredOrgs.Load(RelatedPartyFilter.Query);

			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			relatedParty1.PR_GC = comp1.PK;
			Factory.Save();

			filteredOrgs.Load(RelatedPartyFilter.Query);

			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			relatedParty1.PR_GC = ZGuid.Empty;
			Factory.Save();

			filteredOrgs.Load(RelatedPartyFilter.Query);

			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);
		}

		public virtual void TestFilter_RelatedParty()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			filteredOrgs.Load(RelatedPartyFilter.Query);

			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
		}

		public virtual void TestFilter_Direction()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);

			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
		}

		public virtual void TestFilter_Mode()
		{
			RelatedPartyFilter.Direction = "";
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyFilter.ContainerMode = Core.Constants.ContainerModes.FCL;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.All;
			RelatedPartyFilter.ContainerMode = ZString.Empty;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyFilter.ContainerMode = Core.Constants.ContainerModes.LCL;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.All;
			RelatedPartyFilter.ContainerMode = ZString.Empty;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);
		}

		public virtual void TestFilter_PartyType()
		{
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.All;
			RelatedPartyFilter.ContainerMode = ZString.Empty;
			AssertEquals("Transport Mode should be ALL", Core.Constants.TransportModes.All, RelatedPartyFilter.TransportMode);
			AssertEquals("Container Mode should be blank", ZString.Empty, RelatedPartyFilter.ContainerMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ForwarderGroup;
			AssertEquals("Transport Mode should be blank", ZString.Empty, RelatedPartyFilter.TransportMode);
			AssertEquals("Container Mode should be blank", ZString.Empty, RelatedPartyFilter.ContainerMode);

			RelatedPartyFilter.Direction = "";
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.TransportMode = "";
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			AssertEquals("Direction should be 'PIC'", "PIC", RelatedPartyFilter.Direction);
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			AssertEquals("Direction should be Empty", ZString.Empty, RelatedPartyFilter.Direction);
		}

		public virtual void TestFilter_All()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyFilter.ContainerMode = Core.Constants.ContainerModes.FCL;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyFilter.ContainerMode = Core.Constants.ContainerModes.LCL;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
		}

		public virtual void TestClearAndIsEmpty()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			RelatedPartyFilter.TransportMode = Core.Constants.TransportModes.Sea;
			RelatedPartyFilter.ContainerMode = Core.Constants.ContainerModes.LCL;
			RelatedPartyFilter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			AssertEquals(false, RelatedPartyFilter.IsEmpty);
			AssertEquals(false, RelatedPartyFilter.Query.IsEmpty);

			RelatedPartyFilter.Clear();
			AssertEquals(true, RelatedPartyFilter.IsEmpty);
			AssertEquals(true, RelatedPartyFilter.Query.IsEmpty);

			AssertEquals("", RelatedPartyFilter.PartyType);
			AssertEquals("", RelatedPartyFilter.TransportMode);
			AssertEquals("", RelatedPartyFilter.ContainerMode);
			AssertEquals("", RelatedPartyFilter.Direction);
			AssertEquals(ZGuid.Empty, RelatedPartyFilter.RelatedParty);
		}

		public virtual void TestLists()
		{
			string expected =
				"DLV - Delivery\r\n" +
				"PIC - Pickup\r\n" +
				"PAD - Pickup and Delivery";
			AssertEquals(expected, RelatedPartyFilter.DirectionList.ElementsAsString);
		}

		public virtual void TestShouldCalculateDirection()
		{
			RelatedPartyFilter.PartyType = "MMM";
			AssertEquals(true, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransportBillTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ReportRevenueTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ReceivingAgent;
			AssertEquals(true, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.SendingAgent;
			AssertEquals(true, RelatedPartyFilter.ShouldCalculateDirection);
		}

		public virtual void TestShouldHaveMode()
		{
			RelatedPartyFilter.PartyType = "MMM";
			AssertEquals(false, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransportBillTo;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ForwarderCFS;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ForwarderLocalTransport;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ReceivingAgent;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.SendingAgent;
			AssertEquals(true, RelatedPartyFilter.ShouldHaveMode);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var filter = new OrgRelatedPartiesModuleFilter("Related Parties");
			var filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.PartyType = "AGS";
			filter.Direction = "DLV";
			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			filter.RelatedParty = org.PK;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (OrgRelatedPartiesModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Party Type", "AGS", loadedFilter.PartyType);
			AssertEquals("Direction", "DLV", loadedFilter.Direction);
			AssertEquals("TransportMode", "SEA", loadedFilter.TransportMode);
			AssertEquals("ContainerMode", "FCL", loadedFilter.ContainerMode);
			AssertEquals("Related Party", org.PK, loadedFilter.RelatedParty);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgRelatedPartiesModuleFilter("Test");
		}

		OrgRelatedPartiesModuleFilter RelatedPartyFilter
		{
			get
			{
				if (fRelatedPartyFilter == null)
				{
					fRelatedPartyFilter = new OrgRelatedPartiesModuleFilter("Test");
				}
				return fRelatedPartyFilter;
			}
		}

		OrgRelatedPartiesModuleFilter fRelatedPartyFilter;

		protected override void SetUp()
		{
			base.SetUp();

			filteredOrgs = new OrgHeaderCollection(Factory);

			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";

			party1 = Factory.New<OrgHeader>();
			party1.OH_Code = "p1";

			party2 = Factory.New<OrgHeader>();
			party2.OH_Code = "p2";

			relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_OH_Parent = org1.PK;
			relatedParty1.PR_OH_RelatedParty = party1.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty1.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty1.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			relatedParty2 = Factory.New<OrgRelatedParty>();
			relatedParty2.PR_OH_Parent = org2.PK;
			relatedParty2.PR_OH_RelatedParty = party2.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			relatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty2.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();
			ResetFilter();
		}

		void ResetFilter()
		{
			RelatedPartyFilter.RelatedParty = ZGuid.Empty;
			RelatedPartyFilter.TransportMode = "";
			RelatedPartyFilter.ContainerMode = "";
			RelatedPartyFilter.Direction = "";
			RelatedPartyFilter.PartyType = "";
		}

		OrgHeaderCollection filteredOrgs;
		OrgHeader org1;
		OrgHeader org2;
		OrgHeader party1;
		OrgHeader party2;
		OrgRelatedParty relatedParty1;
		OrgRelatedParty relatedParty2;

		#endregion
	}
}
