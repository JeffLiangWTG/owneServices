using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer.Testing
{
	public class TRAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactory
	{
		public void TestManifestHeaderDataObjectReaderType()
		{
			var shipment = new Shipment();
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.DataContext = DataContextFactory.New();
			hvlvShipment.DataContext.AddDataSource(new DataSource { Type = nameof(DataContextType.HVLVConsignment), Key = "Key" });

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var helper = new TRAsycudaManifestDataObjectReaderHelper("TR", Factory);

			var reader = helper.GetBillDataObjectReader(shipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			AssertEquals("Return normal reader for a non-hvlv shipment", false, typeof(TRHVLVAsycudaBillDataObjectReader).IsAssignableFrom(reader.GetType()));
			var hvlvReader = helper.GetBillDataObjectReader(hvlvShipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			AssertType<TRHVLVAsycudaBillDataObjectReader>(hvlvReader);
		}
	}
}
