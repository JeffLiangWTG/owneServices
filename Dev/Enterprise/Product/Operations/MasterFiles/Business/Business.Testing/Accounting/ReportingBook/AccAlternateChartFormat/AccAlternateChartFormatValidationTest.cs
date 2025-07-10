using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccAlternateChartFormatValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAlternateGLAccountCreated()
		{
			Format.Validation.ValidateAll();
			AssertNoErrors(Format.ANF_FormatInfo);

			var alternateGLAccount = new AccountingTestObjectCreator(new BusinessObjectFactory()).CreateAccAlternateGlAccount(Chart.PK, "1");
			alternateGLAccount.Factory.Save();
			Format.Validation.ValidateAll();
			AssertNoRowErrors(Format);
			Format.ANF_Separator = "";
			Format.RunPreSaveValidation();
			AssertHasRowError(Format, "Alternate GL Account is created for this chart, you cannot change Account Format.");
		}

		public void TestCheckANF_Tier()
		{
			var format = Factory.New<AccAlternateChartFormat>();
			format.Validation.ValidateANF_Tier();
			AssertHasError(format.ANF_TierInfo, "The Alternate Chart of Accounts should have at least one Tier.");
			var format2 = Chart.AlternateChartFormats.AddNew();
			format2.ANF_Tier = 1;
			AssertHasError(format2.ANF_TierInfo, "Tier numbers must be consecutive, starting from 1, maximum number allowed is 20.");
			format2.ANF_Tier = 2;
			AssertNoErrors(format2.ANF_TierInfo);
		}

		public void TestCheckANF_Description()
		{
			AssertNoErrors(Format.ANF_TierInfo);
			Format.ANF_Description = ZString.Empty;
			AssertHasError(Format.ANF_DescriptionInfo, "Please enter a value.");
		}

		public void TestCheckANF_Format()
		{
			AssertNoErrors(Format.ANF_FormatInfo);
			Format.ANF_Format = "99999999999";
			var format2 = Chart.AlternateChartFormats.AddNew();
			format2.ANF_Format = "99999999999";
			AssertHasError(format2.ANF_FormatInfo, "The Total length of Format in all Tier should not exceed 20 characters.");
			format2.ANF_Format = "xx";
			AssertEquals("XX", format2.ANF_Format);
			format2.ANF_Format = "ss";
			AssertHasError(format2.ANF_FormatInfo, "Format invalid. Please enter 9 to represent number, enter X to represent letter.");
		}

		public void TestCheckANF_Separator()
		{
			AssertNoErrors(Format.ANF_SeparatorInfo);
			Format.ANF_Separator = "s";
			AssertHasError(Format.ANF_SeparatorInfo, "Separator invalid. Please enter . or -.");
		}

		AccAlternateChartFormat Format;
		AccAlternateChart Chart;
		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();
			Chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Format = Creator.CreateAccAlternateChartFormat(Chart, 1, "9", "Description", "-");
			Factory.Save();
		}
	}
}
