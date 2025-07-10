using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsOrderModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var whsOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			whsOrder.UserEditableNoteHelper.EditableNoteText = "Test";
			return whsOrder;
		}
	}
}
