using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountAttribute))]
	sealed class AccAlternateGLAccountAttributeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValueDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TS1";
			Factory.Save();

			var attribute = Factory.NewWithValidTestData<AccAlternateGLAccountAttribute>();
			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG;
			attribute.AAA_AttributeValueID = org.PK;
			AssertEquals(attribute.ValueDescription, "TS1");

			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			attribute.AAA_Value = "TPY";
			AssertEquals(attribute.ValueDescription, "Third Party");

			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE;
			attribute.AAA_Value = AccountingMasterFilesConstants.LFECodes.WEU;
			AssertEquals(attribute.ValueDescription, "Within EU");

			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
			attribute.AAA_Value = AccountingMasterFilesConstants.LFOCodes.FOR;
			AssertEquals(attribute.ValueDescription, "Foreign");

			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC;
			attribute.AAA_Value = AccountingMasterFilesConstants.TICCodes.ETI;
			AssertEquals(attribute.ValueDescription, "Tax ID with Extra Tax");

			attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			attribute.AAA_Value = AccountingMasterFilesConstants.SPRCodes.SPS;
			AssertEquals(attribute.ValueDescription, "Sales/Purchases");
		}

		public void TestHumanReadableShortcutNameCore()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new AccountingTestObjectCreator(factory);
			var glHeader = testObjectCreator.CreateAccGLHeader("GL1");
			var chart = testObjectCreator.CreateAlternateChart("CH1");
			factory.Save();

			var alternateGLAccount = testObjectCreator.CreateAccAlternateGlAccount(chart.PK, "99");
			alternateGLAccount.AGA_Description = "TEST";
			var attribute = testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, attribute: "");
			factory.Save();

			AssertEquals(attribute.HumanReadableShortcutName, "99 - TEST - GL1");
		}
	}
}
