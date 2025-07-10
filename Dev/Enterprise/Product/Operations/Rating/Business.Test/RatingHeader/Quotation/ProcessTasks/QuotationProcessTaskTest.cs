using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business
{
	[TestedType(typeof(QuotationProcessTask))]
	public class QuotationProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			var quote = Factory.New<Quote>();
			var milestone = quote.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(Quote), milestone.Parent.GetType());
		}

		public void TestP9_ParentID()
		{
			var quote = Factory.New<Quote>();
			var processTask = quote.WorkflowItems.AddNew();
			AssertNull(processTask.Organisation);

			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			processTask.P9_ParentID = quote.PK;
			AssertNotNull(processTask.Organisation);
		}

		public void TestIQuotationProcessTask()
		{
			var quote = Factory.New<Quote>();
			var milestone = quote.WorkflowItems.Milestones.AddNew();
			Assert(milestone is IQuotationProcessTask);
		}

		public void TestQuotationProcessTaskValidation()
		{
			var quote = Factory.New<Quote>();
			var task = quote.WorkflowItems.Tasks.AddNew();
			Assert(task.Validation is RatingHeaderProcessTaskValidation);

			var milestone = quote.WorkflowItems.Milestones.AddNew();
			Assert(milestone.Validation is MilestoneOrTriggerValidation);

			var trigger = quote.WorkflowItems.Triggers.AddNew();
			Assert(trigger.Validation is MilestoneOrTriggerValidation);

			var exception = quote.WorkflowItems.Exceptions.AddNew();
			Assert(exception.Validation is ExceptionValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = Factory.New<Quote>();
			return quote.WorkflowItems.AddNew();
		}
	}
}
