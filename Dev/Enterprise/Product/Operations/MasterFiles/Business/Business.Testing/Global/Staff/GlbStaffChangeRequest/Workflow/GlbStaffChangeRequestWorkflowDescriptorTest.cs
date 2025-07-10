using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestWorkflowDescriptor))]
	class GlbStaffChangeRequestWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbStaffChangeRequestWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Change Request", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequestTemplate>();
			changeRequest.GSG_Code = "TCODE";
			changeRequest.GSG_TemplateName = "TCODE Name";
			Factory.Save();
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Change Request Templates", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("TCODE", ((ICodeDescription)WorkflowDescriptor.SubTypeInformation[0].List[0]).Code);
			AssertEquals("TCODE Name", ((ICodeDescription)WorkflowDescriptor.SubTypeInformation[0].List[0]).Description);
		}

		public override void TestSupportsEventTracking() => AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var staffChangeRequest = Factory.NewWithValidTestData<GlbStaffChangeRequest>();
			return new IWorkflowProvider[] { staffChangeRequest };
		}

		public void TestSupportsBufferManagement() => AssertEquals(true, WorkflowDescriptor.SupportsBufferManagement);

		[ExpectNoExceptions]
		public void TestDocManagerSupportImplementation()
		{
			var changeRequest = Factory.New<GlbStaffChangeRequest>();
			AssertNotNull(changeRequest.DocManagerInfo);
			AssertEquals(changeRequest.DocManagerInfo.DocManagerCode, Core.Constants.DocManagerCodes.ChangeRequest);
		}
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;
	}
}
