using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ProcedureCodesModuleFilter))]
	sealed class ProcedureCodesModuleFilterTest : ModuleFilterTestCase<ProcedureCodesModuleFilter>
	{
		public void TestCPCCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("XX"))
			{
				var tester = new ProcedureCodesModuleFilter("DESC", (val1, val2) => new ZQuery());
				AssertEquals(3, tester.CPCCodes.Count);
				AssertArrayEqualsByElements(new string[] { "AA", "BB", "CC" }, tester.CPCCodes.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("YY"))
			{
				var tester = new ProcedureCodesModuleFilter("DESC", (val1, val2) => new ZQuery());
				AssertEquals(0, tester.CPCCodes.Count);
			}
		}

		public void TestPPCCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("XX"))
			{
				var tester = new ProcedureCodesModuleFilter("DESC", (val1, val2) => new ZQuery());
				tester.Property1 = "";
				AssertEquals(4, tester.PPCCodes.Count);
				AssertArrayEqualsByElements(new string[] { "00", "AA", "BB", "CC" }, tester.PPCCodes.GetAllCodes());
				tester.Property1 = "XX";
				AssertEquals(0, tester.PPCCodes.Count);
				tester.Property1 = "AA";
				AssertEquals(2, tester.PPCCodes.Count);
				AssertArrayEqualsByElements(new string[] { "XX", "YY" }, tester.PPCCodes.GetAllCodes());
				tester.Property1 = "BB";
				AssertEquals(3, tester.PPCCodes.Count);
				AssertArrayEqualsByElements(new string[] { "XX", "YY", "ZZ" }, tester.PPCCodes.GetAllCodes());
				tester.Property1 = "CC";
				AssertEquals(0, tester.PPCCodes.Count);
				tester.Property1 = "DD";
				AssertEquals(0, tester.PPCCodes.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("YY"))
			{
				var tester = new ProcedureCodesModuleFilter("DESC", (val1, val2) => new ZQuery());
				CombineAssertions(() =>
				{
					tester.Property1 = "";
					AssertEquals(0, tester.PPCCodes.Count);
					tester.Property1 = "XX";
					AssertEquals(0, tester.PPCCodes.Count);
					tester.Property1 = "AA";
					AssertEquals(0, tester.PPCCodes.Count);
					tester.Property1 = "BB";
					AssertEquals(0, tester.PPCCodes.Count);
					tester.Property1 = "CC";
					AssertEquals(0, tester.PPCCodes.Count);
					tester.Property1 = "DD";
					AssertEquals(0, tester.PPCCodes.Count);
				});
			}
		}

		public void TestAdditionalValidationOnCodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("XX"))
			{
				var tester = new ProcedureCodesModuleFilter("DESC", (val1, val2) => new ZQuery());
				tester.Property1 = "AA";
				tester.Property2 = "";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "XX";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "ZZ";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "AA";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "00";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property1 = "BB";
				tester.Property2 = "";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "XX";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "ZZ";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "AA";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "00";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property1 = "CC";
				tester.Property2 = "";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "XX";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "ZZ";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "AA";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "00";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property1 = "XX";
				tester.Property2 = "";
				AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "XX";
				AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "ZZ";
				AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "AA";
				AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "00";
				AssertHasWarningOfInvalidCodeOnly(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property1 = "";
				tester.Property2 = "";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "XX";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "ZZ";
				AssertNoNotifications(tester.Property1Info);
				AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
				tester.Property2 = "AA";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
				tester.Property2 = "00";
				AssertNoNotifications(tester.Property1Info);
				AssertNoNotifications(tester.Property2Info);
			}
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ProcedureCodesModuleFilter GetNewModuleFilter() => new ProcedureCodesModuleFilter("moo", (val1, val2) => new ZQuery());

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override void SetUp()
		{
			base.SetUp();
			var testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_Code = "XX";
			testCountry.RN_Desc = "Test Country";
			testCountry = Factory.NewWithValidTestData<RefCountry>();
			testCountry.RN_Code = "YY";
			testCountry.RN_Desc = "Test Country2";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("XX", "A", "AA", "", "", "AA__", "IMP");
			helper.CreateRefCusProcedure("XX", "A", "AA", "XX", "", "AAXX", "IMP,EXW");
			helper.CreateRefCusProcedure("XX", "A", "AA", "YY", "", "AAYY", "IMP");
			helper.CreateRefCusProcedure("XX", "A", "BB", "", "", "BB__", "EXP");
			helper.CreateRefCusProcedure("XX", "A", "BB", "XX", "", "BBXX", "EXP");
			helper.CreateRefCusProcedure("XX", "A", "BB", "YY", "", "BBYY", "EXP");
			helper.CreateRefCusProcedure("XX", "A", "BB", "ZZ", "", "BBYY", "EXP");
			helper.CreateRefCusProcedure("XX", "A", "CC", "", "", "CC__", "EXP");
			Factory.Save();
		}

		static void AssertHasWarningOfInvalidCodeOnly(ZPropertyInfo info)
		{
			AssertNoErrors(info);
			AssertNoMessageErrors(info);
			AssertHasWarningContaining(info, ListValidation.InvalidCodeMessage);
		}
	}
}
