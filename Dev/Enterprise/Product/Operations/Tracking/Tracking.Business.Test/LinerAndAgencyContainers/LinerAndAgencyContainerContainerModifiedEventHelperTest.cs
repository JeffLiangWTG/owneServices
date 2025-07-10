using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyContainerContainerModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();

			container.UserEditableNoteHelper.EditableNoteText = "Test";

			return container;
		}
	}
}
