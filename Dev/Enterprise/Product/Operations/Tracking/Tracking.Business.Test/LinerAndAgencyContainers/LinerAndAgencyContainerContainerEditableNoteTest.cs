namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyContainerContainerEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return Factory.New<LinerAndAgencyContainer>();
		}
	}
}
