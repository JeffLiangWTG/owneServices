namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingContainerEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return Factory.New<TrackingContainer>();
		}
	}
}
