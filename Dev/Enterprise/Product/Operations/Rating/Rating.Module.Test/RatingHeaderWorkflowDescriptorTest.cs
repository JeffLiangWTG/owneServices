using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module.Testing
{
	public abstract class RatingHeaderWorkflowDescriptorTest<T, D> : WorkflowDescriptorTestCase<D>
		where T : RatingHeader
		where D : WorkflowDescriptor, new()
	{
		#region Requires

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region Workflow Trigger

		public override void TestIsMessagingOrEmailNotificationTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.SendXML));
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail));
		}

		#endregion

		#region Implementation

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return true; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new[] { (IWorkflowProvider)RatingHeaderWithParties };
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return (IWorkflowProvider)RatingHeaderWithParties;
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Client |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
			}
		}

		protected T RatingHeaderWithParties
		{
			get { return ratingHeaderWithParties ?? (ratingHeaderWithParties = NewRatingHeaderWithParties()); }
		}
		T ratingHeaderWithParties;

		protected virtual T NewRatingHeaderWithParties()
		{
			var ratingHeader = Factory.New<T>();
			ratingHeader.TH_OH = ClientOrg.PK;

			return ratingHeader;
		}

		#endregion
	}
}
