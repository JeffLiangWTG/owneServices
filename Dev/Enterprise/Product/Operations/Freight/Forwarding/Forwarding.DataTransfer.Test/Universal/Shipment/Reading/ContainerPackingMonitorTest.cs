using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class ContainerPackingMonitorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMonitorTareWeight_Null()
		{
			ContainerPackingMonitor.MonitorContainerTareWeightChanges(null, null);

			var dataObject = new UniversalShipment(new DefaultDataObjectWriterStrategy());
			dataObject.DataContext = new DataContext();
			ContainerPackingMonitor.MonitorContainerTareWeightChanges(dataObject, null);

			var shipment = Factory.New<ForwardingShipment>();
			ContainerPackingMonitor.MonitorContainerTareWeightChanges(dataObject, shipment);
		}

		public void TestMonitorTareWeight()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();

			Factory.Save();

			var dataObject = GetUniversalShipmentForMonitoring();
			AssertNotNull("UniversalXml has been created", dataObject);

			using (ContainerPackingMonitor.MonitorContainerTareWeightChanges(dataObject, shipment))
			{
				container.JC_TareWeight = 100;
				Assert("ErrorReporter has not yet been triggered", string.IsNullOrWhiteSpace(ErrorReporter.LastKeyReported));
			}

			Assert("ErrorReporter has been triggered", !string.IsNullOrWhiteSpace(ErrorReporter.LastKeyReported));

			AssertContains("ErrorReporter contains the location where the JC_TareWeight was changed",
				nameof(TestMonitorTareWeight), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		UniversalShipment GetUniversalShipmentForMonitoring()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);

			const string fileName = "Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Shipment.TestFiles.UniversalShipmentWithPackingLines.xml";

			using (var inputStream = (SubStreamableStream)new MemoryStream(resourceRetriever.GetBytes(fileName)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipmentDataObject, inputStream, new TestErrorLogger());
			}

			return shipmentDataObject;
		}
	}
}
