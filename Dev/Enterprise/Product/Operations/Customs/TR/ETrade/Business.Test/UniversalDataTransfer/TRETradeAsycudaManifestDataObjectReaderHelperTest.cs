using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(TRETradeHVLVAsycudaManifestDataObjectReaderHelper))]
	class TRETradeAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactory
	{
		public void TestAsycudaBillDataObjectReaderType()
		{
			var hvlvShipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipmentDataObject.DataContext = DataContextFactory.New();
			hvlvShipmentDataObject.DataContext.AddDataSource(new DataSource { Type = nameof(DataContextType.HVLVConsignment), Key = "Key" });

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var helper = new TRETradeHVLVAsycudaManifestDataObjectReaderHelper("TR", Factory);

			var hvlvReader = helper.GetBillDataObjectReader(hvlvShipmentDataObject, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			AssertType<TRETradeHVLVAsycudaBillDataObjectReader>(hvlvReader);
		}

		public void TestFillManifestSpecificDataCore()
		{
			var consolDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var hvlvShipmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipmentDataObject.DataContext = DataContextFactory.New();
			hvlvShipmentDataObject.ShipmentType = new CodeDescriptionPair { Code = "HVL" };
			hvlvShipmentDataObject.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			hvlvShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "NZAKL" };

			consolDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { hvlvShipmentDataObject });
			consolDataObject.VoyageFlightNo = "Flight";
			consolDataObject.PortOfOrigin = new UNLOCO { Code = "CNSHA" };

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var helper = new TRETradeHVLVAsycudaManifestDataObjectReaderHelper("TR", Factory);

			helper.FillManifestSpecificData(consolDataObject, new DummyLogger(), manifestHeader, new UniversalObjectFactory(manifestHeader.Factory));

			CombineAssertions("Header read correctly", () =>
			{
				AssertEquals("Departure Flight", manifestHeader.DepartureFlight, "Flight");
				AssertEquals("Departure Country Code", manifestHeader.DepartureCountryCode, "NZ");
			});
		}
	}
}
