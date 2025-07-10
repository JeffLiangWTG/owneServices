using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class CreditChecker
	{
		public CreditChecker(OrgHeader org)
		{
			Organisation = org;
			TraceBuilder = new CreditCheckTracer();
			Provider = ObjectFactory.Get<IOrgCreditLimitAndBalanceDetailsProvider>();
			expiryMinutes = AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.Value;

			lockRoot = new object();
		}

		public void ValidateIsCreditOnHold(ZPropertyInfo info, string ledger)
		{
			if (InAsyncCall)
			{
				return;
			}

			string error = GetCreditOnHoldValidation(ledger);
			if (!string.IsNullOrEmpty(error))
			{
				info.AddError(error);
			}
		}

		public void ValidateIsCreditLimitExceeded(ZPropertyInfo info, string ledger)
		{
			if (InAsyncCall)
			{
				return;
			}

			ValidateIsCreditLimitExceeded(info, ledger, 0m);
		}

		public void ValidateIsCreditLimitExceeded(ZPropertyInfo info, string ledger, decimal currentInvoiceOutstanding = decimal.Zero)
		{
			if (InAsyncCall)
			{
				return;
			}

			if (!info.HasErrors())
			{
				string warning = GetCreditLimitExceededValidation(ledger, currentInvoiceOutstanding);
				if (!string.IsNullOrEmpty(warning))
				{
					info.AddWarning(warning);
				}
			}
		}

		public string GetCreditLimitExceededValidation(string ledger, decimal currentInvoiceOutstanding = 0m)
		{
			string result = string.Empty;

			if (!InAsyncCall && !IsCreditLimitValidationSuspended(Organisation.Factory))
			{
				GetCreditLimitCheckCore(ledger, currentInvoiceOutstanding, out result);
			}

			return result;
		}

		public bool DoesExceedCreditLimit()
		{
			var result = false;

			var creditLimitCheck = GetCreditLimitCheckCore(LedgerTypes.AccountsReceivable);

			if (creditLimitCheck.OverCreditLimit || creditLimitCheck.OverGlobalCreditLimit)
			{
				result = true;
			}

			return result;
		}

		public int RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit()
		{
			int result = 0;
			if (InAsyncCall)
			{
				return -1;
			}

			var creditLimitCheck = GetCreditLimitCheckCore(LedgerTypes.AccountsReceivable);
			AmountOrPercentageBasedThreeLevelAuthorisationRequirement setting = null;

			if (creditLimitCheck.OverCreditLimit)
			{
				var overTheLimitAmount = creditLimitCheck.TotalOutstandingAmount - creditLimitCheck.CreditLimit;
				setting = CreditControllerOverrideThresholdHelper.Instance.GetApplicableSettings(overTheLimitAmount, creditLimitCheck.CreditLimit);
			}
			else if (creditLimitCheck.IsGlobalCreditApproved && creditLimitCheck.OverGlobalCreditLimit)
			{
				var overTheGlobalLimitAmount = creditLimitCheck.GlobalTotalOutstandingAmount - creditLimitCheck.GlobalCreditLimit;
				setting = CreditControllerOverrideGlobalThresholdHelper.Instance.GetApplicableSettings(overTheGlobalLimitAmount, creditLimitCheck.GlobalCreditLimit);
			}

			if (setting != null)
			{
				switch (setting.AuthorisationRequirement)
				{
					case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired:
						result = 0;
						break;
					case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
						result = 1;
						break;
					case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
						result = 2;
						break;
					case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
						result = 3;
						break;
				}
			}

			return result;
		}

		public bool IsCreditOnHold()
		{
			bool result = false;

			using (Db.DisposableActionForDbConnection())
			{
				if (!InAsyncCall && Organisation.OH_IsDebtor)
				{
					string message;
					var creditLimitCheckResult = GetCreditOnHoldValidationCore(LedgerTypes.AccountsReceivable, out message);
					result = (creditLimitCheckResult & CreditLimitCheckResult.CreditOnHold) != 0;
				}
			}

			return result;
		}

		public bool IsOverGlobalCreditLimit()
		{
			var result = false;

			using (Db.DisposableActionForDbConnection())
			{
				if (!InAsyncCall && Organisation.OH_IsDebtor)
				{
					var creditLimitCheck = GetCreditLimitCheckCore(LedgerTypes.AccountsReceivable);
					result = creditLimitCheck.IsGlobalCreditApproved && creditLimitCheck.OverGlobalCreditLimit;
				}
			}

			return result;
		}

		public CreditLimitDetails GetCreditDetails(string ledger)
		{
			var checkResult = GetCreditLimitCheckCore(ledger);

			return new CreditLimitDetails(checkResult.CreditLimit, checkResult.TotalOutstandingAmount, checkResult.OnCreditHold,
				checkResult.IsGlobalCreditApproved, checkResult.GlobalCreditCurrencyCode, checkResult.GlobalCreditLimit, checkResult.GlobalTotalOutstandingAmount, checkResult.OnGlobalCreditHold);
		}

		public bool AsyncFetchCreditLimitAndOutstandingBalance()
		{
			var result = false;

			bool canFetch = false;
			lock (lockRoot)
			{
				if (!inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK)
				{
					inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK = true;
					canFetch = true;
				}
			}

			if (canFetch)
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						try
						{
							var cached = CachedCreditLimitAndOutstandingBalance;
							if (cached != null)
							{
								cached.FetchCreditLimitAndBalanceFromWebService(LedgerTypes.AccountsReceivable);
								result = true;
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							HandleException(ex);
						}
					}
				}
				finally
				{
					InAsyncCall = false;
				}
			}

			return result;
		}

		internal bool InAsyncCall
		{
			get
			{
				lock (lockRoot)
				{
					return inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK;
				}
			}
			set
			{
				lock (lockRoot)
				{
					inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK = value;
				}
			}
		}
		bool inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK;

		public void ClearCreditLimitAndOutstandingBalanceCache()
		{
			lock (lockRoot)
			{
				if (!inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK)
				{
					cachedBalance = null;
					cachedError = null;

					#region SuppressResourceStringsCheckRegion for logging

					if (TraceBuilder.IsEnabled())
					{
						var message = FormattableString.Invariant($"""
															{CriticalValidationInfoCollectorServiceKeyType.CreditLimitCacheCleared_IfCollectionActivated}:
															Clearing cache for {Organisation.OH_Code}:
															{new StackTrace().ToString()}
															""");

						TraceBuilder.TraceInformation(() => message);
					}

					#endregion
				}
			}
		}

		#region CreditLimitValidationSuspender

		public static IDisposable GetCreditLimitValidationSuspender(BusinessObjectFactory factory)
		{
			return new CreditLimitValidationSuspender(factory);
		}

		public static bool IsCreditLimitValidationSuspended(BusinessObjectFactory factory)
		{
			return CreditLimitValidationSuspender.IsSuspended(factory);
		}

		internal class CreditLimitValidationSuspender : IDisposable
		{
			internal CreditLimitValidationSuspender(BusinessObjectFactory factory)
			{
				this.Factory = factory;
				CreditLimitValidationSuspenderService service = factory.ServiceContainer.GetService<CreditLimitValidationSuspenderService>();
				if (service == null)
				{
					factory.ServiceContainer.AddService(new CreditLimitValidationSuspenderService());
				}
				else
				{
					service.UseCount++;
				}
			}

			readonly BusinessObjectFactory Factory;

			void IDisposable.Dispose()
			{
				CreditLimitValidationSuspenderService service = Factory.ServiceContainer.GetService<CreditLimitValidationSuspenderService>();
				if (service != null)
				{
					service.UseCount--;
					if (service.UseCount == 0)
					{
						Factory.ServiceContainer.RemoveService<CreditLimitValidationSuspenderService>();
					}
				}
			}

			internal static bool IsSuspended(BusinessObjectFactory factory)
			{
				return factory.ServiceContainer.GetService<CreditLimitValidationSuspenderService>() != null;
			}

			class CreditLimitValidationSuspenderService : IService
			{
				internal int UseCount = 1;
			}
		}

		#endregion

		#region Implementation

		internal bool? IsCreditLimitExceededAfterThisAmount(string ledger, decimal amountInLocalCurrency)
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var cached = CachedCreditLimitAndOutstandingBalance;
					if (cached != null)
					{
						decimal creditLimit = cached.CreditLimit(ledger);
						decimal outstandingAmount = cached.OutstandingBalance(ledger);
						decimal claimAmount = cached.Claim(ledger);

						decimal overBalance = outstandingAmount + amountInLocalCurrency - creditLimit;

						if (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
						{
							overBalance -= claimAmount;
						}

						return creditLimit != 0m && overBalance > 0m;
					}
					else
					{
						return null;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);
					return null;
				}
			}
		}

		[Flags]
		enum CreditLimitCheckResult
		{
			Undefined = 0,
			WithinCreditLimit = 1,
			AtCreditLimit = 2,
			OverCreditLimit = 4,
			UnableToGetCreditLimit = 8,
			UnableToGetOutstandingBalance = 16,
			UnableToGetUnpostedRecognised = 32,
			UnableToGetUnpostedUnrecognised = 64,
			CreditOnHold = 128,
			UnableToGetCreditOnHold = 256,
			WithinGlobalCreditLimit = 512,
			AtGlobalCreditLimit = 1024,
			OverGlobalCreditLimit = 2048,
			UnableToGetGlobalCreditLimit = 4096,
			UnableToGetGlobalOutstandingBalance = 8192,
			UnableToGetGlobalUnpostedRevenue = 16384,
			GlobalCreditOnHold = 32768,
			UnableToGetGlobalCreditOnHold = 65536,
			InvalidGlobalCreditCurrencyOrMissingExRate = 131072
		}

		class CreditLimitCheckData
		{
			internal CreditLimitCheckResult CheckResult;
			internal string Ledger;
			internal string Message;
			internal decimal CreditLimit;
			internal decimal TotalOutstandingAmount;
			internal decimal PostedOutstandingBalance;
			internal decimal ClaimAmount;
			internal decimal UnpostedRevenueRecognised;
			internal decimal UnpostedRevenueUnrecognised;
			internal bool UnableToGetCreditOnHold => (CheckResult & CreditLimitCheckResult.UnableToGetCreditOnHold) != 0;
			internal bool OnCreditHold => (CheckResult & CreditLimitCheckResult.CreditOnHold) != 0;
			internal bool OverCreditLimit => (CheckResult & CreditLimitCheckResult.OverCreditLimit) != 0;

			internal bool IsGlobalCreditApproved;
			internal string GlobalCreditCurrencyCode;
			internal decimal GlobalCreditLimit;
			internal decimal GlobalTotalOutstandingAmount;
			internal decimal GlobalPostedOutstandingBalance;
			internal decimal GlobalClaim;
			internal decimal GlobalUnpostedRevenueRecognised;
			internal decimal GlobalUnpostedRevenueUnrecognised;
			internal bool OnGlobalCreditHold => (CheckResult & CreditLimitCheckResult.GlobalCreditOnHold) != 0;
			internal bool OverGlobalCreditLimit => (CheckResult & CreditLimitCheckResult.OverGlobalCreditLimit) != 0;
		}

		CreditLimitCheckResult GetCreditLimitCheckCore(string ledger, decimal currentInvoiceOutstanding, out string message)
		{
			var result = GetCreditLimitCheckCore(ledger, currentInvoiceOutstanding);
			message = result.Message;
			return result.CheckResult;
		}

		CreditLimitCheckData GetCreditLimitCheckCore(string ledger, decimal currentInvoiceOutstanding = decimal.Zero)
		{
			var result = new CreditLimitCheckData();
			result.CheckResult = CreditLimitCheckResult.Undefined;
			result.Ledger = ledger;
			result.Message = string.Empty;
			result.CreditLimit = decimal.Zero;
			result.TotalOutstandingAmount = decimal.Zero;

			using (Db.DisposableActionForDbConnection())
			{
				IOrgCreditLimitAndBalanceDetails cached = null;

				if (!TryGetPostedOutstandingBalance(ref cached, currentInvoiceOutstanding, result) ||
					!TryGetCreditLimit(cached, result))
				{
					return result;
				}

				var unpostedRevenueRegistryItem = ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInCreditLimitCalculation;
				var unpostedRevenueRegistrySetting = unpostedRevenueRegistryItem.Value?.ToString();

				if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedAndRecognized
				|| unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
				{
					TryGetUnpostedRevenueRecognised(cached, result);
				}

				if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
				{
					TryGetUnpostedRevenueUnrecognised(cached, result);
				}

				TryGetOnCreditHold(cached, result);

				CheckCreditState(currentInvoiceOutstanding, unpostedRevenueRegistrySetting, result);

				#region Global Credit check if not over local Credit Limit

				var includeUnpostedRevenueInGlobalCreditLimitCalculation = ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInGlobalCreditLimitCalculation.Value?.ToString();

				TryGetGlobalCreditDetails(cached, result);
				TryGetOnGlobalCreditHold(cached, result);
				TryGetGlobalTotalOutstandingAmount(cached, currentInvoiceOutstanding, includeUnpostedRevenueInGlobalCreditLimitCalculation, result);

				#endregion
			}

			return result;
		}

		bool TryGetPostedOutstandingBalance(ref IOrgCreditLimitAndBalanceDetails cached, decimal currentInvoiceOutstan, CreditLimitCheckData result)
		{
			try
			{
				// First method should retrieve cached values to keep and use them through all other TryGet methods
				cached = CachedCreditLimitAndOutstandingBalance;

				if (cached != null)
				{
					result.PostedOutstandingBalance = cached.OutstandingBalance(result.Ledger);
					result.ClaimAmount = cached.Claim(result.Ledger);
					result.TotalOutstandingAmount = result.PostedOutstandingBalance + currentInvoiceOutstan;

					if (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
					{
						result.TotalOutstandingAmount -= result.ClaimAmount;
					}

					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);

				result.Message = Res.GetString("ab453946-96d0-4c10-83a3-cea011f4343c", "Unable to retrieve a value for Outstanding Balance.");
				if (cached != null)
				{
					result.Message = cached.GetErrorMessage(result.Message, ex);
				}
				result.CheckResult = CreditLimitCheckResult.UnableToGetOutstandingBalance;
			}
			return false;
		}

		bool TryGetCreditLimit(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			try
			{
				if (cached != null)
				{
					result.CreditLimit = cached.CreditLimit(result.Ledger);
					return true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);

				result.Message = Res.GetString("f67de200-2948-4659-9fcb-83856031687a", "Unable to retrieve a value for Credit Limit.");
				if (cached != null)
				{
					result.Message = cached.GetErrorMessage(result.Message, ex);
				}
				result.CheckResult = CreditLimitCheckResult.UnableToGetCreditLimit;
			}
			return false;
		}

		void TryGetUnpostedRevenueRecognised(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			try
			{
				if (cached != null)
				{
					result.UnpostedRevenueRecognised = cached.UnpostedRevenueRecognised(result.Ledger);
					if (result.Ledger == LedgerTypes.AccountsReceivable)
					{
						result.TotalOutstandingAmount += result.UnpostedRevenueRecognised;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);

				string currentError = Res.GetString("5dff0f81-b704-42e7-b210-9cdd6e992ada", "Unable to retrieve a value for Unposted Recognized Revenue.");
				if (cached != null)
				{
					currentError = cached.GetErrorMessage(currentError, ex);
				}
				result.Message += currentError;
				result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetUnpostedRecognised;
			}
		}

		void TryGetUnpostedRevenueUnrecognised(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			try
			{
				result.UnpostedRevenueUnrecognised = cached.UnpostedRevenueUnrecognised(result.Ledger);
				if (result.Ledger == LedgerTypes.AccountsReceivable)
				{
					result.TotalOutstandingAmount += result.UnpostedRevenueUnrecognised;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);

				string currentError = Res.GetString("3867da63-0e77-4e37-b237-8c25ee68b766", "Unable to retrieve a value for Unposted Unrecognized Revenue.");
				if (cached != null)
				{
					currentError = cached.GetErrorMessage(currentError, ex);
				}
				result.Message += (result.Message.Length > 0 ? System.Environment.NewLine : "") + currentError;
				result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetUnpostedUnrecognised;
			}
		}

		void TryGetOnCreditHold(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			var onCreditHold = false;
			try
			{
				if (cached != null && Organisation.OH_IsDebtor)
				{
					onCreditHold = cached.OnCreditHold(result.Ledger);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);

				result.Message += Res.GetString("eef8d008-423c-4f0f-816b-b929154979f3", "Unable to retrieve a value for Credit On Hold.");
				if (cached != null)
				{
					result.Message += cached.GetErrorMessage(result.Message, ex);
				}
				result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetCreditOnHold;
			}

			if (onCreditHold)
			{
				result.Message += Res.GetString("10d5b425-1d3e-4e3a-9a76-c0b74d8c211a", "{0} is on Credit Hold.", Organisation.OH_Code.Trim()) + "\r\n\r\n";
				result.CheckResult = result.CheckResult | CreditLimitCheckResult.CreditOnHold;
			}
		}

		void CheckCreditState(decimal currentInvoiceOutstanding, string unpostedRevenueRegistrySetting, CreditLimitCheckData result)
		{
			if (result.CreditLimit != 0m)
			{
				if (result.TotalOutstandingAmount > result.CreditLimit)
				{
					result.CheckResult = result.CheckResult | CreditLimitCheckResult.OverCreditLimit;
					GenerateCreditLimitNotification(currentInvoiceOutstanding, unpostedRevenueRegistrySetting, result);
				}
				else
				{
					result.CheckResult = result.TotalOutstandingAmount == result.CreditLimit ?
						result.CheckResult | CreditLimitCheckResult.AtCreditLimit :
						result.CheckResult | CreditLimitCheckResult.WithinCreditLimit;

					if (result.OnCreditHold)
					{
						GenerateCreditLimitNotification(currentInvoiceOutstanding, unpostedRevenueRegistrySetting, result);
					}
				}
			}
		}

		void GenerateCreditLimitNotification(decimal currentInvoiceOutstanding, string unpostedRevenueRegistrySetting, CreditLimitCheckData result)
		{
			var creditLimitOrganisation = string.Empty;
			OrgHeader creditApprovalOrganisation = null;
			var forThisSettlementGroupMessage = true;
			var outOfLimit = (result.CheckResult & CreditLimitCheckResult.OverCreditLimit) != 0;
			var messageBuilder = new ZStringBuilder(result.Message);

			if (Organisation.IsSettlementGroup(result.Ledger))
			{
				messageBuilder.AppendLine(Res.GetString("902c0095-6112-4f06-89b7-96afa70b1c34", "{0} is a settlement group.", Organisation.OH_Code.Trim()));
				creditLimitOrganisation = Organisation.OH_Code.Trim();
				creditApprovalOrganisation = Organisation;
			}
			else if ((result.Ledger == LedgerTypes.AccountsPayable && !Organisation.APSettlementGroupPK.IsEmpty)
				|| (result.Ledger == LedgerTypes.AccountsReceivable && !Organisation.ARSettlementGroupPK.IsEmpty && Organisation.CompanyData.OB_ARUseSettlementGroupCreditLimit))
			{
				OrgHeader settlementGroup = result.Ledger == LedgerTypes.AccountsReceivable ? Organisation.ARSettlementGroup : Organisation.APSettlementGroup;
				ZString settlementGroupCode = settlementGroup != null ? settlementGroup.OH_Code.Trim() : ZString.Empty;
				messageBuilder.AppendLine(Res.GetString("d9c4919a-5a64-4348-82a9-816697974e5e", "{0} is configured to use the credit limit of settlement group {1}.", Organisation.OH_Code.Trim(), settlementGroupCode));
				creditLimitOrganisation = settlementGroupCode;
				creditApprovalOrganisation = settlementGroup;
			}
			else
			{
				creditLimitOrganisation = Organisation.OH_Code.Trim();
				forThisSettlementGroupMessage = false;
				creditApprovalOrganisation = Organisation;
			}

			messageBuilder.AppendLine(Res.GetString("863aeefc-7597-451e-9bcc-508460639ee8", "The Credit Limit for {0} is set to {1} {2}.",
					creditLimitOrganisation,
					FormatNumber(result.CreditLimit, GlbCompany.CurrentCompany.LocalCurrency.Decimals),
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				+ " "
				+ (creditApprovalOrganisation != null && creditApprovalOrganisation.CompanyData.OB_ARCreditApproved
					? Res.GetString("1b440486-89a3-4e53-89af-8156db12b968", "Credit approved.")
					: Res.GetString("fbea0b03-75b7-43d5-824f-663e2e47a44c", "Credit pending approval.")));

			if (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
			{
				messageBuilder.Append(forThisSettlementGroupMessage
					? Res.GetString("8929FA5E-D87A-4772-9944-B662A72FC125", "The Total Outstanding Balance for this settlement group (excluding Open Claims Amount) is {0} {1}, ",
						FormatNumber((result.TotalOutstandingAmount), GlbCompany.CurrentCompany.LocalCurrency.Decimals),
						GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					: Res.GetString("5A9D0DB4-2A1F-4D8D-8D6D-90CB977715D3", "The Total Outstanding Balance (excluding Open Claims Amount) is {0} {1}, ",
						FormatNumber((result.TotalOutstandingAmount), GlbCompany.CurrentCompany.LocalCurrency.Decimals),
						GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			}
			else
			{
				messageBuilder.Append(forThisSettlementGroupMessage
					? Res.GetString("885f24be-a558-4f31-b3ff-c7963a95845f", "The Total Outstanding Balance for this settlement group is {0} {1}, ",
						FormatNumber((result.TotalOutstandingAmount), GlbCompany.CurrentCompany.LocalCurrency.Decimals),
						GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					: Res.GetString("748b173d-6240-4789-8fe8-06a8e05aae7c", "The Total Outstanding Balance is {0} {1}, ",
						FormatNumber((result.TotalOutstandingAmount), GlbCompany.CurrentCompany.LocalCurrency.Decimals),
						GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			}

			messageBuilder.AppendLine(outOfLimit
				? Res.GetString("db726092-55da-4974-a73b-1300d8425936", "which is over the credit limit.")
				: Res.GetString("99a5f110-a483-416f-ad37-4435889f9a6d", "which has not exceeded the credit limit threshold."));

			messageBuilder.AppendLine();
			messageBuilder.AppendLine(Res.GetString("79b8063d-6baf-4c8d-80ae-810ea2fcfcbc", "The Total Outstanding Balance is calculated by summing the following:"));
			messageBuilder.AppendLine("  *  " + Res.GetString("3892cb0f-5f8a-4448-81e5-8050ffe95367", "the Posted Outstanding Balance, {0} {1}", FormatNumber(result.PostedOutstandingBalance, GlbCompany.CurrentCompany.LocalCurrency.Decimals), GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));

			if (currentInvoiceOutstanding != 0)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("cf27af7e-c03b-4e78-a190-268c844d8cad", "the Current Transaction Amount, {0} {1}", FormatNumber(currentInvoiceOutstanding, GlbCompany.CurrentCompany.LocalCurrency.Decimals), GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			}

			if (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("781EDF09-B926-4154-8531-3A03CE02507C", "the Open Claims Amounts Excluded, {0} {1}", FormatNumber(-result.ClaimAmount, GlbCompany.CurrentCompany.LocalCurrency.Decimals), GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			}

			if (result.Ledger == LedgerTypes.AccountsReceivable)
			{
				if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedAndRecognized
					|| unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
				{
					messageBuilder.AppendLine("  *  " + Res.GetString("60b5d324-6f6d-49c1-8099-7325d4531a12", "the Unposted Recognized Revenue, {0} {1}", FormatNumber(result.UnpostedRevenueRecognised, GlbCompany.CurrentCompany.LocalCurrency.Decimals), GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
				}

				if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
				{
					messageBuilder.AppendLine("  *  " + Res.GetString("d82990a5-d787-433f-8eaf-5424fa37cdca", "the Unposted Unrecognized Revenue, {0} {1}", FormatNumber(result.UnpostedRevenueUnrecognised, GlbCompany.CurrentCompany.LocalCurrency.Decimals), GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
				}
			}

			if (creditApprovalOrganisation.CompanyData.TemporaryCreditLimitInEffect)
			{
				messageBuilder.AppendLine();
				messageBuilder.AppendLine(Res.GetString("43fbb78d-6bf2-410b-a91c-2aa20041be8c", @"The Credit Limit is calculated by summing the following:
  *  the Credit Limit, {0} {1}
  *  the Temporary Credit Limit Increase, {2} {3}",
					FormatNumber(creditApprovalOrganisation.CompanyData.OB_ARCreditLimit, GlbCompany.CurrentCompany.LocalCurrency.Decimals),
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency,
					FormatNumber(creditApprovalOrganisation.CompanyData.OB_ARTemporaryCreditLimitIncrease, GlbCompany.CurrentCompany.LocalCurrency.Decimals),
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			}

			result.Message = messageBuilder.ToString().TrimEnd();
		}

		#region Global Credit

		void TryGetGlobalCreditDetails(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			if (result.Ledger == LedgerTypes.AccountsReceivable && Organisation.OH_IsDebtor)
			{
				try
				{
					if (cached != null)
					{
						result.IsGlobalCreditApproved = cached.IsARGlobalCreditApproved;
						if (result.IsGlobalCreditApproved)
						{
							result.GlobalCreditLimit = cached.ARGlobalCreditLimit;
							result.GlobalCreditCurrencyCode = cached.ARGlobalCreditCurrencyCode;
						}
						if (cached.UnableToCalculateARGlobalOutstandingBalance)
						{
							result.CheckResult = result.CheckResult | CreditLimitCheckResult.InvalidGlobalCreditCurrencyOrMissingExRate;
							result.Message += !result.Message.IsNullOrEmpty() ? "\r\n\r\n" : string.Empty;
							result.Message += Organisation.InvalidGlobalCreditCurrencyOrMissingExRateMessage;
							return;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);

					result.Message += Res.GetString("2161519b-c8cc-4eef-9591-183bce92c66f", "Unable to retrieve for Global Credit Limit.");
					if (cached != null)
					{
						result.Message += cached.GetErrorMessage(result.Message, ex);
					}
					result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetGlobalCreditLimit;
				}
			}
		}

		void TryGetOnGlobalCreditHold(IOrgCreditLimitAndBalanceDetails cached, CreditLimitCheckData result)
		{
			if (result.Ledger == LedgerTypes.AccountsReceivable && result.IsGlobalCreditApproved)
			{
				var onGlobalCreditHold = false;
				try
				{
					if (cached != null)
					{
						onGlobalCreditHold = cached.OnARGlobalCreditHold;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);

					result.Message += Res.GetString("cd90bf63-cf1d-4082-99af-5af06c4c5dc1", "Unable to retrieve value for Global Credit On Hold.");
					if (cached != null)
					{
						result.Message += cached.GetErrorMessage(result.Message, ex);
					}
					result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetGlobalCreditOnHold;
				}

				if (onGlobalCreditHold)
				{
					result.Message += Res.GetString("b5ab9c28-03cc-4930-9ba1-802b3e29d8db", "{0} is on Global Credit Hold.", Organisation.OH_Code.Trim());
					result.CheckResult = result.CheckResult | CreditLimitCheckResult.GlobalCreditOnHold;
				}
			}
		}

		void TryGetGlobalTotalOutstandingAmount(IOrgCreditLimitAndBalanceDetails cached, decimal currentInvoiceOutstanding, string unpostedRevenueRegistrySetting, CreditLimitCheckData result)
		{
			if (result.Ledger == LedgerTypes.AccountsReceivable && result.IsGlobalCreditApproved)
			{
				try
				{
					if (cached != null)
					{
						result.GlobalPostedOutstandingBalance = cached.ARGlobalOutstandingBalance;
						result.GlobalClaim = cached.ARGlobalClaim;
						result.GlobalTotalOutstandingAmount = result.GlobalPostedOutstandingBalance;
						if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedAndRecognized ||
							unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
						{
							result.GlobalUnpostedRevenueRecognised = cached.ARGlobalUnpostedRevenueRecognised;
							result.GlobalTotalOutstandingAmount += result.GlobalUnpostedRevenueRecognised;
						}
						if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
						{
							result.GlobalUnpostedRevenueUnrecognised = cached.ARGlobalUnpostedRevenueUnrecognised;
							result.GlobalTotalOutstandingAmount += result.GlobalUnpostedRevenueUnrecognised;
						}
						if (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
						{
							result.GlobalTotalOutstandingAmount -= result.GlobalClaim;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);

					result.Message += Res.GetString("ba218a93-af5b-44ab-b47f-fcbe7b1c610b", "Unable to retrieve value for Global Outstanding Balance.");
					if (cached != null)
					{
						result.Message += cached.GetErrorMessage(result.Message, ex);
					}
					result.CheckResult = result.CheckResult | CreditLimitCheckResult.UnableToGetGlobalOutstandingBalance;
				}

				if (cached != null && (result.CheckResult & CreditLimitCheckResult.UnableToGetGlobalOutstandingBalance) == 0)
				{
					var currentInvoiceOutstandingInCreditCurrency = decimal.Zero;
					if (currentInvoiceOutstanding != decimal.Zero)
					{
						var rate = GlobalCreditGroupHelper.GetGlobalExchangeRate(Organisation.Factory, result.GlobalCreditCurrencyCode);

						currentInvoiceOutstandingInCreditCurrency = rate == 1m ? currentInvoiceOutstanding
							: GlbCompany.CurrentCompany.GetExchangeRate().LocalToForeign(currentInvoiceOutstanding, rate, result.GlobalCreditCurrencyCode);
						result.GlobalTotalOutstandingAmount += currentInvoiceOutstandingInCreditCurrency;
					}

					if (result.GlobalTotalOutstandingAmount > result.GlobalCreditLimit)
					{
						result.CheckResult = result.CheckResult | CreditLimitCheckResult.OverGlobalCreditLimit;
						GenerateGlobalCreditLimitNotification(currentInvoiceOutstandingInCreditCurrency, unpostedRevenueRegistrySetting, result);
					}
					else
					{
						result.CheckResult = result.GlobalTotalOutstandingAmount == result.GlobalCreditLimit ?
							result.CheckResult | CreditLimitCheckResult.AtGlobalCreditLimit :
							result.CheckResult | CreditLimitCheckResult.WithinGlobalCreditLimit;

						if (result.OnGlobalCreditHold)
						{
							GenerateGlobalCreditLimitNotification(currentInvoiceOutstandingInCreditCurrency, unpostedRevenueRegistrySetting, result);
						}
					}
				}
			}
		}

		void GenerateGlobalCreditLimitNotification(decimal currentInvoiceOutstandingInCreditCurrency, string unpostedRevenueRegistrySetting, CreditLimitCheckData result)
		{
			var messageBuilder = new ZStringBuilder();
			if (!string.IsNullOrEmpty(result.Message))
			{
				messageBuilder.AppendLine(result.Message);
				messageBuilder.AppendLine();
			}

			string creditLimitOrganisationCode = string.Empty;
			OrgHeader creditApprovalOrganisation = null;

			var isGroupOrStandalone = Organisation.MiscServ.ARGlobalCreditGroup == null;
			var outOfLimit = (result.CheckResult & CreditLimitCheckResult.OverGlobalCreditLimit) != 0;

			if (isGroupOrStandalone)
			{
				creditLimitOrganisationCode = Organisation.OH_Code.Trim();
				creditApprovalOrganisation = Organisation;
				messageBuilder.AppendLine(Res.GetString("82e4b324-a87e-4340-b8de-6819a7061663", "{0} is a standalone global credit organization or a global credit group.", creditLimitOrganisationCode));
			}
			else
			{
				var globalGroup = Organisation.MiscServ.ARGlobalCreditGroup;
				var globalGroupCode = globalGroup.OH_Code.Trim();
				messageBuilder.AppendLine(Res.GetString("9281046b-d4ce-4a70-9487-cab68a666356", "{0} is a member of global credit group {1}.", Organisation.OH_Code.Trim(), globalGroupCode));
				creditLimitOrganisationCode = globalGroupCode;
				creditApprovalOrganisation = globalGroup;
			}

			var creditCurrencyCode = creditApprovalOrganisation.MiscServ.OM_RX_NKARGlobalCreditCurrency;
			var creditCurrency = creditApprovalOrganisation.MiscServ.ARGlobalCreditCurrency;
			var creditCurrencyDecimals = creditCurrency != null ? creditCurrency.Decimals : RefCurrency.MAX_DECIMAL;

			messageBuilder.AppendLine(Res.GetString("35edac55-60f1-46e8-8164-f9d15847bef2", "The Global Credit Limit for {0} is set to {1} {2}.",
					creditLimitOrganisationCode,
					FormatNumber(result.GlobalCreditLimit, creditCurrencyDecimals),
					result.GlobalCreditCurrencyCode)
				+ " "
				+ (creditApprovalOrganisation != null && creditApprovalOrganisation.MiscServ.OM_ARGlobalCreditApproved
					? Res.GetString("deabf0e8-f0af-4390-9010-3ff303986a24", "Global credit approved.")
					: Res.GetString("55e0306c-a0b1-4edf-88ee-a76563794786", "Global credit pending approval.")));

			if (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
			{
				messageBuilder.Append(isGroupOrStandalone
					? Res.GetString("75E342CC-6E56-4A65-B899-65C4A8F35C00", "The Total Global Outstanding Balance for this group or standalone organization (excluding Open Claims Amount) is {0} {1}, ",
							FormatNumber((result.GlobalTotalOutstandingAmount), creditCurrencyDecimals),
							creditCurrencyCode)
					: Res.GetString("9535F527-5F21-4E8B-88F9-C97464CF9428", "The Total Global Outstanding Balance (excluding Open Claims Amount) is {0} {1}, ",
							FormatNumber((result.GlobalTotalOutstandingAmount), creditCurrencyDecimals),
							creditCurrencyCode));
			}
			else
			{
				messageBuilder.Append(isGroupOrStandalone
					? Res.GetString("64b6d8b8-6428-4c8f-a8f0-03eca6cb2d2b", "The Total Global Outstanding Balance for this group or standalone organization is {0} {1}, ",
							FormatNumber((result.GlobalTotalOutstandingAmount), creditCurrencyDecimals),
							creditCurrencyCode)
					: Res.GetString("44d957ed-0081-4d81-80fa-4e1af208657c", "The Total Global Outstanding Balance is {0} {1}, ",
							FormatNumber((result.GlobalTotalOutstandingAmount), creditCurrencyDecimals),
							creditCurrencyCode));
			}

			messageBuilder.AppendLine(outOfLimit
				? Res.GetString("b22d8a9a-9f73-4bb0-8100-17563aebc0b8", "which is over the global credit limit.")
				: Res.GetString("9809443f-f557-4d77-8c1f-7fdadd8ec1a1", "which has not exceeded the global credit limit threshold."));

			messageBuilder.AppendLine();
			messageBuilder.AppendLine(Res.GetString("32d76cbe-c7a9-45b5-898a-0d813a547ed5", "The Total Global Outstanding Balance is calculated by summing the following:"));
			messageBuilder.AppendLine("  *  " + Res.GetString("cd3d63a4-b649-458d-adab-b5c39de5dae8", "the Posted Global Outstanding Balance, {0} {1}", FormatNumber(result.GlobalPostedOutstandingBalance, creditCurrencyDecimals), creditCurrencyCode));

			if (currentInvoiceOutstandingInCreditCurrency != 0)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("635168d5-d7b2-4e8b-82a3-9ee020bcaed6", "the Current Transaction Amount, {0} {1}", FormatNumber(currentInvoiceOutstandingInCreditCurrency, creditCurrencyDecimals), creditCurrencyCode));
			}

			if (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("B9D197B3-1B1E-41A5-9B88-A49E504FC2ED", "the Open Global Claims Amounts Excluded, {0} {1}", FormatNumber(-result.GlobalClaim, creditCurrencyDecimals), creditCurrencyCode));
			}

			if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedAndRecognized
				|| unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("2810bad8-36ee-4a49-a527-11218bae92ee", "the Unposted Global Recognized Revenue, {0} {1}", FormatNumber(result.GlobalUnpostedRevenueRecognised, creditCurrencyDecimals), creditCurrencyCode));
			}

			if (unpostedRevenueRegistrySetting == Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized)
			{
				messageBuilder.AppendLine("  *  " + Res.GetString("f63cfe18-e044-42a3-bc87-a78e0b0a01d0", "the Unposted Global Unrecognized Revenue, {0} {1}", FormatNumber(result.GlobalUnpostedRevenueUnrecognised, creditCurrencyDecimals), creditCurrencyCode));
			}

			result.Message = messageBuilder.ToString().TrimEnd();
		}

		#endregion

		string GetCreditOnHoldValidation(string ledger)
		{
			string result = string.Empty;

			if (!IsCreditLimitValidationSuspended(Organisation.Factory))
			{
				GetCreditOnHoldValidationCore(ledger, out result);
			}

			return result;
		}

		CreditLimitCheckResult GetCreditOnHoldValidationCore(string ledger, out string message)
		{
			var result = CreditLimitCheckResult.Undefined;
			message = string.Empty;

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				bool onCreditHold = false;

				IOrgCreditLimitAndBalanceDetails cached = null;
				try
				{
					cached = CachedCreditLimitAndOutstandingBalance;
					if (cached != null)
					{
						onCreditHold = cached.OnCreditHold(ledger);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleException(ex);

					message = Res.GetString("f683bed3-9589-473f-bc27-e1c934c8ab01", "Unable to retrieve a value for Credit On Hold.");
					if (cached != null)
					{
						message = cached.GetErrorMessage(message, ex);
					}
					result = CreditLimitCheckResult.UnableToGetCreditOnHold;
				}

				if (onCreditHold)
				{
					message = Res.GetString("72862033-28f0-4fb5-8c7a-06a891cf96ca", "This account is on credit hold and cannot be billed to");
					result = CreditLimitCheckResult.CreditOnHold;
				}
				else if ((result & CreditLimitCheckResult.UnableToGetCreditOnHold) == 0)
				{
					var onGlobalCreditHold = false;
					try
					{
						if (cached != null && cached.IsARGlobalCreditApproved)
						{
							onGlobalCreditHold = cached.OnARGlobalCreditHold;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleException(ex);

						message = Res.GetString("6a97752a-bed7-4346-93fa-3be701174a39", "Unable to retrieve a value for Global Credit On Hold.");
						if (cached != null)
						{
							message = cached.GetErrorMessage(message, ex);
						}
						result = result | CreditLimitCheckResult.UnableToGetGlobalCreditOnHold;
					}

					if (onGlobalCreditHold)
					{
						message = Res.GetString("ef4028e9-b2e5-4929-b415-30ffe975d3ad", "This account is on global credit hold and cannot be billed to");
						result = CreditLimitCheckResult.CreditOnHold;
					}
				}
			}

			return result;
		}

		string FormatNumber(decimal number, int decimalPlaces)
		{
			return number.ToString(string.Format(CultureInfo.InvariantCulture, "N{0}", decimalPlaces), CultureInfo.InvariantCulture);
		}

		#region Cached Balance

		public void ResetCache(bool inAsyncCall)
		{
			var details = Provider.GetOrgCreditLimitAndBalanceDetails(Organisation.OH_Code);

			cachedBalance = details;
			cachedError = null;
			SetCacheExpiry(new TimeSpan(0, expiryMinutes, 0));

			#region SuppressResourceStringsCheckRegion for logging

			if (!inAsyncCall && TraceBuilder.IsEnabled())
			{
				var message = FormattableString.Invariant($"""
															{CriticalValidationInfoCollectorServiceKeyType.CreditLimitCacheReset_IfCollectionActivated}:
															Resetting cache for {Organisation.OH_Code}: 
															{new StackTrace()}
															""");

				TraceBuilder.TraceInformation(() => message);
			}

			#endregion
#if DEBUG
			if (Globals.IsTest)
			{
				CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest++;
			}
#endif
		}

		internal IOrgCreditLimitAndBalanceDetails CachedCreditLimitAndOutstandingBalance
		{
			get
			{
				lock (lockRoot)
				{
					if ((cachedBalance == null && string.IsNullOrEmpty(cachedError)) ||
						(cacheExpireStopwatch != null && cacheExpireStopwatch.Elapsed >= expiryTime))
					{
						ResetCache(inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK);
					}
					else if (!string.IsNullOrEmpty(cachedError))
					{
						throw new InvalidOperationException(cachedError);
					}

					#region SuppressResourceStringsCheckRegion for logging

					if (!inAsyncCall_DO_NOT_ACCESS_WITHOUT_LOCK && TraceBuilder.IsEnabled())
					{
						var message = FormattableString.Invariant($"""
																	{CriticalValidationInfoCollectorServiceKeyType.CreditCheckerCache_IfCollectionActivated}:
																	.{nameof(cachedBalance)} for {Organisation.OH_Code}:
																	{(cachedBalance?.GetAsString(LedgerTypes.AccountsReceivable) ?? string.Empty)}
																	.stackTrace:
																	{new StackTrace()}
																	""");

						TraceBuilder.TraceInformation(() => message);
					}

					#endregion

					return cachedBalance;
				}
			}
		}
		internal IOrgCreditLimitAndBalanceDetails cachedBalance;
		internal string cachedError;
		Stopwatch cacheExpireStopwatch;
		TimeSpan expiryTime;

		void SetCacheExpiry(TimeSpan expiryTime)
		{
			cacheExpireStopwatch = new Stopwatch();
			cacheExpireStopwatch.Start();
			this.expiryTime = expiryTime;
		}

		internal readonly object lockRoot;

		internal void HandleException(Exception ex)
		{
			var isWebServiceError = ex.InnerException is OrgCreditLimitAndBalanceExceptions.WebServiceException;
			if (!isWebServiceError)
			{
				cachedBalance = null;
				cachedError = ex.Message;
				SetCacheExpiry(new TimeSpan(0, AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.Value, 0));
			}
		}

#if DEBUG
		public int CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest
		{
			get;
			set;
		}
#endif

		#endregion

		readonly OrgHeader Organisation;
		readonly CreditCheckTracer TraceBuilder;
		readonly IOrgCreditLimitAndBalanceDetailsProvider Provider;
		readonly int expiryMinutes;

#if DEBUG
		public bool? IsCreditLimitExceededClearCacheForTest(string ledger)
		{
			ClearCreditLimitAndOutstandingBalanceCache();

			return IsCreditLimitExceededAfterThisAmount(ledger, 0m);
		}
#endif

		bool GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				var registry = ObjectFactory.Get<IAccounting>().Registry?.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem;
				return registry?.Value ?? false;
			}
		}

		bool ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				var registry = ObjectFactory.Get<IAccounting>().Registry?.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem;
				return registry?.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) ?? false;
			}
		}

		#endregion
	}
}
