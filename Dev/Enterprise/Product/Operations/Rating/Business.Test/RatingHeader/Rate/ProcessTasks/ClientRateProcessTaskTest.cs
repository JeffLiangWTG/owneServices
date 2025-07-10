using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business
{
	[TestedType(typeof(ClientRateProcessTask))]
	public class ClientRateProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			var clientRate = Factory.New<ClientRate>();
			var milestone = clientRate.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(ClientRate), milestone.Parent.GetType());
		}

		public void TestP9_ParentID()
		{
			var clientRate = Factory.New<ClientRate>();
			var processTask = clientRate.WorkflowItems.AddNew();
			AssertNull(processTask.Organisation);

			clientRate.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			processTask.P9_ParentID = clientRate.PK;
			AssertNotNull(processTask.Organisation);
		}

		public void TestClientRateProcessTaskValidation()
		{
			var clientRate = Factory.New<ClientRate>();
			var task = clientRate.WorkflowItems.Tasks.AddNew();
			Assert(task.Validation is RatingHeaderProcessTaskValidation);

			var milestone = clientRate.WorkflowItems.Milestones.AddNew();
			Assert(milestone.Validation is MilestoneOrTriggerValidation);

			var trigger = clientRate.WorkflowItems.Triggers.AddNew();
			Assert(trigger.Validation is MilestoneOrTriggerValidation);

			var exception = clientRate.WorkflowItems.Exceptions.AddNew();
			Assert(exception.Validation is ExceptionValidation);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (ClientRateProcessTask)GetNewBusinessObject();
			result.Parent.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var clientRate = Factory.New<ClientRate>();
			return clientRate.WorkflowItems.AddNew();
		}
	}
}
