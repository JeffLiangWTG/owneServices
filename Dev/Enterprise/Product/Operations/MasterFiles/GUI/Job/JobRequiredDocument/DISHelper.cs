using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class DISHelper
	{
		public static void DISButtonClick(ZForm mainForm, IDISHost disHost, Action<bool> showDISForm)
		{
			if (mainForm == null)
			{ return; }

			var msg = Res.GetString("76b87196-40f5-4f11-8715-ca7abae0890e", "There are changes in the main form. Do you want the system to save these changes and proceed?");
			var caption = Res.GetString("bdd2b6cc-f1b4-47d6-aa2d-9f58a0157417", "Continue With Save");
			if (mainForm.BusinessEntity.HasChanges && Globals.Message.Show(msg, caption, MessageBoxButtons.OKCancel, DialogResult.OK) != DialogResult.OK)
			{
				return;
			}

			if (mainForm.BusinessEntity.HasChanges && mainForm.FireSaveButton() != ContinueWithSave.Yes)
			{
				return;
			}

			if (disHost == null)
			{
				Globals.Message.ShowError(Res.GetString("c6cfcab4-919e-48e8-a275-8d2783850a87", "This is not supported on this form."));
				return;
			}

			var dataSourceAsIHaveRequiredDocuments = disHost.RequiredDocumentsProvider;
			if (dataSourceAsIHaveRequiredDocuments != null && dataSourceAsIHaveRequiredDocuments.RequiredDocuments.Count == 0)
			{
				Globals.Message.ShowInformation(
					Res.GetString(
						"ADD6B2B1-7316-495D-9F40-FE492BF01C2E",
						"There are no Tracking documents entered in the grid 'Document Tracking'. DIS documents will need to be linked to a document entered in the grid."));
				return;
			}

			if (disHost.ErrorMessages.Any())
			{
				var errors = new ZStringBuilder();
				foreach (var error in disHost.ErrorMessages)
				{
					errors.Append(error + "\r\n");
				}

				Globals.Message.ShowError(errors.ToString());
				return;
			}

			// there is no need to check number fountain for read-only mode
			var isEditAllowed = disHost.DISEditable;
			if (isEditAllowed)
			{
				var numberFountainStrategy = disHost.DISReferenceNumberFountainStrategy;
				var state = numberFountainStrategy?.CheckState();

				if (state?.Severity == Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Error)
				{
					Globals.Message.ShowError(state.Message);
					return;
				}

				if (state?.Severity == Enterprise.Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Warning)
				{
					Globals.Message.ShowInformation(state.Message);
				}
			}

			var runner = DISPreFormActionRegistrar.GetDISPreFormActionRunner(disHost);
			if (runner == null || runner.Execute())
			{
				if (mainForm.BusinessEntity.HasChanges && mainForm.FireSaveButton() != ContinueWithSave.Yes)
				{
					return;
				}

				showDISForm(isEditAllowed);
			}
		}
	}
}
