using System.Linq;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LogFinderUtilsTest : TemplateApplicationTestCase
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2019, 3, 3)]
		public void TestOnlyDetectEstimateEventsRaisedByTheRelevantMilestone()
		{
			TestDateAttribute.UseUNLOCO = true;
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone1 = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			var milestone2 = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			Factory.Save();
			milestone1.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			Factory.Save();
			milestone1.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(1);
			Factory.Save();
			milestone2.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			Factory.Save();
			var mp2 = new WorkflowMilestoneProxy(milestone2);
			var definition = new RelatedMilestoneIsEstimateEvent(mp2, Events.CustomisableEvent00Code, originalDate: ZDateTimeOffset.Now.ToDateTimeOffset(), previousDate: mp2.EstimateDate, newDate: ZDateTimeOffset.Now.AddDays(1).ToDateTimeOffset());
			var logs = LogFinderUtils.FindOldEstimatedEvents(milestone2, definition, true, true, true).ToArray();
			AssertEquals("Do not find the log added for Milestone1", 0, logs.Length);

			var mp1 = new WorkflowMilestoneProxy(milestone1);
			definition = new RelatedMilestoneIsEstimateEvent(mp1, Events.CustomisableEvent00Code, originalDate: ZDateTimeOffset.Now.AddDays(1).ToDateTimeOffset(), previousDate: mp1.EstimateDate, newDate: ZDateTimeOffset.Now.AddDays(2).ToDateTimeOffset());
			logs = LogFinderUtils.FindOldEstimatedEvents(milestone1, definition, true, true, true).ToArray();
			AssertEquals("Do not find the log added for Milestone2", 2, logs.Length);
		}
	}
}
