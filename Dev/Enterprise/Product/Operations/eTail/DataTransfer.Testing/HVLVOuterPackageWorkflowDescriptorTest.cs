using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOuterPackageWorkflowDescriptor))]
	public class HVLVOuterPackageWorkflowDescriptorTest : WorkflowDescriptorTestCase<HVLVOuterPackageWorkflowDescriptor>
	{
		public override void TestDescription() => AssertEquals("HVLV Outer Package", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("HVO", WorkflowDescriptor.Code);

		public override void TestSubTypes() => AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

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

		public void TestSupportsCreateTransportBooking() => AssertEquals(false, WorkflowDescriptor.SupportsCreateTransportBooking);

		#region Implementation

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { OuterPackageWithOrganisations };

		HVLVOuterPackage OuterPackageWithOrganisations => outerPackageWithOrganisations ?? (outerPackageWithOrganisations = CreateOuterPackageWithOrganisations());
		HVLVOuterPackage outerPackageWithOrganisations;

		HVLVOuterPackage CreateOuterPackageWithOrganisations()
		{
			var outerPackage = Factory.New<HVLVOuterPackage>();
			outerPackage.HVO_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			outerPackage.HVO_OH_LastMileCarrier = Factory.NewWithValidTestData<OrgHeader>().PK;

			return outerPackage;
		}

		#endregion
	}
}
