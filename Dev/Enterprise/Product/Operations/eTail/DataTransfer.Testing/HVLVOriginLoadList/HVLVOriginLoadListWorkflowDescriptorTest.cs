using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOriginLoadListWorkflowDescriptor))]
	public class HVLVOriginLoadListWorkflowDescriptorTest : WorkflowDescriptorTestCase<HVLVOriginLoadListWorkflowDescriptor>
	{
		public override void TestDescription() => AssertEquals("HVLV Origin Load List", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("HVL", WorkflowDescriptor.Code);

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresBranch() => AssertEquals(true, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresClient() => AssertEquals(true, WorkflowDescriptor.RequiresClient);

		public void TestClientName() => AssertEquals("eTailer", WorkflowDescriptor.ClientName);

		public override void TestRequiresDepartment() => AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsWorkflowTemplates() => AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);

		public override void TestSupportsEventTracking() => Assert(WorkflowDescriptor.SupportsEventTracking);

		public void TestSupportsCreateTransportBooking() => AssertEquals(true, WorkflowDescriptor.SupportsCreateTransportBooking);

		#region Implementation

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { OriginLoadListWithOrganisations };
		}

		HVLVOriginLoadList OriginLoadListWithOrganisations => originLoadListWithOrganisations ?? (originLoadListWithOrganisations = CreateOriginLoadListWithOrganisations());
		HVLVOriginLoadList originLoadListWithOrganisations;

		HVLVOriginLoadList CreateOriginLoadListWithOrganisations()
		{
			var originLoadList = Factory.New<HVLVOriginLoadList>();
			originLoadList.HVL_OA_DestinationDepot = ArrivalCFSOrg.Addresses[0].PK;
			originLoadList.HVL_OA_OriginDepot = DepartureCFSOrg.Addresses[0].PK;

			return originLoadList;
		}

		#endregion
	}
}
