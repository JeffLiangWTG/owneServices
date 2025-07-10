using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class HVLVShipmentDataObjectWrapperTest : TestCaseWithUniversalObjectFactory
	{
		public void TestShipmentPK()
		{
			var forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S00200701";
			Factory.SaveForTesting();

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SubShipmentCollection.Add(hvlvShipment);

			hvlvShipment.DataContext = DataContextFactory.New();
			hvlvShipment.DataContext.AddDataSource(UniversalDataBuss.Integration.DataContextType.ForwardingShipment, "S00200701");

			var wrapper = new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory);
			AssertEquals("Should return the shipment PK from the data source key.", forwardingShipment.PK, wrapper.ShipmentPK);
		}

		public void TestContainerModeAndNumber()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SubShipmentCollection.Add(hvlvShipment);

			hvlvShipment.ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.LCL };
			hvlvShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			hvlvShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN1234" });
			hvlvShipment.PackingLineCollection.Content = CollectionContent.Complete;

			var wrapper = new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory);
			AssertEquals("CN1234", wrapper.ContainerNumber);
			AssertEquals(Core.Constants.ContainerModes.LCL, wrapper.ContainerMode.Code);
		}

		public void TestDeparturDate()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SubShipmentCollection.Add(hvlvShipment);

			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var leg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, EstimatedDeparture = ZDateTime.Today };
			consol.TransportLegCollection.Add(leg);
			AssertEquals(ZDateTime.Today, new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory).DepartureDate);

			hvlvShipment.SetDateCollection(() => new List<Date>());
			hvlvShipment.DateCollection.Add(new Date() { Type = DateType.Departure, Value = ZDateTime.Today.AddDays(-1), IsEstimate = true });
			AssertEquals(ZDateTime.Today.AddDays(-1), new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory).DepartureDate);

			leg.ActualDeparture = ZDateTime.Today.AddDays(-2);
			AssertEquals(ZDateTime.Today.AddDays(-2), new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory).DepartureDate);

			consol.SetDateCollection(() => new List<Date>());
			var userOverrideDate = new Date() { Type = DateType.LoadingDate, Value = ZDateTime.Today, IsEstimate = false };
			consol.DateCollection.Add(userOverrideDate);
			AssertEquals(ZDateTime.Today.AddDays(-2), new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory).DepartureDate);

			userOverrideDate.Value = ZDateTime.Today.AddDays(-3);
			AssertEquals(ZDateTime.Today.AddDays(-3), new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory).DepartureDate);
		}

		public void TestCurrencyConverterDataProvider()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SubShipmentCollection.Add(hvlvShipment);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetCompanyAndDataProviderDetails(company);

			var provider = (ICurrencyConverterDataProvider)new HVLVShipmentDataObjectWrapper(hvlvShipment, consol, Factory);
			AssertEquals(ExchangeRateType.Customs, provider.RateType);
			AssertEquals("When processing Universal XML, the system will setup the correct environment", Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK), provider.Company);
		}
	}
}
