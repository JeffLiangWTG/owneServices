using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementRecipientModifiedLogsManager : CommissionAgreementModifiedLogsManager<OrgCommissionAgreementRecipient>
	{
		public OrgCommissionAgreementRecipientModifiedLogsManager(OrgCommissionAgreementRecipient draft)
			: base(draft)
		{
		}

		protected override void AddLogsOnFactorySavingCore()
		{
			var commissionAgreement = NewSource.CommissionAgreement;
			if (MainVersion.IsInDatabase)
			{
				AddLogIfChanged(x => x.CAR_CommentInfo, AutoApprovalHelper.Always);
				AddLogIfChanged(x => x.CAR_CommissionTypeInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAR_GS_NKStaffInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAR_IsCommissionRateOverridenInfo, AutoApprovalHelper.Always);
				AddLogIfChanged(x => x.CAR_OH_PartyInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement), (pk) =>
				{
					var party = Factory.Load<OrgHeader>((ZGuid)pk);
					return party != null ? party.OH_Code : ZString.Empty;
				});

				AddLogIfChanged(x => x.CAR_ShareInfo, AutoApprovalHelper.OnlyIfNotEffectiveInThePast(commissionAgreement));
				AddLogIfChanged(x => x.CAR_EndDateInfo, AutoApprovalHelper.OnlyIfNoDatesInThePast(NewSource.CAR_EndDate, ((ZDate)NewSource.CAR_EndDateInfo.OriginalValue)));
			}
			else if (MainVersion.CommissionAgreement != null && MainVersion.CommissionAgreement.MainVersion.IsInDatabase)
			{
				MainVersion.CommissionAgreement.ModifiedLogs.AddChildObjectAttachedLog(NewSource);
			}
		}

		protected override void AddDetachedLogOnDeleteCore()
		{
			MainVersion.CommissionAgreement.ModifiedLogs.AddChildObjectDetachedLog(NewSource);
		}
	}
}
