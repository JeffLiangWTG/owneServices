using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public static class SaveDataFirst
	{
		public static bool Confirm(BusinessObject parent, ZForm mainForm, string caption = null, string message = null)
		{
			bool result = true;

			if (parent.HasChanges)
			{
				if (Globals.Message.Show(message ?? Res.GetString("739328BE-CBB1-4D83-8C3A-72931DC85811", "You need to save first. Would you like to save now and proceed?")
					, caption ?? Res.GetString("B8763486-2B14-4943-B5DF-78DFF1A7179B", "Save first?")
					, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					result = mainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					result = false;
				}
			}

			return result;
		}
	}
}
