using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingContainerModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var container = Factory.NewWithValidTestData<TrackingContainer>();

			container.UserEditableNoteHelper.EditableNoteText = "Test";

			return container;
		}
	}
}
