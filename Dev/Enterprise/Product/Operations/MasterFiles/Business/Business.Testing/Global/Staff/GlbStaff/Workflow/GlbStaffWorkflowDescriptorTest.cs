using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffWorkflowDescriptor))]
	sealed class GlbStaffWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbStaffWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(GlbStaffWorkflowDescriptor.WorkflowTypeCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Staff and Resources", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Staff; }
		}

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new[] { BusinessContext.GlbStaff }, WorkflowDescriptor.DocumentBusinessContext);
		}

		public void TestGetMessageRecipientParty_ForStaff()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DW", "Davey Wavey", "davey@wavey.com");
			var parties = WorkflowDescriptor.GetMessageRecipientParty(staff, MessageRecipientPartyTypeList.Codes.Staff).ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "davey@wavey.com" }, parties.Select(x => x.FallbackEmail));
		}

		protected override void SetNotificationEmailAddress(BusinessObject line, ProcessTaskNotification notification, string emailAddress)
		{
			if (notification.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Staff)
			{
				((GlbStaff)notification.Parent.GetParent()).GS_EmailAddress = emailAddress;
			}
			else
			{
				base.SetNotificationEmailAddress(line, notification, emailAddress);
			}
		}

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			GlbStaff glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			return new IWorkflowProvider[] { glbStaff };
		}
		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeStaff;

		#endregion
	}
}
