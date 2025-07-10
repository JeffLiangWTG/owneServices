using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Common
{
	class AssemblyMasterAcknowledgementHelper
	{
		internal (DialogResult dialogResult, Dictionary<string, string> keyValuePairsToLog) ShowDialog(Func<string> getStaffCode)
		{
			var promptText = Res.GetString("08df12d9-04f4-4b98-b67d-1acb4dcd825e", "I am aware that {0}", FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster);
			var displayMessageBoxWithCheckbox = FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.Value;
			var dialogResult = DialogResult.None;
			if (displayMessageBoxWithCheckbox)
			{
				using (var acknowledgementMessageBox = new AcknowledgementMessageBox(FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster,
					FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster, (MessageBoxButtons)ZMessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, ""))
				{
					dialogResult = ZFormModaliser.ShowDialogWithoutDispose(acknowledgementMessageBox);
				}
			}
			else
			{
				dialogResult = Globals.Message.ShowConfirmation(FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster,
					FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster, promptText, MessageBoxIcon.Warning);
			}

			var keyValuePairsToLog = new Dictionary<string, string>();
			if (dialogResult == DialogResult.OK)
			{
				var acknowledgementMethodStr = displayMessageBoxWithCheckbox ? (NoResString)"by checking the \"I acknowledge\" tick box" : (NoResString)"by typing the disclaimer";
				keyValuePairsToLog.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, getStaffCode());
				keyValuePairsToLog.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, (NoResString)$"that using Assembly Master as Direct Master might breach customs law {acknowledgementMethodStr}");
			}

			return (dialogResult, keyValuePairsToLog);
		}
	}
}
