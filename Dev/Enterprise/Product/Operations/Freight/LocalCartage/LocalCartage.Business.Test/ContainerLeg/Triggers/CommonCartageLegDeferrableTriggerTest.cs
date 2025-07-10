using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageLeg))]
	class CommonCartageLegDeferrableTriggerTest : DeferrableTriggerTestCase<CommonCartageLeg>
	{
		public void TestDeferredTrigger_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob()
		{
			var factory = Factory;
			var cartageJob = factory.NewWithValidTestData<CommonCartage>();
			var bookedMove = factory.New<CommonBookedCtgMove>();
			bookedMove.EW_JJ = cartageJob.PK;
			bookedMove.FillWithValidTestData();
			var originalCartageLeg = factory.New<CommonCartageLeg>();
			originalCartageLeg.JU_EW = bookedMove.PK;
			originalCartageLeg.JU_SplitDeliverySuffix = "A";
			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var duplicateCartageLeg = factory2.New<CommonCartageLeg>();
			duplicateCartageLeg.JU_EW = bookedMove.PK;
			duplicateCartageLeg.JU_SplitDeliverySuffix = "A";

			AssertInnermostException("trigger should prevent this - duplicateCartageLeg duplicates originalCartageLeg.JU_SplitDeliverySuffix 'A'",
				typeof(ZConcurrencyCheckFailureException),
				"TriggerLikelyConcurrencyError: Attempted to insert duplicate suffix on cartage job leg(s).",
				() => factory2.Save());
		}
	}
}
