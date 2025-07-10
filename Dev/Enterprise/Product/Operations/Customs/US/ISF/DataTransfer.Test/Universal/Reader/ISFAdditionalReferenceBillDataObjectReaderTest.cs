using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader.Testing
{
	sealed class ISFAdditionalReferenceBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingISFAdditionalReferenceBillData()
		{
			var headerBO = Factory.New<CusISFHeader>();
			var billDataObject = SetupAdditionalReference("BRF234567");
			var reader = new ISFAdditionalReferenceBillDataObjectReader(billDataObject, logger, Factory, headerBO);
			var billBO = reader.ReadIntoBusinessObject();
			AssertNotNull(billBO);
			CombineAssertions(delegate
			{
				AssertEquals("billBO.BB_BillType", BillTypeList.Codes.BondReferenceNumber, billBO.BB_BillType);
				AssertEquals("billBO.BB_BillNum", "BRF234567", billBO.BB_BillNum);
			});
		}

		AdditionalReference SetupAdditionalReference(ZString billNumber)
		{
			var result = new AdditionalReference()
			{
				Type = new EntryType()
				{
					Code = BillTypeList.Codes.BondReferenceNumber
				},
				ReferenceNumber = billNumber,
			};
			return result;
		}
	}
}
