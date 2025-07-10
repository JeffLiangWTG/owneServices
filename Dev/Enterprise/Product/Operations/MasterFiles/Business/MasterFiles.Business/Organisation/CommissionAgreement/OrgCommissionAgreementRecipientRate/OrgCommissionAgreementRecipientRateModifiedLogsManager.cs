namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientRateModifiedLogsManager : CommissionAgreementModifiedLogsManager<OrgCommissionAgreementRecipientRate>
	{
		public OrgCommissionAgreementRecipientRateModifiedLogsManager(OrgCommissionAgreementRecipientRate draft)
			: base(draft)
		{
		}

		protected override void AddLogsOnFactorySavingCore()
		{
			var commissionAgreement = NewSource.CommissionAgreement;
			if (MainVersion.IsInDatabase)
			{
				AddLogIfChanged(x => x.CAT_CommissionPeriodInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAT_CommissionAmountInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAT_RX_NKCommissionCurrencyInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAT_CommissionPeriodInfo, AutoApprovalHelper.Always);
				AddStartDateModifiedLog();
				AddEndDateModifiedLog();
			}
			else if (MainVersion.CommissionAgreementRecipient != null && MainVersion.CommissionAgreementRecipient.GetMainVersion().IsInDatabase)
			{
				MainVersion.CommissionAgreementRecipient.ModifiedLogs.AddChildObjectAttachedLog(NewSource);
			}
		}

		void AddStartDateModifiedLog()
		{
			if (NewSource.CAT_CommissionPeriodInfo.HasChanges || NewSource.CAT_CommissionStartDateOverrideInfo.HasChanges)
			{
				var originalValue = NewSource.GetCAT_CommissionStartDateOriginalValue();
				var newValue = NewSource.CAT_CommissionStartDate;

				if (originalValue != newValue)
				{
					AddChangeLog(NewSource.CAT_CommissionStartDateInfo.HumanReadableName, originalValue.ToShortDateString(), newValue.ToShortDateString(), AutoApprovalHelper.OnlyIfNoDatesInThePast(originalValue, newValue));
				}
			}
		}

		void AddEndDateModifiedLog()
		{
			if (NewSource.CAT_CommissionPeriodInfo.HasChanges || NewSource.CAT_CommissionEndDateOverrideInfo.HasChanges)
			{
				var originalValue = NewSource.GetCAT_CommissionEndDateOriginalValue();
				var newValue = NewSource.CAT_CommissionEndDate;

				if (originalValue != newValue)
				{
					AddChangeLog(NewSource.CAT_CommissionEndDateInfo.HumanReadableName, originalValue.ToShortDateString(), newValue.ToShortDateString(), AutoApprovalHelper.OnlyIfNoDatesInThePast(originalValue, newValue));
				}
			}
		}

		protected override void AddDetachedLogOnDeleteCore()
		{
			MainVersion.CommissionAgreementRecipient.ModifiedLogs.AddChildObjectDetachedLog(NewSource);
		}
	}
}
