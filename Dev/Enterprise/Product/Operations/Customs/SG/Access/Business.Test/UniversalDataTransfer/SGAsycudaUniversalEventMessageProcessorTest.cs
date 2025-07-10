using CargoWise.Types;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	class SGAsycudaUniversalEventMessageProcessorTest : ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaUniversalEventMessageProcessorTest
	{
		public void TestSetCycleOrBatchToBillCountries()
		{
			Header.AMA_MasterBill = "08122223333";
			Header.AMA_ManifestType = SGManifestTypes.Codes.MGI;

			var bill1 = Header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill1Country = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill1;

			var bill2 = Header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			var bill2Country = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill2;

			message.EM_MessageText = MessageWithCycleDateAndNumber;
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();

			var processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, message, Header);
			processor.Process();

			AssertEquals("CycleDate of Bill1 should be set", new ZDateTime(2018, 1, 2), bill1Country.CycleDate);
			AssertEquals("CycleNumber of Bill1 should be set", "1000001", bill1Country.CycleNumber);
			Assert("BatchDate of Bill1 should be empty", bill1Country.BatchDate.IsEmpty);
			Assert("BatchNumber of Bill1 should be empty", bill1Country.BatchNumber.IsEmpty);
			Assert("CycleDate of bill2 should be empty", bill2Country.CycleDate.IsEmpty);
			Assert("CycleNumber of bill2 should be empty", bill2Country.CycleNumber.IsEmpty);

			message.EM_MessageText = MessageWithBatchDateAndNumber;
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			Header.AMA_ManifestType = SGManifestTypes.Codes.MGE;

			processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, message, Header);
			processor.Process();

			AssertEquals("BatchDate of Bill2 should be set", new ZDateTime(2018, 1, 3), bill2Country.BatchDate);
			AssertEquals("BatchNumber of Bill2 should be set", "1000002", bill2Country.BatchNumber);
		}

		protected override ASYCUDA.Business.AsycudaManifestHeader CreateManifestHeader() => Factory.New<AsycudaManifestHeader>();

		protected AsycudaManifestHeader Header => (AsycudaManifestHeader)header;
	}
}
