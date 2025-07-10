using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;

namespace Enterprise.MasterFiles.GUI
{
	public class ManualDataExportProgressFormSendOnButtonPress : ManualDataExportProgressFormBehaviour
	{
		public ManualDataExportProgressFormSendOnButtonPress(BusinessObjectFactory factory, ManualDataExport dataExport)
		{
			this.dataExport = dataExport;
			this.factory = factory;
		}

		readonly ManualDataExport dataExport;
		readonly BusinessObjectFactory factory;

		protected override void ApplyCore(IManualDataExportProgressForm progressForm)
		{
			progressForm.Form.Shown += (s, e) => progressForm.SendButton.Focus();

			progressForm.SendButton.Click += (s, e) =>
			{
				progressForm.SendButton.Enabled = false;
				using (factory.AddDisposableService())
				{
					dataExport.SendData(this);
					try
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, () => { }, true, true);
					}
					catch (ZSaveException ex)
					{
						this.AddMessageError(Res.GetString("dd554eb9-521d-4a97-bd69-ed2ec811e2fa", "Export has failed due to an error during saving. The error message was \"{0}\". Please try again.", ex.FriendlyMessage));
					}
				}

				progressForm.CloseButton.Focus();
			};
		}
	}
}
