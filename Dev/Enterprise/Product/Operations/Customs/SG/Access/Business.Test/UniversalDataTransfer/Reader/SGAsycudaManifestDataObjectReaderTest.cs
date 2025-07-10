using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGAsycudaManifestDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportCustomsPort()
		{
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var existingHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			existingHeader.AMA_JobReference = "MAN001";
			existingHeader.MasterBill.ABL_BillNumber = "MST001";
			existingHeader.AMA_RN_NKCountry = "SG";

			var portOfLoading = new UNLOCO() { Code = "ADXXX" };
			var portOfDischarge = new UNLOCO() { Code = "SGXXX" };
			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);
			Factory.SaveForTesting();

			var headerDataObject = helper.SetupManifestHeader("MST001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", SGManifestTypes.Codes.MGI);
			headerDataObject.CustomsLoadPort = new CodeDescriptionPair10Char()
			{
				Code = "ADZZZ",
				Description = "ADZZZ desc"
			};
			headerDataObject.CustomsDischargePort = new CodeDescriptionPair10Char()
			{
				Code = "SGAYC",
				Description = "SGAYC desc"
			};
			Factory.SaveForTesting();

			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);

			AssertEquals("ADZZZ", headerBO.AMA_CustomsLoadPort);
			AssertEquals("SGAYC", headerBO.AMA_CustomsDischargePort);
		}
	}
}
