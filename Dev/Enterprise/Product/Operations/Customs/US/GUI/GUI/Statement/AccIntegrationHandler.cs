using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class AccIntegrationHandler
	{
		public void PerformIntegration(CusStatementHeader statement, ZForm parentForm)
		{
			if (CheckPreRequisiteConditions(statement, parentForm))
			{
				AutoBillingResult result = statement.PerformAccIntegration();

				if (result.HasChanges || result.WasSuccessful)
				{
					if (result.HasChanges)
					{
						statement.HasChanges = true;//Save button enabled
					}

					string message = result.HasChanges ? FinishedWithActionsTakenReviewEntry : NoActionTaken;

					if (result.Message.Length > 0)
					{
						message += @"

However there are following warning messages. Please review them.
" + result.Message;
					}

					Globals.Message.ShowInformation(message, "Accounting Integration");
				}
				else if (!result.WasSuccessful)
				{
					using (var form = new HtmlInterpretationForm(ErrorExist + result.Message))
					{
						ZFormModaliser.ShowDialogAndDispose(form, parentForm);
					}
				}
			}
		}

		bool CheckPreRequisiteConditions(CusStatementHeader statement, ZForm parentForm)
		{
			bool result = false;

			if (!Env.Security.USCustomsImportStatementAccIntegrationModify.IsAllowed)
			{
				Env.Security.USCustomsImportStatementAccIntegrationModify.ShowError();
			}
			else
			{
				if (!statement.HasChanges || QueryUser(SaveFirst))
				{
					result = true;

					if (statement.HasChanges)
					{
						result = parentForm.FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes;
					}
				}

				if (result)
				{
					string errors = statement.GetErrorNotificationsBeforePerformingAccIntegration();

					result = string.IsNullOrEmpty(errors);
					if (!string.IsNullOrEmpty(errors))
					{
						Globals.Message.ShowError(errors, "Accounting Integration");
					}
					else
					{
						string warning = statement.GetWarningNotificationsBeforePerformingAccIntegration();

						result = string.IsNullOrEmpty(warning) || QueryUser(warning + "\r\nAre you sure you wish to continue?");
					}
				}
			}

			return result;
		}

		bool QueryUser(string message)
		{
			return Globals.Message.Show(message, "Accounting Integration", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.DialogResult.Cancel) == System.Windows.Forms.DialogResult.OK;
		}

		public const string ErrorExist = "<b>The requested actions were not executed. Please review and correct the errors, then try again.</b><br />";
		public const string SaveFirst = "There are changes on the form. If you proceed, system will perform saving the changes for you. Are you sure you wish to continue?";

		public const string FinishedWithActionsTakenReviewEntry = "Accounting Integration is finished. Please review each of line above. If result is satisfactory, please save the result.";

		public const string NoActionTaken = "No actions needed to be taken for the selected options.";
	}
}
