using System.Linq;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer.Testing
{
	sealed class TWHVLVAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportBill()
		{
			var (hvlvShipment, _) = PrepareDataObject();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.MasterBill.ABL_RL_NKPortOfLoading = "AUSYD";
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(manifestHeader.AMA_RN_NKCountry, Factory.BOFactory);
			var bill = new TWHVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, readerHelper, isUpdateEnabled: true).ReadIntoBusinessObject();

			CombineAssertions("Bill read correctly", () =>
			{
				AssertNotNull(bill);
				AssertEquals("Remark", "PackingLine Description", bill.ABL_Remarks);
				AssertEquals("Split Quantity", 1, bill.Header.ArrivalHeaders.Single().ArrivalDetails.Single().ATL_Quantity);
				AssertEquals("Loading port", "AUSYD", bill.ABL_RL_NKPortOfLoading);
			});
		}

		public void TestRemarksFallback()
		{
			var (hvlvShipment, packingLine) = PrepareDataObject();
			packingLine.GoodsDescription = string.Empty;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(manifestHeader.AMA_RN_NKCountry, Factory.BOFactory);
			var bill = new TWHVLVAsycudaBillDataObjectReader(hvlvShipment, logger, Factory, manifestHeader, readerHelper, isUpdateEnabled: true).ReadIntoBusinessObject();

			AssertEquals("Remarks should fallback to consignment goods description", "ConsignmentGoodsDescription", bill.ABL_Remarks);
		}

		(Shipment, PackingLine) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.WayBillNumber = "TESTCONSIGNMENT1";
			hvlvShipment.GoodsDescription = "ConsignmentGoodsDescription";
			hvlvShipment.TotalNoOfPacks = 1;
			hvlvShipment.TotalWeight = 1;
			hvlvShipment.TotalWeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms };
			hvlvShipment.TotalVolume = 2;
			hvlvShipment.TotalVolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicCentimeters };

			var packingLine = new PackingLine();
			packingLine.GoodsDescription =
@"PackingLine
Description";

			hvlvShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			return (hvlvShipment, packingLine);
		}
	}
}
