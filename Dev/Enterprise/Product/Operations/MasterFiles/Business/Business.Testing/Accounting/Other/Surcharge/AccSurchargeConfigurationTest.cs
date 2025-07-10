using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccSurchargeConfiguration))]
	sealed class AccSurchargeConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDeleteAndReasonForNotAbleToDelete()
		{
			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			var surchargeApplication = Factory.NewWithValidTestData<AccSurchargeApplication>();
			surchargeApplication.ASP_ASC_NKSurchargeCode = surchargeConfiguration.ASC_Code;
			surchargeApplication.ASP_GC_Company = surchargeConfiguration.ASC_GC_Company;

			AssertEquals("surchargeConfiguration cannot be deleted.", false, surchargeConfiguration.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete is 'This code is reference by a Surcharge Application rule, it cannot be deleted.'", "This code is reference by a Surcharge Application rule, it cannot be deleted.", surchargeConfiguration.ReasonForNotAbleToDelete);

			surchargeApplication.ASP_ASC_NKSurchargeCode = string.Empty;

			AssertEquals("surchargeConfiguration can be deleted.", true, surchargeConfiguration.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete is empty", string.Empty, surchargeConfiguration.ReasonForNotAbleToDelete);
		}

		public void TestASC_Code_ReadOnly()
		{
			var surchargeConfig = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfig.ASC_GC_Company = GlbCompany.CurrentCompany.PK;
			surchargeConfig.ASC_Code = "TST";

			AssertEquals("ASC_Code is not readonly", false, surchargeConfig.ASC_CodeInfo.ReadOnly);

			Factory.Save();

			var surchargeApplication = Factory.NewWithValidTestData<AccSurchargeApplication>();
			surchargeApplication.ASP_ASC_NKSurchargeCode = "TST";
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;

			AssertEquals("ASC_Code is readonly after reference to application.", true, surchargeConfig.ASC_CodeInfo.ReadOnly);

			surchargeConfig.ASC_Code = string.Empty;
			surchargeApplication.ASP_ASC_NKSurchargeCode = string.Empty;

			AssertEquals("ASC_Code is not readonly when ASC_Code is empty.", false, surchargeConfig.ASC_CodeInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var chargeCode = factory.NewWithValidTestData<AccChargeCode>();

			var result = (AccSurchargeConfiguration)base.GetNewBusinessObjectForDeleteTest(factory);
			result.ASC_GC_Company = GlbCompany.CurrentCompany.PK;
			result.ASC_AC_ChargeCode = chargeCode.PK;
			result.ASC_Amount = 10;
			result.ASC_BasisType = "ALL";
			result.ASC_Code = "TS1";
			result.ASC_Description = "Test 1";
			result.ASC_Rate = 1m;
			result.ASC_Type = "PER";

			return result;
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			Assert(true);
		}
	}
}
