using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Constants = Enterprise.Customs.US.ISF.Business.ISFConstants;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader.Testing
{
	sealed class ISFEntryNumberBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingISFEntryNumberBillData()
		{
			var headerBO = Factory.New<CusISFHeader>();
			var billDataObject = SetupEntryNumber("ENS12345");
			var reader = new ISFEntryNumberBillDataObjectReader(billDataObject, logger, Factory, headerBO);
			var billBO = reader.ReadIntoBusinessObject();
			AssertNotNull(billBO);
			CombineAssertions(delegate
			{
				AssertEquals("billBO.BB_BillType", BillTypeList.Codes.USCBPEntryNumber, billBO.BB_BillType);
				AssertEquals("billBO.BB_BillNum", "ENS12345", billBO.BB_BillNum);
			});
		}

		EntryNumber SetupEntryNumber(ZString billNumber)
		{
			var result = new EntryNumber()
			{
				Type = new EntryType()
				{
					Code = Constants.EntryNumberConstants.ENS
				},
				Number = billNumber,
			};
			return result;
		}
	}
}
