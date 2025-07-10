using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(ETradeData))]
	public class ETradeDataTest : Customs.Business.Testing.CusCodeDataTest<ETradeData>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var newTestData = header.ETradeDatas.AddNew();
			newTestData.CY_Code = "TEST001";
			newTestData.CY_Data = "TEST002";
			return newTestData;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var newTestData = header.ETradeDatas.AddNew();
			newTestData.CY_Code = "TEST001";
			newTestData.CY_Data = "TEST002";
			return newTestData;
		}

		public void TestValidationTypeForHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var etradeDatasForHeader = header.ETradeDatas.AddNew();
			etradeDatasForHeader.CY_Code = "TEST001";
			AssertEquals(etradeDatasForHeader.Validation.GetType(), typeof(ETradeDataValidation));
		}

		public void TestCY_DataAllowWesternEuropeanCharactersOnly()
		{
			var eTradeData = Factory.New<ETradeData>();
			AssertEquals(false, eTradeData.CY_DataAllowWesternEuropeanCharactersOnly);
		}

		public void TestCY_Data()
		{
			var eTradeData = Factory.New<ETradeData>();
			eTradeData.CY_Data = "SELAMİ ŞAHİN";
			AssertEquals("SELAMİ ŞAHİN", eTradeData.CY_Data);
			AssertNoErrors(eTradeData.CY_DataInfo);
		}

		public void TestOnSaving()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var etradeDataInfo = bill.ETradeBillDatas.AddNew();
			etradeDataInfo.CY_Code = "testcytype";

			Factory.Save();
			AssertEquals(etradeDataInfo.Parent.GetType(), typeof(AsycudaBill));
			AssertEquals("ETradeDatas should contain 1 records", 1, bill.ETradeBillDatas.Count);

			etradeDataInfo.CY_Code = ZString.Empty;
			Factory.Save();
			AssertEquals("ETradeDatas should contain no records", 0, bill.ETradeBillDatas.Count);

			var etradeDatasForHeader = header.ETradeDatas.AddNew();
			etradeDatasForHeader.CY_Data = "testCYData";
			etradeDatasForHeader.CY_Code = "testcytype";
			etradeDatasForHeader.CY_Date = ZDateTime.Today;

			Factory.Save();
			AssertEquals(etradeDatasForHeader.Parent.GetType(), typeof(AsycudaManifestHeader));
			AssertEquals("ETradeDatas should contain 1 records", 1, header.ETradeDatas.Count);

			etradeDatasForHeader.CY_Data = ZString.Empty;
			etradeDatasForHeader.CY_Date = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("ETradeDatas should not contains records", 0, header.ETradeDatas.Count);
		}
	}
}
