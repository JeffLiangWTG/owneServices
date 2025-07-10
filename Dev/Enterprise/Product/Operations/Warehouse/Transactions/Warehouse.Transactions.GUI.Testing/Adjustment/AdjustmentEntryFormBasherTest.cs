using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(AdjustmentEntryForm))]
	class AdjustmentEntryFormBasherTest : ZFormBasherTest
	{
		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var adjustment = Factory.NewWithValidTestData<WhsAdjustment>();
			adjustment.WD_BookingDate = ZDateTimeOffset.Now;
			var helper = new NotificationSubscriberGuiHelper();
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new AdjustmentEntryForm(adjustment, helper));
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var helper = new NotificationSubscriberGuiHelper();
			AdjustmentEntryForm result = new AdjustmentEntryForm(Factory.New<WhsAdjustment>(), helper);
			result.ControllerID = ControllerIDs.WhsAdjustment;
			return result;
		}

		#endregion
	}
}
