using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(OfficeCode))]
	class OfficeCodeTest : CusCodeDataTest<OfficeCode>
	{
		public void TestCanDelete()
		{
			AssertEquals(false, officeCode.CanDelete);
		}

		public void TestCreate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CY_Code", EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented, officeCode.CY_Code);
				AssertEquals("Parent", declaration, officeCode.Parent);
			});
		}

		public void TestOffice()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CUSOF", "CustomsOffice");
			helper.CreateCusCodeType("CUSDP", "CustomsDepartment");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "CUSOF", "TR000001", "Office 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "CUSDP", "TR000001", "Department 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "CUSDP", "TR000002", "Department 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			officeCode.CY_Data = "TR000002";
			AssertNull(officeCode.Office);

			officeCode.CY_Data = "TR000001";
			AssertEquals("Office 1", officeCode.Office.ZZD_Description);

			officeCode.CY_Data = ZString.Empty;
			AssertNull(officeCode.Office);
		}

		protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
		}

		protected override BusinessObject GetNewBusinessObject() => officeCode;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().CustomsOffices.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			officeCode = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented);
		}

		JobDeclaration declaration;
		OfficeCode officeCode;
	}
}
