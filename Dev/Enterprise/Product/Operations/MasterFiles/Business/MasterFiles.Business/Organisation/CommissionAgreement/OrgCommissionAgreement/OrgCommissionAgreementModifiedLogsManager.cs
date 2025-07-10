using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementModifiedLogsManager : CommissionAgreementModifiedLogsManager<OrgCommissionAgreement>
	{
		public OrgCommissionAgreementModifiedLogsManager(OrgCommissionAgreement draft)
			: base(draft)
		{
		}

		protected override void AddLogsOnFactorySavingCore()
		{
			if (MainVersion.IsInDatabase)
			{
				AddLogIfChanged(x => x.CA0_NameInfo, AutoApprovalHelper.Always);
				AddLogIfChanged(x => x.CA0_OH_CustomerInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource), (pk) =>
				{
					var customer = Factory.Load<OrgHeader>((ZGuid)pk);
					return customer != null ? customer.OH_Code : ZString.Empty;
				});

				AddLogIfChanged(x => x.CA0_CommissionTriggerTypeInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource));
				AddLogIfEffectiveDateChanged();
				AddLogIfChanged(x => x.CA0_CommissionBasisInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(NewSource));
				AddLogIfChanged(x => x.CA0_ExpiredDateInfo, AutoApprovalHelper.OnlyIfNoDatesInThePast(NewSource.CA0_ExpiredDate, ((ZDate)NewSource.CA0_ExpiredDateInfo.OriginalValue)));
				AddLogIfReversed();
			}
		}

		void AddLogIfEffectiveDateChanged()
		{
			if (OldSource != NewSource || NewSource.CA0_CommissionTriggerTypeInfo.HasChanges || NewSource.CA0_EffectiveDateInfo.HasChanges)
			{
				var originalValue = OldSource.GetEffectiveDateOriginalValue();
				var newValue = NewSource.EffectiveDate;

				if (originalValue != newValue)
				{
					AddChangeLog(NewSource.EffectiveDateInfo.HumanReadableName, originalValue.ToShortDateString(), newValue.ToShortDateString(), AutoApprovalHelper.OnlyIfNoDatesInThePast(originalValue, newValue));
				}
			}
		}

		void AddLogIfReversed()
		{
			if (OldSource.CA0_ReversedDateUtcInfo.OriginalValue.IsEmpty && !NewSource.CA0_ReversedDateUtc.IsEmpty)
			{
				AddChangeLog(Res.GetString("f0c04200-d821-4a89-a0c4-3a1db2f18c18", "Reversed"), AutoApprovalHelper.Never);
			}
		}

		protected override void AddDetachedLogOnDeleteCore()
		{
			// Don't add any logs
		}
	}
}
