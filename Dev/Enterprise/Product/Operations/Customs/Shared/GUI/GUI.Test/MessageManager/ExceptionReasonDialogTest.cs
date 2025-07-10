using System.Windows.Forms;
using Enterprise.Customs.Common.Shared;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ExceptionReasonDialog))]
	sealed class ExceptionReasonDialogTest : ZFormBasherTest
	{
		public void TestExceptionErrorsAreReported()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var overdueCargoReportException = (ForwardingShipmentProcessTask)shipment1.WorkflowItems.Exceptions.AddNew();
			overdueCargoReportException.TriggerConditions.TriggerEventCode = Events.CargoReportAcceptedCode;

			DialogResult result = DialogResult.None;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var reasonDlg = (ExceptionReasonDialog)obj;
				reasonDlg.YesButtonX.PerformClick();
				result = reasonDlg.DialogResult;
			});

			ZFormModaliser.ShowDialogsInTest = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogAndDispose(new ExceptionReasonDialog(overdueCargoReportException));

			AssertContains("There are errors", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(DialogResult.None, result);

			overdueCargoReportException.LateCargoReportReason = LateCargoReportingReasons.Codes.MISCReferNotes;
			overdueCargoReportException.LateCargoReportText = "Some Reason";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ShowDialogAndDispose(new ExceptionReasonDialog(overdueCargoReportException));

			AssertEquals(DialogResult.OK, result);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override Form GetFormToBashCore() => new ExceptionReasonDialog(Factory.New<ForwardingShipmentProcessTask>());
	}
}
