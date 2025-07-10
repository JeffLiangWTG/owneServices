using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccExchangeRateConfigurationViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOnlyAPLedgerIsAcceptedWhenJobTypeIsFCN()
		{
			var expectedErrorMessage = "Forwarding Consol can only have configuration for AP ledger.";

			AssertNoError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertHasError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertNoError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_Ledger = ZString.Empty;
			AssertHasError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertNoError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertNoError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);

			exRateConfig.JCE_Ledger = ZString.Empty;
			AssertNoError(exRateConfig.JCE_LedgerInfo, expectedErrorMessage);
		}

		public void TestJCE_Ledger()
		{
			exRateConfig.JCE_GC = ZGuid.Empty;
			exRateConfig.JCE_ParentID = ZGuid.Empty;
			AssertARAPEmptyLedgerAccepted();

			exRateConfig.JCE_GC = Environment.Env.CurrentCompanyPK;
			AssertARAPEmptyLedgerAccepted();

			exRateConfig.JCE_ParentID = ZGuid.NewZGuid();
			foreach (var prefix in new[] { OrgDebtorGroupSchema.Constants.Prefix, OrgCreditorGroupSchema.Constants.Prefix, OrgHeaderSchema.Constants.Prefix })
			{
				exRateConfig.JCE_ParentTableCode = prefix;
				AssertOnlyLookupListLedgerCodesAccepted();
			}
		}

		void AssertARAPEmptyLedgerAccepted()
		{
			exRateConfig.JCE_Ledger = "";
			exRateConfig.Validation.ValidateJCE_Ledger();
			AssertNoErrors(exRateConfig.JCE_LedgerInfo);

			foreach (var ledger in new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable })
			{
				exRateConfig.JCE_Ledger = ledger;
				exRateConfig.Validation.ValidateJCE_Ledger();
				AssertNoErrors(exRateConfig.JCE_LedgerInfo);
			}

			exRateConfig.JCE_Ledger = "ZZ";
			exRateConfig.Validation.ValidateJCE_Ledger();
			AssertHasErrors(exRateConfig.JCE_LedgerInfo);
		}

		void AssertOnlyLookupListLedgerCodesAccepted()
		{
			exRateConfig.Validation.ValidateJCE_Ledger();
			AssertHasErrors(exRateConfig.JCE_LedgerInfo);

			foreach (var ledger in exRateConfig.Lookups.LedgerList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_Ledger = ledger;
				exRateConfig.Validation.ValidateJCE_Ledger();
				AssertNoErrors(exRateConfig.JCE_LedgerInfo);
			}

			exRateConfig.JCE_Ledger = "ZZ";
			exRateConfig.Validation.ValidateJCE_Ledger();
			AssertHasErrors(exRateConfig.JCE_LedgerInfo);
		}

		public void TestJCE_JobType()
		{
			exRateConfig.JCE_JobType = "";
			exRateConfig.Validation.ValidateJCE_JobType();
			AssertHasErrors(exRateConfig.JCE_JobTypeInfo);

			foreach (var jobType in exRateConfig.Lookups.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_JobType = jobType;
				exRateConfig.Validation.ValidateJCE_JobType();
				AssertNoErrors(exRateConfig.JCE_JobTypeInfo);
			}

			exRateConfig.JCE_JobType = "ZZZ";
			exRateConfig.Validation.ValidateJCE_JobType();
			AssertHasErrors(exRateConfig.JCE_JobTypeInfo);
		}

		public void TestJCE_ServiceDirection()
		{
			foreach (var jobType in exRateConfig.Lookups.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_JobType = jobType;

				exRateConfig.JCE_ServiceDirection = "";
				exRateConfig.Validation.ValidateJCE_ServiceDirection();
				AssertHasErrors(exRateConfig.JCE_ServiceDirectionInfo);

				foreach (var direction in exRateConfig.Lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					exRateConfig.JCE_ServiceDirection = direction;

					exRateConfig.Validation.ValidateJCE_ServiceDirection();
					AssertNoErrors(exRateConfig.JCE_ServiceDirectionInfo);
				}

				exRateConfig.JCE_ServiceDirection = "ZZZ";
				exRateConfig.Validation.ValidateJCE_ServiceDirection();
				AssertHasErrors(exRateConfig.JCE_ServiceDirectionInfo);
			}
		}

		public void TestJCE_TransportMode()
		{
			foreach (var jobType in exRateConfig.Lookups.JobTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_JobType = jobType;

				exRateConfig.JCE_TransportMode = "";
				exRateConfig.Validation.ValidateJCE_TransportMode();
				AssertHasErrors(exRateConfig.JCE_TransportModeInfo);

				foreach (var mode in exRateConfig.Lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					exRateConfig.JCE_TransportMode = mode;

					exRateConfig.Validation.ValidateJCE_TransportMode();
					AssertNoErrors(exRateConfig.JCE_TransportModeInfo);
				}

				exRateConfig.JCE_TransportMode = "ZZZ";
				exRateConfig.Validation.ValidateJCE_TransportMode();
				AssertHasErrors(exRateConfig.JCE_TransportModeInfo);
			}
		}

		public void TestJCE_Preference()
		{
			exRateConfig.JCE_Preference = "";
			exRateConfig.Validation.ValidateJCE_Preference();
			AssertHasErrors(exRateConfig.JCE_PreferenceInfo);

			foreach (var preference in exRateConfig.Lookups.PreferenceList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				exRateConfig.JCE_Preference = preference;
				exRateConfig.Validation.ValidateJCE_Preference();
				AssertNoErrors(exRateConfig.JCE_PreferenceInfo);
			}

			exRateConfig.JCE_Preference = "ZZZ";
			exRateConfig.Validation.ValidateJCE_Preference();
			AssertHasErrors(exRateConfig.JCE_PreferenceInfo);
		}

		public void TestJCE_OffsetCanBeEmpty()
		{
			exRateConfig.JCE_Offset = ZInt.Zero;
			exRateConfig.Validation.ValidateJCE_Offset();
			AssertNoErrors(exRateConfig.JCE_OffsetInfo);
		}

		public void TestJCE_ParentTableCodeCanBeEmpty()
		{
			exRateConfig.JCE_ParentTableCode = ZString.Empty;

			exRateConfig.JCE_ParentID = ZGuid.Empty;
			exRateConfig.Validation.ValidateJCE_ParentTableCode();
			AssertNoErrors(exRateConfig.JCE_ParentTableCodeInfo);

			exRateConfig.JCE_ParentID = ZGuid.NewZGuid();
			exRateConfig.Validation.ValidateJCE_ParentTableCode();
			AssertHasErrors(exRateConfig.JCE_ParentTableCodeInfo);
		}

		public void TestCheckForIncorrectInvoiceCurrencyTypeConfigCombination()
		{
			AssertCollectionCombination(true, new[] { Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable });
			AssertCollectionCombination(false, new[] { Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign });
			AssertCollectionCombination(false, new[] { Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable });
			AssertCollectionCombination(false, new[] { Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Constants.InvoicePostingExchangeRateCurrencyType.Code.NotApplicable });

			void AssertCollectionCombination(bool expectError, params string[] invoiceCurrencyTypes)
			{
				var exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory);
				foreach (var invoiceCurrencyType in invoiceCurrencyTypes)
				{
					var exRateConfig = exRateConfigCollection.AddNew();
					exRateConfig.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
					exRateConfig.JCE_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
					exRateConfig.JCE_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
					exRateConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
					exRateConfig.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;
					exRateConfig.JCE_InvoiceCurrencyType = invoiceCurrencyType;
				}

				foreach (AccExchangeRateConfiguration config in exRateConfigCollection)
				{
					config.Validation.ValidateJCE_InvoiceCurrencyType();
					if (expectError)
					{
						AssertHasRowError(config, @"You cannot have duplicate configurations with invoice currency type equals LOC, FOR and ALL at the same time.
It should only allow configuration at the same time when: LOC & FOR / LOC & Blank / FOR & Blank");
					}
					else
					{
						AssertNoRowErrors(config);
					}
				}
			}
		}

		public void TestJCE_CurrencyType()
		{
			exRateConfig.JCE_Calc_CurrencyType = null;
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertHasError("Cannot be null", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Please enter a Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = ZString.Empty;
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertHasError("Cannot be empty", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Please enter a Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = "XXX";
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertHasError("Should have valid value", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Enter a valid Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoErrors(exRateConfig.JCE_Calc_CurrencyTypeInfo);

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoErrors(exRateConfig.JCE_Calc_CurrencyTypeInfo);

			exRateConfig.CurrencyConfigurations.AddNew();
			exRateConfig.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoErrors(exRateConfig.JCE_Calc_CurrencyTypeInfo);
		}

		public void TestJCE_CurrencyType_RunPreSaveValidation()
		{
			exRateConfig.JCE_Calc_CurrencyType = null;
			exRateConfig.RunPreSaveValidation();
			AssertHasError("Cannot be null", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Please enter a Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = ZString.Empty;
			exRateConfig.RunPreSaveValidation();
			AssertHasError("Cannot be empty", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Please enter a Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = "XXX";
			exRateConfig.RunPreSaveValidation();
			AssertHasError("Should have valid value", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Enter a valid Currency Selection.");

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			exRateConfig.RunPreSaveValidation();
			AssertHasError("Currency Type CUR, should have at least one currency configuration", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Cannot set Currency Selection to CUR. Please add at least one Currency Configuration.");

			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			exRateConfig.RunPreSaveValidation();
			AssertNoErrors(exRateConfig.JCE_Calc_CurrencyTypeInfo);

			var curConfig = exRateConfig.CurrencyConfigurations.AddNew();
			curConfig.JCT_Code = Constants.CurrencyCodes.UnitedStates;
			exRateConfig.RunPreSaveValidation();
			AssertHasError("Currency Type ALL, should have no currency configurations", exRateConfig.JCE_Calc_CurrencyTypeInfo, "Cannot set Currency Selection to ALL. Please remove Custom Currency Configuration.");
		}

		public void TestCheckForDuplicates()
		{
			var exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory);
			AssertCollectionForDuplicates(exRateConfigCollection, LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable);

			exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory, Environment.Env.CurrentCompanyPK);
			AssertCollectionForDuplicates(exRateConfigCollection, LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable);

			var debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory, Environment.Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, debtorGroup.PK);
			AssertCollectionForDuplicates(exRateConfigCollection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory, Environment.Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, debtorGroup.PK, org.PK);
			AssertCollectionForDuplicates(exRateConfigCollection);

			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory, Environment.Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, creditorGroup.PK);
			AssertCollectionForDuplicates(exRateConfigCollection);

			org.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			exRateConfigCollection = new AccExchangeRateConfigurationCollection(Factory, Environment.Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, creditorGroup.PK, org.PK);
			AssertCollectionForDuplicates(exRateConfigCollection);
		}

		public void TestCheckForDuplicates_JCE_CurrencyType()
		{
			var exRateConfig2 = Factory.New<AccExchangeRateConfiguration>();
			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			exRateConfig2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

			exRateConfig2.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoRowErrors(exRateConfig2);

			var collection = new AccExchangeRateConfigurationCollection(Factory);
			collection.Add(exRateConfig);
			collection.Add(exRateConfig2);

			exRateConfig2.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoRowErrors(exRateConfig2);

			exRateConfig2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			exRateConfig2.Validation.ValidateJCE_Calc_CurrencyType();
			AssertNoRowErrors(exRateConfig2);
		}

		public void TestCheckForDuplicates_JCE_CurrencyType_RunPreSaveValidation()
		{
			var exRateConfig2 = Factory.New<AccExchangeRateConfiguration>();
			exRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;
			exRateConfig2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

			exRateConfig2.RunPreSaveValidation();
			AssertNoRowErrors(exRateConfig2);

			var collection = new AccExchangeRateConfigurationCollection(Factory);
			collection.Add(exRateConfig);
			collection.Add(exRateConfig2);

			exRateConfig2.RunPreSaveValidation();
			AssertHasRowError(exRateConfig2, "At least one more record already sets Exchange Rate details for the same Job parameters.");

			exRateConfig2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			exRateConfig2.RunPreSaveValidation();
			AssertNoRowErrors(exRateConfig2);
		}

		void AssertCollectionForDuplicates(AccExchangeRateConfigurationCollection exRateConfigCollection, params string[] ledgersToSet)
		{
			var exRateConfig = exRateConfigCollection.AddNew();

			exRateConfig.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			exRateConfig.JCE_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			exRateConfig.JCE_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			exRateConfig.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;

			exRateConfig.Validation.ValidateAll();
			AssertNoErrors(exRateConfig);

			var exRateConfigDup = exRateConfigCollection.AddNew();
			exRateConfigDup.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			exRateConfigDup.JCE_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			exRateConfigDup.JCE_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			exRateConfigDup.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;

			exRateConfigDup.Validation.ValidateAll();

			AssertNoRowErrors(exRateConfigDup);
			exRateConfigDup.JCE_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			AssertHasRowError(exRateConfigDup, AccExchangeRateConfigurationViewValidation.IsDuplicateErrorString);

			exRateConfigDup.JCE_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			AssertNoRowErrors(exRateConfigDup);
			exRateConfigDup.JCE_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			AssertHasRowError(exRateConfigDup, AccExchangeRateConfigurationViewValidation.IsDuplicateErrorString);

			exRateConfigDup.JCE_JobType = JobInvoicingConsumerTypes.TransportBookingCode;
			AssertNoRowErrors(exRateConfigDup);
			exRateConfigDup.JCE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertHasRowError(exRateConfigDup, AccExchangeRateConfigurationViewValidation.IsDuplicateErrorString);

			exRateConfig.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Local;
			exRateConfigDup.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
			AssertNoRowErrors(exRateConfigDup);
			exRateConfig.JCE_InvoiceCurrencyType = string.Empty;
			exRateConfigDup.JCE_InvoiceCurrencyType = Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
			AssertNoRowErrors(exRateConfigDup);
			exRateConfigDup.JCE_InvoiceCurrencyType = string.Empty;
			AssertHasRowError(exRateConfigDup, AccExchangeRateConfigurationViewValidation.IsDuplicateErrorString);

			var defaultLedger = exRateConfigDup.JCE_Ledger;
			foreach (var ledger in ledgersToSet)
			{
				exRateConfigDup.JCE_Ledger = ledger;
				AssertNoRowErrors(exRateConfigDup);
				exRateConfigDup.JCE_Ledger = defaultLedger;
				AssertHasRowError(exRateConfigDup, AccExchangeRateConfigurationViewValidation.IsDuplicateErrorString);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			exRateConfig = Factory.New<AccExchangeRateConfiguration>();
		}
		AccExchangeRateConfiguration exRateConfig;

		#endregion
	}
}
