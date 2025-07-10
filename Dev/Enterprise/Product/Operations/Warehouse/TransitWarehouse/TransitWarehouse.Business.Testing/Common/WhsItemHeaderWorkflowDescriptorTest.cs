using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	abstract class WhsItemHeaderWorkflowDescriptorTest<Descriptor, Header> : WorkflowDescriptorTestCase<Descriptor>
		where Descriptor : WorkflowDescriptor, new()
		where Header : BusinessObject, IItemHeader, IDocAddresses
	{
		#region TestGetMessageRecipientParty

		public void TestGetMessageRecipientParty()
		{
			var header = Factory.New<Header>();
			var descriptor = new Descriptor();
			AssertEquals("Since BizO is null must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.TransportCo).Count());
			AssertEquals("Since header doesn't have a transport company must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(header, MessageRecipientPartyTypeList.Codes.TransportCo).Count());

			header.TransportCompany.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertEquals(header.TransportCompany.Organisation, descriptor.GetMessageRecipientParty(header, MessageRecipientPartyTypeList.Codes.TransportCo).Single().Party);
		}

		public void TestGateManagementGetRecipientParty()
		{
			var header = Factory.NewWithValidTestData<Header>();
			var descriptor = new Descriptor();

			AssertEquals(
				"Gate organisation should fallback to same organisation as the warehouse",
				header.Warehouse.WarehouseAddress.Header,
				descriptor.GetMessageRecipientParty(header, MessageRecipientPartyTypeList.Codes.GateManagement).Single().Party);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Warehouse |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.TransportCo |
					MessageRecipientPartyType.CustomsOutturnAgent |
					MessageRecipientPartyType.Forwarder |
					MessageRecipientPartyType.GateManagement |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var helper = new WhsTransitTestHelper(Factory);

			var header = (Header)workflowProvider;
			var transportOrg = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptorType);
			LoadOrCreateJobDocAddressFromAddress(header, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportOrg.MainAddress);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptorType).MainAddress.PK;
			header[WarehouseColumnName] = warehouse.PK;
		}

		void LoadOrCreateJobDocAddressFromAddress(Header header, string type, OrgAddress address)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, header.PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, type);
			query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);

			var jobDocAddressCount = Factory.Load<JobDocAddress>(query).Length;
			if (jobDocAddressCount == 0)
			{
				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = address.PK;
				jobDocAddress.E2_AddressType = type;
				jobDocAddress.E2_ParentID = header.PK;
				jobDocAddress.E2_ParentTableCode = header.TablePrefix;
			}
		}

		protected abstract string WorkflowDescriptorType { get; }
		protected abstract SchemaGuidColumn WarehouseColumnName { get; }

		#endregion
	}
}
