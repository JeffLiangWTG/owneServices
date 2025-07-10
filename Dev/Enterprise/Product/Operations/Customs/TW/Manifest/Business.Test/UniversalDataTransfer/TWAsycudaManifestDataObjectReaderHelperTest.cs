using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer.Testing
{
	public sealed class TWAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactory
	{
		public void TestManifestHeaderDataObjectReaderType()
		{
			var shipment = new Shipment();

			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.DataContext = DataContextFactory.New();
			hvlvShipment.DataContext.AddDataSource(new DataSource { Type = nameof(DataContextType.HVLVConsignment), Key = "Key" });

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var helper = new TWAsycudaManifestDataObjectReaderHelper("TW", Factory);

			var reader = helper.GetBillDataObjectReader(shipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			AssertEquals("Return normal reader for a non-hvlv shipment", false, typeof(TWHVLVAsycudaBillDataObjectReader).IsAssignableFrom(reader.GetType()));

			var hvlvReader = helper.GetBillDataObjectReader(hvlvShipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			Assert("Return TW HVLV reader for a hvlv shipment", typeof(TWHVLVAsycudaBillDataObjectReader).IsAssignableFrom(hvlvReader.GetType()));
		}
	}
}
