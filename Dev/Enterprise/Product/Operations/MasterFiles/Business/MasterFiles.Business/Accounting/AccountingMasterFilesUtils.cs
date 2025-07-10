using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business
{
	public static class AccountingMasterFilesUtils
	{
		public static int GetMaxRequiredAuthorizationDueToExceedingCreditLimit(params OrgHeader[] organisations)
		{
			int result = 0;
			foreach (OrgHeader org in organisations)
			{
				if (org != null)
				{
					var val = org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit;

					if (val > result)
					{
						result = val;

						if (result == AccountingMasterFilesConstants.MaxPossibleAuthorizationLevel)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		public static IRegistryItem[] NotAllowedForDissectionControlAccount => new IRegistryItem[]
		{
			AccountingMasterFilesRegistry.Instance.TaxTransactionPrepaidAssetControlAccount,
			AccountingMasterFilesRegistry.Instance.TaxTransactionRemittanceLiabilityControlAccount,
			AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount,
			AccountingMasterFilesRegistry.Instance.PendingTaxTransactionRemittanceLiabilityControlAccount,
			AccountingMasterFilesRegistry.Instance.TaxTransactionExpenseAccount,
			AccountingMasterFilesRegistry.Instance.TaxTransactionNegativeRevenueAccount,
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount,
		};

		public static bool IsNotAllowedForDissectionControlAccount(ZGuid glHeaderPK)
		{
			if (glHeaderPK.IsEmpty)
			{
				return false;
			}

			foreach (var item in NotAllowedForDissectionControlAccount)
			{
				if (item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) is Guid && ((Guid)item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == glHeaderPK))
				{
					return true;
				}
			}

			return false;
		}

		public static int GetAuthorisationRequirementWeight(string authorisationRequirement)
		{
			int weight = -1;
			switch (authorisationRequirement)
			{
				case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired:
					weight = 0;
					break;
				case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
					weight = 1;
					break;
				case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
					weight = 2;
					break;
				case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
					weight = 3;
					break;
				case AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.MissingExRate:
					weight = 10;
					break;
			}

			return weight;
		}

		public const int HighestOnCreditHoldControllerSecurityLevel = 3;

		public static SecurityCheckpoint GetOnCreditHoldControllerSecurityCheckPoint(int authorizationLevel)
		{
			SecurityCheckpoint checkPoint = null;
			switch (authorizationLevel)
			{
				case 1:
					checkPoint = Env.Security.OnCreditHoldControllerFirstLevel;
					break;
				case 2:
					checkPoint = Env.Security.OnCreditHoldControllerSecondLevel;
					break;
				case 3:
					checkPoint = Env.Security.OnCreditHoldControllerThirdLevel;
					break;
			}

			return checkPoint;
		}

		public static bool IsForeignAndLocalAmountSameWhenUsingLocalCurrency(ZString transactionCurrency, ZDecimal osAmount, ZDecimal localAmount, string chargeLocalCurrency = null)
		{
			var localCurrencySafe = string.IsNullOrEmpty(chargeLocalCurrency) ? GlbCompany.CurrentCompany.LocalCurrency.Code : chargeLocalCurrency;
			if (!transactionCurrency.IsEmpty && transactionCurrency == localCurrencySafe && localAmount != osAmount)
			{
				return false;
			}
			return true;
		}

		public static bool IsTermWithoutDays(string term)
		{
			return InvoiceTerm.GetIsTermWithoutDays(term) || term == AccountingMasterFilesConstants.DefaultInvoiceTerm;
		}

		public static bool ShouldPreventCreateCreditNote(string ledger, ZGuid companyPK)
		{
			bool isApplicable = false;

			if (companyPK.IsValid)
			{
				switch (ledger)
				{
					case LedgerTypes.AccountsReceivable:
						isApplicable = AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
						break;
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.UnapprovedPayableTransactions:
					case LedgerTypes.TransactionsPendingAllocation:
					case LedgerTypes.IncompleteTransactions:
						isApplicable = AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
						break;
					default:
						throw new NotSupportedException("Unsupported LedgerTypes");
				}
			}

			return isApplicable;
		}

		public static bool ShouldPreventCreateReversalTransactions(string ledger, ZGuid companyPK)
		{
			bool isApplicable = false;
			if (companyPK.IsValid)
			{
				switch (ledger)
				{
					case LedgerTypes.AccountsReceivable:
						isApplicable = AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
						break;
					case LedgerTypes.AccountsPayable:
						isApplicable = AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
						break;
					default:
						throw new NotSupportedException("Unsupported LedgerTypes");
				}
			}

			return isApplicable;
		}

		internal static ZDateTime GetMaxLocationDateTimeByCountryCode(BusinessObjectFactory factory, string countryCode)
		{
			var collection = new DynamicBusinessObjectCollection(factory);

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@CountryCode", countryCode, RefUNLOCOSchema.RL_RN_NKCountryCode);
			sqlParams.Add("@IsActive", true, RefUNLOCOSchema.RL_IsActive);
			collection.Load(@"SELECT MAX(RL_PK) RL_PK FROM dbo.RefUNLOCO WHERE RL_Code LIKE @CountryCode + '%' AND RL_IsActive = @IsActive GROUP BY RL_RN_NKCountryCode, RL_R3;", sqlParams);

			var pks = collection.Select(x => x[RefUNLOCOSchema.PK]).ToArray();
			return factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.PK, pks)).Select(x => x.LocationDateTime).DefaultIfEmpty(ZDateTime.Empty).Max();
		}

		public static ZString ARCreditNoteDisallowedMessage => Res.GetString("6E1606AE-31EF-4D9A-B238-9B9FF5A63606", "Posting of Credit Notes is prevented. {0}", ARCreditNoteDisallowed_RegistryOnlyMessage);

		public static ZString APCreditNoteDisallowedMessage => Res.GetString("6E1606AE-31EF-4D9A-B238-9B9FF5A63606", "Posting of Credit Notes is prevented. {0}", APCreditNoteDisallowed_RegistryOnlyMessage);

		public static ZString ARCreditNoteDisallowed_RegistryOnlyMessage => Res.GetString("4B5548CA-1C4A-4232-943D-38F4A6C88FE6", "This is controlled by the registry setting {0}.", AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.Location());

		public static ZString APCreditNoteDisallowed_RegistryOnlyMessage => Res.GetString("4B5548CA-1C4A-4232-943D-38F4A6C88FE6", "This is controlled by the registry setting {0}.", AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.Location());

		public static ZString ARInvoiceReversalDisallowedMessage => Res.GetString("AAF1E998-3A33-4FE8-9E5F-1703A477CDFE", "Posting is not permitted. Receivables Invoice Transactions cannot be reversed. This is controlled by the registry setting {0}.", AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.Location());

		public static ZString APInvoiceReversalDisallowedMessage => Res.GetString("5DCA4AC4-651F-46DF-8D6B-AE4462F7AAB5", "Posting is not permitted. Payables Invoice Transactions cannot be reversed. This is controlled by the registry setting {0}.", AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.Location());

		public static string GetCountryCodeFromRegistryFallBackLevel(BusinessObjectFactory factory, FallbackLevel currentFallbackLevel)
		{
			Argument.NotNull(factory, nameof(factory));

			var companyPK = currentFallbackLevel != null ? currentFallbackLevel.CompanyPK(false) : Guid.Empty;
			return GetCountryCodeFromCompanyPK(factory, companyPK);
		}

		public static string GetCountryCodeFromCompanyPK(BusinessObjectFactory factory, Guid companyPK)
		{
			Argument.NotNull(factory, nameof(factory));

			return companyPK != Guid.Empty && companyPK != Env.CurrentCompanyPK
				? (string)factory.Load<GlbCompany>(companyPK)?.GC_RN_NKCountryCode
				: Env.CurrentCompany.Country?.Code;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public static GlbBranchCollection GetBranchesOfCurrentCompany(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Accounting" + GlbCompany.CurrentCompany.PK.ToStringKey() + nameof(GlbBranchCollection) + "ActiveCurrentCompany", () =>
			{
				var filter = new ZQuery(GlbBranchSchema.GB_IsActive, true);
				filter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				var branches = new GlbBranchCollection(factory, filter);
				branches.Load();

				return branches;
			});
		}

		public static bool IsTaxBranchApplicable => GlbCompany.CurrentCompany.GC_IsGSTRegistered && AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;

		public static ZGuid GetTaxBranchResetValue(bool otherCondition = true)
			=> (IsTaxBranchApplicable && otherCondition) ? GlbBranch.CurrentBranch.PK : ZGuid.Empty;

		public static bool IsTaxBranchApplicableForTransaction(AccTransactionHeader transactionHeader) => IsTaxBranchApplicable
			&& ((transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable && (transactionHeader.Header?.CompanyData?.IsAPTaxApplicable ?? false))
			|| (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable && (transactionHeader.Header?.CompanyData?.IsARTaxApplicable ?? false)));

		public static bool IsAllowedWithConstraint(this SecurityCheckpoint checkpoint) => (checkpoint as ISupportAllowWithConstraint)?.IsAllowedWithConstraint ?? checkpoint.IsAllowed;

		public static bool IsEnableChinaEInvoicing => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public static bool HasGLAccountSelectionAndEntry => AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) != Guid.Empty;

		public static bool IsValidSIC(string countryCode, string value)
		{
			return new MYMSICCodeDescriptionPairList().ContainsCode(value)
					|| IsRegistrationNumberInFeatureControl(countryCode, value, OrgCusCode.CodeTypes.StandardIndustrialClassification);
		}

		static bool IsRegistrationNumberInFeatureControl(string countryCode, string value, string cusCodeType)
		{
			var registrationNumber = GetRegistrationNumber(countryCode, cusCodeType);
			if (registrationNumber != null)
			{
				return registrationNumber.Numbers?.Any(x => x.Number == value) ?? false;
			}

			return false;
		}

		public static string GetRegistrationNumberDescriptionInFeatureControl(string countryCode, string number, string cusCodeType)
		{
			var registrationNumber = GetRegistrationNumber(countryCode, cusCodeType);
			if (registrationNumber != null)
			{
				return registrationNumber.Numbers?.FirstOrDefault(x => x.Number == number)?.Description ?? string.Empty;
			}

			return string.Empty;
		}

		static RegistrationNumberEntry GetRegistrationNumber(string countryCode, string cusCodeType)
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingRegistrationNumberFeature);
			if (featureData != null && featureData.TryDeserializeParameterAsJson<AccRegistrationNumberFeatureControlData>(out var featureControlData))
			{
				return featureControlData.RegistrationNumbers?.FirstOrDefault(x => x.Country == countryCode && x.Type == cusCodeType);
			}

			return null;
		}

		public static void NudgeServiceTask(string serviceTaskCode)
		{
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(serviceTaskCode);
		}

		public static void UpdateStmLinkDescription(string moduleID, Dictionary<ZGuid, ZString> itemDescriptionDictionary)
		{
			var factory = new BusinessObjectFactory();
			var stmLinkQuery = new ZQuery();
			stmLinkQuery.AddToFilter(StmLinkSchema.STL_ModuleID, moduleID);
			stmLinkQuery.AddToFilter(StmLinkSchema.STL_ItemPK, itemDescriptionDictionary.Select(x => x.Key));
			var stmLinks = factory.Load<StmLink>(stmLinkQuery);
			stmLinks.Cast<StmLink>().ForEach(x => x.STL_ItemDescription = itemDescriptionDictionary.GetValueSafe(x.STL_ItemPK));

			factory.Save();
		}
	}
}
