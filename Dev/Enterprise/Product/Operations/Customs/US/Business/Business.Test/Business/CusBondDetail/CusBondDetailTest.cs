using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusBondDetail))]
	sealed class CusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFundDetail()
		{
			var bondData = Factory.New<CusBondDetail>();
			AssertEquals("PW_Status", FundIndicatorList.Codes.HasSufficientFund, bondData.PW_Status);
			AssertEquals("PW_StatusDesc", FundIndicatorList.Descriptions.HasSufficientFund, bondData.PW_StatusDesc);
			AssertEquals("HasSufficientFund", ZBool.True, bondData.HasSufficientFund);
			bondData.HasSufficientFund = ZBool.False;
			AssertEquals("PW_Status", FundIndicatorList.Codes.HasInsufficientFund, bondData.PW_Status);
			AssertEquals("PW_StatusDesc", FundIndicatorList.Descriptions.HasInsufficientFund, bondData.PW_StatusDesc);
		}

		public void TestDefaultValues()
		{
			var bondData = (CusBondDetail)GetNewBusinessObject();
			AssertEquals(ApplicationCodeList.Codes.UsaInBond, bondData.PW_ApplicationCode);
			AssertEquals(FundIndicatorList.Codes.HasSufficientFund, bondData.PW_Status);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var bondData2 = newFactory.Load<CusBondDetail>(bondData.PK);
			AssertEquals(ApplicationCodeList.Codes.UsaInBond, bondData2.PW_ApplicationCode);
			AssertEquals(FundIndicatorList.Codes.HasSufficientFund, bondData2.PW_Status);
		}

		public void TestIsBondActive()
		{
			CusBondDetail bondData = Factory.New<CusBondDetail>();
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(1);
			AssertEquals(false, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-11);
			AssertEquals(true, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondEffectiveDate = ZDateTime.Empty;
			AssertEquals(true, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(false, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(1);
			AssertEquals(true, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondExpiryDate = ZDateTime.Today;
			AssertEquals(true, bondData.IsBondActive(ZDateTime.Today));
			bondData.PW_BondExpiryDate = ZDateTime.Empty;
			AssertEquals(true, bondData.IsBondActive(ZDateTime.Today));
		}

		public void TestIsEmpty()
		{
			CusBondDetail bondData = (CusBondDetail)GetNewBusinessObject();
			AssertEquals("IsEmpty", true, bondData.IsEmpty);
			bondData.PW_ActivityCode = "1";
			AssertEquals("IsEmpty", false, bondData.IsEmpty);
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			AssertEquals(true, bondData.IsContinuousBond);
			AssertEquals(false, bondData.IsSingleTransactionBond);
			bondData.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			AssertEquals(false, bondData.IsContinuousBond);
			AssertEquals(true, bondData.IsSingleTransactionBond);
		}

		public void TestValidationAndLookups()
		{
			var bondData = Factory.New<CusBondDetail>();
			AssertEquals(typeof(CusBondDetailValidation), bondData.Validation.GetType());
			AssertEquals(typeof(CusBondDetailLookups), bondData.Lookups.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<CusBondDetail>();
			result.Parent = factory.NewWithValidTestData<OrgHeader>();
			return result;
		}
	}
}
