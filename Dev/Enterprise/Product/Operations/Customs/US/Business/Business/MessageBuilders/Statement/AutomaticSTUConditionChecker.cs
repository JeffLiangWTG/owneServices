using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.STU;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	class AutomaticSTUConditionChecker
	{
		public bool ShouldSend(JobDeclaration declaration, ZDate newPSD)
		{
			return ShouldSend(declaration, newPSD, () => false);
		}

		public bool ShouldSend(JobDeclaration declaration, ZDate newPSD, Func<bool> extraConditionNotToSend)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			if (entry != null)
			{
				if (declaration.US_PSC)
				{
					declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUPSCFlagTicked, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
					return false;
				}

				var overrideAllOrByOrganisation = USCustomsDataRegistry.Instance.AutoSendSDCR.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty).OverrideAllOrByOrganisation;
				bool registryIsOn = overrideAllOrByOrganisation == "ALL" || (overrideAllOrByOrganisation == "ORG" && ShouldAutoSendMessageForIOR(declaration.IOR));

				if (!registryIsOn)
				{
					var logReference = string.Empty;
					if (overrideAllOrByOrganisation == "ORG" && !ShouldAutoSendMessageForIOR(declaration.IOR))
					{
						logReference = string.Format(NotificationSuppressSTUAutoSendDisabledForOrg, declaration.IOR?.OH_Code ?? ZString.Empty);
					}
					else
					{
						logReference = string.Format(NotificationSuppressSTUAutoSendRegistryDisabled, declaration.Branch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code);
					}
					declaration.Logs.AddNew(Events.SuppressSendingMessage, logReference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
					return false;
				}
				else
				{
					var isEntryCleared = entry.HasBeenLodgedAtCustoms && !entry.HasBeenWithdrawn;
					if (!isEntryCleared)
					{
						declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUEntryNotLodged, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					var baseDate = ((IPrelimStatementDetailsDefault)declaration).BaseDateToCalculateOn;
					var baseDateValid = baseDate.IsValid;
					if (!baseDateValid)
					{
						declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUBaseDateNotValid, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					var psdAcceptedAtCustoms = !declaration.US_PSDAccepted.IsEmpty ? declaration.US_PSDAccepted : declaration.US_PreliminaryStatementPrintDate;
					var newPeriodicStatementMonth = new PSMonthCalculator().GetMonth(baseDate);

					bool newStatmentDateIsDifferent = (psdAcceptedAtCustoms.Date != newPSD) ||
									(PaymentTypeList.IsPeriodicPayment(declaration.US_PaymentType) && newPeriodicStatementMonth != declaration.US_PeriodicStatementMM);
					//if there is a pending STU, this comparison is wrong because STU can be accepted and PSD Accepted will be different
					if (!declaration.IsSTUPending() && !newStatmentDateIsDifferent)
					{
						var logReference = string.Format(NotificationSuppressSTUPendingAndNewDateNotDifferent, newPSD.ToShortDateString());
						declaration.Logs.AddNew(Events.SuppressSendingMessage, logReference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					bool newDateTodayorEalier = ZDate.Today >= newPSD;
					if (newDateTodayorEalier)
					{
						var logReference = string.Format(NotificationSuppressSTUNewPSDSLaterThanToday, newPSD.ToShortDateString(), ZDate.Today.ToShortDateString());
						declaration.Logs.AddNew(Events.SuppressSendingMessage, logReference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					if (EntryTypeList.IsQuotaVisa(declaration.US_EntryType) && declaration.ENSStatusNotifications.Count > 0)
					{
						var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
						if (workingDays != null)
						{
							var dispositionQs = declaration.ENSStatusNotifications.Where(x => x.DispositionCode == ENSStatusDispositionCodeList._Q);
							if (dispositionQs.Any())
							{
								var latestQuotaPresentationDate = dispositionQs.Max(x => x.StatusDate);
								var standardWorkingDayForQuota = (ZDateTime)workingDays.GetAnotherStandardWorkingDay(latestQuotaPresentationDate.ToDateTime(), 10);
								if (newPSD > standardWorkingDayForQuota.Date)
								{
									var logReference = string.Format(NotificationSuppressSTUNewPSDThan10WorkingDayFromQuotaPresentationDate, newPSD.ToShortDateString(), latestQuotaPresentationDate.ToShortDateString());
									declaration.Logs.AddNew(Events.SuppressSendingMessage, logReference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
									return false;
								}
							}
						}
					}

					bool statementPaymentType = declaration.US_PaymentType != PaymentTypeList.Codes.IndividualBasis && !declaration.US_PaymentType.IsEmpty;
					if (!statementPaymentType)
					{
						declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUPaymentTypeEmptyOrIndividualBasis, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					if (DataRegistry.Business.USCustomsDataRegistry.Instance.SuppressStatementUpdate.GetValueWithoutFallback(declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) && (declaration.RelatedStatement?.IsPreliminary ?? false))
					{
						declaration.Logs.AddNew(Events.SuppressSendingMessage, string.Format(NotificationSuppressSTUSuppressRegistryEnabledAndEntryOnPreliminaryStatement, declaration.RelatedStatement.B2_StatementNumber), DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					if (declaration.RelatedStatement is CusStatementHeader relatedStatement && relatedStatement.B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationAccepted && relatedStatement.ActiveLines.Count > 0)
					{
						declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUPaymentAuthorized, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					if (extraConditionNotToSend())
					{
						var logReference = string.Format(NotificationSuppressSTUUnderExtraCondition, newPSD.ToShortDateString(), psdAcceptedAtCustoms.ToShortDateString());
						declaration.Logs.AddNew(Events.SuppressSendingMessage, logReference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
						return false;
					}

					foreach (var logs in declaration.Logs.GetAllLogs().OfType<StmALog>().Where(x => x.SL_SE_NKEvent == Events.SuppressSendingMessageCode))
					{
						logs.Cancel(ZDateTime.Now);
					}
					return true;
				}
			}

			declaration.Logs.AddNew(Events.SuppressSendingMessage, NotificationSuppressSTUNoActiveHeaders, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch));
			return false;
		}
		internal const string NotificationSuppressSTUPSCFlagTicked = "STU message suppressed because PSC flag is ticked.";
		internal const string NotificationSuppressSTUEntryNotLodged = "STU message suppressed because entry summary has not been lodged or withdrawn.";
		internal const string NotificationSuppressSTUBaseDateNotValid = "STU message suppressed because the base date to be calculated is not Valid.";
		internal const string NotificationSuppressSTUPendingAndNewDateNotDifferent = "STU message suppressed because STU message is pending and the new statement date {0} is not different.";
		internal const string NotificationSuppressSTUNewPSDSLaterThanToday = "STU message suppressed because the new PSD {0} should be later than Today {1}.";
		internal const string NotificationSuppressSTUPaymentTypeEmptyOrIndividualBasis = "STU message suppressed because the payment type is either empty or 01 (Individual Basis).";
		internal const string NotificationSuppressSTUPaymentAuthorized = "STU message suppressed because the payment has been authorized.";
		internal const string NotificationSuppressSTUUnderExtraCondition = "STU message suppressed because this is a Live entry and the new PSD {0} is later than PSD accepted {1}.";
		internal const string NotificationSuppressSTUSuppressRegistryEnabledAndEntryOnPreliminaryStatement = "STU message suppressed because the entry appears on a statement {0} and registry setting to suppress is active.";
		internal const string NotificationSuppressSTUNewPSDThan10WorkingDayFromQuotaPresentationDate = "STU message suppressed because the new PSD ({0}) is 10 working days greater than Quota Presentation Date ({1}).";
		internal const string NotificationSuppressSTUAutoSendDisabledForOrg = "STU message suppressed because auto sending is disabled for organization {0}.";
		internal const string NotificationSuppressSTUAutoSendRegistryDisabled = "STU message suppressed because auto sending is not enabled in the registry for branch {0}.";
		internal const string NotificationSuppressSTUNoActiveHeaders = "STU message suppressed because there are no active headers on the entry.";

		bool ShouldAutoSendMessageForIOR(OrgHeader iOR)
		{
			return iOR != null && !OrgHeaderWrapper.New(iOR).ZO_DoNotAutoGenerateSDCR;
		}
	}
}
