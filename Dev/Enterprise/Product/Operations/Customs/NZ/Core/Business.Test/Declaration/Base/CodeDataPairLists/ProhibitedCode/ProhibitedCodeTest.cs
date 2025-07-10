using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(ProhibitedCode))]
	public class ProhibitedCodeTest : CodeDataPairTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProhibitedCode(Factory, null);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Prohibited Code", testCode.HumanReadableName);
		}

		public void TestValidateZO_Code()
		{
			testCode.ZO_Code = "XXX";
			Assert("Invalid code", testCode.ZO_CodeInfo.HasMessageErrors());

			testCode.ZO_Code = testCode.ZO_CodeList[0].Code;
			Assert("Valid code", !testCode.ZO_CodeInfo.HasMessageErrors());
		}

		public void TestZO_CodeList()
		{
			Assert("Code list type", testCode.ZO_CodeList is ProhibitedCodeList);
		}

		public void TestValidateZO_Data()
		{
			testCode.ZO_Code = testCode.ZO_CodeList[0].Code;
			testCode.ZO_Data = "ZZZ";
			Assert("ProhibitedCode requires no data", testCode.ZO_DataInfo.HasNotifications());

			testCode.ZO_Data = ZString.Empty;
			Assert("ProhibitedCode requires no data", !testCode.ZO_DataInfo.HasNotifications());
		}

		public void TestCorrectListForCusClassPartPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPROEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "PGE", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "PGB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "NZPROIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "PGI", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "PGB", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var prohibitedCode = pivot.ProhibitedCodes.AddNew();
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGE", "PGB" }, new string[] { "PGI" });

			// list reflects a change of classification type
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGI", "PGB" }, new string[] { "PGE" });

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertCodesInOutOfList(prohibitedCode.ZO_CodeList, new string[] { "PGB", "PGI", "PGE" }, System.Array.Empty<string>());
		}

		public void TestIsImportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "NZPROIMP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsImport, "BEF", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();

			testCode.ZO_Code = "BEF";
			AssertEquals("Prohibited Goods Import Code", true, testCode.IsImportCode);
			testCode.ZO_Code = "XXX";
			AssertEquals("Prohibited Goods Import Code", false, testCode.IsImportCode);
		}

		public void TestIsExportCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "NZPROEXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "ANT", new ZDateTime(2010, 1, 1), new ZDateTime(2076, 1, 1));
			Factory.Save();
			testCode.ZO_Code = "ANT";
			AssertEquals("Prohibited Goods Import Code", true, testCode.IsExportCode);
			testCode.ZO_Code = "XXX";
			AssertEquals("Prohibited Goods Import Code", false, testCode.IsExportCode);
		}
		#region Implementation

		ProhibitedCode testCode;
		protected override void SetUp()
		{
			base.SetUp();
			testCode = new ProhibitedCode(Factory, null);
		}

		protected override BusinessObject BOCodeInfoIsAttachedTo
		{
			get { return InvoiceLine; }
		}

		protected override CodeDataPairCollection GetCodeInfoCollection()
		{
			return InvoiceLine.ProhibitedCodes;
		}
		#endregion
	}
}
