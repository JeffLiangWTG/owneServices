namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return Factory.New<TrackingOrder>();
		}
	}
}
