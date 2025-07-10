using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;

namespace Enterprise.MasterFiles.GUI
{
	public class ManualDataExportProgressFormSendOnShow : ManualDataExportProgressFormBehaviour
	{
		public ManualDataExportProgressFormSendOnShow(ManualDataExport dataExport)
		{
			this.dataExport = dataExport;
		}

		readonly ManualDataExport dataExport;

		protected override void ApplyCore(IManualDataExportProgressForm progressForm)
		{
			progressForm.SendButton.Visible = false;
			progressForm.CloseButton.Enabled = false;

			progressForm.Form.Shown += (s, e) =>
			{
				using (dataExport.Factory.AddDisposableService())
				{
					dataExport.SendData(this);

					try
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(dataExport.Factory.Save, () => { }, true, true);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						this.AddError(Res.GetString("61c2fde2-58af-4127-9742-101f0a47fe62", "Sending failed due to an error. Try sending again."));
						this.AddError(Res.GetString("61c2fde2-58af-4127-9742-101f0a47fe64", "The error was: {0}", ex));
					}

					progressForm.CloseButton.Enabled = true;
					progressForm.CloseButton.Focus();
				}
			};
		}
	}
}
