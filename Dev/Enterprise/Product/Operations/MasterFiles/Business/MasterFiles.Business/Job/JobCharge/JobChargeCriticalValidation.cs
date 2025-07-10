using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	public sealed class JobChargeCriticalValidation : CriticalValidation<JobCharge>
	{
		public JobChargeCriticalValidation(JobCharge parent)
			: base(parent)
		{
		}

		protected override IEnumerable<CriticalValidationResult> AfterSavingCriticalChecks()
		{
			foreach (var result in base.AfterSavingCriticalChecks())
			{
				yield return result;
			}

			yield return CheckHasSkippedDataRefreshBusUpdate();
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return CheckOsAndLocalAmount();
			yield return CheckOSAndLocalAmountSign();
			yield return CheckAccrualAndWip();
			yield return CheckJobChargeMoreThanOneFieldLinkToTheSameLine();
			yield return CheckOtherChargesLinkedToTheSameTransactionLine();
			yield return CheckChargeAndConsolCostInvoiceDetailsAreEqual();
			yield return CheckChargeLinkedToConsolCostHasNonZeroCost();

			foreach (var linkInfo in new ZPropertyInfo[] { Parent.JR_AL_ARLineInfo, Parent.JR_AL_APLineInfo })  // JR_AL_CFXLine is not included as we cannot calculate CFX Amount on JobCharge, only on BaseCharge. CFX Line has no Tax too.
			{
				yield return CheckRelatedTransactionLineHasMatchingAmountAndSign(linkInfo);
				yield return CheckRelatedPostedTransactionLineReferenceIsNotChanged(linkInfo);
				yield return CheckRelatedTransactionLineHasMatchingTaxCode(linkInfo);
			}

			yield return CheckRelatedPostedTransactionLineReferenceIsNotChanged(Parent.JR_AL_CFXLineInfo);
			yield return CheckChargeLinkedToPostedConsolCostIsCostPosted();
			yield return CheckChargeLinkedToPostedConsolCostIsPostedToSameAPInvoice();
			yield return CheckChargeLinkedToUnpostedConsolCostIsNotCostPosted();
			yield return CheckChargeSellExRateWhenLocalCompanyCurrencyEqualsChargeSellCurrency();
			yield return CheckChargeCostExRateWhenLocalCompanyCurrencyEqualsChargeCostCurrency();
			yield return CheckJobChargeNegativeRevenueIsNotPermitted();
			yield return CheckChargeAmountExceedMaximumAllowedAmount();
			yield return CheckChargeLinkedToClosedJob();
			yield return CheckChargeLinkedToInactiveJob();
			yield return CheckJobChargeHasInvalidProFormaCostOrProFormaRevenue();
		}

		CriticalValidationResult CheckChargeAmountExceedMaximumAllowedAmount()
		{
			var toCheckAmounts = new List<ZDecimal>();

			if (!Parent.IsInDatabase)
			{
				toCheckAmounts.Add(Parent.JR_LocalCostAmt);
				toCheckAmounts.Add(Parent.JR_LocalSellAmt);
			}
			else
			{
				if (Parent.JR_LocalCostAmtInfo.HasChanges)
				{
					toCheckAmounts.Add(Parent.JR_LocalCostAmt);
				}
				if (Parent.JR_LocalSellAmtInfo.HasChanges)
				{
					toCheckAmounts.Add(Parent.JR_LocalSellAmt);
				}
			}

			if (toCheckAmounts.Any())
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.JR_GC, CriticalValidationHelpers.MaximumAmountLevel.Charge, toCheckAmounts.ToArray());
				if (message != null)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, message);
				}
			}

			return null;
		}

		CriticalValidationResult CheckOsAndLocalAmount()
		{
			CriticalValidationResult result = null;
			var chargeLocalCurrency = Parent.Company?.LocalCurrency?.Code;
			if ((!Parent.IsInDatabase || Parent.JR_OSSellExRateInfo.HasChanges || Parent.JR_OSSellAmtInfo.HasChanges || Parent.JR_LocalSellAmtInfo.HasChanges)
					&& !AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.JR_RX_NKSellCurrency, Parent.JR_OSSellAmt, Parent.JR_LocalSellAmt, chargeLocalCurrency))
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMsg1 = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed);
				var extraMsg2 = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmountNotRecalculatedWhenSellCurrencyIsSetToLocalCurrency);

				result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeOsSellAmountNotEqualLocalSellAmount_8,
																CriticalValidationMessageTemplate.JobChargeOsSellAmountNotEqualLocalSellAmountErrorMessage,
																(NoResString)"Charge with incorrect data:",
																Parent.GetJobChargeInfo(),
																extraMsg1,
																extraMsg2);
			}

			if ((!Parent.IsInDatabase || Parent.JR_OSCostExRateInfo.HasChanges || Parent.JR_OSCostAmtInfo.HasChanges || Parent.JR_LocalCostAmtInfo.HasChanges)
					&& !AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.JR_RX_NKCostCurrency, Parent.JR_OSCostAmt, Parent.JR_LocalCostAmt, chargeLocalCurrency))
			{
				result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeOsCostAmountNotEqualLocalCostAmount_4,
																	CriticalValidationMessageTemplate.JobChargeOsCostAmountNotEqualLocalCostAmountErrorMessage,
																	(NoResString)"Charge with incorrect data:",
																	Parent.GetJobChargeInfo());
			}
			return result;
		}

		CriticalValidationResult CheckOSAndLocalAmountSign()
		{
			CriticalValidationResult result = null;
			if ((!Parent.IsInDatabase || Parent.JR_OSSellExRateInfo.HasChanges || Parent.JR_OSSellAmtInfo.HasChanges || Parent.JR_LocalSellAmtInfo.HasChanges)
					&& ((Parent.JR_LocalSellAmt > 0 && Parent.JR_OSSellAmt < 0) || (Parent.JR_LocalSellAmt < 0 && Parent.JR_OSSellAmt > 0)))
			{
				var jobChargeService = Parent.Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>();
				if (jobChargeService != null && jobChargeService.IsJobChargeExRateNegative(Parent.PK))
				{
					result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeOSSellExRateIsNegative,
														  CriticalValidationMessageTemplate.JobChargeNegativeOSSellExRateErrorMessage);
					jobChargeService.Remove(Parent.PK);
				}
				else
				{
					var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
					var developerMessage_JobChargeSellAmountsSignsNotMatching = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount);
					var developerMessage_CarryForwardChargeAmountWithDifferentSigns = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns);

					result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmount_5,
																CriticalValidationMessageTemplate.JobChargeOsSellAmountNotMatchingSignOfLocalSellAmountErrorMessage(Parent.JR_OSSellAmt, Parent.JR_LocalSellAmt),
																(NoResString)"Charge with incorrect data:",
																Parent.GetJobChargeInfo(),
																developerMessage_JobChargeSellAmountsSignsNotMatching,
																developerMessage_CarryForwardChargeAmountWithDifferentSigns);
				}
			}

			if ((!Parent.IsInDatabase || Parent.JR_OSCostExRateInfo.HasChanges || Parent.JR_OSCostAmtInfo.HasChanges || Parent.JR_LocalCostAmtInfo.HasChanges)
					&& ((Parent.JR_LocalCostAmt > 0 && Parent.JR_OSCostAmt < 0) || (Parent.JR_LocalCostAmt < 0 && Parent.JR_OSCostAmt > 0)))
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var developerMessage_JobChargeCostAmountsSignsNotMatching = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount);
				var developerMessage_CarryForwardChargeAmountWithDifferentSigns = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns);

				var accTransactionLines =
					Parent.JR_AL_ARLine.IsEmpty
					? Parent.JR_AL_APLine.IsEmpty
						? null
						: Parent.APLine
					: Parent.ARLine;

				result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount_3,
																CriticalValidationMessageTemplate.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmountErrorMessage(Parent.JR_OSCostAmt, Parent.JR_LocalCostAmt),
																(NoResString)"Charge with incorrect data:",
																Parent.GetJobChargeInfo(),
																accTransactionLines != null ? accTransactionLines.GetTransactionLineInfo() : string.Empty,
																developerMessage_JobChargeCostAmountsSignsNotMatching,
																developerMessage_CarryForwardChargeAmountWithDifferentSigns);
			}
			return result;
		}

		CriticalValidationResult CheckAccrualAndWip()
		{
			AccTransactionLines relatedLine;
			List<JobCharge> otherCharges;

			foreach (string lineType in new string[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual })
			{
				if (!CheckOriginalRelatedWIPAccrualLineIsReversed(lineType, out relatedLine))
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeHasPreviouslyLinkedLineThatMustBeReversed_3,
														CriticalValidationMessageTemplate.GetJobChargeHasPreviouslyLinkedLineThatMustBeReversedErrorMessage(lineType),
														relatedLine.GetTransactionLineInfo());
				}

				if (!CheckNewRelatedWIPAccrualLineIsNotReversed(lineType, out relatedLine))
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedLineShouldNotBeReversed_3,
														CriticalValidationMessageTemplate.GetJobChargeLinkedLineShouldNotBeReversedErrorMessage(lineType),
														relatedLine.GetTransactionLineInfo());
				}

				if (!CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount(lineType, out relatedLine))
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
														CriticalValidationMessageTemplate.GetJobChargeAmountNotEqualRelatedLineAmountErrorMessage(lineType),
														Parent.GetJobChargeInfo(), relatedLine.GetTransactionLineInfo());
				}

				if (!CheckNewRelatedWIPAccrualLineOrganisationIsTheSameAsChargeOrganisation(lineType, out relatedLine))
				{
					var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
					var extraMsgInfo = new ZStringBuilder();
					extraMsgInfo.AppendLine(collectorService.GetInfoSafe(relatedLine.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine));
					extraMsgInfo.AppendLine(collectorService.GetInfoSafe(relatedLine.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb));

					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeOrganisationIsNotSameAsLineOne_5,
														CriticalValidationMessageTemplate.GetJobChargeOrganisationIsNotSameAsLineOneErrorMessage(lineType),
														Parent.GetJobChargeInfo(),
														relatedLine.GetTransactionLineInfo(),
														extraMsgInfo.ToString());
				}

				if (!CheckNewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges(lineType, out relatedLine, out otherCharges))
				{
					StringBuilder chargeInfo = new StringBuilder();
					foreach (JobCharge charge in otherCharges)
					{
						chargeInfo.AppendLine(charge.GetJobChargeInfo());
					}
					return new CriticalValidationResult(CriticalValidationErrorType.NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges_3,
						CriticalValidationMessageTemplate.NewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges,
						string.Format(CultureInfo.InvariantCulture, (NoResString)"Related {0}", relatedLine.AL_LineType),
														relatedLine.GetTransactionLineInfo(),
														Parent.GetJobChargeInfo(),
														(NoResString)"Other charges",
														chargeInfo.ToString());
				}
			}
			return null;
		}

		CriticalValidationResult CheckJobChargeNegativeRevenueIsNotPermitted()
		{
			AccTransactionLines accTransactionLines;
			AccTransactionHeader accTransactionHeader;
			return (!Parent.IsInDatabase || Parent.JR_OSSellAmtInfo.HasChanges)
				&& Parent.JR_OSSellAmt < ZDecimal.Zero
				&& !AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.GetValueWithoutFallback(Parent.JR_GC.ToGuid(), Guid.Empty, Guid.Empty)
				&& !Parent.IsRevenuePosted
				? new CriticalValidationResult(CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted,
					CriticalValidationMessageTemplate.JobChargeNegativeRevenueIsNotPermittedErrorMessage,
					Parent.GetJobChargeInfo(),
					(accTransactionLines = Parent.JR_AL_ARLine.IsEmpty ? Parent.JR_AL_APLine.IsEmpty ? null : Parent.APLine : Parent.ARLine) != null ? accTransactionLines.GetTransactionLineInfo() : string.Empty,
					(accTransactionHeader = accTransactionLines?.TransactionHeader) != null ? accTransactionHeader.GetTransactionHeaderInfo() : string.Empty)
				: null;
		}

		CriticalValidationResult CheckChargeLinkedToClosedJob()
		{
			if (Parent.IsInDatabase || !Parent.IsCreatedOnClosedJob || Parent.Job.JH_Status != JobHeaderStatus.Codes.Closed)
			{
				return null;
			}

			var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
			var errorType = CriticalValidationErrorType.JobChargeLinkedToClosedJob;
			var userMessage = CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobErrorMessage;
			var developerMessage = Invariant($@"Job Status HasChanges: {Parent.Job.JH_StatusInfo.HasChanges} ({Parent.Job.JH_StatusInfo.OriginalValue})
User: {GlbStaff.CurrentUser.GS_Code}
User can re-open jobs: {Env.Security.ReopenJob.IsAllowed}, {Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed}
Last Job Opened Time: {Parent.Job.Logs.MostRecentLogByEventTime(AutoEvents.JobOpen)?.SL_PostedTimeUtc.ToStandardDateTimeString() ?? "n/a"}
Last Job Closed Time: {Parent.Job.Logs.MostRecentLogByEventTime(AutoEvents.JobClose)?.SL_PostedTimeUtc.ToStandardDateTimeString() ?? "n/a"}
Last Job Closed User: {Parent.Job.Logs.MostRecentLogByEventTime(AutoEvents.JobClose)?.SL_GS_NKUser ?? "n/a"}

JobCharge: {Parent.GetJobChargeInfo()}
{collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeCreatedOnClosedJobStackTrace)}
{collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace)}
{collectorService.GetInfoSafe(Parent.Job.PK, CriticalValidationInfoCollectorServiceKeyType.LoginFormCancelledJobReopenJobChargeCreatedOnClosedJobStackTrace)}
{collectorService.GetInfoSafe(Parent.Job.PK, CriticalValidationInfoCollectorServiceKeyType.ReopeningJobStackTrace)}");

			var consolCostMessage = ZString.Empty;
			if (Parent.ParentConsolCost is IHaveConstructorStackTrace consolCostStackTrace)
			{
				consolCostMessage = Invariant($"ConsolCost ConstructorStackTrace: {consolCostStackTrace.ConstructorStackTrace}");
			}

			if (Parent.Factory.HasContext(BusinessContext.CreatingProfitShareCharge))
			{
				errorType = CriticalValidationErrorType.JobChargeLinkedToClosedJobWhenCreatingProfitShareCharges;
				userMessage = CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobWhenCreatingProfitShareChargesErrorMessage;
			}

			return new CriticalValidationResult(errorType, userMessage, developerMessage, consolCostMessage);
		}

		CriticalValidationResult CheckChargeLinkedToInactiveJob()
		{
			if (!Parent.IsInDatabase && !Parent.Job.JH_IsActive)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedToInactiveJob, CriticalValidationMessageTemplate.JobChargeLinkedToInactiveJobErrorMessage);
			}
			return null;
		}

		protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
		{
			foreach (var result in base.DeletedObjectOnSavingCriticalChecks())
			{
				yield return result;
			}

			foreach (var linkInfo in new ZPropertyInfo[] { Parent.JR_AL_ARLineInfo, Parent.JR_AL_APLineInfo, Parent.JR_AL_CFXLineInfo })
			{
				yield return CheckRelatedPostedTransactionLineReferenceIsNotChanged(linkInfo);
			}

			foreach (string lineType in new string[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual })
			{
				AccTransactionLines relatedLine;

				if (!CheckOriginalRelatedWIPAccrualLineIsReversed(lineType, out relatedLine))
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.JobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeleted_3,
														CriticalValidationMessageTemplate.GetJobChargeRelatedWIP_ACR_MustBeReversedWhenChargeIsDeletedErrorMessage(lineType),
														Parent.GetJobChargeOriginalInfo(), relatedLine.GetTransactionLineInfo());
				}
			}
		}

		GlbCompany ParentCompany => parentCompany ?? (parentCompany = Parent.Company);
		GlbCompany parentCompany;

		AccTransactionLines[] AllRelatedLines
		{
			get
			{
				if (allRelatedLines == null)
				{
					var allLinePKs = new List<ZGuid>();
					allLinePKs.AddRange(new LineRelatedChargeInfo(Parent.JR_AL_ARLineInfo).LinePKs);
					allLinePKs.AddRange(new LineRelatedChargeInfo(Parent.JR_AL_APLineInfo).LinePKs);
					allLinePKs.AddRange(new LineRelatedChargeInfo(Parent.JR_AL_CFXLineInfo).LinePKs);

					if (allLinePKs.Any())
					{
						var cachedQuery = new ZQuery(AccTransactionLinesSchema.PK, allLinePKs.ToArray());
						cachedQuery.FetchOnlyFromLocalCache = true;
						var relatedLinesList = new List<AccTransactionLines>(Parent.Factory.Load<AccTransactionLines>(cachedQuery));

						if (relatedLinesList.Count < allLinePKs.Count)
						{
							var unloadedLinePKs = allLinePKs.Where(x => !relatedLinesList.Exists(y => y.PK == x)).ToArray();
							foreach (var pk in unloadedLinePKs)
							{
								var line = Parent.Factory.Load<AccTransactionLines>(pk);
								if (line != null)
								{
									relatedLinesList.Add(line);
								}
							}
						}

						allRelatedLines = relatedLinesList.ToArray();
					}
					else
					{
						allRelatedLines = Array.Empty<AccTransactionLines>();
					}
				}
				return allRelatedLines;
			}
		}
		AccTransactionLines[] allRelatedLines;

		AccTransactionLines[] AllRelatedPostedLines
		{
			get
			{
				var postedLineTypes = new ZString[] { TransactionLineTypes.Revenue, TransactionLineTypes.Cost, TransactionLineTypes.UnapprovedCost };
				return AllRelatedLines.Where(x => postedLineTypes.Contains(x.AL_LineType)).ToArray();
			}
		}

		internal struct LineRelatedChargeInfo
		{
			public LineRelatedChargeInfo(ZPropertyInfo linkInfo)
			{
				LinkInfo = linkInfo;
				Parent = (JobCharge)linkInfo.BizObj;
			}

			readonly JobCharge Parent;
			readonly ZPropertyInfo LinkInfo;

			public ZGuid[] LinePKs
			{
				get
				{
					var result = new List<ZGuid>();

					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							result.AddRange(Parent.GetARLineValueHistory());
							break;
						case JobChargeSchema.Constants.JR_AL_APLine:
							result.AddRange(Parent.GetAPLineValueHistory());
							break;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							if (!LinkInfo.OriginalValue.IsEmpty)
							{
								result.Add((ZGuid)LinkInfo.OriginalValue);
							}
							break;
						default:
							return Array.Empty<ZGuid>(); // Only these three columns are supported. Anything else will get an empty array
					}

					if (!LinkInfo.OriginalValue.IsEmpty && !result.Contains((ZGuid)LinkInfo.OriginalValue))
					{
						result.Add((ZGuid)LinkInfo.OriginalValue);  // Add original value in case it was changed via setting directly on DataRow
					}
					if (!LinkInfo.Value.IsEmpty)
					{
						result.Add((ZGuid)LinkInfo.Value);  // Add current value
					}

					return result.ToArray();
				}
			}

			public ZDecimal OSAmount
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_OSSellAmt + Parent.JR_OSSellGSTAmt_Calc;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return -(Parent.JR_OSCostAmt + Parent.JR_OSCostGSTAmt);
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return LocalAmount;
						default:
							return ZDecimal.Zero;
					}
				}
			}

			public ZString CurrencyCode
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_RX_NKSellCurrency;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_RX_NKCostCurrency;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return Environment.Env.CurrentCompany.LocalCurrency.Code;
						default:
							return Environment.Env.CurrentCompany.LocalCurrency.Code;
					}
				}
			}

			public ZDecimal LocalAmount
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_LocalSellAmt;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return -Parent.JR_LocalCostAmt;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							{
								var result = ZDecimal.Zero;

								if (Parent.IsCFXPosted &&
									!Parent.IsRevenuePosted && Parent.ARLine != null)
								{
									// CFX is posted but Revenue is not. AR Line is WIP. Should not be in this situation as Revenue and CFX posted and reversed simultaneously
									// We can use WIP Amount to check
									result = -(Parent.JR_LocalSellAmt + Parent.ARLine.AL_LineAmount);
								}
								else if (Parent.CFXLine != null)
								{
									// No check as we cannot calculate CFX. Expected == Actual to bypass checking
									result = Parent.CFXLine.AL_LineAmount;
								}

								return result;
							}
						default:
							return ZDecimal.Zero;
					}
				}
			}

			public bool AmountFieldsHaveChanges
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_OSSellAmtInfo.HasChanges || Parent.JR_RX_NKSellCurrencyInfo.HasChanges || Parent.JR_LocalSellAmtInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_OSCostAmtInfo.HasChanges || Parent.JR_OSCostGSTAmtInfo.HasChanges || Parent.JR_RX_NKCostCurrencyInfo.HasChanges || Parent.JR_LocalCostAmtInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return Parent.JR_LineCFXInfo.HasChanges;
						default:
							return false;
					}
				}
			}

			public bool TaxRateHasChanges
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_AT_SellGSTRateInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_AT_CostGSTRateInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return false;
						default:
							return false;
					}
				}
			}

			public bool TaxDateHasChanges
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_SellTaxDateInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_CostTaxDateInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return false;
						default:
							return false;
					}
				}
			}

			public ZGuid TaxRatePK
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_AT_SellGSTRate;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_AT_CostGSTRate;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return ZGuid.Empty;
						default:
							return ZGuid.Empty;
					}
				}
			}

			public bool TaxClassHasChanges
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_A9_SellVATClassInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_A9_CostVATClassInfo.HasChanges;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return false;
						default:
							return false;
					}
				}
			}

			public ZGuid TaxClassPK
			{
				get
				{
					switch (LinkInfo.Name)
					{
						case JobChargeSchema.Constants.JR_AL_ARLine:
							return Parent.JR_A9_SellVATClass;
						case JobChargeSchema.Constants.JR_AL_APLine:
							return Parent.JR_A9_CostVATClass;
						case JobChargeSchema.Constants.JR_AL_CFXLine:
							return ZGuid.Empty;
						default:
							return ZGuid.Empty;
					}
				}
			}
		}

		bool CheckOriginalRelatedWIPAccrualLineIsReversed(string lineType, out AccTransactionLines relatedLine)
		{
			bool result = true;
			relatedLine = null;

			bool isDeleted = Parent.IsDeleted;
			bool isAR = lineType == TransactionLineTypes.Revenue || lineType == TransactionLineTypes.WIP;

			ZPropertyInfo linkInfo = isAR ? Parent.JR_AL_ARLineInfo : Parent.JR_AL_APLineInfo;
			var valueHistory = isAR ? Parent.GetARLineValueHistory() : Parent.GetAPLineValueHistory();
			if (Parent.IsInDatabase && (isDeleted || linkInfo.HasChanges) && !linkInfo.OriginalValue.IsEmpty)
			{
				foreach (var linePK in valueHistory)
				{
					relatedLine = Parent.Factory.Load<AccTransactionLines>(linePK);
					if (relatedLine != null && relatedLine.AL_LineType == lineType)
					{
						result = !relatedLine.AL_ReverseDate.IsEmpty;
						if (!result)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		bool CheckNewRelatedWIPAccrualLineIsNotReversed(string lineType, out AccTransactionLines relatedLine)
		{
			ZPropertyInfo linkInfo = lineType == TransactionLineTypes.WIP ? Parent.JR_AL_ARLineInfo : Parent.JR_AL_APLineInfo;

			bool result = true;
			relatedLine = null;

			if (!Parent.IsInDatabase || linkInfo.HasChanges)
			{
				if (!linkInfo.Value.IsEmpty)
				{
					relatedLine = Parent.Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, linkInfo.Value));
					if (relatedLine != null && relatedLine.AL_LineType == lineType)
					{
						result = relatedLine.AL_ReverseDate.IsEmpty;
					}
				}
			}

			return result;
		}

		bool CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount(string lineType, out AccTransactionLines relatedLine)
		{
			ZPropertyInfo linkInfo = lineType == TransactionLineTypes.WIP ? Parent.JR_AL_ARLineInfo : Parent.JR_AL_APLineInfo;

			bool result = true;
			relatedLine = null;

			if (!Parent.IsInDatabase || linkInfo.HasChanges)
			{
				if (!linkInfo.Value.IsEmpty)
				{
					relatedLine = Parent.Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, linkInfo.Value));
					if (relatedLine != null && relatedLine.AL_LineType == lineType && relatedLine.AL_ReverseDate.IsEmpty)
					{
						if (lineType == TransactionLineTypes.WIP)
						{
							if (Parent.JR_AL_CFXLine.IsEmpty &&
								(Parent.JR_LineCFX.IsEmpty || !Parent.IsApplyCFX) &&
								!(Parent.BillInInvoiceCurrencyWithLocalSellCurrency))
							{
								// We can check WIP Amount only when there is no CFX, as we can calculate CFX only at BaseCharge in Accounting
								// We also cannot check WIP amount when Charge is billed in Foreign Sell Invoice currency have Local Sell Currency, as JR_LocalSellInvoiceAmt is calculated by BaseCharge
								result = Parent.JR_LocalSellAmt == -relatedLine.AL_LineAmount;
							}
						}
						else
						{
							result = Parent.JR_LocalCostAmt == relatedLine.AL_LineAmount;
						}
					}
				}
			}

			return result;
		}

		bool CheckNewRelatedWIPAccrualLineOrganisationIsTheSameAsChargeOrganisation(string lineType, out AccTransactionLines relatedLine)
		{
			bool result = true;
			relatedLine = null;

			ZPropertyInfo linkInfo = lineType == TransactionLineTypes.WIP ? Parent.JR_AL_ARLineInfo : Parent.JR_AL_APLineInfo;
			ZPropertyInfo orgInfo = lineType == TransactionLineTypes.WIP ? Parent.JR_OH_SellAccountInfo : Parent.JR_OH_CostAccountInfo;

			if (!Parent.IsInDatabase || linkInfo.HasChanges || orgInfo.HasChanges)
			{
				if (!linkInfo.Value.IsEmpty)
				{
					relatedLine = Parent.Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.PK, linkInfo.Value));

					if (relatedLine != null && relatedLine.AL_LineType == lineType && relatedLine.AL_ReverseDate.IsEmpty)
					{
						var companydata = OrgCompanyData.Load(Parent.Factory, (ZGuid)orgInfo.Value, Parent.Branch != null ? Parent.Branch.GB_GC : ZGuid.Empty);

						if (lineType == TransactionLineTypes.WIP)
						{
							var expected = (companydata != null && companydata.OB_IsDebtor) ? Parent.JR_OH_SellAccount : ZGuid.Empty;
							result = expected == relatedLine.AL_OH;
						}
						else
						{
							var expected = (companydata != null && companydata.OB_IsCreditor) ? Parent.JR_OH_CostAccount : ZGuid.Empty;
							result = expected == relatedLine.AL_OH;
						}
					}
				}
			}

			return result;
		}

		CriticalValidationResult CheckChargeAndConsolCostInvoiceDetailsAreEqual()
		{
			bool result = true;
			IJobConsolCost problemConsolCost = null;
			bool problemConsolCostIsSaved = true;

			if (!Parent.JR_E6.IsEmpty &&
				(!Parent.IsInDatabase || Parent.JR_E6Info.HasChanges ||
				Parent.JR_APInvoiceNumInfo.HasChanges || Parent.JR_APInvoiceDateInfo.HasChanges ||
				Parent.JR_PaymentDateInfo.HasChanges || Parent.JR_OH_CostAccountInfo.HasChanges || Parent.JR_CostReferenceInfo.HasChanges ||
				Parent.JR_AT_CostGSTRateInfo.HasChanges || Parent.JR_CostTaxDateInfo.HasChanges || Parent.JR_A9_CostVATClassInfo.HasChanges ||
				Parent.JR_CostSupplyTypeInfo.HasChanges ||
				Parent.JR_GB_CostTaxBranchInfo.HasChanges))
			{
				var consolCost = Parent.ParentConsolCost;
				if (consolCost != null)
				{
					if (Parent.JR_APInvoiceNum != (ZString)consolCost[JobConsolCostSchema.E6_InvoiceNum.Name] ||
						!AreDateTimesEqualSafe(Parent.JR_APInvoiceDateInfo, consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceDate.Name]) ||
						!AreDateTimesEqualSafe(Parent.JR_PaymentDateInfo, consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_PaymentDate.Name]) ||
						Parent.JR_OH_CostAccount != (ZGuid)consolCost[JobConsolCostSchema.E6_OH_Creditor.Name] ||
						Parent.JR_CostReference != (ZString)consolCost[JobConsolCostSchema.E6_CostReference.Name] ||
						Parent.JR_AT_CostGSTRate != (ZGuid)consolCost[JobConsolCostSchema.E6_AT_TaxRate] ||
						Parent.JR_CostTaxDate != (ZDate)consolCost[JobConsolCostSchema.E6_TaxDate] ||
						Parent.JR_A9_CostVATClass != (ZGuid)consolCost[JobConsolCostSchema.E6_A9_VATClass] ||
						Parent.JR_CostSupplyType != (ZString)consolCost[JobConsolCostSchema.E6_SupplyType] ||
						Parent.JR_GB_CostTaxBranch != (ZGuid)consolCost[JobConsolCostSchema.E6_GB_CostTaxBranch])
					{
						result = false;
						problemConsolCost = (IJobConsolCost)consolCost;
					}
					else
					{
						if (!ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)consolCost).Row) &&
							(!consolCost.IsInDatabase ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceNum.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_InvoiceDate.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_PaymentDate.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_OH_Creditor.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_CostReference.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_AT_TaxRate.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_TaxDate.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_A9_VATClass.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_SupplyType.Name].HasChanges ||
							consolCost.ZPropertyInfoHash[JobConsolCostSchema.E6_GB_CostTaxBranch.Name].HasChanges))
						{
							result = false;
							problemConsolCostIsSaved = false;
							problemConsolCost = (IJobConsolCost)consolCost;
						}
					}
				}
			}

			if (!result)
			{
				if (!problemConsolCostIsSaved)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
														CriticalValidationMessageTemplate.JobChargeInvoiceDetailsNotEqualConsolCostOnesErrorMessage,
														(NoResString)"Consol cost is not saved:",
														problemConsolCost.GetJobConsolCostInfo());
				}
				else
				{
					bool isExistingDataWithoutChange;
					var extraMsg = CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(Parent, problemConsolCost, out isExistingDataWithoutChange);

					if (isExistingDataWithoutChange)
					{
						return new CriticalValidationResult(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnesWithoutChanges,
															CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_NeedToBeSyncErrorMessage(extraMsg));
					}
					else
					{
						var collectedInfo1 = CriticalValidationInfoCollectorService.GetService(Parent.Factory).GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);
						var collectedInfo2 = CriticalValidationInfoCollectorService.GetService(Parent.Factory).GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails);
						var collectedInfo3 = CriticalValidationInfoCollectorService.GetService(Parent.Factory).GetInfoSafe(Parent.ParentConsolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);
						var collectedInfo4 = CriticalValidationInfoCollectorService.GetService(Parent.Factory).GetInfoSafe(Parent.ParentConsolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

						return new CriticalValidationResult(CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
															CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(extraMsg),
															problemConsolCost.GetJobConsolCostInfo(),
															(NoResString)"Charge with incorrect data:",
															Parent.GetJobChargeInfo(),
															collectedInfo1,
															collectedInfo2,
															collectedInfo3,
															collectedInfo4);
					}
				}
			}

			return null;
		}

		CriticalValidationResult CheckChargeLinkedToConsolCostHasNonZeroCost()
		{
			bool result = true;
			IJobConsolCost problemConsolCost = null;
			ZGuid consolCostPK = ZGuid.Empty;
			if (!Parent.JR_E6.IsEmpty &&
				(!Parent.IsInDatabase || Parent.JR_OSCostAmtInfo.HasChanges || Parent.JR_LocalCostAmtInfo.HasChanges))
			{
				var consolCost = Parent.ParentConsolCost;
				if (consolCost != null &&
					!((ZDecimal)consolCost[JobConsolCostSchema.Constants.E6_OSCostAmount]).IsEmpty &&  // ConsolCost Amount can be set to 0 by reversing of AP Credit Note posted from  the Consol Cost. Need to exclude this case
					(Parent.JR_OSCostAmt.IsEmpty || Parent.JR_LocalCostAmt.IsEmpty))
				{
					result = false;
					problemConsolCost = (IJobConsolCost)consolCost;
					consolCostPK = consolCost.PK;
				}
			}

			if (!result)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var jobChargeInfo = string.Empty;
				var extraMsg1 = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero);
				var extraMsg2 = string.Empty;
				var extraMsg3 = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount);
				var extraMsg4 = collectorService?.GetInfo(consolCostPK, CriticalValidationInfoCollectorServiceKeyType.ReorganizeConsolCosts);
				var extraMsg5 = collectorService?.GetInfo(consolCostPK, CriticalValidationInfoCollectorServiceKeyType.DefaultChargeCreationForConsolCost);
				var consolCostExchangeRateCallStack = collectorService.GetInfo(Parent.JR_E6, CriticalValidationInfoCollectorServiceKeyType.JobConsolCostExchangeRateChangeToZero);

				if (!Parent.IsInDatabase && !Parent.HasChanges && Parent.IsSavedByFactory)
				{
					jobChargeInfo = Parent.GetJobChargeInfo();
					extraMsg2 = FormattableString.Invariant($"IsSavedByFactory Evaluation Info: {Parent.GetIsSavedByFactoryEvaluationInfo()}");
				}
				else
				{
					jobChargeInfo = Parent.GetJobChargeInfo();
				}

				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmount_11,
													CriticalValidationMessageTemplate.JobChargeLinkedToConsolCostInvoiceHasZeroCostAmountErrorMessage,
													problemConsolCost.GetJobConsolCostInfo(),
													(NoResString)"Charge with incorrect data:",
													jobChargeInfo,
													extraMsg1,
													extraMsg2,
													extraMsg3,
													extraMsg4,
													extraMsg5,
													consolCostExchangeRateCallStack);
			}

			return null;
		}

		CriticalValidationResult CheckChargeLinkedToPostedConsolCostIsCostPosted()
		{
			bool result = true;
			IJobConsolCost problemConsolCost = null;

			if (!Parent.JR_E6.IsEmpty &&
				(!Parent.IsInDatabase || Parent.JR_E6Info.HasChanges))
			{
				var consolCostBizo = Parent.ParentConsolCost;
				var jobConsolCost = (IJobConsolCost)consolCostBizo;
				if (consolCostBizo != null &&
					!((ZGuid)consolCostBizo[JobConsolCostSchema.Constants.E6_AH_APInvoice]).IsEmpty &&
					consolCostBizo.IsInDatabase && !jobConsolCost.E6_AH_APInvoiceInfo.HasChanges &&
					!Parent.IsCostPostedWithAPTransaction)
				{
					result = false;
					problemConsolCost = jobConsolCost;
				}
			}

			if (!result)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedToPostedConsolCostIsNotCostPosted_3,
													CriticalValidationMessageTemplate.JobChargeLinkedToPostedConsolCostIsNotCostPostedErrorMessage,
													problemConsolCost.GetJobConsolCostInfo(),
													(NoResString)"Charge with incorrect data:",
													Parent.GetJobChargeInfo());
			}

			return null;
		}

		CriticalValidationResult CheckChargeLinkedToPostedConsolCostIsPostedToSameAPInvoice()
		{
			bool result = true;
			IJobConsolCost problemConsolCost = null;

			if (!Parent.JR_E6.IsEmpty &&
				(!Parent.IsInDatabase || Parent.JR_AL_APLineInfo.HasChanges))
			{
				var consolCost = Parent.ParentConsolCost;
				if (consolCost != null &&
					!((ZGuid)consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice]).IsEmpty &&
					Parent.IsCostPostedWithAPTransaction && !Parent.IsCostPostedWithJobRevenueJournal && Parent.APLine.AL_AH != (ZGuid)consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice])
				{
					result = false;
					problemConsolCost = (IJobConsolCost)consolCost;
				}
			}

			if (!result)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedToPostedConsolCostIsPostedToDifferentInvoice_4,
													CriticalValidationMessageTemplate.JobChargeLinkedToPostedConsolCostIsPostedToDifferentInvoiceErrorMessage,
													problemConsolCost.GetJobConsolCostInfo(),
													(NoResString)"Charge with incorrect data:",
													Parent.GetJobChargeInfo());
			}

			return null;
		}

		CriticalValidationResult CheckChargeLinkedToUnpostedConsolCostIsNotCostPosted()
		{
			bool result = true;
			IJobConsolCost problemConsolCost = null;

			if (!Parent.JR_E6.IsEmpty &&
				(!Parent.IsInDatabase || Parent.JR_AL_APLineInfo.HasChanges))
			{
				var consolCost = Parent.ParentConsolCost;
				if (consolCost != null &&
					((ZGuid)consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice]).IsEmpty &&
					Parent.IsCostPostedWithAPTransaction)
				{
					result = false;
					problemConsolCost = (IJobConsolCost)consolCost;
				}
			}

			if (!result)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeLinkedToUnpostedConsolCostIsCostPosted_3,
													CriticalValidationMessageTemplate.JobChargeLinkedToUnpostedConsolCostIsCostPostedErrorMessage,
													problemConsolCost.GetJobConsolCostInfo(),
													(NoResString)"Charge with incorrect data:",
													Parent.GetJobChargeInfo());
			}

			return null;
		}

		CriticalValidationResult CheckChargeSellExRateWhenLocalCompanyCurrencyEqualsChargeSellCurrency()
		{
			var result = true;
			if (!Parent.IsInDatabase || Parent.JR_RX_NKSellCurrencyInfo.HasChanges || Parent.JR_OSSellExRateInfo.HasChanges)
			{
				result = ParentCompany.GC_RX_NKLocalCurrency != Parent.JR_RX_NKSellCurrency || Parent.JR_OSSellExRate == 1m;
			}

			if (!result)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMsg = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrency);

				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrency_4,
					CriticalValidationMessageTemplate.JobChargeOSSellExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeSellCurrencyErrorMessage,
					Parent.GetJobChargeInfo(), extraMsg);
			}

			return null;
		}

		CriticalValidationResult CheckChargeCostExRateWhenLocalCompanyCurrencyEqualsChargeCostCurrency()
		{
			var result = true;
			if (!Parent.IsInDatabase || Parent.JR_RX_NKCostCurrencyInfo.HasChanges || Parent.JR_OSCostExRateInfo.HasChanges)
			{
				result = ParentCompany.GC_RX_NKLocalCurrency != Parent.JR_RX_NKCostCurrency || Parent.JR_OSCostExRate == 1m;
			}

			if (!result)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				var extraMsgOnJobCharge = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency);
				var extraMsgOnTransactionLine = collectorService?.GetInfo(Parent.JR_AL_APLine, CriticalValidationInfoCollectorServiceKeyType.TransactionLineExRateShouldBeOneWhenLocalCompanyCurrencyEqualsLineCurrency);

				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency_9,
					CriticalValidationMessageTemplate.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrencyErrorMessage,
					Parent.GetJobChargeInfo(), extraMsgOnJobCharge, extraMsgOnTransactionLine);
			}

			return null;
		}

		CriticalValidationResult CheckOtherChargesLinkedToTheSameTransactionLine()
		{
			var linesToCheck = new HashSet<ZGuid>();

			if (Parent.JR_AL_ARLine.IsValid && (!Parent.IsInDatabase || Parent.JR_AL_ARLineInfo.HasChanges) && ShouldLineBeChecked(Parent.JR_AL_ARLine))
			{
				linesToCheck.Add(Parent.JR_AL_ARLine);
			}
			if (Parent.JR_AL_APLine.IsValid && (!Parent.IsInDatabase || Parent.JR_AL_APLineInfo.HasChanges) && ShouldLineBeChecked(Parent.JR_AL_APLine))
			{
				linesToCheck.Add(Parent.JR_AL_APLine);
			}

			var otherCharges = IsOtherChargeLinkedToTheSameLines(linesToCheck);

			if (otherCharges.Any())
			{
				var chargeInfo = new StringBuilder();
				foreach (JobCharge charge in otherCharges)
				{
					chargeInfo.AppendLine(charge.GetJobChargeInfo());
				}

				return new CriticalValidationResult(CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_ChargeSide_4,
													CriticalValidationMessageTemplate.JobTransactionLineWithMoreThanOneJobCharge_ChargeSideErrorMessage,
													Parent.GetJobChargeInfo(),
													(NoResString)"Other charges:",
													chargeInfo.ToString());
			}

			return null;
		}

		JobCharge[] IsOtherChargeLinkedToTheSameLines(IEnumerable<ZGuid> linesToCheck)
		{
			if (!linesToCheck.Any())
			{
				return Array.Empty<JobCharge>();
			}

			var query = new ZQuery(JobChargeSchema.JR_JH, Parent.JR_JH);
			var chargesInJobHeader = Parent.Factory.Load<JobCharge>(query);

			return chargesInJobHeader
				.Where(x => x.PK != Parent.PK && (linesToCheck.Contains(x.JR_AL_ARLine) || linesToCheck.Contains(x.JR_AL_APLine)))
				.ToArray();
		}

		bool ShouldLineBeChecked(ZGuid linePK)
		{
			var line = Parent.Factory.LoadTop1<AccTransactionLines>(
				new ZQuery(AccTransactionLinesSchema.PK, linePK)
				{
					FetchOnlyFromLocalCache = true
				}
			);
			return (line == null || line.IsInDatabase);
		}

		bool CheckNewJobChargeLinkedToWIPAccrualInDbReferredByOtherJobCharges(string lineType, out AccTransactionLines relatedLine, out List<JobCharge> otherCharges)
		{
			bool result = true;
			relatedLine = null;
			otherCharges = new List<JobCharge>();

			var linkInfo = lineType == TransactionLineTypes.WIP ? Parent.JR_AL_ARLineInfo : Parent.JR_AL_APLineInfo;
			if (!Parent.IsInDatabase && linkInfo.Value.IsValid)
			{
				relatedLine = lineType == TransactionLineTypes.WIP ? Parent.ARLine : Parent.APLine;
				if (relatedLine != null && relatedLine.IsInDatabase && relatedLine.AL_LineType == lineType)
				{
					var linkColumn = lineType == TransactionLineTypes.WIP ? JobChargeSchema.JR_AL_ARLine : JobChargeSchema.JR_AL_APLine;
					var query = new ZQuery(linkColumn, relatedLine.PK);
					query.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					otherCharges.AddRange(Parent.Factory.Load<JobCharge>(query));
					result = !otherCharges.Any();
				}
			}

			return result;
		}

		CriticalValidationResult CheckRelatedTransactionLineHasMatchingAmountAndSign(ZPropertyInfo linkInfo)
		{
			bool result = true;
			AccTransactionLines relatedLine = null;

			var linePK = (ZGuid)linkInfo.Value;
			if (linkInfo.Name != JobChargeSchema.Constants.JR_AL_CFXLine && linePK.IsValid)
			{
				relatedLine = AllRelatedPostedLines.FirstOrDefault(x => x.PK == linePK);
				var chargeInfo = new LineRelatedChargeInfo(linkInfo);

				if (relatedLine != null &&
					(!Parent.IsInDatabase || linkInfo.HasChanges || chargeInfo.AmountFieldsHaveChanges ||
					 !relatedLine.IsInDatabase || relatedLine.AL_OSAmountInfo.HasChanges || relatedLine.AL_LineAmountInfo.HasChanges))
				{
					if (!(linkInfo.Name == JobChargeSchema.Constants.JR_AL_ARLine && ((JobCharge)linkInfo.BizObj).BillInInvoiceCurrencyWithLocalSellCurrency))
					{
						ZDecimal osAmount = chargeInfo.OSAmount;
						ZDecimal localAmount = chargeInfo.LocalAmount;
						ZString currency = chargeInfo.CurrencyCode;

						result = relatedLine.AL_LineAmount == localAmount &&
							(relatedLine.AL_RX_NKTransactionCurrency != currency || relatedLine.AL_OSAmount == osAmount);
					}
				}
			}

			if (!result)
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);

				var developerMessage_JobChargeLocalCostOrSellAmtNotEqualRelatedLineAmount = collectorService.GetInfoSafe(Parent.PK, linkInfo.Name == JobChargeSchema.Constants.JR_AL_ARLine ? CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount : CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount);
				var developerMessage_JobChargeOSSellAmtNotEqualRelatedLineOSAmount = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount);
				var developerMessage_JobChargeOSSellExRateNotEqualRelatedLineExRate = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateNotEqualRelatedLineExRate);
				var developerMessage_TransactionLineAmountNotEqualRelatedJobChargeLocalSellAmount = collectorService.GetInfoSafe(linePK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
				var developerMessage_TransactionLineOSAmountNotEqualRelatedJobChargeOSSellAmount = collectorService.GetInfoSafe(linePK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
				var developerMessage_JobChargeOSCostGSTAmountChangedWhenCostPosted = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostGSTAmountChangedWhenCostPosted);

				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeAmountNotEqualRelatedLineAmount_13,
													CriticalValidationMessageTemplate.GetJobChargeAmountNotEqualRelatedLineAmountErrorMessage(relatedLine.AL_LineType),
													Parent.GetJobChargeInfo(),
													Parent.GetJobChargeInternalFieldsInfo(),
													relatedLine.GetTransactionLineInfo(),
													relatedLine.TransactionHeader?.GetTransactionHeaderInfo(),
													developerMessage_JobChargeLocalCostOrSellAmtNotEqualRelatedLineAmount,
													developerMessage_JobChargeOSSellAmtNotEqualRelatedLineOSAmount,
													developerMessage_JobChargeOSSellExRateNotEqualRelatedLineExRate,
													developerMessage_TransactionLineAmountNotEqualRelatedJobChargeLocalSellAmount,
													developerMessage_TransactionLineOSAmountNotEqualRelatedJobChargeOSSellAmount,
													developerMessage_JobChargeOSCostGSTAmountChangedWhenCostPosted);
			}

			return null;
		}

		CriticalValidationResult CheckRelatedPostedTransactionLineReferenceIsNotChanged(ZPropertyInfo linkInfo)
		{
			bool result = true;
			AccTransactionLines relatedLine = null;

			bool isDeleted = Parent.IsDeleted;

			if (Parent.IsInDatabase && (isDeleted || linkInfo.HasChanges) && !linkInfo.OriginalValue.IsEmpty)
			{
				var linePK = (ZGuid)linkInfo.OriginalValue;
				relatedLine = AllRelatedPostedLines.FirstOrDefault(x => x.PK == linePK);

				if (relatedLine != null)
				{
					ZQuery transactionFilter = new ZQuery(AccTransactionHeaderSchema.PK, relatedLine.AL_AH);
					transactionFilter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					var transactionHeader = Parent.Factory.LoadTop1<AccTransactionHeader>(transactionFilter);
					result = transactionHeader == null;
				}
			}

			if (!result)
			{
				if (Parent.IsDeleted)
				{
					var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
					var developerMessage_JobChargeConstructorStackTrace = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace);
					var developerMessage_JobChargeDeleteStackTrace = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeDeleteStackTrace);

					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeRelatedToPostedTransactionLineCannotBeDeleted_4,
														CriticalValidationMessageTemplate.GetJobChargeRelatedToPostedTransactionLineCannotBeDeletedErrorMessage(relatedLine.AL_LineType),
														Parent.GetAllPropertyValues(), relatedLine.GetAllPropertyValues(),
														developerMessage_JobChargeConstructorStackTrace,
														developerMessage_JobChargeDeleteStackTrace);
				}
				else
				{
					return new CriticalValidationResult(CriticalValidationErrorType.JobChargeReferenceToPostedTransactionLineCannotBeChanged_3,
														CriticalValidationMessageTemplate.GetJobChargeReferenceToPostedTransactionLineCannotBeChangedErrorMessage(relatedLine.AL_LineType),
														Parent.GetJobChargeInfo(), relatedLine.GetTransactionLineInfo());
				}
			}

			return null;
		}

		CriticalValidationResult CheckRelatedTransactionLineHasMatchingTaxCode(ZPropertyInfo linkInfo)
		{
			bool result = true;

			var linePK = (ZGuid)linkInfo.Value;
			var relatedLine = AllRelatedPostedLines.FirstOrDefault(x => x.PK == linePK);
			var chargeInfo = new LineRelatedChargeInfo(linkInfo);

			if (relatedLine != null
				&& (((!Parent.IsInDatabase || chargeInfo.TaxRateHasChanges) && chargeInfo.TaxRatePK != relatedLine.AL_AT) ||
					((!Parent.IsInDatabase || chargeInfo.TaxDateHasChanges) && CheckTaxDate(linkInfo, relatedLine)) ||
					((!Parent.IsInDatabase || chargeInfo.TaxClassHasChanges) && chargeInfo.TaxClassPK != relatedLine.AL_A9_VATClass)))
			{
				result = false;
			}

			if (!result)
			{
				var developerMessage = CriticalValidationInfoCollectorService.GetService(Parent.Factory).GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal);

				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeTaxCodeNotEqualRelatedLineTaxCode_7,
													CriticalValidationMessageTemplate.GetJobChargeTaxCodeNotEqualRelatedLineTaxCodeErrorMessage(relatedLine.AL_LineType),
													Parent.GetJobChargeInfo(),
													(NoResString)"Job Consol Cost:",
													Parent.ParentConsolCost?.GetAllPropertyValues(),
													relatedLine.GetTransactionLineInfo(),
													relatedLine.TransactionHeader?.GetTransactionHeaderInfo(),
													developerMessage);
			}

			return null;
		}

		bool CheckTaxDate(ZPropertyInfo linkInfo, AccTransactionLines relatedLine)
		{
			var parent = (JobCharge)linkInfo.BizObj;

			switch (linkInfo.Name)
			{
				case JobChargeSchema.Constants.JR_AL_ARLine:
					return (parent.JR_SellTaxDate != ZDate.Empty && parent.JR_SellTaxDate != relatedLine.AL_TaxDate);
				case JobChargeSchema.Constants.JR_AL_APLine:
					return (parent.JR_CostTaxDate != ZDate.Empty && parent.JR_CostTaxDate != relatedLine.AL_TaxDate);
				case JobChargeSchema.Constants.JR_AL_CFXLine:
					return false;
				default:
					return false;
			}
		}

		CriticalValidationResult CheckHasSkippedDataRefreshBusUpdate()
		{
			CriticalValidationResult result = null;
			var developerMessage = (NoResString)"Job charge skipped data refresh bus update, but it was saved successfully.";

			if (ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().HasSkippedDataRefreshBusUpdate(Parent))
			{
				var chargeInDb = new BusinessObjectFactory().Load<JobCharge>(Parent.PK);

				if (Parent.IsDeleted || (Parent.JR_E6Info.HasChanges && !((ZGuid)Parent.JR_E6Info.OriginalValue).IsEmpty && ((ZGuid)Parent.JR_E6Info.Value).IsEmpty))
				{
					result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_DeletedOrUnlinkedFromConsolCost,
										CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
										developerMessage);
				}
				else
				{
					result = new CriticalValidationResult(CriticalValidationErrorType.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully_4,
										CriticalValidationMessageTemplate.JobChargeSkippedDataRefreshBusUpdateButWasSavedSuccessfully,
										developerMessage,
										Parent.GetAllPropertyValues(),
										Invariant($"Charge in db: {(chargeInDb == null ? (NoResString)"Not found" : chargeInDb.GetAllPropertyValues())}"));
				}
			}

			return result;
		}

		CriticalValidationResult CheckJobChargeMoreThanOneFieldLinkToTheSameLine()
		{
			bool shouldCheck = !Parent.IsInDatabase ||
				Parent.JR_AL_APLineInfo.HasChanges ||
				Parent.JR_AL_ARLineInfo.HasChanges;
			if (shouldCheck && Parent.JR_AL_APLine.IsValid && Parent.JR_AL_ARLine.IsValid && Parent.JR_AL_APLine == Parent.JR_AL_ARLine)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.JobChargeMoreThanOneFieldLinkToTheSameLine,
					CriticalValidationMessageTemplate.JobChargeMoreThanOneFieldLinkToTheSameLineErrorMessage,
					"JR_AL_APLine and JR_AL_ARLine of the Job Charge are linked to the same transaction line.",
					$"JobCharge Info:{Parent.GetAllPropertyValues()}",
					$"Transaction Line Info:{Parent.ARLine?.GetAllPropertyValues()}");
			}

			return null;
		}

		CriticalValidationResult CheckJobChargeHasInvalidProFormaCostOrProFormaRevenue()
		{
			if ((Parent.JR_ProFormaCost == (Parent.Job.JH_ParentTableCode != RatingHeaderSchema.Constants.Prefix))
			|| (Parent.JR_ProFormaRevenue == (Parent.Job.JH_ParentTableCode != RatingHeaderSchema.Constants.Prefix)))
			{
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);

				var developerMessage_JobChargeJR_ProFormaCostSetterCallStack = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaCostSetterCallStack);
				var developerMessage_JobChargeJR_ProFormaRevenueSetterCallStack = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaRevenueSetterCallStack);
				var developerMessage_JobChargeConstructorStackTrace = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace);

				return new CriticalValidationResult(CriticalValidationErrorType.InvalidJobChargeProFormaCostOrProFormaRevenue,
				CriticalValidationMessageTemplate.InvalidJobChargeProFormaCostOrProFormaRevenueErrorMessage,
				Parent.GetAllPropertyValues(),
				developerMessage_JobChargeJR_ProFormaCostSetterCallStack,
				developerMessage_JobChargeJR_ProFormaRevenueSetterCallStack,
				developerMessage_JobChargeConstructorStackTrace
				);
			}

			return null;
		}
	}
}
