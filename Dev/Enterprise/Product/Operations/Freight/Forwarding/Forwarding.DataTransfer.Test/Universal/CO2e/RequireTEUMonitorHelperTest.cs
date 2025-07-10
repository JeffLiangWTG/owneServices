using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class RequireTEUMonitorHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMonitorRequireTEU_Null()
		{
			AssertEquals(DisposableAction.NoAction, RequireTEUMonitorHelper.MonitorRequireTEUChanges(null, null));

			var dataObject = new UniversalShipment(new DefaultDataObjectWriterStrategy());
			dataObject.DataContext = new DataContext();
			AssertEquals(DisposableAction.NoAction, RequireTEUMonitorHelper.MonitorRequireTEUChanges(dataObject, null));

			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(DisposableAction.NoAction, RequireTEUMonitorHelper.MonitorRequireTEUChanges(dataObject, shipment));
		}

		public void TestMonitorRequireTEU()
		{
			// Arrange
			var fortyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var twentyGP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_RC = fortyGP.PK;
			container1.JC_ContainerCount = 1;
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 1;
			packline1.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			container1.AddPackLine(packline1);
			Assert((shipment as ICO2eProvider).RequireTEU);
			shipment.SetCO2eStatus("CUR");

			Factory.Save();

			// Act & Assert
			var requeireTEUCalled = 0;
			shipment.OnRequireTEUCalled += delegate { requeireTEUCalled++; };

			var dataObject = GetUniversalShipmentForMonitoring();
			AssertNotNull("UniversalXml has been created", dataObject);

			using (RequireTEUMonitorHelper.MonitorRequireTEUChanges(dataObject, shipment))
			{
				container1.JC_ContainerCount = 2;
				container1.JC_RC = twentyGP.PK;
				container1.JC_TareWeight = 100;
				packline1.JL_ActualWeight = 30;
				var packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_ActualWeight = 2;
				container1.AddPackLine(packline2);
			}

			AssertEquals(2, requeireTEUCalled);
			AssertEquals("NCU", shipment.GetCO2eStatus());

			packline1.JL_ActualWeight = 40;
			AssertEquals(2, requeireTEUCalled);

			shipment.SetCO2eStatus("CUR");
			packline1.JL_ActualWeight = 50;
			AssertEquals(3, requeireTEUCalled);
			AssertEquals("NCU", shipment.GetCO2eStatus());
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
