using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class USLVConsolidatedDeclarationReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestReadIntoBusinessObject_ShouldCreateNewDeclaration()
		{
			var masterBillNumber = "555555";
			var shipment = SetupDeclaration(null, masterBillNumber, new WayBillType() { Code = "MWB", Description = "Master Waybill" });

			var existingDeclaration = Factory.New<JobDeclaration>();
			existingDeclaration.JE_MasterBill = masterBillNumber;

			Factory.SaveForTesting();

			var reader = new USLVConsolidatedDeclarationReader(shipment, logger, Factory);
			var newDeclaration = reader.ReadIntoBusinessObject();
			AssertNotEquals(newDeclaration.PK, existingDeclaration.PK);
		}
	}
}
