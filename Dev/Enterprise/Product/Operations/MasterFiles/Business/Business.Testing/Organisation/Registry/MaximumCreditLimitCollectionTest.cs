using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumCreditLimitCollection))]
	sealed class MaximumCreditLimitCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<MaximumCreditLimitCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override MaximumCreditLimitCollection GetCollectionToTest()
		{
			return new MaximumCreditLimitCollection(new FallbackLevel(Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MaximumCreditLimitItem();
		}

		public void TestGetClone_WhenFallbackLevelCompany_FilterCurrencyBelongingToTheCompany()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_ARCreditLimit = new ZDecimal(2000001);
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_RX_NKLocalCurrency = "AUD";
			companyData1.OB_OH = header.PK;
			companyData1.OB_GC = glbCompany1.PK;

			Factory.Save();

			var creditLimitItem1 = Collection.AddNew();
			creditLimitItem1.CurrencyPK = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD").PK;
			creditLimitItem1.CreditLimit = "200";

			var creditLimitItem2 = Collection.AddNew();
			creditLimitItem2.CurrencyPK = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD").PK;
			creditLimitItem2.CreditLimit = "180";

			var companyLevelCreditLimitCollection = Collection.Clone(new FallbackLevel(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory) as MaximumCreditLimitCollection;
			AssertEquals("Cloning at CompanyLevel Should only consider Currencies that are Local", 1, companyLevelCreditLimitCollection.Count);
			AssertEquals("Cloned creditLimitItems must have CurrencyPk as the LocalCurrencyPk of Company", glbCompany1.LocalCurrency.PK, companyLevelCreditLimitCollection[0].CurrencyPK);
		}
	}
}
