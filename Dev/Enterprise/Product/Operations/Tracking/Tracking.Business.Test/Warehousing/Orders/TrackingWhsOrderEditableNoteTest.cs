using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsOrderEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
		}

		protected override BusinessObject ExpectedNotesParentBO
		{
			get { return ((TrackingWhsOrder)base.ExpectedNotesParentBO).WhsOrder; }
		}
	}
}
