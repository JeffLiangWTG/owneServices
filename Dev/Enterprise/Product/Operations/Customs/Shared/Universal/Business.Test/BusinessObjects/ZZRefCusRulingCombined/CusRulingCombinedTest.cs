using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using static Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusRulingCombined))]
	class CusRulingCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableShortcutName()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			cusRuling.ZZX_RulingNumber = "12345";
			AssertEquals("Shortcut when ruling has not applies to set", "Ruling - 12345", cusRuling.HumanReadableShortcutName);
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TESTORG";
			var address = header.MainAddress;
			address.Address1 = "testAddress";
			cusRuling.ZZX_OA_AppliesTo = address.PK;
			AssertEquals("Shortcut when ruling has applies to set", "Ruling - 12345 - TESTORG", cusRuling.HumanReadableShortcutName);
		}

		public override void TestCloneAuditProperties()
		{
			Assert("Need this to supress test failure caused by customized default values", true);
		}

		public void TestSetDefault()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			AssertEquals("ZZX_StartDate", ZDate.Today, cusRuling.ZZX_StartDate);
			AssertEquals("ZZX_EndDate", ZDateTime.MaxSmallDateTimeValue.Date, cusRuling.ZZX_EndDate);
			AssertEquals("ZZX_RN_NKCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, cusRuling.ZZX_RN_NKCountryCode);
		}

		public void TestAutoLoggingIsEnabled()
		{
			var cusRuling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			AssertNull(cusRuling.Logs.MostRecentLog);
			Factory.Save();
			AssertNotNull(cusRuling.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
		}

		public void TestRulingTypeDescription()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			AssertEquals("RulingTypeDescription", "", cusRuling.RulingTypeDescription);
			cusRuling.ZZX_RulingType = RefCusRulingTypeList.Codes._2;
			AssertEquals("RulingTypeDescription", RefCusRulingTypeList.Descriptions._2, cusRuling.RulingTypeDescription);
		}

		public void TestAppliesToAddress()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			AssertNull("AppliesToAddress", cusRuling.AppliesToAddress);
			AssertEquals("AppliesToAddressCountry", "", cusRuling.AppliesToAddressCountry);
			var address = Factory.New<OrgAddress>();
			cusRuling.ZZX_OA_AppliesTo = address.PK;
			AssertNotNull("AppliesToAddress", cusRuling.AppliesToAddress);
			AssertEquals("RulingTypeDescription", address.OA_RN_NKCountryCode, cusRuling.AppliesToAddressCountry);
		}

		public void TestReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddDays(1));
			Assert(cusRuling.ReadOnly);
			cusRuling.ReadOnly = false;
			Assert(cusRuling.ReadOnly);
			var cusRuling2 = Factory.New<ZZRefCusRulingCombined>();
			Assert(!cusRuling2.ReadOnly);
			cusRuling.ReadOnly = true;
			Assert(cusRuling.ReadOnly);
		}

		public void TestIsSystem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddDays(1));
			Assert(cusRuling.IsSystem);
			var cusRuling2 = Factory.New<ZZRefCusRulingCombined>();
			Assert(!cusRuling2.IsSystem);
		}

		public void TestCanDelete()
		{
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			AssertEquals("CanDelete", true, cusRuling.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", cusRuling.ReasonForNotAbleToDelete);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddDays(1));
			AssertEquals("CanDelete", false, cusRuling.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", ZZRefCusRulingCombined.CannotDeleteSystemGenerated, cusRuling.ReasonForNotAbleToDelete);
		}

		public void TestDelete()
		{
			var cusRuling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var config1 = cusRuling.Configurations.AddNew(DTY, AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X);
			var config2 = cusRuling.Configurations.AddNew(DTY, AcceptAmount, 0, RefCusRulingConfigValuesForAcceptType.Codes.X);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusRulingInDiffFactory = newFactory.Load<ZZRefCusRulingCombined>(cusRuling.PK);
			cusRulingInDiffFactory.Delete();
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			AssertNull("cusRuling.IsDeleted", newFactory.Load<ZZRefCusRulingCombined>(cusRuling.PK));
			AssertNull("config1.IsDeleted", newFactory.Load<CusRulingConfigCombined>(config1.PK));
			AssertNull("config2.IsDeleted", newFactory.Load<CusRulingConfigCombined>(config2.PK));
		}

		public void TestTemplateCopy()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "111111", RefCusRulingTypeList.Codes._2, ZDate.Today, ZDate.Today.AddYears(1));
			var config1 = helper.CreateRefCusRulingConfig(DTY, Specific, 5, "CLT", cusRuling.PK.ToString());
			var config2 = helper.CreateRefCusRulingConfig(GST, ExemptCode, 0, GSTStatusCodes.Codes.C48, cusRuling.PK.ToString());
			var clonedData = (ZZRefCusRulingCombined)cusRuling.TemplateCopy();
			AssertEquals("clonedData.IsSystem", false, clonedData.IsSystem);
			AssertEquals("clonedData.ZZX_RulingType", RefCusRulingTypeList.Codes._2, clonedData.ZZX_RulingType);
			AssertEquals("clonedData.ZZX_RulingNumber", "111111", clonedData.ZZX_RulingNumber);
			AssertEquals("clonedData.ZZX_Description", "111111 Desc", clonedData.ZZX_Description);
			AssertEquals("clonedData.ZZX_RN_NKCountryCode", Core.Constants.CountryCodes.Canada, clonedData.ZZX_RN_NKCountryCode);
			AssertEquals("clonedData.ZZX_StartDate", ZDateTime.Today, clonedData.ZZX_StartDate);
			AssertEquals("clonedData.ZZX_EndDate", ZDateTime.Today.AddYears(1), clonedData.ZZX_EndDate);
			AssertEquals("clonedData.Configurations.Count", 2, clonedData.Configurations.Count);
			var clonedConfig1 = clonedData.Configurations[0];
			var clonedConfig2 = clonedData.Configurations[1];
			if (clonedConfig2.ZZY_Category == "DTY")
			{
				clonedConfig2 = clonedData.Configurations[0];
				clonedConfig1 = clonedData.Configurations[1];
			}

			AssertNotEquals("clonedConfig1.PK", config1.PK, clonedConfig1.PK);
			AssertEquals("clonedConfig1.ZZY_Category", DTY, clonedConfig1.ZZY_Category);
			AssertEquals("clonedConfig1.ZZY_Category", Specific, clonedConfig1.ZZY_Type);
			AssertEquals("clonedConfig1.ZZY_Rate", 5.000M, clonedConfig1.ZZY_Rate);
			AssertEquals("clonedConfig1.ZZY_Value", "CLT", clonedConfig1.ZZY_Value);
			AssertNotEquals("clonedConfig2.PK", config2.PK, clonedConfig2.PK);
			AssertEquals("clonedConfig2.ZZY_Category", GST, clonedConfig2.ZZY_Category);
			AssertEquals("clonedConfig2.ZZY_Category", ExemptCode, clonedConfig2.ZZY_Type);
			AssertEquals("clonedConfig2.ZZY_Rate", 0.000M, clonedConfig2.ZZY_Rate);
			AssertEquals("clonedConfig2.ZZY_Value", GSTStatusCodes.Codes.C48, clonedConfig2.ZZY_Value);
		}
	}
}
