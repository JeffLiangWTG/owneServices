using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLinesCriticalValidation : CriticalValidation<AccTransactionLines>
	{
		public AccTransactionLinesCriticalValidation(AccTransactionLines parent)
			: base(parent)
		{
		}

		#region Check Grouping Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Only For developers, Developer info only.")]
		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return CheckTransactionLineAmountExceedMaximumAllowedAmount();

			if (Parent.AL_AH.IsValid && Parent.TransactionHeader != null && Parent.TransactionHeader.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionHeaderWasCriticallyChangedByDataRefreshBus_2,
													CriticalValidationMessageTemplate.TransactionHeaderWasCriticallyChangedByDataRefreshBus,
													(NoResString)"Standard validation must catch this case and doesn't not allow to save.",
													GetHeaderWithLinesInfo());
			}

			if (!CheckReversedWIPOrACRWasNotDetachedFromJobCharge())
			{
				//report to edienterprise with more information about how WIP/ACR is still linked
				ExceptionReporter.Instance.ReportDeveloperException(ReversedJobRelatedLineErrorKey, ReversedJobRelatedLineErrorMessageTrap, new Exception(ReversedJobRelatedLineErrorMessageTrap));
			}

			if (!CheckALAHIsNotNullForCstRevUctLines())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineWithoutTransactionHeader_2,
													CriticalValidationMessageTemplate.TransactionLineWithoutTransactionHeaderErrorMessage,
													Parent.GetTransactionLineInfo());
			}

			if (!CheckAmountOnAccountReceivableTransactionIsPositiveOrAllowed())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowed,
													CriticalValidationMessageTemplate.TransactionLineNegativeAmountOnAccountReceivableTransactionsIsNotAllowedErrorMessage(Parent.Company.Country.Description, Parent.TransactionHeader.AH_TransactionType),
													Parent.GetTransactionLineInfo());
			}

			if (!CheckJobRelatedLineHasRevenueRecognitionType())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.JobTransactionLineWithoutRevenueRecognitionType_7,
													CriticalValidationMessageTemplate.JobTransactionLineWithoutRevenueRecognitionTypeErrorMessage,
													Parent.GetTransactionLineInfo(),
													GetHeaderWithLinesInfo(),
													Parent.AL_JH.IsValid && Parent.Job != null ? Parent.Job.GetJobInfo() : string.Empty,
													Parent.AL_JH.IsValid && Parent.Job != null ? Parent.Job.GetJobRevenueRecognitionInfo() : string.Empty,
													CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.AL_AC, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation),
													CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.AL_AC, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringLineCreation),
													string.Format(Culture.Invariant, (NoResString)"\r\nPost Save Revenue Recognition Details:\r\n{0}", Parent.Job.GetRevenueRecognitionDetails(Parent.ChargeCode)),
													CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeSetFromValidToEmpty));
			}

			string chargeInfo;
			string userMessage;
			string developerMessage;
			JobRelatedLineReferencesJobChargeResult hasCharges = CheckJobRelatedLineHasReferencingJobCharge(out chargeInfo, out userMessage, out developerMessage);

			switch (hasCharges)
			{
				case JobRelatedLineReferencesJobChargeResult.NoCharge:
					{
						yield return GetTransactionLineWithoutJobChargeCriticalValidationResult(developerMessage);
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.MoreThanOneCharge:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.JobTransactionLineWithMoreThanOneJobCharge_LineSide_5,
															CriticalValidationMessageTemplate.JobTransactionLineWithMoreThanOneJobCharge_LineSideErrorMessage,
															Parent.GetTransactionLineInfo(),
															chargeInfo);
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.IncorrectAmount:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineLocalAmountNotEqualJobChargeLocalAmount_9,
															CriticalValidationMessageTemplate.GetTransactionLineLocalAmountNotEqualJobChargeLocalAmountErrorMessage(userMessage),
															Parent.GetTransactionLineInfo(),
															chargeInfo,
															Parent.TransactionHeader != null ? Parent.TransactionHeader.GetTransactionHeaderInfo() : string.Empty,
															developerMessage);
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.IncorrectTaxCode:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineTaxCodeNotEqualJobChargeTaxCode_5,
															CriticalValidationMessageTemplate.GetTransactionLineTaxCodeNotEqualJobChargeTaxCodeErrorMessage(userMessage),
															Parent.GetTransactionLineInfo(),
															chargeInfo,
															CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineHasChangedAfterItIsPosted_TaxId));
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.IncorrectTaxClass:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineTaxClassNotEqualJobChargeTaxClass_4,
															CriticalValidationMessageTemplate.GetTransactionLineTaxClassNotEqualJobChargeTaxClassErrorMessage(userMessage),
															Parent.GetTransactionLineInfo(),
															chargeInfo,
															CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineHasChangedAfterItIsPosted_TaxMessage));
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.IncorrectSupplyType:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineSupplyTypeNotEqualJobChargeSupplyType_2,
															CriticalValidationMessageTemplate.GetTransactionLineSupplyTypeNotEqualJobChargeSupplyTypeErrorMessage(userMessage),
															Parent.GetTransactionLineInfo(), chargeInfo);
						break;
					}
				case JobRelatedLineReferencesJobChargeResult.NoChargeToSave:
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.JobTransactionLineWithoutJobChargeToSave_3,
															CriticalValidationMessageTemplate.JobTransactionLineWithoutJobChargeToSaveErrorMessage,
															Parent.GetTransactionLineInfo(),
															chargeInfo);
						break;
					}
			}

			if (!CheckWIPHaveDebtorCode())
			{
				var relatedCharge = Parent.LoadRelatedJobCharge();

				StringBuilder message = new StringBuilder();
				message.AppendLine();
				message.AppendLine();
				message.Append(Parent.Job.GetJobInfo());
				message.AppendLine();
				message.AppendLine();
				message.Append(Parent.GetTransactionLineInfo());

				if (relatedCharge == null)
				{
					message.AppendLine();
					message.AppendLine();
					message.Append(Res.GetString("47b21187-5ef0-46f4-b992-dae3b5d044fd", "Job Charge: Not Set"));
				}
				else
				{
					var debtor = Parent.Factory.Load<OrgHeader>(Parent.Job.GetDebtorPK(relatedCharge));
					if (debtor != null)
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(Res.GetString("857fbe19-70de-4248-9608-69c2468547c0", "Default Debtor: {0}", debtor.GetOrganizationInfo(relatedCharge.ChargeCode.AC_GC)));
					}
					else
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(Res.GetString("ee3c45b4-cb35-447b-8962-90b37499a0ee", "Default Debtor: Not Set"));
					}

					message.AppendLine();
					message.AppendLine();
					message.Append(relatedCharge.GetJobChargeInfo());

					string postedCostMsg = relatedCharge.GetPostedCostInfo();
					if (!string.IsNullOrEmpty(postedCostMsg))
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(postedCostMsg);
					}

					string consolCostMsg = relatedCharge.GetConsolCostInfo();
					if (!string.IsNullOrEmpty(consolCostMsg))
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(consolCostMsg);
					}
				}

				yield return new CriticalValidationResult(CriticalValidationErrorType.WIPMustHaveDebtor_1,
													CriticalValidationMessageTemplate.WIPMustHaveDebtorErrorMessage,
													CriticalValidationMessageTemplate.WIPMustHaveDebtorErrorMessage.GetUnresolvedString(),
													message.ToString());
			}

			if (!CheckACRHaveCreditorCode())
			{
				var relatedCharge = Parent.LoadRelatedJobCharge();

				IJobInvoicingPlugIn pluginData = null;
				if (Parent.Job.Parent == null)
				{
					Parent.Job.InitializeParentFromGenericJobWithSettingDefaults();
				}

				pluginData = Parent.Job.Parent as IJobInvoicingPlugIn;

				StringBuilder message = new StringBuilder();
				message.AppendLine();
				message.AppendLine();
				message.Append(Parent.Job.GetJobInfo());
				message.AppendLine();
				message.AppendLine();
				message.Append(Parent.GetTransactionLineInfo());

				if (relatedCharge == null)
				{
					message.AppendLine();
					message.AppendLine();
					message.Append(Res.GetString("47b21187-5ef0-46f4-b992-dae3b5d044fd", "Job Charge: Not Set"));
				}
				else
				{
					OrgHeader creditor = null;
					if (pluginData != null)
					{
						creditor = pluginData.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(relatedCharge.ChargeCode, relatedCharge.JR_InvoiceType));
					}

					if (creditor != null)
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(Res.GetString("14053863-8263-43ee-aaf2-aca9046df155", "Default Creditor: {0}", creditor.GetOrganizationInfo(relatedCharge.ChargeCode.PK)));
					}
					else
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(Res.GetString("5d491c53-28fb-40b4-a5f3-cdaae74a3b19", "Default Creditor: Not Set, Job Parent: {0}", pluginData == null ? "Not Set" : "PK = " + pluginData.PK));
					}

					message.AppendLine();
					message.AppendLine();
					message.Append(relatedCharge.GetJobChargeInfo());

					string consolCostMsg = relatedCharge.GetConsolCostInfo();
					if (!string.IsNullOrEmpty(consolCostMsg))
					{
						message.AppendLine();
						message.AppendLine();
						message.Append(consolCostMsg);
					}
				}

				yield return new CriticalValidationResult(CriticalValidationErrorType.AccrualMustHaveCreditor_1,
													CriticalValidationMessageTemplate.AccrualMustHaveCreditorErrorMessage,
													message.ToString());
			}

			if (!CheckIsLocalAmountEqualToForeignAmount())
			{
				var additionalInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory)?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineLocalAmountInconsistentWithOSAmountWhenExRateIsOne);
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineLocalAmountNotEqualForeignWithExRate_5,
													CriticalValidationMessageTemplate.TransactionLineLocalAmountNotEqualForeignWithExRate1ErrorMessage,
													Parent.GetTransactionLineInfo(),
													Parent.TransactionHeader?.GetTransactionHeaderInfo(),
													additionalInfo);
			}

			if (!CheckJobIsNotEmptyWhenChargeCodeRequiredIt())
			{
				var errorMessage = CriticalValidationMessageTemplate.GetTransactionLineHasNoJobWhenRequiredByChargeCodeErrorMessage(Parent.ChargeCode.AC_Code);

				var helper = ObjectFactory.Get<IAccTransactionHeaderCriticalValidationHelper>();
				if (Parent.TransactionHeader.IsInDatabase || !helper.IsReversalOfOriginalTransaction(Parent.TransactionHeader))
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2,
														errorMessage,
														Parent.GetTransactionLineInfo(), Parent.GetTransactionLineInfo());
				}
			}

			yield return CheckTransactionLineHasValidTaxIdAndTaxMessageMapping();

			yield return CheckWIPOrACRRelatedChargeHasMatchingOrganisation();

			if (!CheckSubAccountDetailsIsEmptyWhenJobIsPresent())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.SubAccountDetailsCannotBeSetForJobRelatedLines_1,
													CriticalValidationMessageTemplate.SubAccountDetailsCannotBeSetForJobRelatedLinesErrorMessage,
													Parent.GetTransactionLineInfo());
			}

			if (!CheckTransactionLineBranchBelongToTransactionHeaderCompany())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.AccTransactionLineBranchDoesNotBelongToAccTransactionHeaderCompany_2,
									CriticalValidationMessageTemplate.TransactionLineBranchDoesNotBelongToTransactionHeaderCompanyErrorMessage,
									Parent.GetTransactionLineInfo());
			}

			if (CheckTransactionLineWithoutGLAccountAndChargeCode())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineWithoutGLAccountAndChargeCode_2,
													CriticalValidationMessageTemplate.GetTransactionLineWithoutGLAccountAndChargeCodeErrorMessage(Parent.AL_LineType),
													Parent.GetTransactionLineInfo());
			}

			var transactionHeader = Parent.TransactionHeader;
			if (!CheckTransactionLineTypeIsCompatibleWithTransactionHeader(transactionHeader))
			{
				var errorMessage = Res.GetString("4cd75d96-56f5-41e5-a665-a0cedbbfcdd3", "Transaction Line Type {0} is not compatible with Transaction Header Ledger {1} and Transaction Type {2}", Parent.AL_LineType, transactionHeader.AH_Ledger, transactionHeader.AH_TransactionType);
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineTypeNotCompatibleWithTransactionHeader_4,
													CriticalValidationMessageTemplate.TransactionLineTypeNotCompatibleWithTransactionHeaderErrorMessage,
													errorMessage,
													transactionHeader?.GetTransactionHeaderInfo() ?? string.Empty,
													Parent.GetTransactionLineInfo());
			}

			if (!CheckTransactionLineTaxBranchEqualToTransactionHeaderTaxBranch())
			{
				var errorMessage = Res.GetString("2DA9DDF2-AB05-43F6-9A4F-5DFC8381D92B", "Transaction header tax branch: {0}, transaction line tax branch: {1}.", Parent.TransactionHeader.AH_GB_TaxBranch, Parent.AL_GB_TaxBranch);
				yield return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineTaxBranchNotEqualToTransactionHeaderTaxBranch,
									CriticalValidationMessageTemplate.GetTransactionLineTaxBranchNotEqualToTransactionHeaderTaxBranchErrorMessage(errorMessage),
									transactionHeader?.GetTransactionHeaderInfo() ?? string.Empty,
									Parent.GetTransactionLineInfo());
			}

			yield return CheckLineTaxBranchAreSyncWithChargeTaxBranch();
			yield return CheckReverseDate();
		}

		CriticalValidationResult CheckReverseDate()
		{
			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IReverseDateValidation>;
			var error = provider?.Get()?.ValidateReverseDate(Parent);
			if (error != null)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.InvalidReverseDate, error);
			}

			return null;
		}

		#endregion

		#region OnSaving Check methods

		CriticalValidationResult CheckTransactionLineAmountExceedMaximumAllowedAmount()
		{
			var transactionHeader = Parent.TransactionHeader;

			if (Parent.AL_LineType != TransactionLineTypes.Accrual &&
				Parent.AL_LineType != TransactionLineTypes.WIP &&
				(!Parent.IsInDatabase || Parent.AL_LineAmountInfo.HasChanges || Parent.AL_GSTVATInfo.HasChanges) &&
				!(Parent.TransactionHeader?.IsCancelled ?? false))
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AL_GC, CriticalValidationHelpers.MaximumAmountLevel.Line, Parent.AL_LineAmount, Parent.AL_GSTVAT);
				if (message != null)
				{
					return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineAmountExceedMaximumAllowedAmount, message);
				}
			}

			return null;
		}

		bool CheckTransactionLineTypeIsCompatibleWithTransactionHeader(AccTransactionHeader transactionHeader)
		{
			bool result = true;
			if (transactionHeader != null &&
				(!transactionHeader.IsInDatabase
				|| transactionHeader.AH_LedgerInfo.HasChanges
				|| transactionHeader.AH_TransactionTypeInfo.HasChanges))
			{
				result = AccTransactionLinesCompatibilityMatrix.IsLineTypeCompatibleWithTransactionHeader(Parent.AL_LineType, transactionHeader.AH_Ledger, transactionHeader.AH_TransactionType);
			}
			return result;
		}

		protected enum JobRelatedLineReferencesJobChargeResult
		{
			SingleCharge, MoreThanOneCharge, NoCharge, IncorrectAmount, NoChargeToSave, IncorrectTaxCode, IncorrectTaxDate, IncorrectTaxClass, IncorrectSupplyType
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		JobRelatedLineReferencesJobChargeResult CheckJobRelatedLineHasReferencingJobCharge(out string chargeInfo, out string userMessage, out string developerMessage)
		{
			chargeInfo = "";
			userMessage = "";
			developerMessage = "";

			JobRelatedLineReferencesJobChargeResult result = JobRelatedLineReferencesJobChargeResult.SingleCharge;
			if (!Parent.AL_JH.IsEmpty && !Parent.AL_AH.IsEmpty &&
				(Parent.AL_LineType == TransactionLineTypes.Revenue || Parent.AL_LineType == TransactionLineTypes.Cost || Parent.AL_LineType == TransactionLineTypes.UnapprovedCost) &&
				!Parent.TransactionHeader.AH_IsCancelled)
			{
				ZQuery query = new ZQuery(JobChargeSchema.JR_JH, Parent.AL_JH);
				ZQuery queryMultipleLinks = null;

				bool shouldCheckAmounts = false;
				if (Parent.TransactionHeader.AH_Ledger == LedgerTypes.JobCosting && Parent.TransactionHeader.AH_TransactionType == TransactionTypes.Journal)
				{
					query.AddToFilter(JobChargeSchema.JR_AL_CFXLine, Parent.PK);
				}
				else if (Parent.TransactionHeader.AH_Ledger == LedgerTypes.JobCosting && Parent.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal)
				{
					query.AddToFilter(GetRelatedChargesSubqueryLinkedByAPARLine(Parent.PK));
				}
				else if (Parent.AL_LineType == TransactionLineTypes.Revenue)
				{
					query.AddToFilter(JobChargeSchema.JR_AL_ARLine, Parent.PK);
					queryMultipleLinks = CreateMultiLinkQuery();
					shouldCheckAmounts = true;
				}
				else
				{
					query.AddToFilter(JobChargeSchema.JR_AL_APLine, Parent.PK);
					queryMultipleLinks = CreateMultiLinkQuery();
					shouldCheckAmounts = true;
				}
				query.MaximumRows = 2;

				JobCharge[] charges = Parent.Factory.Load<JobCharge>(query);

				if (!Parent.IsInDatabase)
				{
					if (charges.Length == 0)
					{
						result = JobRelatedLineReferencesJobChargeResult.NoCharge;
						developerMessage = string.Concat((NoResString)"Query:", query.GetAsWhereClause(true).Trim(), System.Environment.NewLine,
							GetInvoiceAndChargesDetailedInfo());
					}
					else if (charges.Length > 1)
					{
						result = JobRelatedLineReferencesJobChargeResult.MoreThanOneCharge;
						chargeInfo = CreateMultiLinkErrorMessage(charges);
					}

					if (IsJobRelatedLineReferencesValid(result) && queryMultipleLinks != null)
					{
						var multiLinkedCharges = Parent.Factory.Load<JobCharge>(queryMultipleLinks);
						if (multiLinkedCharges.Length > 1)
						{
							result = JobRelatedLineReferencesJobChargeResult.MoreThanOneCharge;
							chargeInfo = CreateMultiLinkErrorMessage(multiLinkedCharges);
						}
					}
				}

				if (IsJobRelatedLineReferencesValid(result) && charges.Length == 1)
				{
					var charge = charges[0];
					var linkedViaARLine = charge.JR_AL_ARLine == Parent.PK;
					var linkedviaCFXLine = charge.JR_AL_CFXLine == Parent.PK;
					var column = linkedViaARLine ? JobChargeSchema.Constants.JR_AL_ARLine : (linkedviaCFXLine ? JobChargeSchema.Constants.JR_AL_CFXLine : JobChargeSchema.Constants.JR_AL_APLine);

					//Here we check if found charge will really be saved in db during this saving.
					//This is applicable for charges because they override IsSavedByFactory and so can be skipped from saving.
					//IsSavedByFactory can't be used directly here as charge is usually loaded by Critical Validation as another JobCharge instance and IsSavedByFactory is a nonpersistent property.
					//At the beginning of saving process Factory uses ZDataUtils to set "should be saved" flag for each its data row based on the IsSavedByFactory flag.
					//Later ZDataUtils.ShouldRowBeSaved method is used to define if there are any data rows in the Factory that should be saved.
					//That's why DataUtils. CheckJobRelatedLineHasReferencingJobCharge method is used here as replacement for IsSavedByFactory.
					if (!ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)charge).Row) &&
						(!charge.IsInDatabase || (ZGuid)charge.ZPropertyInfoHash[column].OriginalValue != Parent.PK))
					{
						result = JobRelatedLineReferencesJobChargeResult.NoChargeToSave;
						chargeInfo = (charge.IsInDatabase ? Res.GetString("80839882-ed76-4054-a5d8-8fcf39be661f", "Charge is in database.") : Res.GetString("d5c4321c-612b-4858-a5c6-00e5bca83008", "Charge is not in database.")) +
							" " + charge.GetJobChargeInfo();
					}
					else if (shouldCheckAmounts)
					{
						if (linkedViaARLine)
						{
							if (charge.JR_LocalSellAmt != Parent.AL_LineAmount && !(charge.BillInInvoiceCurrency && charge.IsSellLocal))
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectAmount;
								userMessage = Res.GetString("7fcada94-2754-45ec-ba61-9c3b98dfa546", "Charge amount: {0}, line amount: {1}.", charge.JR_LocalSellAmt, Parent.AL_LineAmount);

								var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
								if (collectorService != null)
								{
									var info = new ZStringBuilder();
									info.AppendLine(collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenPostingReceivableCharges));
									info.AppendLine(collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineCurrencyChangeWhenPostingReceivableCharges));
									info.AppendLine(collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineExChangeRateChangeWhenPostingReceivableCharges));

									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtChangeWhenPostingReceivableCharges));
									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeSellCurrencyChangeWhenPostingReceivableCharges));
									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateChangeWhenPostingReceivableCharges));
									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeSellCurrencyChangeWhenPostingOverseasAgentChargesFromConsol));
									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating));
									info.AppendLine(collectorService.GetInfo(Parent.AL_AH, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction));
									info.AppendLine(collectorService.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty));

									developerMessage = info.ToString();
								}
							}
							if ((!Parent.IsInDatabase || Parent.AL_ATInfo.HasChanges) && charge.JR_AT_SellGSTRate != Parent.AL_AT)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectTaxCode;
								var chargeTaxCode = charge.SellGSTRate != null ? charge.SellGSTRate.AT_Code.ToString() : EmptyValueString;
								var lineTaxCode = Parent.TaxRate != null ? Parent.TaxRate.AT_Code.ToString() : EmptyValueString;
								userMessage = Res.GetString("d7e3a8c9-ac08-4cb7-a6e1-502e9fda8c85", "Charge tax code: {0}, line tax code: {1}.", chargeTaxCode, lineTaxCode);
							}
							if ((!Parent.IsInDatabase || Parent.AL_A9_VATClassInfo.HasChanges) && charge.JR_A9_SellVATClass != Parent.AL_A9_VATClass)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectTaxClass;
								var chargeTaxClass = charge.SellVATClass != null ? charge.SellVATClass.A9_Code.ToString() : EmptyValueString;
								var lineTaxClass = Parent.VATClass != null ? Parent.VATClass.A9_Code.ToString() : EmptyValueString;
								userMessage = Res.GetString("b7e7cb26-bb14-4b19-ac96-012921673f2a", "Charge tax class: {0}, line tax class: {1}.", chargeTaxClass, lineTaxClass);
							}
							if ((!Parent.IsInDatabase || Parent.AL_SupplyTypeInfo.HasChanges) && charge.JR_SellSupplyType != Parent.AL_SupplyType)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectSupplyType;
								userMessage = Res.GetString("4819D285-22A0-4564-AC73-2428C1FBFD6D", "Charge supply type: {0}, line supply type: {1}.", charge.JR_SellSupplyType, Parent.AL_SupplyType);
							}
						}
						else
						{
							if ((Parent.AL_LineType == TransactionLineTypes.Cost || Parent.AL_LineType == TransactionLineTypes.UnapprovedCost) && charge.JR_LocalCostAmt != -Parent.AL_LineAmount)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectAmount;
								userMessage = Res.GetString("7fcada94-2754-45ec-ba61-9c3b98dfa546", "Charge amount: {0}, line amount: {1}.", charge.JR_LocalCostAmt, -Parent.AL_LineAmount);
							}
							else if (Parent.AL_LineType == TransactionLineTypes.Revenue && charge.JR_LocalCostAmt != -Parent.AL_LineAmount)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectAmount;
								userMessage = Res.GetString("7fcada94-2754-45ec-ba61-9c3b98dfa546", "Charge amount: {0}, line amount: {1}.", charge.JR_LocalCostAmt, -Parent.AL_LineAmount);
							}
							if ((!Parent.IsInDatabase || Parent.AL_ATInfo.HasChanges) && charge.JR_AT_CostGSTRate != Parent.AL_AT)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectTaxCode;
								var chargeTaxCode = charge.CostGSTRate != null ? charge.CostGSTRate.AT_Code.ToString() : EmptyValueString;
								var lineTaxCode = Parent.TaxRate != null ? Parent.TaxRate.AT_Code.ToString() : EmptyValueString;
								userMessage = Res.GetString("d7e3a8c9-ac08-4cb7-a6e1-502e9fda8c85", "Charge tax code: {0}, line tax code: {1}.", chargeTaxCode, lineTaxCode);
							}
							if ((!Parent.IsInDatabase || Parent.AL_TaxDateInfo.HasChanges) && (charge.JR_CostTaxDate != ZDate.Empty && charge.JR_CostTaxDate != Parent.AL_TaxDate))
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectTaxDate;
								userMessage = Res.GetString("4cc928e4-f10f-4364-872a-bdd7979bf9e7", "Charge tax date: {0}, line tax date: {1}.", charge.JR_CostTaxDate, Parent.AL_TaxDate);
							}
							if ((!Parent.IsInDatabase || Parent.AL_A9_VATClassInfo.HasChanges) && charge.JR_A9_CostVATClass != Parent.AL_A9_VATClass)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectTaxClass;
								var chargeTaxClass = charge.CostVATClass != null ? charge.CostVATClass.A9_Code.ToString() : EmptyValueString;
								var lineTaxClass = Parent.VATClass != null ? Parent.VATClass.A9_Code.ToString() : EmptyValueString;
								userMessage = Res.GetString("b7e7cb26-bb14-4b19-ac96-012921673f2a", "Charge tax class: {0}, line tax class: {1}.", chargeTaxClass, lineTaxClass);
							}
							if ((!Parent.IsInDatabase || Parent.AL_SupplyTypeInfo.HasChanges) && charge.JR_CostSupplyType != Parent.AL_SupplyType)
							{
								result = JobRelatedLineReferencesJobChargeResult.IncorrectSupplyType;
								userMessage = Res.GetString("4819D285-22A0-4564-AC73-2428C1FBFD6D", "Charge supply type: {0}, line supply type: {1}.", charge.JR_CostSupplyType, Parent.AL_SupplyType);
							}
						}

						if (result != JobRelatedLineReferencesJobChargeResult.SingleCharge) // Found some error
						{
							chargeInfo = charge.GetJobChargeInfo();

							if (!linkedViaARLine && (result == JobRelatedLineReferencesJobChargeResult.IncorrectAmount || result == JobRelatedLineReferencesJobChargeResult.IncorrectTaxClass || result == JobRelatedLineReferencesJobChargeResult.IncorrectTaxCode || result == JobRelatedLineReferencesJobChargeResult.IncorrectSupplyType))
							{
								var consolCostInfo = charge.GetConsolCostInfo();
								if (!string.IsNullOrEmpty(consolCostInfo))
								{
									chargeInfo += "\r\n" + consolCostInfo;
								}
							}
						}
					}
				}
			}

			return result;
		}

		ZQuery GetRelatedChargesSubqueryLinkedByAPARLine(ZGuid linePK)
		{
			var result = new ZQuery();
			result.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_ARLine, linePK);
			result.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_APLine, linePK);
			return result;
		}

		ZQuery CreateMultiLinkQuery()
		{
			var query = new ZQuery(JobChargeSchema.JR_JH, Parent.AL_JH);
			query.AddToFilter(GetRelatedChargesSubqueryLinkedByAPARLine(Parent.PK));
			query.MaximumRows = 2;
			query.FetchOnlyFromLocalCache = true;
			return query;
		}

		string CreateMultiLinkErrorMessage(JobCharge[] charges)
		{
			var chargeInfos = new ZStringBuilder();
			foreach (var charge in charges)
			{
				chargeInfos.Append(charge.GetJobChargeInfo());
			}
			return chargeInfos.ToStringWithNewLineBetweenAppends();
		}

		bool IsJobRelatedLineReferencesValid(JobRelatedLineReferencesJobChargeResult result) => result == JobRelatedLineReferencesJobChargeResult.SingleCharge;

		internal CriticalValidationResult GetTransactionLineWithoutJobChargeCriticalValidationResult(string developerMessage)
		{
			var criticalValidationType = CriticalValidationErrorType.JobTransactionLineWithoutJobCharge_3;
			string extraMsgForRevenueTransactionLineWithoutJobCharge = string.Empty;
			string extraMsgForDeletingChargeWithLinkedREVLineNotInDb = string.Empty;
			string extraMsgForTransactionHeaderConstructorStackTraceInfo = string.Empty;
			var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);

			switch (Parent.AL_LineType)
			{
				case TransactionLineTypes.Cost:
					criticalValidationType = CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5;
					if (Parent.TransactionHeader != null && !Parent.TransactionHeader.IsInDatabase)
					{
						var extraMsgForTransactionHeaderConstructorStackTrace = (Parent.TransactionHeader as IHaveConstructorStackTrace)?.ConstructorStackTrace?.ToString();
						if (string.IsNullOrEmpty(extraMsgForTransactionHeaderConstructorStackTrace))
						{
							extraMsgForTransactionHeaderConstructorStackTrace = collectorService?.GetInfo(Parent.TransactionHeader.PK, CriticalValidationInfoCollectorServiceKeyType.APInvoiceConstructor);
						}
						extraMsgForTransactionHeaderConstructorStackTraceInfo = FormattableString.Invariant($"Header Object constructor call stack: {extraMsgForTransactionHeaderConstructorStackTrace}");
					}
					break;
				case TransactionLineTypes.Revenue:
					criticalValidationType = CriticalValidationErrorType.RevenueTransactionLineWithoutJobCharge_6;

					extraMsgForRevenueTransactionLineWithoutJobCharge = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueTransactionLineWithoutJobCharge);
					extraMsgForDeletingChargeWithLinkedREVLineNotInDb = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.DeletingChargeWithLinkedREVLineNotInDb);
					break;
				case TransactionLineTypes.UnapprovedCost:
					criticalValidationType = CriticalValidationErrorType.UnapprovedCostTransactionLineWithoutJobCharge_4;
					break;
			}

			return new CriticalValidationResult(criticalValidationType,
												CriticalValidationMessageTemplate.JobTransactionLineWithoutJobChargeErrorMessage(Parent.AL_LineType.ToString()),
												Parent.GetTransactionLineInfo(), developerMessage, extraMsgForRevenueTransactionLineWithoutJobCharge, extraMsgForDeletingChargeWithLinkedREVLineNotInDb,
												extraMsgForTransactionHeaderConstructorStackTraceInfo);
		}

		static string EmptyValueString
		{
			get { return Res.GetString("C997E824-B5F9-4961-9F9D-0B8C8D6529CB", "<empty>"); }
		}

		string GetInvoiceAndChargesDetailedInfo()
		{
			var detailedInfo = new ZStringBuilder();

			var transactionHeader = Parent.TransactionHeader;

			if (transactionHeader != null)
			{
				detailedInfo.AppendLine(transactionHeader.GetTransactionHeaderInfo());

				var lines = Parent.Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AH, Parent.AL_AH).AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AL_GC));

				foreach (var line in lines)
				{
					detailedInfo.AppendLine(line.GetTransactionLineInfo());
				}
			}

			if (Parent.Job != null)
			{
				detailedInfo.AppendLine(Parent.Job.GetJobInfo());
				var charges = Parent.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, Parent.Job.PK));
				foreach (var charge in charges)
				{
					detailedInfo.AppendLine(charge.GetJobChargeInfo());
				}
			}

			return detailedInfo.ToString();
		}

		string GetHeaderWithLinesInfo()
		{
			var detailedInfo = new ZStringBuilder();

			var transactionHeader = Parent.TransactionHeader;

			if (transactionHeader != null)
			{
				detailedInfo.AppendLine();
				detailedInfo.AppendLine(transactionHeader.GetTransactionHeaderInfo());

				var lines = Parent.Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AH, Parent.AL_AH).AddToFilter(AccTransactionLinesSchema.AL_GC, Parent.AL_GC));

				foreach (var line in lines)
				{
					detailedInfo.AppendLine(line.GetTransactionLineInfo());
				}
			}
			return detailedInfo.ToString();
		}

		bool CheckIsLocalAmountEqualToForeignAmount()
		{
			bool result = true;

			if (Parent.AL_ExchangeRate == 1M
				&& Parent.AL_RX_NKTransactionCurrency == Parent.Company.GC_RX_NKLocalCurrency
				&& (
					!Parent.IsInDatabase
					|| Parent.AL_ExchangeRateInfo.HasChanges
					|| Parent.AL_LineAmountInfo.HasChanges
					|| Parent.AL_GSTVATInfo.HasChanges
					|| Parent.AL_OSAmountInfo.HasChanges
				)
			)
			{
				result = Math.Abs(Parent.AL_LineAmount + Parent.AL_GSTVAT) == Math.Abs(Parent.AL_OSAmount);
			}

			return result || (Parent.TransactionHeader != null && Parent.TransactionHeader.HasContext(BusinessContext.CheckTransactionLineTotalsMatchTransactionHeaderAmountsIsSuspended));
		}

		bool CheckWIPHaveDebtorCode()
		{
			bool result = true;
			if (!Parent.IsInDatabase && Parent.AL_LineType == TransactionLineTypes.WIP && Parent.AL_OH.IsEmpty)
			{
				result = !ObjectFactory.Get<IAccounting>().WIPMustHaveDebtorCode(RelatedCompanyPK.ToGuid());
			}

			return result;
		}

		bool CheckACRHaveCreditorCode()
		{
			bool result = true;

			if (!Parent.IsInDatabase && Parent.AL_LineType == TransactionLineTypes.Accrual && Parent.AL_OH.IsEmpty)
			{
				result = !ObjectFactory.Get<IAccounting>().AccrualMustHaveCreditorCode(RelatedCompanyPK.ToGuid());
			}

			return result;
		}

		ZGuid RelatedCompanyPK
		{
			get
			{
				return !Parent.AL_GC.IsEmpty ? Parent.AL_GC :
					Parent.Branch != null ? Parent.Branch.GB_GC : GlbCompany.CurrentCompany.PK;
			}
		}

		protected bool CheckReversedWIPOrACRWasNotDetachedFromJobCharge()
		{
			bool result = true;

			if (!Parent.AL_JH.IsEmpty &&
				(Parent.AL_LineType == TransactionLineTypes.Accrual || Parent.AL_LineType == TransactionLineTypes.WIP) &&
				Parent.AL_ReverseDateInfo.HasChanges && !Parent.AL_ReverseDate.IsEmpty && Parent.AL_ReverseDateInfo.OriginalValue.IsEmpty)
			{
				result = Parent.LoadRelatedJobCharge() == null;
			}

			return result;
		}

		CriticalValidationResult CheckWIPOrACRRelatedChargeHasMatchingOrganisation()
		{
			if ((Parent.AL_LineType == TransactionLineTypes.Accrual || Parent.AL_LineType == TransactionLineTypes.WIP) && (!Parent.IsInDatabase || Parent.AL_OHInfo.HasChanges))
			{
				var relatedCharge = Parent.LoadRelatedJobCharge();

				if (relatedCharge != null && Parent.Branch != null)
				{
					var result = true;

					var orgPK = Parent.AL_LineType == TransactionLineTypes.WIP ? relatedCharge.JR_OH_SellAccount : relatedCharge.JR_OH_CostAccount;
					var companydata = OrgCompanyData.Load(Parent.Factory, orgPK, Parent.Branch != null ? Parent.Branch.GB_GC : ZGuid.Empty);

					var extraMsgForDebtorCreditorInfo = new ZStringBuilder();

					if (Parent.AL_LineType == TransactionLineTypes.WIP)
					{
						extraMsgForDebtorCreditorInfo.AppendLine((NoResString)"Is Debtor For Company: " + relatedCharge.SellAccount?.IsDebtorForCompany(relatedCharge.Company).ToYesNoString());
						var expected = (companydata != null && companydata.OB_IsDebtor) ? orgPK : ZGuid.Empty;
						result = expected == Parent.AL_OH;
					}
					else
					{
						extraMsgForDebtorCreditorInfo.AppendLine((NoResString)"Is Creditor For Company: " + relatedCharge.CostAccount?.IsCreditorForCompany(relatedCharge.Company).ToYesNoString());
						var expected = (companydata != null && companydata.OB_IsCreditor) ? orgPK : ZGuid.Empty;
						result = expected == Parent.AL_OH;
					}

					if (!result)
					{
						var relatedChargeMessage = string.Concat("RelatedCharge:", System.Environment.NewLine, relatedCharge.GetAllPropertyValues());

						extraMsgForDebtorCreditorInfo.AppendLine((NoResString)"Branch Company PK: " + Parent.Branch.GB_GC.ToString());
						extraMsgForDebtorCreditorInfo.AppendLine((NoResString)"Org PK: " + orgPK.ToString());

						var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
						var extraMsgInfo = new ZStringBuilder();
						extraMsgInfo.AppendLine(collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine));
						extraMsgInfo.AppendLine(collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine_InDb));
						extraMsgInfo.AppendLine(collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet));

						return new CriticalValidationResult(CriticalValidationErrorType.WIPACROrganisationDoesNotMatchOneOnJobCharge_10,
															CriticalValidationMessageTemplate.WIPACROrganisationDoesNotMatchOneOnJobChargeErrorMessage,
															Parent.GetAllPropertyValues(),
															relatedChargeMessage,
															extraMsgInfo.ToString(),
															extraMsgForDebtorCreditorInfo.ToString());
					}
				}
			}

			return null;
		}

		protected bool CheckSubAccountDetailsIsEmptyWhenJobIsPresent() =>
			!(
				(!Parent.IsInDatabase || Parent.AL_JHInfo.HasChanges) &&
				!Parent.AL_JH.IsEmpty &&
				(ObjectFactory.Get<IAccounting>()?.HasSubAccounts(Parent) ?? false)
			);

		protected bool CheckALAHIsNotNullForCstRevUctLines()
		{
			if (Parent.AL_AH.IsEmpty && (Parent.AL_LineType == TransactionLineTypes.Cost || Parent.AL_LineType == TransactionLineTypes.Revenue || Parent.AL_LineType == TransactionLineTypes.UnapprovedCost))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected bool CheckAmountOnAccountReceivableTransactionIsPositiveOrAllowed()
		{
			if (Parent.AL_LineType != TransactionLineTypes.Revenue
				|| (Parent.IsInDatabase && !Parent.AL_OSAmountInfo.HasChanges)
				|| AccTransactionLinesValidationHelper.IsNegativeChargeAllowed(Parent))
			{
				return true;
			}

			var transactionHeader = Parent.TransactionHeader;
			if (transactionHeader == null
				|| transactionHeader.IsCancelled
				|| transactionHeader.AH_Ledger != LedgerTypes.AccountsReceivable)
			{
				return true;
			}

			var osAmount = Parent.AL_OSAmount;
			var transactionType = transactionHeader?.AH_TransactionType ?? string.Empty;
			return (osAmount >= 0 && (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.AdjustmentNote))
				|| (osAmount <= 0 && transactionType == TransactionTypes.CreditNote);
		}

		bool CheckJobRelatedLineHasRevenueRecognitionType()
		{
			return !Parent.AL_RevRecognitionType.IsEmpty || Parent.IsInDatabase || Parent.AL_LineAmount.IsEmpty || Parent.AL_AC.IsEmpty || Parent.AL_JH.IsEmpty;
		}

		bool CheckJobIsNotEmptyWhenChargeCodeRequiredIt()
		{
			return Parent.CheckJobIsNotEmptyWhenChargeCodeRequiredIt();
		}

		bool CheckTransactionLineBranchBelongToTransactionHeaderCompany()
		{
			bool result = true;
			if (!Parent.IsInDatabase || Parent.AL_GBInfo.HasChanges)
			{
				if (Parent.TransactionHeader != null && Parent.TransactionHeader.Branch != null && Parent.Branch != null)
				{
					result = Parent.TransactionHeader.Branch.GB_GC == Parent.Branch.GB_GC;
				}
			}
			return result;
		}

		CriticalValidationResult CheckTransactionLineHasValidTaxIdAndTaxMessageMapping()
		{
			var helper = ObjectFactory.Get<ITaxIdAndTaxMessageMappingHelper>();
			var errorMsg = helper?.ValidateMappingForLine(Parent);
			if (string.IsNullOrEmpty(errorMsg))
			{
				return null;
			}

			return new CriticalValidationResult(CriticalValidationErrorType.TransactionLineTaxIdAndTaxMessageMappingInvalid_1, errorMsg, Parent.GetTransactionLineInfo());
		}

		bool CheckTransactionLineTaxBranchEqualToTransactionHeaderTaxBranch()
		{
			bool result = true;
			if ((!Parent.IsInDatabase || Parent.AL_GB_TaxBranchInfo.HasChanges) && Parent.TransactionHeader != null)
			{
				result = Parent.AL_GB_TaxBranch == Parent.TransactionHeader.AH_GB_TaxBranch;
			}

			return result;
		}

		bool CheckTransactionLineWithoutGLAccountAndChargeCode()
		{
#if DEBUG
			if (Globals.IsTest && Testing.SuspendGLAccountAndChargeCodeCriticalValidationAttribute.IsActive)
			{
				return false;
			}
#endif
			return Parent.AL_AG.IsEmpty
					&& (!Parent.IsInDatabase
						|| Parent.AL_AGInfo.HasChanges
						|| Parent.AL_ACInfo.HasChanges
						|| Parent.AL_GEInfo.HasChanges)
					&& (Parent.AL_LineType == TransactionLineTypes.Revenue
						|| Parent.AL_LineType == TransactionLineTypes.Cost
						|| Parent.AL_LineType == TransactionLineTypes.WIP
						|| Parent.AL_LineType == TransactionLineTypes.Accrual)
					&& (Parent.ChargeCode == null || !Parent.ChargeCode.IsComment)
					&& Parent.Branch != null
					&& Parent.Branch.GB_GC == GlbCompany.CurrentCompany.PK;
		}

		CriticalValidationResult CheckLineTaxBranchAreSyncWithChargeTaxBranch()
		{
			if (!Parent.IsInDatabase
				&& Parent.ChargeCode != null
				&& Parent.TransactionHeader?.AH_TransactionCategory.ToString() != Core.Constants.TransactionCategory.Codes.AutoJobRevenueJournal)
			{
				var charge = Parent.LoadRelatedJobCharge();
				if (charge == null)
				{
					return null;
				}

				string chargeTaxBranchCode = null;

				if ((Parent.AL_LineType == TransactionLineTypes.Cost || Parent.AL_LineType == TransactionLineTypes.UnapprovedCost) && Parent.AL_GB_TaxBranch != charge.JR_GB_CostTaxBranch)
				{
					chargeTaxBranchCode = charge.CostTaxBranch?.GB_Code ?? EmptyValueString;
				}

				if (chargeTaxBranchCode == null &&
					(Parent.AL_LineType == TransactionLineTypes.Revenue && Parent.AL_GB_TaxBranch != charge.JR_GB_SellTaxBranch))
				{
					chargeTaxBranchCode = charge.SellTaxBranch?.GB_Code ?? EmptyValueString;
				}

				if (chargeTaxBranchCode != null)
				{
					var lineTaxBranchCode = Parent.TaxBranch?.GB_Code ?? EmptyValueString;

					return new CriticalValidationResult(
						CriticalValidationErrorType.TransactionLineTaxBranchDifferentFromChargeTaxBranch,
						CriticalValidationMessageTemplate.GetTransactionLineTaxBranchDifferentFromChargeTaxBranchErrorMessage(lineTaxBranchCode, chargeTaxBranchCode),
						Parent.GetTransactionLineInfo());
				}
			}

			return null;
		}

		#endregion

		#region ReversedJobRelatedLineError

		protected ZString ReversedJobRelatedLineErrorKey
		{
			get { return (NoResString)"WIP/Accrual has been reversed and still has an associated Job Charge"; }
		}

		protected ZString ReversedJobRelatedLineErrorMessageTrap
		{
			get
			{
				JobCharge relatedJobCharge = Parent.LoadRelatedJobCharge();

				return string.Format((NoResString)"A reversed {0} has an associated job charge. Reverse Date {1}, type {2} and PK: {3}.\r\n{0} other details: {4}\r\nRelated Job Charge details: {5}",
					Parent.AL_LineType,
					Parent.AL_ReverseDate.ToAUString(),
					Parent.GetType(),
					Parent.PK,
					Parent.GetTransactionLineInfo(),
					relatedJobCharge == null ? (NoResString)"No related Job Charge" : relatedJobCharge.GetJobChargeInfo());
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	//[Reminder] This class wasn't moved to test proejct, because it's used by DocumentWrapper DocJobChargeTest class to create instance with reflection.
	class ChargeNotForSaveLikeApportionment : JobCharge
	{
		public ChargeNotForSaveLikeApportionment(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{ }

		public override bool IsSavedByFactory
		{
			get
			{
				return false;
			}
		}

		public override ZDecimal JR_OSCostGSTAmt_Calc { get => JR_OSCostGSTAmt; set => JR_OSCostGSTAmt = value; }

		public override bool CanReautorate(CostSell costOrSell, params ZString[] adapterIDs)
		{
			throw new NotImplementedException();
		}

		public override ILocation CostPlaceOfSupplyLocation => null;

		public override ILocation SellPlaceOfSupplyLocation => null;
	}
}

#endif
#endregion
