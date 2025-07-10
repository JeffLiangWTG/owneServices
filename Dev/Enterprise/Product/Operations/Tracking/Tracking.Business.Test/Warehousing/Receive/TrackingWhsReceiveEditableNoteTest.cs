using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	sealed class TrackingWhsReceiveEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
		}

		protected override BusinessObject ExpectedNotesParentBO
		{
			get { return ((TrackingWhsReceive)base.ExpectedNotesParentBO).WhsReceive; }
		}
	}
}
