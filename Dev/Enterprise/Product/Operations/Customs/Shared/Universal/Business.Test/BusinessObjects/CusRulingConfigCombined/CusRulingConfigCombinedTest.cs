using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using static Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRulingConfigCombined))]
	sealed class CusRulingConfigCombinedTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return config;
		}

		public void TestReadOnly()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			var config = cusRuling.Configurations.AddNew();
			AssertEquals(false, config.ReadOnly);
			config.ReadOnly = true;
			AssertEquals(true, config.ReadOnly);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			config = helper.CreateRefCusRulingConfig(DTY, AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X, cusRuling.PK.ToString());
			AssertEquals(true, config.ReadOnly);
			config.ReadOnly = false;
			AssertEquals(true, config.ReadOnly);
		}

		public void TestClone()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			var config = cusRuling.Configurations.AddNew(DTY, Specific, 5, "CLT");
			var clonedConfig = (CusRulingConfigCombined)config.Clone();
			AssertEquals("clonedConfig.ZZY_ZZX_CusRuling", ZGuid.Empty, clonedConfig.ZZY_ZZX_CusRuling);
			AssertEquals("clonedConfig.ZZY_Category", DTY, clonedConfig.ZZY_Category);
			AssertEquals("clonedConfig.ZZY_Type", Specific, clonedConfig.ZZY_Type);
			AssertEquals("clonedConfig.ZZY_Rate", 5.000M, clonedConfig.ZZY_Rate);
			AssertEquals("clonedConfig.ZZY_Value", "CLT", clonedConfig.ZZY_Value);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			config = helper.CreateRefCusRulingConfig(GST, ExemptCode, 0, GSTStatusCodes.Codes.C48, cusRuling.PK.ToString());
			clonedConfig = (CusRulingConfigCombined)config.Clone();
			AssertEquals("clonedConfig.ZZY_ZZX_CusRuling", ZGuid.Empty, clonedConfig.ZZY_ZZX_CusRuling);
			AssertEquals("clonedConfig.ZZY_Category", GST, clonedConfig.ZZY_Category);
			AssertEquals("clonedConfig.ZZY_Type", ExemptCode, clonedConfig.ZZY_Type);
			AssertEquals("clonedConfig.ZZY_Rate", 0.000M, clonedConfig.ZZY_Rate);
			AssertEquals("clonedConfig.ZZY_Value", GSTStatusCodes.Codes.C48, clonedConfig.ZZY_Value);
		}

		public void TestICanDeleteMembers()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			var config = cusRuling.Configurations.AddNew();
			ICanDelete attributeCanDelete = config;
			AssertEquals(true, attributeCanDelete.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", attributeCanDelete.ReasonForNotAbleToDelete);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			config = helper.CreateRefCusRulingConfig(DTY, AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X, cusRuling.PK.ToString());
			attributeCanDelete = config;
			AssertEquals(false, attributeCanDelete.CanDelete);
			AssertEquals(ZZRefCusCodeListCombined.CannotDeleteSystemGenerated, attributeCanDelete.ReasonForNotAbleToDelete);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return config;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			cusRuling.ZZX_Description = "11111 Des";
			cusRuling.ZZX_RulingNumber = "11111";
			cusRuling.ZZX_RulingType = RefCusRulingConfigValuesForAcceptType.Codes.X;
			cusRuling.ZZX_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			config = cusRuling.Configurations.AddNew(DTY, AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X);
		}

		CusRulingConfigCombined config;
	}
}
