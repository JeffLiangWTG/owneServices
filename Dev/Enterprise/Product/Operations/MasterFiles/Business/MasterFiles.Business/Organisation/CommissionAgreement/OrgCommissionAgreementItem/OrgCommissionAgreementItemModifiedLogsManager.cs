using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemModifiedLogsManager : CommissionAgreementModifiedLogsManager<OrgCommissionAgreementItem>
	{
		public OrgCommissionAgreementItemModifiedLogsManager(OrgCommissionAgreementItem draft)
			: base(draft)
		{
		}

		protected override void AddLogsOnFactorySavingCore()
		{
			if (MainVersion.IsInDatabase)
			{
				if ((ZBool)OldSource.CAI_IsIncludeInfo.OriginalValue != NewSource.CAI_IsInclude || (ZString)OldSource.CAI_CodeInfo.OriginalValue != NewSource.CAI_Code)
				{
					AddOriginalItemDeletedLog(false);
					AddItemAddedLog();
				}
			}
			else if (MainVersion.CommissionAgreement != null && MainVersion.CommissionAgreement.MainVersion.IsInDatabase)
			{
				AddItemAddedLog();
			}
		}

		void AddItemAddedLog()
		{
			// Should only add logs for items that don't have children (i.e. sub-module items or exclude items)
			if ((NewSource.CAI_Type == OrgCommissionAgreementItemTypes.Codes.SubModule || !NewSource.CAI_IsInclude) && MainVersion.CommissionAgreement != null)
			{
				var message = NewSource.CAI_IsInclude ?
					Res.GetString("2d06e18e-5582-4a04-9d97-216eb6d744e9", "Item Inclusion Added: {0}", NewSource.GetItemPathDisplayText()) :
					Res.GetString("fb95fb8f-b7d3-49cd-932d-5aa8d37e515f", "Item Exclusion Added: {0}", NewSource.GetItemPathDisplayText());

				MainVersion.CommissionAgreement.ModifiedLogs.AddChangeLog(message, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource.CommissionAgreement));
			}
		}

		protected override void AddDetachedLogOnDeleteCore()
		{
			AddOriginalItemDeletedLog(true);
		}

		void AddOriginalItemDeletedLog(bool isOnDeleteLog)
		{
			// Should only add logs for items that don't have children (i.e. sub-module items or exclude items)
			if ((NewSource.CAI_Type == OrgCommissionAgreementItemTypes.Codes.SubModule || !(ZBool)OldSource.CAI_IsIncludeInfo.OriginalValue) && MainVersion.CommissionAgreement != null)
			{
				var message = (ZBool)OldSource.CAI_IsIncludeInfo.OriginalValue ?
					Res.GetString("9a40e47d-7472-4084-b244-67d82a2c6778", "Item Inclusion Deleted: {0}", NewSource.GetItemPathDisplayTextOriginalValue()) :
					Res.GetString("c3ceffc0-2958-47a2-81d0-44fcc46d1d1a", "Item Exclusion Deleted: {0}", NewSource.GetItemPathDisplayTextOriginalValue());

				MainVersion.CommissionAgreement.ModifiedLogs.AddChangeLog(message, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource.CommissionAgreement), isOnDeleteLog);
			}
		}
	}
}
