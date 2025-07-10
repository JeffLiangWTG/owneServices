using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccJobConfigPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExRateTypeListDepenedsOnLevel()
		{
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var itemsCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			exRateConfig.JCE_ParentTableCode = "";
			exRateConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			exRateConfig.JCE_GC = GlbCompany.CurrentCompany.PK;
			var currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			AssertEquals("For Company level config, should contain items from system AND company registry", 203, currencyConfigLookup.ExRateTypeList.Count);

			exRateConfig.JCE_GC = ZGuid.Empty;
			currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			AssertEquals("For System level config, should only contain items from system registry", 104, currencyConfigLookup.ExRateTypeList.Count);

			exRateConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			AssertEquals("For OrgHeader level config, should contain items from system AND company registry", 203, currencyConfigLookup.ExRateTypeList.Count);

			exRateConfig.JCE_ParentTableCode = OrgDebtorGroupSchema.Constants.Prefix;
			currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			AssertEquals("For OrgDebtor level config, should contain items from system AND company registry", 203, currencyConfigLookup.ExRateTypeList.Count);

			exRateConfig.JCE_ParentTableCode = OrgCreditorGroupSchema.Constants.Prefix;
			currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			AssertEquals("For OrgCreditor level config, should contain items from system AND company registry", 203, currencyConfigLookup.ExRateTypeList.Count);
		}

		public void TestExRateTypeList()
		{
			Assert("CustomsExRate is allowed by deafult", Env.Security.CustomsExchangeRateUpdate.IsAllowed);

			var currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);

			var expectExRateTypeList = new[]
			{
				(Constants.ExchangeRateTypes.Code.BuyRate, Constants.ExchangeRateTypes.Description.BuyRate.ToString()),
				(Constants.ExchangeRateTypes.Code.SellRate, Constants.ExchangeRateTypes.Description.SellRate.ToString()),
				(Constants.ExchangeRateTypes.Code.CustomsRate, Constants.ExchangeRateTypes.Description.CustomsRate.ToString()),
				(Constants.ExchangeRateTypes.Code.PeriodEndRate, Constants.ExchangeRateTypes.Description.PeriodEndRate.ToString()),
				(Constants.ExchangeRateTypes.Code.IATARate, Constants.ExchangeRateTypes.Description.IATARate.ToString()),
			};

			var exRateTypeAcutalList = currencyConfigLookup.ExRateTypeList.Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();

			foreach (var jobType in allJobTypes)
			{
				exRateConfig.JCE_JobType = jobType;

				AssertArrayEqualsByElements(expectExRateTypeList, exRateTypeAcutalList);
			}

			try
			{
				Env.Security.CustomsExchangeRateUpdate.IsAllowed = false;

				foreach (var jobType in allJobTypes)
				{
					exRateConfig.JCE_JobType = jobType;

					AssertArrayEqualsByElements(expectExRateTypeList, exRateTypeAcutalList);
				}
			}
			finally
			{
				Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			}
		}

		public void TestExRateList_WithCustomRate()
		{
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var itemsCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			exRateConfig.JCE_GC = GlbCompany.CurrentCompany.PK;
			var currencyConfigLookup = new AccJobConfigPivotLookups(currencyConfig);
			var exRateTypeAcualList = currencyConfigLookup.ExRateTypeList.ToArray().Select(p => p.Code);

			AssertEquals("Valid ex rate code should be in ex rate list", true, exRateTypeAcualList.Contains("C01"));
			AssertEquals("Valid ex rate code should be in ex rate list", true, exRateTypeAcualList.Contains("C99"));
			AssertEquals("Valid ex rate code should be in ex rate list", true, exRateTypeAcualList.Contains("L01"));
			AssertEquals("Valid ex rate code should be in ex rate list", true, exRateTypeAcualList.Contains("L99"));

			AssertExRateListNotContainsCodes("Z01", "WZZ", "C00", "L00", "CZZ", "LWW");

			void AssertExRateListNotContainsCodes(params string[] codes)
			{
				codes.ForEach(code => itemsSystemLevel.Add(code));
				AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);
				var exRateTypeAcualList = new AccJobConfigPivotLookups(currencyConfig).ExRateTypeList.ToArray().Select(p => p.Code);
				AssertEquals("Invalid ex rate codes should not contain in ex rate list.", 0, exRateTypeAcualList.Intersect(codes).Count());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			exRateConfig = Factory.New<AccExchangeRateConfiguration>();
			currencyConfig = Factory.New<ExchangeRateCurrencyConfiguration>();
			currencyConfig.JCT_JCF_JobConfig = exRateConfig.PK;
			allJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList().Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
		}
		AccExchangeRateConfiguration exRateConfig;
		ExchangeRateCurrencyConfiguration currencyConfig;
		string[] allJobTypes;
	}
}
