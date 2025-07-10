using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsOrderWebUserVisibleNotesTest : IWebUserVisibleNotesSupportTest
	{
		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			TestHelper helper = new TestHelper(Factory);
			TrackingWhsOrder testOrder = helper.CreateWhsOrder();
			return testOrder;
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
