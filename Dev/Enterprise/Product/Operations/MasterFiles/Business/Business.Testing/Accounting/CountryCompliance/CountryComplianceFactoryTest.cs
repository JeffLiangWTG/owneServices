using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.CountryCompliance
{
	sealed class CountryComplianceFactoryTest : TestCaseWithFactory
	{
		public void TestNoCountriesReturnNull()
		{
			var allCountries = Constants.CountryCodes.GetAll().ToArray();

			foreach (var country in allCountries)
			{
				var compliance = CountryComplianceFactory.GetCountryComplianceInfo(country);
				AssertNotNull("Country compliance should never be null", compliance);
			}
		}

		public void TestCountryComplianceReturnsFallback()
		{
			var compliance = CountryComplianceFactory.GetCountryComplianceInfo("__");
			AssertNotNull("Country compliance should never be null", compliance);
			Assert("Country compliance should return a fallback when no country is matched", compliance is CountryComplianceFactory.FallbackCountryComplianceInfo);
		}

		public void TestGetICountryComplianceFactory()
		{
			AssertType<CountryComplianceFactory>(ObjectFactory.Get<ICountryComplianceFactory>());
		}

		public void TestGetICountryComplianceFactoryIsNotSingleton()
		{
			var countryComplianceFactory = ObjectFactory.Get<ICountryComplianceFactory>();
			var countryComplianceFactory2 = ObjectFactory.Get<ICountryComplianceFactory>();
			AssertNotEquals(countryComplianceFactory, countryComplianceFactory2);
		}

		public void TestPreventAddingNewCountryFeaturesToObsoleteCountryComplianceFactory()
		{
			var whiteListedFeaturesOfCountryComplianceFactory = new HashSet<Type>()
			{
				typeof(ICountryComplianceInfo),
				typeof(ICountryComplianceInfoBase),
				typeof(IComplianceRegistryDefaultProvider),
				typeof(IComplianceSubTypeCodeProvider),
				typeof(IComplianceSubTypeRulesWithMultipleRuleSetProvider),
				typeof(IComplianceSubTypeTaxRegistrationTypeRuleProvider),
				typeof(IComplianceInfoElectronicInvoicing),
				typeof(IComplianceInfoElectronicInvoicingEligibleSubType),
				typeof(ITaxMessagesGroupProvider),
				typeof(IBankAccountValidation),
				typeof(IEquivalentComplianceSubTypeProvider),
				typeof(IQRCodeDataProvider),
				typeof(IComplianceSubTypeRuleProvider),
				typeof(IComplianceSubTypeAndNumberUpdateRules),
				typeof(IComplianceDocumentStatusProvider),
				typeof(IComplianceInfoEInvoicingGUIActionDocumentRequest),
				typeof(IComplianceInfoEInvoicingGUIActionDocumentRequestAP),
				typeof(IComplianceInfoEInvoicingGUIActionStatusRequest),
				typeof(IComplianceInfoEInvoicingGUIActionStatusRequestAP),
				typeof(IComplianceInfoEInvoicingGUIActionProvider),
				typeof(IComplianceSubTypeValidation),
				typeof(IComplianceInfoEInvoicingGUIActionQueuePendingInvoice),
				typeof(IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider),
				typeof(IComplianceDocumentInfo),
				typeof(IEInvoicingRegistryProvider),
				typeof(IComplianceInfoEInvoicingGUIActionQueueReversedTransaction),
				typeof(IFiscalTaxCodeProvider),
				typeof(IOrgCusCodePredicateProvider),
				typeof(IComplianceSubTypeDependencyConfigurationProvider),
				typeof(IComplianceSubTypeTaxInvoiceRulePrecedenceProvider),
				typeof(IEInvoiceCredentialsProvider),
				typeof(IComplianceInfoEInvoicingRequeueHandler),
				typeof(IComplianceSubTypeGUIProvider),
				typeof(ITransactionAuthorisationRecordProvider),
				typeof(IComplianceNumberSequenceConfigurationProvider),
				typeof(IOriginalInvoiceReference),
				typeof(ITransactionAuthorizationNumber),
				typeof(IComplianceSequenceValidationProvider),
				typeof(IComplianceSubTypeTaxRegistrationTypePrecedenceProvider),
				typeof(IComplianceSubTypeRuleSortByTaxRegistrationLocation),
				typeof(IComplianceSubTypeRuleSortByParentTransactionSubType),
				typeof(IComplianceSubTypeTaxInvoiceRuleWithTaxIDAndZeroAmount),
				typeof(IOrgHeaderPostingValidation),
				typeof(IProtectComplianceSubTypeForEInvoicingTransactions),
				typeof(IEnableTransactionsPendingAllocationAllocateAsReceivable),
				typeof(IComplianceSubTypeAllocationOverrideConfigurationProvider),
				typeof(IOriginalInvoiceNumberAndDateValidationDecider)
			};

			var allCountries = Constants.CountryCodes.GetAll().ToArray();
			var actualFeatures = new HashSet<Type>();
			foreach (var country in allCountries)
			{
				var compliance = CountryComplianceFactory.GetCountryComplianceInfo(country);
				var inheritedInterfaces = compliance.GetType().GetInterfaces().ToList();
				foreach(var inheritedInterface in inheritedInterfaces)
				{
					if (inheritedInterface.IsGenericType && inheritedInterface.GetGenericTypeDefinition() == typeof(IInstanceProvider<>))
					{
						var parameterType = inheritedInterface.GenericTypeArguments.FirstOrDefault();
						if (parameterType != null)
						{
							actualFeatures.Add(parameterType);
						}
					}
					else
					{
						actualFeatures.Add(inheritedInterface);
					}
				}
			}

			var missingInActual = whiteListedFeaturesOfCountryComplianceFactory.Except(actualFeatures).ToList();
			var unexpectedInActual = actualFeatures.Except(whiteListedFeaturesOfCountryComplianceFactory).ToList();

			AssertEquals($"The following features from whiteListedFeaturesOfCountryComplianceFactory are not implemented anywhere in CountryComplianceFactory: {string.Join(", ", missingInActual.Select(type => type.Name))}", missingInActual.Count, 0);

			AssertEquals($"The following features implemented in CountryComplianceFactory are not from whiteListedFeaturesOfCountryComplianceFactory: {string.Join(", ", unexpectedInActual.Select(type => type.Name))}\nCountryComplianceFactory should not be used to implement new features. Please refer: https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FProduct%2FOperations%2FMasterFiles%2FBusiness%2FMasterFiles.Business%2FAccounting%2FCountryCompliance%2FCountryComplianceFactory.cs", unexpectedInActual.Count, 0);
		}
	}
}
