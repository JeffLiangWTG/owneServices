using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	sealed class TrackingWhsReceiveWebUserVisibleNotesTest : IWebUserVisibleNotesSupportTest
	{
		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			TestHelper helper = new TestHelper(Factory);
			TrackingWhsReceive testReceive = helper.CreateWhsReceive();
			return testReceive;
		}

		protected override void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility)
		{
		}

		protected override BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent)
		{
			return null;
		}
	}
}
