using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccInvMsgLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRates()
		{
			AccInvMsg message = Factory.NewWithValidTestData<AccInvMsg>();
			ZQuery query = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccTaxRate[] rates = Factory.Load<AccTaxRate>(query);
			message.Lookups.TaxRates.Load();
			AssertEquals(rates.Length, message.Lookups.TaxRates.Count);
		}

		public void TestActiveTaxGroupCodes()
		{
			// Populate Registry with some data
			var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
			testValue.Add("N1", (NoResString)"Description N1", true, "N1.0");
			testValue.Add("N2", (NoResString)"Description N2", false, "N2.0");
			testValue.Add("N3", (NoResString)"Description N3", false, "N3.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testValue);
			var numberOfActiveCodes = 1;

			var codes = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();

			AssertEquals("Number of active codes must be 1", numberOfActiveCodes, codes.Count);

			// Double check each code in the collection
			bool isActive = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetBoolFromCode("N1");
			AssertEquals("Code should be active", true, isActive);
			isActive = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetBoolFromCode("N2");
			AssertEquals("Code should be not active", false, isActive);
			isActive = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetBoolFromCode("N3");
			AssertEquals("Code should be not active", false, isActive);
		}
	}
}
