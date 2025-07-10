using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRule.Loader))]
	sealed class CusGuaranteeRuleLoaderTest : LoaderTestCase
	{
		public void TestLoadOrCreateMainAccessCode()
		{
			var header = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			header.AdditionalAccessCodes.AddNew().FillWithValidTestData();
			header.CusGuaranteeRules.AddNew().FillWithValidTestData();
			var mainAccessCode = CusGuaranteeRule.Loader.LoadOrCreateMainAccessCode(header);
			AssertNotNull(mainAccessCode);
			mainAccessCode.CPR_ValueFrom = "1234";
			AssertEquals(mainAccessCode, CusGuaranteeRule.Loader.LoadOrCreateMainAccessCode(header));

			Factory.Save();
			var headerInOtherFactory = new BusinessObjectFactory().Load<BaseCusGuaranteeHeader>(header.PK);
			AssertEquals(mainAccessCode.PK, CusGuaranteeRule.Loader.LoadOrCreateMainAccessCode(headerInOtherFactory).PK);
		}

		public void TestCreateMainAccessCode()
		{
			var header = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var mainAccessCode = CusGuaranteeRule.Loader.CreateMainAccessCode(header);
			CombineAssertions(() =>
			{
				AssertEquals("CPR_CPH_PermitHeader", header.PK, mainAccessCode.CPR_CPH_PermitHeader);
				AssertEquals("CPR_RuleCode", PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin, mainAccessCode.CPR_RuleCode);
			});
		}

		public void TestLoadMainAccessCode()
		{
			var header = Factory.New<BaseCusGuaranteeHeader>();
			header.AdditionalAccessCodes.AddNew();
			header.CusGuaranteeRules.AddNew();
			AssertNull(CusGuaranteeRule.Loader.LoadMainAccessCode(header));

			var mainAccessCode = Factory.New<CusGuaranteeRule>();
			mainAccessCode.CPR_CPH_PermitHeader = header.PK;
			mainAccessCode.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;

			AssertEquals(mainAccessCode, CusGuaranteeRule.Loader.LoadMainAccessCode(header));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusGuaranteeRule.Loader(Factory);
		}
	}
}
