using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public static class ManualRegistrationHelper
	{
		public static void ManualRegistrationNoEntry(IRegistrationNoEntryProvider provider, ZForm parentForm)
		{
			if (parentForm != null && Enterprise.Customs.GUI.SaveDataFirst.Confirm(provider.ParentBusinessObject, parentForm))
			{
				if (provider.CanModifyRegistrationNumbers.IsAllowed)
				{
					var response = Globals.Message.Show(Res.GetString("266A5CD8-0FC7-475B-A766-E6C45B57EA2A", "Do you want to enter Registration No manually?"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, DialogResult.No);
					if (response == DialogResult.Yes)
					{
						var manualRegistrationNoEntry = new ManualRegistrationNoEntry(provider.RegistrationNumber, provider.RegistrationDate);
						using (var form = new ManualRegistrationNoEntryForm(manualRegistrationNoEntry))
						{
							ZFormModaliser.ShowDialogAndDispose(form);
							if (form.DialogResult == DialogResult.OK)
							{
								provider.RegistrationNumber = manualRegistrationNoEntry.RegistrationNumber;
								provider.RegistrationDate = manualRegistrationNoEntry.RegistrationDate;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
								provider.ParentBusinessObject.GetLogs().AddNew(AutoEvents.EditedARecord, "Manual Change Registration No");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
								parentForm.FireSaveButton();
							}
						}
					}
				}
				else
				{
					Env.Security.ShowError(provider.CanModifyRegistrationNumbers);
				}
			}
		}
	}
}
