using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert))]
	class JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsertTest : TestCaseWithFactory
	{
		public void TestShouldDeferTriggerWhenInserting()
		{
			var leg = CreateCartageWithLeg();

			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IJobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert));
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(leg));
		}

		public void TestShouldNotDeferTriggerWhenUpdating()
		{
			var insertedLeg = CreateCartageWithLeg();
			Factory.Save();

			var updatedLeg = Factory.Load<CommonCartageLeg>(insertedLeg.PK);
			updatedLeg.JU_DisplayOrder = updatedLeg.JU_DisplayOrder + 1;
			AssertEquals("Cartage leg has changes", true, updatedLeg.HasChanges);

			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IJobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert));
			AssertEquals("Trigger should not be deferred for an update, as trigger does not happen on update anyway.", false, strategy.ShouldDeferTrigger(updatedLeg));
		}

		CommonCartageLeg CreateCartageWithLeg()
		{
			var cartageJob = Factory.NewWithValidTestData<CommonCartage>();

			var bookedMove = Factory.New<CommonBookedCtgMove>();
			bookedMove.EW_JJ = cartageJob.PK;
			bookedMove.FillWithValidTestData();

			var cartageLeg = Factory.New<CommonCartageLeg>();
			cartageLeg.JU_EW = bookedMove.PK;
			cartageLeg.JU_SplitDeliverySuffix = "A";
			cartageLeg.FillWithValidTestData();

			return cartageLeg;
		}
	}
}
