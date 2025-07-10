using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	sealed class TrackingWhsInwardsModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var whsInwards = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			whsInwards.UserEditableNoteHelper.EditableNoteText = "Test";
			return whsInwards;
		}
	}
}
