using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	internal class WhereMacroClause : MacroClause
	{
		public override string Keyword => "Where";

		public override void Validate(string clause, INotifications notifications, WorkflowMacroValidation validation)
		{
			if (clause != null && clause.Trim().Length == 0)
			{
				NotifyError(notifications, WhereClauseErrorMessage, validation.DetailWarningMessage);
				return;
			}

			base.Validate(clause, notifications, validation);
		}

		#region Implementation

		internal static string WhereClauseErrorMessage => Res.GetString("DA4354F6-043C-4088-8BE7-1A796B017D0F", "Where clause must be properly formatted.");

		#endregion
	}
}
