using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using NUnit.Framework.TestHelper;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	public abstract class CountryComplianceInfoTest : TestCaseWithFactory
	{
		#region Fields

		protected readonly Type TestsInstancesOfType;

		protected abstract string CountryCode { get; }

		protected ICountryComplianceInfoBase GetCountryComplianceInfo => ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode);

		protected IEInvoiceCredentialsProvider EInvoiceCredentialsProvider
			=> eInvoiceCredentialsProvider ??= ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as IEInvoiceCredentialsProvider;

		IEInvoiceCredentialsProvider eInvoiceCredentialsProvider;

		#endregion

		public CountryComplianceInfoTest()
		{
			TestsInstancesOfType = TestedTypeHelper.GetTestedType(GetType());
		}

		public void TestCountryCodeMatches()
		{
			// Make an instance of the tested type
			var countryComplianceInfo = (CountryComplianceInfo)Activator.CreateInstance(TestsInstancesOfType);

			AssertEquals($"{CountryCode} compliance info match the country code for the test file", CountryCode, countryComplianceInfo.CountryCode);
		}

		public void TestCountryComplianceEligibilityForReQueueOfReversedTransaction()
		{
			var countryComplianceInfo = CreateCurrentCountryComplianceInfo() as IComplianceInfoEInvoicingGUIActionQueueReversedTransaction;

			var testSubTypeList = new List<string> { "ANY", string.Empty, null };
			testSubTypeList.AddRange(ExpectedComplianceSubTypes);

			var exclusionSubTypes = new List<string>(ExpectedRejectReQueueForReversedTransactionExclusionSubTypes);

			foreach (var oneSubType in testSubTypeList)
			{
				var shouldReject = RejectReQueueForReversedTransactionCountries.Contains(CountryCode) && !exclusionSubTypes.Contains(oneSubType);
				var actuallyRejected = countryComplianceInfo?.RejectReQueueForReversedTransaction(oneSubType) ?? false;

				AssertEquals($"{CountryCode} compliance info should {(shouldReject ? "reject" : "allow")} requeue of subtype {oneSubType}", shouldReject, actuallyRejected);
			}
		}

		List<string> RejectReQueueForReversedTransactionCountries => new List<string>
		{
			CountryCodes.Turkey,
			CountryCodes.China
		};

		protected virtual string[] ExpectedRejectReQueueForReversedTransactionExclusionSubTypes => Array.Empty<string>();

		public void TestProtectComplianceSubTypeForEInvoicingTransactions()
		{
			var countryComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IProtectComplianceSubTypeForEInvoicingTransactions;
			ProtectComplianceSubTypeForEInvoicingTransactionsEligibleComplianceCountries.TryGetValue(CountryCode, out var expected);
			AssertEquals(expected, countryComplianceInfo is IProtectComplianceSubTypeForEInvoicingTransactions);
		}

		Dictionary<string, bool> ProtectComplianceSubTypeForEInvoicingTransactionsEligibleComplianceCountries =>
			new Dictionary<string, bool>
		{
		   { CountryCodes.Turkey, true },
		};

		public void TestEnableTransactionsPendingAllocationAllocateAsReceivable()
		{
			var countryComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode);
			AssertEquals(EnableTransactionsPendingAllocationAllocateAsReceivableCountries.Contains(CountryCode), countryComplianceInfo is IEnableTransactionsPendingAllocationAllocateAsReceivable);
		}

		List<string> EnableTransactionsPendingAllocationAllocateAsReceivableCountries => new List<string>
		{
		   CountryCodes.Turkey
		};

		#region TestComplianceSubTypes

		public void TestComplianceSubTypes()
		{
			if (!typeof(IComplianceSubTypeCodeProvider).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}

			CombineAssertions("Preconditions", () =>
			{
				Assert("ExpectedComplianceSubTypes.Length > 0", ExpectedComplianceSubTypes.Length > 0);
				AssertNotNullOrEmpty("ExpectedCompliaceSubTypeCodeForDescriptionTest", ExpectedComplianceSubTypeCodeForDescriptionTest);
				AssertNotNullOrEmpty("ExpectedComplianceSubTypeDescription", ExpectedComplianceSubTypeDescription);
				AssertNotNullOrEmpty("ExpectedComplianceSubTypeLocalDescription", ExpectedComplianceSubTypeLocalDescription);
			});

			AssertContainsExactElementsInAnyOrder(ExpectedComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode).GetAllCodes());
			AssertContainsExactElementsInAnyOrder(ExpectedComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode).GetAllCodes());
			if (ExpectedComplianceSubTypes.Length != 0)
			{
				AssertEquals(ExpectedComplianceSubTypeDescription, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode).GetDescriptionFromCode(ExpectedComplianceSubTypeCodeForDescriptionTest));
				AssertEquals(ExpectedComplianceSubTypeLocalDescription, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode).GetDescriptionFromCode(ExpectedComplianceSubTypeCodeForDescriptionTest));
			}
		}

		public void TestComplianceSubTypesByLedgerAndTransactionType()
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("AR Compliance Sub Types", ExpectedReceivablesComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AR Compliance Sub Types Local Language", ExpectedReceivablesComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsReceivable).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP Compliance Sub Types", ExpectedPayablesComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AP Compliance Sub Types Local Language", ExpectedPayablesComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsPayable).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR INV Compliance Sub Types", ExpectedReceivablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AR INV Compliance Sub Types Local Language", ExpectedReceivablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP INV Compliance Sub Types", ExpectedPayablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.Invoice).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AP INV Compliance Sub Types Local Language", ExpectedPayablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.Invoice).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR CRD Compliance Sub Types", ExpectedReceivablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AR CRD Compliance Sub Types Local Language", ExpectedReceivablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP CRD Compliance Sub Types", ExpectedPayablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote).GetAllCodes());
				AssertContainsExactElementsInAnyOrder("AP CRD Compliance Sub Types Local Language", ExpectedPayablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote).GetAllCodes());
			});
		}

		public void TestComplianceSubTypesForReceivableTransactionsByTransactionCreatingType()
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("AR INV Original Transaction Compliance Sub Types",
					ExpectedReceivablesOriginalInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionCreatingMode.Original).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR CRD Original Transaction Compliance Sub Types",
					ExpectedReceivablesOriginalCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, TransactionCreatingMode.Original).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR INV Amending Transaction Compliance Sub Types",
					ExpectedReceivablesAmendingInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionCreatingMode.Amending).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR CRD Amending Transaction Compliance Sub Types",
					ExpectedReceivablesAmendingCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, TransactionCreatingMode.Amending).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR INV Reversal Transaction Compliance Sub Types",
					ExpectedReceivablesReversalInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionCreatingMode.Reversal).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AR CRD Reversal Transaction Compliance Sub Types",
					ExpectedReceivablesReversalCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, TransactionCreatingMode.Reversal).GetAllCodes());
			});
		}

		public void TestComplianceSubTypesForPayableTransactionsByTransactionCreatingType()
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("AP INV Original Transaction Compliance Sub Types",
					ExpectedPayablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionCreatingMode.Original).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP CRD Original Transaction Compliance Sub Types",
					ExpectedPayablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, TransactionCreatingMode.Original).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP INV Amending Transaction Compliance Sub Types",
					ExpectedPayablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionCreatingMode.Amending).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP CRD Amending Transaction Compliance Sub Types",
					ExpectedPayablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, TransactionCreatingMode.Amending).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP INV Reversal Transaction Compliance Sub Types",
					ExpectedPayablesInvoiceComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionCreatingMode.Reversal).GetAllCodes());

				AssertContainsExactElementsInAnyOrder("AP CRD Reversal Transaction Compliance Sub Types",
					ExpectedPayablesCreditNoteComplianceSubTypes, AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(CountryCode, LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, TransactionCreatingMode.Reversal).GetAllCodes());
			});
		}

		public void TestLedgerOfUse()
		{
			if (!typeof(IComplianceSubTypeCodeProvider).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}

			foreach (var subType in CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(CountryCode)?.GetComplianceSubTypes())
			{
				if (ExpectedComplianceLedgerOfUse != null && ExpectedComplianceLedgerOfUse.TryGetValue(subType.Code, out var ledger))
				{
					AssertEquals(string.Format("{0} - {1}", CountryCode, subType.Code), ledger, subType.Ledger);
				}
				else
				{
					AssertEquals(string.Format("{0} - {1}", CountryCode, subType.Code), LedgerOfUse.ALL, subType.Ledger);
				}
			}
		}

		public virtual void TestTransactionTypeOfUse()
		{
			if (!typeof(IComplianceSubTypeCodeProvider).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}

			foreach (var subType in CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(CountryCode)?.GetComplianceSubTypes())
			{
				if (ExpectedComplianceTransactionTypeOfUse != null && ExpectedComplianceTransactionTypeOfUse.TryGetValue(subType.Code, out var transactionType))
				{
					AssertEquals(string.Format("{0} - {1}", CountryCode, subType.Code), transactionType, subType.TransactionType);
				}
				else if (ExpectedComplianceTransactionTypesOfUse != null && ExpectedComplianceTransactionTypesOfUse.TryGetValue(subType.Code, out var transactionTypes))
				{
					Assert(string.Format("{0} - {1}", CountryCode, subType.Code), transactionTypes.Contains(subType.TransactionType));
				}
				else
				{
					AssertEquals(string.Format("{0} - {1}", CountryCode, subType.Code), TransactionTypeOfUse.ALL, subType.TransactionType);
				}
			}
		}

		protected virtual string[] ExpectedComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedReceivablesComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedPayablesComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedReceivablesInvoiceComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedReceivablesOriginalInvoiceComplianceSubTypes => ExpectedReceivablesInvoiceComplianceSubTypes;

		protected virtual string[] ExpectedReceivablesAmendingInvoiceComplianceSubTypes => ExpectedReceivablesInvoiceComplianceSubTypes;

		protected virtual string[] ExpectedReceivablesReversalInvoiceComplianceSubTypes => ExpectedReceivablesInvoiceComplianceSubTypes;

		protected virtual string[] ExpectedPayablesInvoiceComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedReceivablesCreditNoteComplianceSubTypes => Array.Empty<string>();

		protected virtual string[] ExpectedReceivablesOriginalCreditNoteComplianceSubTypes => ExpectedReceivablesCreditNoteComplianceSubTypes;

		protected virtual string[] ExpectedReceivablesAmendingCreditNoteComplianceSubTypes => ExpectedReceivablesCreditNoteComplianceSubTypes;

		protected virtual string[] ExpectedReceivablesReversalCreditNoteComplianceSubTypes => ExpectedReceivablesCreditNoteComplianceSubTypes;

		protected virtual string[] ExpectedPayablesCreditNoteComplianceSubTypes => Array.Empty<string>();

		protected virtual Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => null;

		protected virtual IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => ImmutableDictionary.Create<string, TransactionTypeOfUse>();

		protected virtual IReadOnlyDictionary<string, TransactionTypeOfUse[]> ExpectedComplianceTransactionTypesOfUse => null;

		protected virtual string ExpectedComplianceSubTypeCodeForDescriptionTest => "";

		protected virtual string ExpectedComplianceSubTypeDescription => "";

		protected virtual string ExpectedComplianceSubTypeLocalDescription => "";

		protected virtual string[] ExpectedEInvoiceEligibleComplianceSubType => Array.Empty<string>();

		#endregion

		#region ExpectedDefaultValues

		protected virtual string ExpectedBusinessRegistrationCode => null;

		protected virtual string ExpectedComplianceSequencePrefixErrorMessage => null;

		protected virtual string ExpectedComplianceSequencePrefixRegex => null;

		protected virtual string ExpectedRecipientLocalBusinessRegNumberCodeType => null;

		protected virtual string ExpectedRecipientLocalBusinessRegHeading => null;

		protected virtual string ExpectedRecipientTaxIDHeading => null;

		protected virtual bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => null;

		protected virtual bool ExpectedOAuthShouldShowCredentialsTab => false;

		protected virtual string ExpectedOAuthAuthorizationURLInProd => null;

		protected virtual string ExpectedOAuthAuthorizationURLInTest => null;

		protected virtual string ExpectedOAuthEncryptionKey => null;

		#endregion

		#region RuleSet

		public void TestAllRuleSetCodesAreUsed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
				if (ruleSetProvider != null)
				{
					var ruleSetsFromProvider = ruleSetProvider.GetRuleSet();
					foreach (CodeDescriptionPair pair in ruleSetsFromProvider)
					{
						var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
						CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection, pair.Code);

						Assert($"Rules should be present with rule set: {pair.Code}", collection.Cast<ComplianceSubTypeAttributionRuleConfiguration>().Any());
					}
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public void TestRuleSetCodesInRulesAreInSyncWithGetRuleSet()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection, string.Empty);

				var ruleSetCodesFromRules = collection.Cast<ComplianceSubTypeAttributionRuleConfiguration>().GroupBy(x => x.RuleSetCode).Select(y => y.Key);

				var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
				if (ruleSetProvider != null)
				{
					var ruleSetCodesFromProvider = ruleSetProvider.GetRuleSet();
					foreach (var ruleSetCode in ruleSetCodesFromRules)
					{
						Assert(ruleSetCodesFromProvider.ContainsCode(ruleSetCode));
					}
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public void TestIComplianceSubTypeRulesWithMultipleRuleSetProvider_GetRuleSetMustNotBeEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				if (typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider).IsAssignableFrom(TestsInstancesOfType))
				{
					var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
					var ruleSetCodesFromProvider = ruleSetProvider.GetRuleSet();
					Assert("Rule Set", ruleSetCodesFromProvider.Count > 0);
				}
				else
				{
					Assert("Not applicable", true);
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleSet()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var registryValue = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).RuleSetCode;

				var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
				if (ruleSetProvider != null)
				{
					var valueFromProvider = ruleSetProvider.GetDefaultRuleSet();
					AssertEquals(valueFromProvider, registryValue);
				}
				else
				{
					AssertEquals(string.Empty, registryValue);
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfigurationRegistry_GetsValuesFrom_IComplianceSubTypeRuleProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				if (typeof(IComplianceSubTypeRuleProvider).IsAssignableFrom(TestsInstancesOfType))
				{
					var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
					var registryValue = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
					CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection);
					AssertTestComplianceSubTypeAttributionRuleConfiguration(registryValue, collection);
				}
				else
				{
					Assert("Test is not applicable", true);
					return;
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfigurationRegistry_GetsValuesFrom_IComplianceSubTypeRulesWithMultipleRuleSetProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				if (typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider).IsAssignableFrom(TestsInstancesOfType))
				{
					var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
					foreach (var ruleSetCode in ruleSetProvider.GetRuleSet().GetAllCodes())
					{
						var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
						var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ruleSetCode);
						AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ruleSet);

						var registryValue = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;

						var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
						ruleSetProvider.SetComplianceSubTypeAttributionRuleConfigurations(collection, ruleSetCode);

						AssertTestComplianceSubTypeAttributionRuleConfiguration(registryValue, collection);
					}
				}
				else
				{
					Assert("Test is not applicable", true);
					return;
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfigurationRegistry_ForCountriesWithoutIComplianceSubTypeRuleProviderInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var countriesWithComplianceSubTypeRulesDeclaredDirectlyInRegistry = new List<string>()
				{
					CountryCodes.Peru,
					CountryCodes.Ecuador,
					CountryCodes.Guatemala,
					CountryCodes.Honduras
				};

				if (!(typeof(IComplianceSubTypeRuleProvider).IsAssignableFrom(TestsInstancesOfType) ||
					typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider).IsAssignableFrom(TestsInstancesOfType)))
				{
					var valuesFromSubTypeAttributionRuleRegistryProperty = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value.ToList();
					if (countriesWithComplianceSubTypeRulesDeclaredDirectlyInRegistry.Contains(CountryCode))
					{
						Assert("Countries declared compliance sub type rules directly in registry", valuesFromSubTypeAttributionRuleRegistryProperty.Count != 0);
					}
					else
					{
						Assert("ComplianceSubTypeAttributionRuleConfiguration for country without compliance sub type rules", valuesFromSubTypeAttributionRuleRegistryProperty.Count == 0);
					}
				}
				else if (countriesWithComplianceSubTypeRulesDeclaredDirectlyInRegistry.Contains(CountryCode))
				{
					Fail($"Please remove country from {nameof(countriesWithComplianceSubTypeRulesDeclaredDirectlyInRegistry)}");
				}
				else
				{
					Assert("Test is not applicable", true);
					return;
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleSetRegistry_RuleSetCodeList_GetsValuesFrom_IComplianceSubTypeRulesWithMultipleRuleSetProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				if (typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider).IsAssignableFrom(TestsInstancesOfType))
				{
					var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);
					var valuesFromGetRuleSetInterfaceMethod = ruleSetProvider.GetRuleSet().GetAllCodes();

					var complianceSubTypeAttributionRuleSetRegistry = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.Value;
					complianceSubTypeAttributionRuleSetRegistry.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					var valuesFromRuleSetCodeListRegistryProperty = complianceSubTypeAttributionRuleSetRegistry.RuleSetCodeList.GetAllCodes();

					AssertArrayEqualsByElements(valuesFromRuleSetCodeListRegistryProperty, valuesFromGetRuleSetInterfaceMethod);
				}
				else
				{
					Assert("Test is not applicable", true);
					return;
				}
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration()
		{
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

			if (typeof(IComplianceSubTypeRuleProvider).IsAssignableFrom(TestsInstancesOfType))
			{
				CountryComplianceFactory.GetIComplianceSubTypeRuleProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection);
			}
			else if (typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider).IsAssignableFrom(TestsInstancesOfType))
			{
				CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode)?.SetComplianceSubTypeAttributionRuleConfigurations(collection, null);
			}
			else
			{
				Assert("Test is not applicable", true);
				return;
			}

			AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, ExpectedComplianceRules);
		}

		protected virtual Func<ComplianceSubTypeAttributionRuleConfiguration, string>[] ExpectedComplianceRuleFields =>
			new Func<ComplianceSubTypeAttributionRuleConfiguration, string>[]
			{
				x => x.Country,
				x => x.SubType,
				x => x.LedgerType,
				x => x.InvoiceType,
				x => x.TaxInvoiceRule,
				x => x.DisbursementRule,
				x => x.OriginalRule,
				x => x.OrganisationLocation,
				x => x.ParentTransactionSubType,
				x => x.TaxRegistrationType,
				x => x.RuleSetCode,
				x => x.RuleSetDescription,
				x => x.SelfBillingRule,
				x => x.TaxRegistrationLocationRule,
				x => x.VATGroupRule,
				x => x.TaxIDCode,
				x => x.RequiredTaxSystem,
				x => x.ExcludedTaxSystem,
			};

		protected void AssertTestComplianceSubTypeAttributionRuleConfiguration(ComplianceSubTypeAttributionRuleConfigurationCollection collection, ComplianceSubTypeAttributionRuleConfigurationCollection expected)
		{
			var expectedBuilder = new ZStringBuilder();
			for (var i = 0; i <= expected.Count - 1; i++)
			{
				expectedBuilder.AppendLine(string.Join(",", ExpectedComplianceRuleFields.Select(x => x(expected[i]))));
			}

			var txt = expectedBuilder.ToString();
			AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, txt);
		}

		protected void AssertTestComplianceSubTypeAttributionRuleConfiguration(ComplianceSubTypeAttributionRuleConfigurationCollection collection, string expectedComplianceRules)
		{
			var builder = new ZStringBuilder();
			for (var i = 0; i <= collection.Count - 1; i++)
			{
				builder.AppendLine(string.Join(",", ExpectedComplianceRuleFields.Select(x => x(collection[i]))));
			}

			var txt = builder.ToString();
			AssertMultilineASCIIEquals(expectedComplianceRules, txt);
		}

		protected void AddComplianceSubTypeAttributionRule(ComplianceSubTypeAttributionRuleConfigurationCollection collection,
			string country = "",
			string subType = "",
			string ledger = "",
			string invoiceType = "",
			string taxInvoiceRule = "",
			string originalRule = "",
			string disbursementRule = "",
			string taxRegistrationType = "",
			string organisationLocation = "",
			string ruleSetCode = "",
			string ruleSetDescription = "",
			string parentTransactionSubType = "",
			string selfBillingRule = "",
			string taxRegistrationLocationRule = "",
			string vatGroupRule = "",
			string taxIDCode = "",
			string requiredTaxSystem = "",
			string excludedTaxSystem = "")
		{
			var configuration = collection.AddNew();
			configuration.Country = country;
			configuration.SubType = subType;
			configuration.LedgerType = ledger;
			configuration.InvoiceType = invoiceType;
			configuration.TaxInvoiceRule = taxInvoiceRule;
			configuration.OriginalRule = originalRule;
			configuration.DisbursementRule = disbursementRule;
			configuration.TaxRegistrationType = taxRegistrationType;
			configuration.OrganisationLocation = organisationLocation;
			configuration.RuleSetCode = ruleSetCode;
			configuration.RuleSetDescription = ruleSetDescription;
			configuration.ParentTransactionSubType = parentTransactionSubType;
			configuration.SelfBillingRule = selfBillingRule;
			configuration.TaxRegistrationLocationRule = taxRegistrationLocationRule;
			configuration.VATGroupRule = vatGroupRule;
			configuration.TaxIDCode = taxIDCode;
			configuration.RequiredTaxSystem = requiredTaxSystem;
			configuration.ExcludedTaxSystem = excludedTaxSystem;
		}

		protected virtual string ExpectedComplianceRules => "";

		#endregion

		#region ICountryComplianceElectronicInvoiceEligibleSubType

		public virtual void TestEInvoiceEliglibleComplianceSubTypes()
		{
			if (!typeof(IComplianceInfoElectronicInvoicingEligibleSubType).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}
			CombineAssertions("Preconditions", () =>
			{
				Assert("ExpectedComplianceSubTypes.Length > 0", ExpectedEInvoiceEligibleComplianceSubType.Length > 0);
			});
			AssertContainsExactElementsInAnyOrder(ExpectedEInvoiceEligibleComplianceSubType, CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode).GetEligibleComplianceSubTypeListForEInvoicing());
		}

		#endregion

		#region IEInvoiceCredentialsProvider

		public virtual void TestShouldShowCredentialsTab()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			AssertEquals(ExpectedOAuthShouldShowCredentialsTab, EInvoiceCredentialsProvider.ShouldShowCredentialsTab(Env.CurrentCompany));
		}

		public void TestCreateOAuthData()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			AssertNoExceptionThrown(() => EInvoiceCredentialsProvider.CreateOAuthData(Env.CurrentCompany));
		}

		protected virtual ICompany ExpectedOAuthCompany()
		{
			var mockCompany = new Mock<ICompany>();
			mockCompany.Setup(x => x.Code).Returns("ABC");

			return mockCompany.Object;
		}

		public virtual void TestGetAuthorizationURL()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			try
			{
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				var encryptedUrl = eInvoiceCredentialsProvider.GetAuthorizationURL(ExpectedOAuthCompany());
				var decryptedUrl = DecyptAuthorizationURL(encryptedUrl);

				AssertEquals("Url launched should start with expectedStart.", ExpectedOAuthAuthorizationURLInProd, decryptedUrl);

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
				encryptedUrl = eInvoiceCredentialsProvider.GetAuthorizationURL(ExpectedOAuthCompany());
				decryptedUrl = DecyptAuthorizationURL(encryptedUrl);

				AssertEquals("Url launched should start with expectedStart.", ExpectedOAuthAuthorizationURLInTest, decryptedUrl);
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		/// <summary>
		/// We get a URL from cw1 code to test.
		/// This URL has a part that is encrypted.
		/// Like this https://...un-encrypted-part...&state=ENCRYPTED-PART
		/// So, We need to decrypt that state part to be able to assert URL.
		/// </summary>
		/// <param name="authorizationURL">URL with encrypted state</param>
		/// <returns>The URL with its state decrypted.</returns>
		protected virtual string DecyptAuthorizationURL(string authorizationURL)
		{
			var stateStr = "&state=";
			var urlWithoutStateLength = authorizationURL.IndexOf(stateStr) + stateStr.Length;
			var urlWithoutState = authorizationURL.Substring(0, urlWithoutStateLength);
			var encryptedState = authorizationURL.Substring(urlWithoutStateLength);

			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(ExpectedOAuthEncryptionKey);
			var aesCrypto = new AESCrypto(HashAlgorithmName.SHA256);
			var decryptedState = aesCrypto.DecryptStringAES(encryptedState, aesEncryptionKey);

			return urlWithoutState + decryptedState;
		}

		[TestDate(2024, 5, 11)]
		public virtual void TestCreateOrUpdateEInvoicingCredential_Create()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var credential = Factory.LoadTop1<GlbCompanyEInvoicingCertificateCredential>(new ZQuery());
			AssertNull(credential);

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(Env.CurrentCompany);

			credential = Factory.LoadTop1<GlbCompanyEInvoicingCertificateCredential>(new ZQuery());
			AssertNotNull("New credential should be created.", credential);
			AssertEquals(OAuthHelper.CredentialStatusReasonApplyingToken, credential.GP_StatusReason);

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(Env.CurrentCompany);

			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Pending, expectedIssueDate: ZDateTime.Empty, expectedExpiryDate: ZDateTime.Empty);
			AssertEquals(OAuthHelper.CredentialStatusReasonApplyingToken, credential.GP_StatusReason);
		}

		[TestDate(2024, 5, 11)]
		public virtual void TestCreateOrUpdateEInvoicingCredential_Update()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var expectedIssueDate = ZDateTime.Today.AddDays(-10);
			var expectedExpiryDate = expectedIssueDate.AddDays(90);

			var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Invalid, expectedIssueDate: ZDateTime.Empty, expectedExpiryDate: ZDateTime.Empty);
			credential.GP_IssueDate = expectedIssueDate;
			credential.GP_ExpiryDate = expectedExpiryDate;
			credential.GP_StatusReason = "Reason";
			Factory.Save();

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(Env.CurrentCompany);

			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Invalid, expectedIssueDate, expectedExpiryDate);
		}

		[TestDate(2024, 5, 11)]
		public virtual void TestCreateOrUpdateEInvoicingCredential_StatusReason()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var expectedIssueDate = ZDateTime.Today.AddDays(-10);
			var expectedExpiryDate = expectedIssueDate.AddDays(90);

			var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Invalid, expectedIssueDate: ZDateTime.Empty, expectedExpiryDate: ZDateTime.Empty);
			credential.GP_IssueDate = expectedIssueDate;
			credential.GP_ExpiryDate = expectedExpiryDate;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(Env.CurrentCompany);

			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Valid, expectedIssueDate, expectedExpiryDate);
			AssertEquals(OAuthHelper.CredentialStatusReasonRefreshingToken, credential.GP_StatusReason);

			EInvoiceCredentialsProvider.CreateOrUpdateEInvoicingCredential(Env.CurrentCompany);

			AssertGlbCompanyEInvoicingCertificateCredential(credential, PasswordStatusList.Codes.Valid, expectedIssueDate, expectedExpiryDate);
			AssertEquals("Status reason should be unchanged when authorize multiple times", OAuthHelper.CredentialStatusReasonRefreshingToken, credential.GP_StatusReason);
		}

		public virtual void TestCreateOAuthData_Loading()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCode;
			Factory.Save();

			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var credential1 = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential1.GP_PasswordStatus = PasswordStatusList.Codes.Pending;
			Factory.Save();

			var eInvoiceOAuthData = EInvoiceCredentialsProvider.CreateOAuthData(company);
			AssertEquals(PasswordStatusList.Descriptions.Pending, eInvoiceOAuthData.OAuthCredentials[0].Status);
			AssertEquals(company.PK, eInvoiceOAuthData.Company.PK);

			var credential2 = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.INT);
			credential2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var credential3 = Factory.NewWithValidTestData<GlbCompanyEInvoicingCertificateCredential>();
			credential3.GP_GC = company.PK;
			credential3.GP_PasswordType = PasswordTypesList.Codes.EIM;
			credential3.GP_PasswordStatus = PasswordStatusList.Codes.Pending;
			Factory.Save();

			CombineAssertions("Should only load EIM type for OAuth.", () =>
			{
				eInvoiceOAuthData = EInvoiceCredentialsProvider.CreateOAuthData(company);
				AssertEquals(2, eInvoiceOAuthData.OAuthCredentials.Count);
				AssertEquals(PasswordStatusList.Descriptions.Pending, eInvoiceOAuthData.OAuthCredentials[0].Status);
				AssertEquals(PasswordStatusList.Descriptions.Pending, eInvoiceOAuthData.OAuthCredentials[1].Status);
			});
		}

		public virtual void TestCreateOAuthData_Reload()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCode;
			Factory.Save();

			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Pending;

			Factory.Save();

			AssertEquals("Prerequisite", PasswordStatusList.Descriptions.Pending, EInvoiceCredentialsProvider.CreateOAuthData(company).OAuthCredentials[0].Status);

			var newFactory = new BusinessObjectFactory();
			var credentialInNewFactory = newFactory.Load<GlbCompanyEInvoicingCertificateCredential>(credential.PK);
			credentialInNewFactory.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			newFactory.Save();

			AssertEquals("Prerequisite", PasswordStatusList.Descriptions.Valid, EInvoiceCredentialsProvider.CreateOAuthData(company).OAuthCredentials[0].Status);
		}

		public virtual void TestCreateOAuthData_LoadedDatesAreInLocal()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCode;
			Factory.Save();

			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			credential.GP_SystemCreateTimeUtc = new ZDateTime(2024, 1, 1, 20, 22, 0);
			credential.GP_IssueDate = new ZDateTime(2024, 1, 1, 21, 22, 0);
			credential.GP_ExpiryDate = new ZDateTime(2024, 1, 1, 22, 22, 0);
			Factory.Save();

			var oAuthCredential = EInvoiceCredentialsProvider.CreateOAuthData(company).OAuthCredentials[0];

			AssertEquals(new ZDateTime(2024, 1, 2, 6, 22, 0), oAuthCredential.AuthorizationDate);
			AssertEquals(new ZDateTime(2024, 1, 2, 7, 22, 0), oAuthCredential.IssueDate);
			AssertEquals(new ZDateTime(2024, 1, 2, 8, 22, 0), oAuthCredential.ExpiryDate);
		}

		[TestDate(2024, 10, 1, 20, 0, 0)]
		public virtual void TestCreateOAuthData_ExpiredStatus()
		{
			if (EInvoiceCredentialsProvider == null)
			{
				Assert("Test is not applicable", condition: true);
				return;
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCode;
			Factory.Save();

			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyEInvoicingCertificateCredential>(PasswordTypesList.Codes.EIM);
			Factory.Save();

			var oAuthCredential = EInvoiceCredentialsProvider.CreateOAuthData(company).OAuthCredentials[0];
			Assert("Prerequisite", oAuthCredential.ExpiryDate.IsEmpty);
			AssertEquals(PasswordStatusList.Descriptions.Invalid, oAuthCredential.Status);

			AssertEquals("Prerequisite", new ZDateTime(2024, 10, 1, 20, 0, 0), ZDateTime.UtcNow);
			credential.GP_ExpiryDate = new ZDateTime(2024, 10, 1, 11, 0, 0);
			Factory.Save();

			oAuthCredential = EInvoiceCredentialsProvider.CreateOAuthData(company).OAuthCredentials[0];
			AssertEquals("Prerequisite", new ZDateTime(2024, 10, 1, 21, 0, 0), oAuthCredential.ExpiryDate);
			AssertEquals("Expire status should be judged by UTC time.", PasswordStatusList.Descriptions.Expired, oAuthCredential.Status);
		}

		void AssertGlbCompanyEInvoicingCertificateCredential(GlbCompanyEInvoicingCertificateCredential credential, string expectedPasswordStatus, ZDateTime expectedIssueDate, ZDateTime expectedExpiryDate)
		{
			AssertNullOrEmpty(credential.GP_MailBoxID);
			AssertNullOrEmpty(credential.GP_Name);
			AssertNullOrEmpty(credential.GP_UserID);

			AssertEquals(PasswordTypesList.Codes.EIM, credential.GP_PasswordType);
			AssertEquals(expectedPasswordStatus, credential.GP_PasswordStatus);
			AssertNullOrEmpty(credential.GP_CurrentPassword);
			AssertNullOrEmpty(credential.GP_NextPassword);

			AssertEquals(ZBlob.Empty, credential.GP_Certificate);
			AssertNullOrEmpty(credential.GP_CertificatePassPhrase);
			AssertNullOrEmpty(credential.GP_CertificateAuthority);
			AssertNullOrEmpty(credential.GP_CertificateSerialNumber);

			AssertEquals(GlbCompany.CurrentCompany.PK, credential.GP_GC);
			AssertEquals(ZGuid.Empty, credential.GP_GS);
			AssertEquals(ZGuid.Empty, credential.GP_GG);
			AssertEquals(ZGuid.Empty, credential.GP_GB);
			AssertEquals(expectedIssueDate, credential.GP_IssueDate);
			AssertEquals(expectedExpiryDate, credential.GP_ExpiryDate);
		}

		#endregion

		#region IOriginalInvoiceNumberAndDateValidationDecider

		protected virtual string[] OriginalInvoiceNumberAndDateValidationSubTypes => [string.Empty];

		protected virtual bool ExpectedShouldValidateOriginalTransactionNumberAndDate => false;

		public virtual void TestIOriginalInvoiceNumberAndDateValidationDecider()
		{
			var decider = TestObject as IOriginalInvoiceNumberAndDateValidationDecider;
			foreach (var complianceSubType in OriginalInvoiceNumberAndDateValidationSubTypes)
			{
				AssertEquals(ExpectedShouldValidateOriginalTransactionNumberAndDate, decider?.ShouldValidateOriginalTransactionNumberAndDate(complianceSubType) ?? false);
			}
		}

		#endregion

		public virtual void TestGetRecipientLocalBusinessReg2NumberCodeType()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(null, complianceInfo.GetRecipientLocalBusinessReg2NumberCodeType());
		}

		public virtual void TestGetRecipientLocalBusinessReg2Heading()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(null, complianceInfo.GetRecipientLocalBusinessReg2Heading());
		}

		public void TestIsComplianceSubTypeEligibleForEInvoicing()
		{
			if (!typeof(IComplianceInfoElectronicInvoicingEligibleSubType).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}

			var complianceSubTypeListForEInvoicing = CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode).GetEligibleComplianceSubTypeListForEInvoicing();

			foreach (var complianceSubType in complianceSubTypeListForEInvoicing)
			{
				Assert("ComplianceSubType is elegible for EInvoicing", CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode).IsComplianceSubTypeElegibleForEInvoicing(complianceSubType));
			}

			AssertEquals(false, (CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode).IsComplianceSubTypeElegibleForEInvoicing("ABC")));
		}

		public virtual void TestGetBusinessRegistrationCode()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedBusinessRegistrationCode, complianceInfo.GetBusinessRegistrationCode());
		}

		public virtual void TestGetComplianceSequencePrefixErrorMessage()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedComplianceSequencePrefixErrorMessage, complianceInfo.GetComplianceSequencePrefixErrorMessage());
		}

		public virtual void TestGetComplianceSequencePrefixRegexy()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedComplianceSequencePrefixRegex, complianceInfo.GetComplianceSequencePrefixRegex());
		}

		public virtual void TestGetRecipientLocalBusinessRegNumberCodeType()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedRecipientLocalBusinessRegNumberCodeType, complianceInfo.GetRecipientLocalBusinessRegNumberCodeType());
		}

		public virtual void TestGetRecipientLocalBusinessRegHeading()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedRecipientLocalBusinessRegHeading, complianceInfo.GetRecipientLocalBusinessRegHeading());
		}

		public virtual void TestGetRecipientTaxIDHeading()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedRecipientTaxIDHeading, complianceInfo.GetRecipientTaxIDHeading());
		}

		public virtual void TestGetDefaultValueForDisplayRecipientTaxIDRegistry()
		{
			var complianceInfo = CreateCurrentCountryComplianceInfo() as ICountryComplianceInfo;
			AssertEquals(ExpectedDefaultValueForDisplayRecipientTaxIDRegistry, complianceInfo.GetDefaultValueForDisplayRecipientTaxIDRegistry());
		}

		public virtual void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(null, complianceInfo.HasExtraTaxInfo());
		}

		public virtual void TestGetExtraTax()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals((complianceInfo.HasExtraTaxInfo().HasValue && complianceInfo.HasExtraTaxInfo().Value) ? "QST" : string.Empty, complianceInfo.GetExtraTaxDescription("QCT"));
		}

		public virtual void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(ResourceStringData.Empty, complianceInfo.GetExtraTaxOSAmountCaption());
		}

		public virtual void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(ResourceStringData.Empty, complianceInfo.GetExtraTaxLocalAmountCaption());
		}

		public virtual void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(ResourceStringData.Empty, complianceInfo.GetTaxOSAmountCaption());
		}

		public virtual void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;

			AssertEquals(ResourceStringData.Empty, complianceInfo.GetTaxLocalAmountCaption());
		}

		#region Compliance Date

		public void TestGetEInvoicingComplianceDate()
		{
			if (ComplianceDateDependOfIsProductionSystem)
			{
				var oldLicenceType = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType;

				foreach (string licenceType in new[] { DatabaseTypes.Codes.Production, DatabaseTypes.Codes.Test })
				{
					IsProductionSystem = licenceType == DatabaseTypes.Codes.Production;
					using (SetTemporaryLicenceType(licenceType))
					{
						AssertGetEInvoicingComplianceDate();
					}
				}

				IDisposable SetTemporaryLicenceType(string licenceType)
				{
					return new DisposableAction(() => LicenceTypeChanger.SetSystemLicence(licenceType), () => LicenceTypeChanger.SetSystemLicence(oldLicenceType));
				}
			}
			else
			{
				AssertGetEInvoicingComplianceDate();
			}

			void AssertGetEInvoicingComplianceDate()
			{
				var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(CountryCode);
				if (complianceInfo != null)
				{
					AssertEquals("EInvoicingComplianceDate", ExpectedEInvoicingComplianceDate, complianceInfo.GetEInvoicingComplianceDate());
					AssertEquals("EInvoicingComplianceDate for Payables", ExpectedEInvoicingComplianceDateForPayables, complianceInfo.GetEInvoicingComplianceDateForPayables());
				}
				else
				{
					Assert(true);
				}
			}
		}

		protected bool IsProductionSystem { get; set; }
		protected virtual bool ComplianceDateDependOfIsProductionSystem => false;
		protected virtual ZDate ExpectedEInvoicingComplianceDate => ZDate.Empty;
		protected virtual ZDate ExpectedEInvoicingComplianceDateForPayables => ZDate.Empty;

		#endregion

		public void TestGetComplianceVersionNo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("Compliance Version No", ExpectedComplianceVersionNo, complianceInfo.GetComplianceVersionNo());
		}

		protected virtual string ExpectedComplianceVersionNo => string.Empty;

		public void TestGetDefaultEInvoicingSubmitPivotStatus()
		{
			AssertGetDefaultEInvoicingSubmitPivotStatus();
		}

		protected virtual void AssertGetDefaultEInvoicingSubmitPivotStatus()
		{
			var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(CountryCode);
			if (complianceInfo != null)
			{
				AssertEquals("DefaultEInvoicingSubmitPivotStatus", ExpectedDefaultEInvoicingSubmitPivotStatus, complianceInfo.GetDefaultEInvoicingSubmitPivotStatus());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual ZString ExpectedDefaultEInvoicingSubmitPivotStatus => Core.Constants.EInvoicingPivotState.Queued;

		public void TestGetDefaultEInvoicingSubmitPivotStatusForPayables()
		{
			AssertGetDefaultEInvoicingSubmitPivotStatusForPayables();
		}

		protected virtual void AssertGetDefaultEInvoicingSubmitPivotStatusForPayables()
		{
			var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(CountryCode);
			if (complianceInfo != null)
			{
				AssertEquals("ExpectedDefaultEInvoicingSubmitPivotStatusForPayables", ExpectedDefaultEInvoicingSubmitPivotStatusForPayables, complianceInfo.GetDefaultEInvoicingSubmitPivotStatusForPayables());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual string ExpectedDefaultEInvoicingSubmitPivotStatusForPayables => string.Empty;

		public void TestGetDefaultEInvoicingPivotPendingStatusDescription()
		{
			AssertGetDefaultEInvoicingPivotPendingStatusDescription();
		}

		protected virtual void AssertGetDefaultEInvoicingPivotPendingStatusDescription()
		{
			var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(CountryCode);
			if (complianceInfo != null)
			{
				AssertEquals("DefaultEInvoicingPivotPendingStatusDescription", ExpectedDefaultEInvoicingPivotPendingStatusDescription, complianceInfo.GetDefaultEInvoicingPivotPendingStatusDescription());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual ZString ExpectedDefaultEInvoicingPivotPendingStatusDescription => "Pending User Action";

		public void TestTaxMessageGroupCodes()
		{
			var taxMessageGroupProvider = CountryComplianceFactory.GetITaxMessageGroupProvider(CountryCode);
			if (taxMessageGroupProvider != null)
			{
				var actualTaxMessageGroups = taxMessageGroupProvider.GetTaxMessageGroup().OfType<CodeDescriptionBoolRelatedItem>();

				CombineAssertions(() =>
				{
					foreach (CodeDescriptionBoolRelatedItem taxMessageGroup in actualTaxMessageGroups)
					{
						Assert(GetMessage(taxMessageGroup, "Not Expected"), ExpectedTaxMessageGroups.Any(etmg => etmg.Code == taxMessageGroup.Code && etmg.Description.ToString() == taxMessageGroup.Description.ToString() && etmg.Bool == taxMessageGroup.Bool && etmg.RelatedItemCode == taxMessageGroup.RelatedItemCode));
					}
					foreach (CodeDescriptionBoolRelatedItem etmg in ExpectedTaxMessageGroups)
					{
						Assert(GetMessage(etmg, "Expected"), actualTaxMessageGroups.Any(taxMessageGroup => etmg.Code == taxMessageGroup.Code && etmg.Description.ToString() == taxMessageGroup.Description.ToString() && etmg.Bool == taxMessageGroup.Bool && etmg.RelatedItemCode == taxMessageGroup.RelatedItemCode));
					}
				});
			}
			else
			{
				Assert(true);
			}

			string GetMessage(CodeDescriptionBoolRelatedItem item, string prefix)
			{
				return FormattableString.Invariant($"{prefix} - code: {item.Code}, description: {item.Description}, IsActive: {item.Bool.ToYesNoString()}, RelatedGroupCode: {item.RelatedItemCode}");
			}
		}

		protected virtual IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups => Enumerable.Empty<CodeDescriptionBoolRelatedItem>();

		protected virtual string ExpectedGovernmentAllocatedNumberColumnName => AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber;

		public void TestGetGovernmentAllocatedNumberColumnName()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoElectronicInvoicing;
			if (complianceInfo != null)
			{
				AssertEquals($"Government Allocated Number should be read from {ExpectedGovernmentAllocatedNumberColumnName}", ExpectedGovernmentAllocatedNumberColumnName, complianceInfo.GetGovernmentAllocatedNumberColumnName());
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		protected virtual string ExpectedAccTransactionHeaderAuthorisationRecordType => string.Empty;

		public void TestGetAccTransactionHeaderAuthorisationRecordType()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoElectronicInvoicing;
			if (complianceInfo != null)
			{
				AssertEquals($"AFH_RecordType should be {ExpectedAccTransactionHeaderAuthorisationRecordType}", ExpectedAccTransactionHeaderAuthorisationRecordType, complianceInfo.GetAccTransactionHeaderAuthorisationRecordType());
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestEInvoicingCountriesImplementCorrectInterface()
		{
			foreach (ZString countryCode in ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().SupportedCountryCodes)
			{
				var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(countryCode) as IComplianceInfoElectronicInvoicing;
				AssertNotNull("ComplianceInfo for country with CountryCode " + countryCode + " must implement IComplianceInfoElectronicInvoicing", complianceInfo);
			}
		}

		public void TestPortugalARTransactionSequencing()
		{
			var ledger = LedgerTypes.AccountsReceivable;
			var complianceInfo = CountryComplianceFactory.GetICountryComplianceInfo(CountryCode);

			if (CountryCode == Core.Constants.CountryCodes.Portugal)
			{
				AssertEquals("PortugalComplianceInfo should return true for AR Transaction sequencing", true, complianceInfo.GetIsTransactionSequencingRequired(ledger, TransactionTypes.Invoice));
				AssertEquals("PortugalComplianceInfo should return true for AR Transaction sequencing", true, complianceInfo.GetIsTransactionSequencingRequired(ledger, TransactionTypes.CreditNote));
				AssertEquals("PortugalComplianceInfo should return true for AR Transaction sequencing", true, complianceInfo.GetIsTransactionSequencingRequired(ledger, TransactionTypes.Receipt));
				AssertEquals("PortugalComplianceInfo should return false for AR Invoice Batch Transaction", false, complianceInfo.GetIsTransactionSequencingRequired(ledger, TransactionTypes.InvoiceBatch));
			}
			else
			{
				AssertEquals("PortugalComplianceInfo should return false for AR Transaction sequencing for other countries", false, complianceInfo.GetIsTransactionSequencingRequired(ledger, TransactionTypes.Invoice));
			}
		}

		public void TestIOriginalInvoiceReferenceShouldShowOriginalInvoiceReferenceFields()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertShouldShowOriginalInvoiceReferenceFields(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertShouldShowOriginalInvoiceReferenceFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestIOriginalInvoiceReferenceShouldShowOriginalInvoiceReferenceDatesFields()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertShouldShowOriginalInvoiceReferenceDatesFields(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertShouldShowOriginalInvoiceReferenceDatesFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestIOriginalInvoiceReferenceShouldShowOriginalInvoiceReferenceReasonFields()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertShouldShowOriginalInvoiceReferenceReasonFields(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertShouldShowOriginalInvoiceReferenceReasonFields(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestIOriginalInvoiceReferenceGetAreAllOriginalInvoiceReferenceFieldsEnabled()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertGetAreAllOriginalInvoiceReferenceFieldsEnabled(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertGetAreAllOriginalInvoiceReferenceFieldsEnabled(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestIOriginalInvoiceReferenceGetAreOriginalTransactionReferenceFieldsMandatory()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertGetAreOriginalTransactionReferenceFieldsMandatory(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertGetAreOriginalTransactionReferenceFieldsMandatory(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestIOriginalInvoiceReferenceGetDocOriginalReferenceReason()
		{
			var originalInvoiceReference = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IOriginalInvoiceReference;
			if (originalInvoiceReference != null)
			{
				AssertGetDocOriginalReferenceReason(originalInvoiceReference);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertGetDocOriginalReferenceReason(IOriginalInvoiceReference originalInvoiceReference)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOriginalInvoiceReference", false);
		}

		public void TestITransactionAuthorizationNumber_IsTransactionAuthorizationNumberEnabled()
		{
			var transactionAuthorizationNumber = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ITransactionAuthorizationNumber;
			if (transactionAuthorizationNumber != null)
			{
				AssertIsTransactionAuthorizationNumberEnabled(transactionAuthorizationNumber);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertIsTransactionAuthorizationNumberEnabled(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements ITransactionAuthorizationNumber", false);
		}

		public void TestITransactionAuthorizationNumber_GetTransactionAuthorizationNumberLabel()
		{
			var transactionAuthorizationNumber = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ITransactionAuthorizationNumber;
			if (transactionAuthorizationNumber != null)
			{
				AssertGetTransactionAuthorizationNumberLabel(transactionAuthorizationNumber);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertGetTransactionAuthorizationNumberLabel(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements ITransactionAuthorizationNumber", false);
		}

		public void TestITransactionAuthorizationNumber_GetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate()
		{
			var transactionAuthorizationNumber = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ITransactionAuthorizationNumber;
			if (transactionAuthorizationNumber != null)
			{
				AssertGetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate(transactionAuthorizationNumber);
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public virtual void AssertGetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate(ITransactionAuthorizationNumber transactionAuthorizationNumber)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements ITransactionAuthorizationNumber", false);
		}

		public void TestGetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			AssertEquals("EInvoicingEnabled", ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled, defaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(true));
			AssertEquals("EInvoicingDisabled", ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled, defaultProvider.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(false));
		}

		protected virtual string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled => null;
		protected virtual string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled => null;

		public void TestGetDefaultValueForComplianceNumberAllocationDateRegistry()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			AssertEquals(ExpectedDefaultValueForComplianceNumberAllocationDateRegistry, defaultProvider.GetDefaultValueForComplianceNumberAllocationDateRegistry());
		}

		protected virtual string ExpectedDefaultValueForComplianceNumberAllocationDateRegistry => AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code;

		public void TestValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			foreach (var kvp in GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry())
			{
				var expected = kvp.Value;

				var actualResult = defaultProvider.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(kvp.Key.code, kvp.Key.isEInvoicingEnabled);

				AssertEquals($"({kvp.Key}) should be '{kvp.Value}'", expected, actualResult);
			}
		}

		public void TestValidateComplianceDocumentNumberAllocation_ReceivablesRegistry_CoversAllPossibleInputs()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			var allPossibleInputs = new HashSet<(string, bool)>();
			foreach (var code in AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.GetAllCodes())
			{
				allPossibleInputs.Add((code, true));
				allPossibleInputs.Add((code, false));
			}

			var missingInputs = allPossibleInputs.Except(
									GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry().Keys
								);

			AssertContainsExactElementsInAnyOrder("Some possible inputs to ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposed, isEInvoicingEnabled) were not covered", Enumerable.Empty<(string, bool)>(), missingInputs);
		}

		protected virtual IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string, bool), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true),  null },
			};

		public void TestValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			foreach (var kvp in GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry())
			{
				var expected = kvp.Value;

				var actualResult = defaultProvider.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(kvp.Key.code, kvp.Key.isEInvoicingEnabled);

				AssertEquals($"({kvp.Key}) should be '{kvp.Value}'", expected, actualResult);
			}
		}

		public void TestValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry_CoversAllPossibleInputs()
		{
			var defaultProvider = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceRegistryDefaultProvider;
			if (defaultProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}

			var allPossibleInputs = new HashSet<(string, bool)>();
			foreach (var code in AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.GetAllCodes())
			{
				allPossibleInputs.Add((code, true));
				allPossibleInputs.Add((code, false));
			}

			var missingInputs = allPossibleInputs.Except(
									GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry().Keys
								);

			AssertContainsExactElementsInAnyOrder("Some possible inputs to ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(proposed, isEInvoicingEnabled) were not covered", Enumerable.Empty<(string, bool)>(), missingInputs);
		}

		protected virtual IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
			=> new Dictionary<(string, bool), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true),  null },
			};

		public void TestOrgCusCodePredicateProvider_PlaceOfSupplyTaxRegistration()
		{
			var orgCusCodePredicateProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIOrgCusCodePredicateProvider(CountryCode);
			if (orgCusCodePredicateProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}
			else
			{
				AssertOrgCusCodePredicateProvider_PlaceOfSupplyTaxRegistration(orgCusCodePredicateProvider);
			}
		}

		public virtual void AssertOrgCusCodePredicateProvider_PlaceOfSupplyTaxRegistration(IOrgCusCodePredicateProvider orgCusCodePredicateProvider)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOrgCusCodePredicateProvider", false);
		}

		public void TestOrgCusCodePredicateProvider_OrgHeaderTaxRegistration()
		{
			var orgCusCodePredicateProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIOrgCusCodePredicateProvider(CountryCode);
			if (orgCusCodePredicateProvider == null)
			{
				Assert("Test not applicable", true);
				return;
			}
			else
			{
				AssertOrgCusCodePredicateProvider_OrgHeaderTaxRegistration(orgCusCodePredicateProvider);
			}
		}

		public virtual void AssertOrgCusCodePredicateProvider_OrgHeaderTaxRegistration(IOrgCusCodePredicateProvider orgCusCodePredicateProvider)
		{
			Assert("This method should be overridden in the respective country test when country compliance info implements IOrgCusCodePredicateProvider", false);
		}

		protected virtual IReadOnlyDictionary<string, string> ExpectedComplianceDocumentNumberAllocationOverride => null;

		public void TestComplianceDocumentNumberAllocationOverride_EnableEInvoicingFunctionality_IsTrue()
		{
			if (ExpectedComplianceDocumentNumberAllocationOverride?.Count > 0)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				{
					using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
					{
						var collection = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
						AssertEquals(ExpectedComplianceDocumentNumberAllocationOverride.Count, collection.Count);

						for (int i = 0; i < ExpectedComplianceDocumentNumberAllocationOverride.Count; i++)
						{
							var compliance = ExpectedComplianceDocumentNumberAllocationOverride.ElementAt(i);
							AssertComplianceSubTypeAllocationOverrideConfigurationCollection(collection[i], CountryCode, compliance.Key, compliance.Value);
						}
					}
				}
			}
			else
			{
				Assert(true);
			}

			void AssertComplianceSubTypeAllocationOverrideConfigurationCollection(ComplianceSubTypeAllocationOverrideConfiguration item, string expectedCountry, string expectedSubType, string expecteAllocationMethod)
			{
				AssertEquals("Country", expectedCountry, item.Country);
				AssertEquals("Sub Type", expectedSubType, item.SubType);
				AssertEquals("Allocation Method", expecteAllocationMethod, item.AllocationMethod);
			}
		}

		public void TestComplianceDocumentNumberAllocationOverride_EnableEInvoicingFunctionality_IsFalse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					var collection = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					AssertEquals(0, collection.Count);
				}
			}
		}

		#region IComplianceInfoEInvoicingGUIActionProvider

		public virtual void TestIsCountryEnableComplianceEInvoicing()
		{
			var ruleSetProvider = TestObject as IComplianceInfoEInvoicingGUIActionProvider;

			if (ruleSetProvider == null)
			{
				Assert("Test not applicable", true);
			}
			else
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					Assert(ruleSetProvider.IsCountryEnableComplianceEInvoicing());
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));

					using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
						Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					Assert(EnableEInvoicingForAPTransaction() ? ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true) : !ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());

					using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
						Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
					}
				}
			}
		}

		protected virtual bool EnableEInvoicingForAPTransaction() => false;

		#endregion

		protected object TestObject => testObject ??= Activator.CreateInstance(TestsInstancesOfType);
		object testObject;

		#region IEInvoicingRegistryProvider

		public void TestIEInvoicingRegistryProvider_ShouldAutoSetEReportingComplianceDate()
		{
			var countryComplianceInfo = CreateCurrentCountryComplianceInfo() as IEInvoicingRegistryProvider;
			AssertEquals(ExpectedShouldAutoSetEReportingComplianceDateValue, countryComplianceInfo?.ShouldAutoSetEReportingComplianceDate ?? false);
		}

		protected virtual bool ExpectedShouldAutoSetEReportingComplianceDateValue => false;

		#endregion

		CountryComplianceInfo CreateCurrentCountryComplianceInfo()
		{
			return CountryComplianceFactory.GetCountryComplianceInfo(CountryCode);
		}
	}
}
