using Enterprise.MailManager;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HREmailsFilterBusinessObject))]
	public class HREmailsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HREmailsFilterBusinessObject();
		}

		public void TestStatuses()
		{
			var filter = new HREmailsFilterBusinessObject();
			var list = new CodeDescriptionPairList();
			list.AddPair(MailStatus.Processed, "Processed");
			list.AddPair(MailStatus.Unprocessed, "Unprocessed");
			list.AddPair(MailStatus.MarkedForReprocessing, "Marked For Reprocessing");
			list.AddPair(MailStatus.Failed, "Failed");
			AssertContainsExactElementsInAnyOrder(list, filter.MI_StatusList);
		}
	}
}
