using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing.Common
{
	[TestedType(typeof(AssemblyMasterAcknowledgementHelper))]
	sealed class AssemblyMasterAcknowledgementHelperTest : TransactionedTestCase
	{
		public void TestShowDialog_ShouldShowAnAcknowledgementMessageBoxIfRegistrySettingIsSetToTrue()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				var shownDialog = ZFormModaliser.LastFormShownDialogForTest as AcknowledgementMessageBox;
				AssertEquals("Ok is clicked", DialogResult.OK, result.dialogResult);
				AssertNotNull("Acknowledgement message box is shown", shownDialog);
			}
		}

		public void TestShowDialog_ShouldShowAUserConfirmationDialogIfRegistrySettingIsSetToFalse()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				var shownMessage = UnitTestUserNotification.Instance.LastConfirmationStringShown;
				AssertEquals("Ok is clicked", DialogResult.OK, result.dialogResult);
				AssertEquals("User confirmation dialog is shown", "I am aware that Assembly Master as Direct Master may breach customs law", shownMessage);
			}
		}

		public void TestShowDialog_WithCheckbox_ReturnsEmptyKeyValuePairsIfUserClicksCancelAndRegistrySettingIsSetToTrue()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				AssertEquals("Cancel is clicked", result.dialogResult, DialogResult.Cancel);
				AssertEquals("Empty dictionary is returned", 0, result.keyValuePairsToLog.Count);
			}
		}

		public void TestShowDialog_WithCheckbox_ReturnsEmptyKeyValuePairsIfUserClicksCancelAndRegistrySettingIsSetToFalse()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				AssertEquals("Cancel is clicked", DialogResult.Cancel, result.dialogResult);
				AssertEquals("Empty dictionary is returned", 0, result.keyValuePairsToLog.Count);
			}
		}

		public void TestShowDialog_ReturnsKeyValuePairsToLogIfUserClicksOkAndRegistrySettingIsSetToTrue()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				AssertEquals("Staff code is logged", "ABC", result.keyValuePairsToLog[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name]);
				AssertEquals("Text to log is correct", "that using Assembly Master as Direct Master might breach customs law by checking the \"I acknowledge\" tick box", result.keyValuePairsToLog[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			}
		}

		public void TestShowDialog_ReturnsKeyValuePairsToLogIfUserClicksOkAndRegistrySettingIsSetToFalse()
		{
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var helper = new AssemblyMasterAcknowledgementHelper();
				var result = helper.ShowDialog(() => "ABC");
				AssertEquals("Staff code is logged", "ABC", result.keyValuePairsToLog[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name]);
				AssertEquals("Text to log is correct", "that using Assembly Master as Direct Master might breach customs law by typing the disclaimer", result.keyValuePairsToLog[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			}
		}
	}
}
