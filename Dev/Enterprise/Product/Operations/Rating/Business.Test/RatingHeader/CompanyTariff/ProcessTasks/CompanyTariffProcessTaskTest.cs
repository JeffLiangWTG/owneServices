using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CompanyTariffProcessTask))]
	public class CompanyTariffProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var milestone = companyTariff.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(CompanyTariff), milestone.Parent.GetType());
		}

		public void TestP9_ParentID()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var processTask = companyTariff.WorkflowItems.AddNew();
			AssertNull(processTask.Organisation);

			companyTariff.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			processTask.P9_ParentID = companyTariff.PK;
			AssertNotNull(processTask.Organisation);
		}

		public void TestClientRateProcessTaskValidation()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var task = companyTariff.WorkflowItems.Tasks.AddNew();
			Assert(task.Validation is RatingHeaderProcessTaskValidation);

			var milestone = companyTariff.WorkflowItems.Milestones.AddNew();
			Assert(milestone.Validation is MilestoneOrTriggerValidation);

			var trigger = companyTariff.WorkflowItems.Triggers.AddNew();
			Assert(trigger.Validation is MilestoneOrTriggerValidation);

			var exception = companyTariff.WorkflowItems.Exceptions.AddNew();
			Assert(exception.Validation is ExceptionValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			return companyTariff.WorkflowItems.AddNew();
		}
	}
}
