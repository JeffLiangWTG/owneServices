using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var order = Factory.NewWithValidTestData<TrackingOrder>();
			order.UserEditableNoteHelper.EditableNoteText = "Test";
			return order;
		}
	}
}
