using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusPermitHeader))]
	sealed class CusPermitHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCPH_SubType_ReadOnly()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			Assert(permitHeader.CPH_SubType_ReadOnly);
			permitHeader.CPH_Type = PermitTypeList.Codes.RCC;
			Assert(!permitHeader.CPH_SubType_ReadOnly);
		}

		public void TestCPH_StartDate_ReadOnly()
		{
			var header = permitHeader;
			AssertEquals(false, permitHeader.CPH_StartDate_ReadOnly);
		}

		public void TestCusPermitRules()
		{
			AssertType<CusPermitRuleCollection>(permitHeader.CusPermitRules);
		}

		public void TestCusPermitLineTransactions()
		{
			AssertType<CusPermitLineTransactionCollection>(permitHeader.CusPermitLineTransactions);
		}

		public void TestValidation()
		{
			AssertType<CusPermitHeaderValidation>(permitHeader.Validation);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(ZDateTime.MaxSmallDateTime.Date, permitHeader.CPH_EndDate);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = permitHeader;
			header.FillWithValidTestData();
			header.CusPermitLineTransactions.DeleteAll();
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<CusPermitHeader>();
		}

		CusPermitHeader permitHeader;
	}

	[TestedType(typeof(CusPermitHeader.Loader))]
	sealed class CusPermitHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var permit = helper.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, org.PK, "NO123", startDate.Date, endDate.Date, "BTH", "IMP");

			Factory.Save();

			var permitHeader = new CusPermitHeader.Loader(Factory).Load(Core.Constants.CountryCodes.SouthAfrica, "NO123", org.PK, ZDateTime.Today);
			AssertEquals(permitHeader.PK, permit.PK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusPermitHeader.Loader(Factory);
		}
	}
}
