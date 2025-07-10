using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class DeferredSubmissionHelper
	{
		public DeferredSubmissionHelper(JobDeclaration declaration)
		{
			Declaration = declaration;
			Factory = declaration.Factory;
		}

		internal readonly JobDeclaration Declaration;
		internal readonly BusinessObjectFactory Factory;

		public void RefreshData()
		{
			fIsDeclarationDeferrable = null;
			fDutiableEntryHeaders = null;
			fDeferralDate = null;
			fCashFANPortMaps = null;
			fDeferrableFANPortMaps = null;
		}

		#region Deferred Account Selection

		public bool CanAlterMessageSubmitDateOrPaymentDetails
		{
			get
			{
				return IsDeclarationDeferrable && FirstDutiableEntryHeader != null && (IsDeferringMessagesEnabled || (IsDeferringPaymentsEnabled && DeferrableFANPortMaps.Count() > 1));
			}
		}

		public bool CanCalculateDeferralAccountAndPaymentMethod => IsDeferringPaymentsEnabled && IsDeclarationDeferrable;

		internal bool IsDeclarationDeferrable
		{
			get
			{
				if (!fIsDeclarationDeferrable.HasValue)
				{
					var importConditions = Declaration.JE_MessageType == JobMessageTypeList.Codes.Import && (Declaration.IsAir || Declaration.IsSea);
					fIsDeclarationDeferrable = !Declaration.JE_DateOfArrival.IsEmpty
						&& (importConditions || Declaration.JE_MessageType == JobMessageTypeList.Codes.ExWarehouse)
						&& Declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected
						&& DeferrableFANPortMaps.Any();
				}
				return fIsDeclarationDeferrable.Value;
			}
		}
		bool? fIsDeclarationDeferrable;

		public bool IsDeferringMessagesEnabled => MessageDeferralSettings.DaysBeforeETA > 0 && Declaration.IsSea;

		public bool IsDeferringPaymentsEnabled => MessageDeferralSettings.AllowAutomaticDeferredSelection;

		internal IEnumerable<CusEntryHeader> DutiableEntryHeaders => fDutiableEntryHeaders ?? (fDutiableEntryHeaders = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>()
																																	.Where(h => h.CH_PaymentMethod == PaymentMethodCodeList.Codes.Defer || h.CH_PaymentMethod == PaymentMethodCodeList.Codes.VATOnly)
																																	.OrderBy(h => h.CH_PaymentMethod)
																																	.ToArray());

		CusEntryHeader[] fDutiableEntryHeaders;

		internal CusEntryHeader FirstDutiableEntryHeader => DutiableEntryHeaders.FirstOrDefault();

		internal ZDateTime DeferralDate
		{
			get
			{
				if (!fDeferralDate.HasValue)
				{
					if (IsDeferringMessagesEnabled)
					{
						var deferralDate = Declaration.JE_DateOfArrival;
						if (!deferralDate.IsEmpty)
						{
							deferralDate = deferralDate.AddDays(-MessageDeferralSettings.DaysBeforeETA);
						}
						if (deferralDate < ZDateTime.Today || deferralDate.IsEmpty)
						{
							deferralDate = ZDateTime.Today;
						}
						fDeferralDate = deferralDate;
					}
					else
					{
						fDeferralDate = ZDateTime.Today;
					}
				}
				return fDeferralDate.Value;
			}
		}
		ZDateTime? fDeferralDate;

		#region MessageDeferralSettings

		AutomaticDeferredSelection MessageDeferralSettings => Factory.GetCachedValue(MessageDeferralSettingsCacheKey,
																() => ZACustomsRegistry.Instance.AutomaticDeferredSelection.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

		public const string MessageDeferralSettingsCacheKey = "ZA.DeferredSubmission.MessageDeferralSettings";

		#endregion

		#region FANPortMaps

		internal IEnumerable<FinancialAccountNumberPortMap> CashFANPortMaps
		{
			get
			{
				if (fCashFANPortMaps == null)
				{
					fCashFANPortMaps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps
										.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
										.Cast<FinancialAccountNumberPortMap>()
										.Where(m => m.Cash && NotImporterPaysOrOrgIsImporterAndOrgExists(m))
										.ToArray();
				}
				return fCashFANPortMaps;
			}
		}
		FinancialAccountNumberPortMap[] fCashFANPortMaps;

		internal IEnumerable<FinancialAccountNumberPortMap> DeferrableFANPortMaps
		{
			get
			{
				if (fDeferrableFANPortMaps == null)
				{
					fDeferrableFANPortMaps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps
												.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)
												.Cast<FinancialAccountNumberPortMap>()
												.Where(m => !m.Cash && m.CustomsOfficeCode == Declaration.JE_CustomsOffice && NotImporterPaysOrOrgIsImporterAndOrgExists(m))
												.ToArray();
				}
				return fDeferrableFANPortMaps;
			}
		}
		FinancialAccountNumberPortMap[] fDeferrableFANPortMaps;

		bool NotImporterPaysOrOrgIsImporterAndOrgExists(FinancialAccountNumberPortMap map) => (!map.ImporterPays || map.OrganizationPK == Declaration.JE_OH_Importer) && map.Organization != null;

		#endregion

		#region AccountSelection

		internal void CalculateDeferralAccountAndPaymentMethod()
		{
			if (CanCalculateDeferralAccountAndPaymentMethod)
			{
				var entryHeaders = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(h => h.CH_PaymentMethod == PaymentMethodCodeList.Codes.Defer).ToArray();
				if (entryHeaders.Any())
				{
					var agent = DeferrableFANPortMaps.FirstOrDefault(m => m.ImporterPays)?.OrganizationPK;
					if (!agent.HasValue)
					{
						var paymentMethod = ZString.Empty;

						var deferrableAccounts = GetDeferrableAccounts();
						if (deferrableAccounts.Any())
						{
							var totalDuty = entryHeaders.Sum(e => e.CustomsDuty);
							var totalVAT = entryHeaders.Sum(e => e.ValueAddedTax);

							var account = SelectFirstAccountWithSufficientBalance(deferrableAccounts, totalDuty, totalVAT);
							if (account.FANMap != null)
							{
								agent = account.FANMap.OrganizationPK;
								paymentMethod = account.PaymentMethod;
							}
						}

						if (!agent.HasValue && CashFANPortMaps.Any())
						{
							var agentCashAccount = CashFANPortMaps.FirstOrDefault(m => m.Organization.GetAgentCode(Declaration.Country) == Declaration.AgentCode)
								?? CashFANPortMaps.First();

							agent = agentCashAccount.OrganizationPK;
							paymentMethod = PaymentMethodCodeList.Codes.Cash;
						}

						foreach (var entry in entryHeaders)
						{
							entry.CH_PaymentMethod = paymentMethod;
						}
					}

					if (agent.HasValue)
					{
						Declaration.JE_OH_AgentOverride = agent.Value;
					}
				}
			}
		}

		internal IEnumerable<FinancialAccountNumberPortMapWrapper> GetDeferrableAccounts()
		{
			var deferrableAccountList = new List<FinancialAccountNumberPortMapWrapper>();
			var deferralDate = DeferralDate;

			foreach (var account in DeferrableFANPortMaps.Where(m => !m.ImporterPays))
			{
				var fanPortMapWrapper = new FinancialAccountNumberPortMapWrapper(account);

				var accountMonth = deferralDate;
				var accountStartDay = account.AccountStartDay;
				if (accountStartDay > deferralDate.Day)
				{
					var daysInDeferralMonth = DateTime.DaysInMonth(accountMonth.Year, accountMonth.Month);
					if (accountStartDay > daysInDeferralMonth)
					{
						accountStartDay = daysInDeferralMonth;
					}

					if (accountStartDay > deferralDate.Day)
					{
						accountMonth = deferralDate.AddMonths(-1);
						var daysInPreviousMonth = DateTime.DaysInMonth(accountMonth.Year, accountMonth.Month);
						if (accountStartDay > daysInPreviousMonth)
						{
							accountStartDay = daysInPreviousMonth;
						}
					}
				}
				fanPortMapWrapper.StartDate = new ZDateTime(accountMonth.Year, accountMonth.Month, accountStartDay);

				var dueMonth = accountMonth.AddMonths(1);
				accountStartDay = account.AccountStartDay;
				var daysInDueMonth = DateTime.DaysInMonth(dueMonth.Year, dueMonth.Month);
				if (accountStartDay > daysInDueMonth)
				{
					accountStartDay = daysInDueMonth;
				}
				fanPortMapWrapper.DueDate = new ZDateTime(dueMonth.Year, dueMonth.Month, accountStartDay);

				deferrableAccountList.Add(fanPortMapWrapper);
			}

			return deferrableAccountList.OrderByDescending(account => account.DueDate).ToArray();
		}

		internal (FinancialAccountNumberPortMap FANMap, ZString PaymentMethod) SelectFirstAccountWithSufficientBalance(IEnumerable<FinancialAccountNumberPortMapWrapper> deferrableAccounts, ZDecimal totalDuty, ZDecimal totalVAT)
		{
			FinancialAccountNumberPortMap dutyAndVatMap = null;
			FinancialAccountNumberPortMap vatOnlyMap = null;

			foreach (var account in deferrableAccounts)
			{
				var used = SumEntryPayInfosForAccount(account);
				var dutyAvailable = Math.Max(account.DutyDefermentAmount - used.duty, ZDecimal.Zero);
				var vatAvailable = Math.Max(account.VatDefermentAmount - used.vat, ZDecimal.Zero);

				if (dutyAvailable >= totalDuty && vatAvailable >= totalVAT)
				{
					dutyAndVatMap = account.FinancialAccountNumberPortMap;
					break;
				}
				else if (vatOnlyMap == null && vatAvailable >= totalVAT)
				{
					vatOnlyMap = account.FinancialAccountNumberPortMap;
				}
			}

			// Payment Code can only be ‘V’ if there are duties and VAT and we want to defer just the Vat.
			var isVatOnly = dutyAndVatMap == null && vatOnlyMap != null && totalDuty > ZDecimal.Zero && totalVAT > ZDecimal.Zero;
			return (dutyAndVatMap ?? vatOnlyMap, isVatOnly ? PaymentMethodCodeList.Codes.VATOnly : PaymentMethodCodeList.Codes.Defer);
		}

		(ZDecimal duty, ZDecimal vat) SumEntryPayInfosForAccount(FinancialAccountNumberPortMapWrapper account)
		{
			var agentCode = account.FinancialAccountNumberPortMap.Organization.GetAgentCode(Declaration.Country);
			var bgmReferencePrefix = agentCode + account.FinancialAccountNumberPortMap.CustomsOfficeCode;

			var sqlParameters = new ZSqlParameterCollection
			{
				{ "@BGMPrefix", bgmReferencePrefix + "%", CusEntryHeaderSchema.CH_BGMReference },
				{ "@StartDate", account.StartDate.ToDateTime(), CusEntryPayInfoSchema.C9_PaymentDate },
				{ "@EndDate", account.DueDate.ToDateTime(), CusEntryPayInfoSchema.C9_PaymentDate }
			};

			var queryResults = new DynamicBusinessObjectCollection(Factory);
			queryResults.Load(SumEntryPayInfosForAccountSqlScript, sqlParameters);

			return queryResults.Count > 0 ? ((ZDecimal)queryResults[0][UniversalReferenceConstants.CusEntryPayTypes.Duty], (ZDecimal)queryResults[0][UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax]) : (ZDecimal.Zero, ZDecimal.Zero);
		}

		string SumEntryPayInfosForAccountSqlScript => FormattableString.Invariant($@"
SELECT ISNULL(PVT.{UniversalReferenceConstants.CusEntryPayTypes.Duty}, 0) AS [{UniversalReferenceConstants.CusEntryPayTypes.Duty}], ISNULL(PVT.{UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax}, 0) AS [{UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax}] FROM 
(
	SELECT {CusEntryPayInfoSchema.Constants.C9_TransactionType}, {CusEntryPayInfoSchema.Constants.C9_PaymentAmount} as Amount
	FROM {CusEntryPayInfoSchema.Constants.SqlSchemaName}.{CusEntryPayInfoSchema.Constants.TableName} 
	JOIN {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName} ON {CusEntryPayInfoSchema.Constants.C9_CH} = {CusEntryHeaderSchema.Constants.PK} AND {CusEntryHeaderSchema.Constants.CH_BGMReference} LIKE @BGMPrefix
	WHERE
		{CusEntryPayInfoSchema.Constants.C9_PaymentDate} >= @StartDate
		AND {CusEntryPayInfoSchema.Constants.C9_PaymentDate} < @EndDate 
		AND {CusEntryPayInfoSchema.Constants.C9_PaymentParty} NOT IN ('C','F')
		AND	({CusEntryPayInfoSchema.Constants.C9_TransactionType} = '{UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax}' OR
			({CusEntryPayInfoSchema.Constants.C9_TransactionType} = '{UniversalReferenceConstants.CusEntryPayTypes.Duty}' AND {CusEntryPayInfoSchema.Constants.C9_PaymentParty} = 'D'))
) D
PIVOT
(
	SUM(D.Amount)
	FOR D.{CusEntryPayInfoSchema.Constants.C9_TransactionType} IN ([{UniversalReferenceConstants.CusEntryPayTypes.Duty}],[{UniversalReferenceConstants.CusEntryPayTypes.ValueAddedTax}])
) PVT");
		#endregion

		#endregion

		#region CancelDeferredMessages

		public bool CanCancelDeferredMessages
		{
			get
			{
				ClearCachedDeferredMessages();
				return DeferredMessages.Any();
			}
		}

		public int CancelDeferredMessages()
		{
			int cancelledMessageCount = 0;

			foreach (var msg in DeferredMessages)
			{
				msg.EM_Status = EDIMessage.Status.Cancelled;
				msg.EM_HeldUntilDate = ZDateTime.Empty;
				msg.VoucherOfCorrectionValueAfters.RemoveAndDeleteAll();

				((CusEntryHeader)msg.EM_LinkedObject).CH_Status = ZString.Empty;
				cancelledMessageCount++;
			}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Declaration.Logs.AddNew(AutoEvents.EditedARecord, "Held messages cancelled.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			return cancelledMessageCount;
		}

		IEnumerable<CUSDECEDIMessage> DeferredMessages
		{
			get
			{
				if (fDeferredMessages == null)
				{
					fDeferredMessages = new List<CUSDECEDIMessage>();
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						fDeferredMessages.AddRange(entryHeader.Messages.OfType<CUSDECEDIMessage>()
															 .Where(msg => !msg.EM_HeldUntilDate.IsEmpty
																		 && msg.EM_Status == EDIMessage.Status.Queued
																		 && msg.EM_ReceiveTransmit == EDIMessage.Direction.Transmit));
					}
				}
				return fDeferredMessages;
			}
		}
		List<CUSDECEDIMessage> fDeferredMessages;

		void ClearCachedDeferredMessages()
		{
			fDeferredMessages = null;
		}

		#endregion
	}
}
