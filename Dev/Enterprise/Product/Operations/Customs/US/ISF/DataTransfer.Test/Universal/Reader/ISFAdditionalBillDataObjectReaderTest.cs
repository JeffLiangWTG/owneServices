using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader.Testing
{
	sealed class ISFAdditionalBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingISFAdditionalBillData()
		{
			var headerBO = Factory.New<CusISFHeader>();
			var billDataObject = SetupAdditionalBill(BillTypeList.Codes.HouseBillOfLading, "HWB1234");
			var reader = new ISFAdditionalBillDataObjectReader(billDataObject, logger, Factory, headerBO);
			var billBO = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				AssertEquals("billBO.BB_BillType", BillTypeList.Codes.HouseBillOfLading, billBO.BB_BillType);
				AssertEquals("billBO.BB_BillNum", "HWB1234", billBO.BB_BillNum);
			});
		}

		AdditionalBill SetupAdditionalBill(ZString billType, ZString billNumber)
		{
			var result = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillType = new WayBillType()
				{ Code = billType },
				BillNumber = billNumber,
			};
			return result;
		}
	}
}
