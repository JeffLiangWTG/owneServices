using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class AcasMessagingExtensionsTest : TestCaseWithFactory
	{
		#region TestGetXmlNamespace_NotSent

		public void TestGetXmlNamespace_NotSent()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			var shipment = Factory.New<ForwardingShipment>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;

			var docDataParameters = new DummyDocDataObjectParameters
			{
				LogProvider = documentData
			};

			var builder = new AirCargoAdvanceScreeningBuilder(shipment, docDataParameters);
			var acas = builder.Build();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(acas);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new AcasMessagingExtensions(document.Object);
			var res = extensions.GetXmlNamespace();

			AssertEquals("xml namespace", "/AirCargoAdvanceScreening/1", res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestGetXmlNamespace_MessageAccepted

		public void TestGetXmlNamespace_MessageAccepted()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			var shipment = Factory.New<ForwardingShipment>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 1));
			Factory.Save();
			Thread.Sleep(10);

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 2));
			Factory.Save();

			var docDataParameters = new DummyDocDataObjectParameters
			{
				LogProvider = documentData
			};

			var builder = new AirCargoAdvanceScreeningBuilder(shipment, docDataParameters);
			var acas = builder.Build();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(acas);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new AcasMessagingExtensions(document.Object);
			var res = extensions.GetXmlNamespace();

			AssertEquals("xml namespace", "/AirCargoAdvanceScreening/1", res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestGetXmlNamespace_AcknowledgementRequired

		public void TestGetXmlNamespace_AcknowledgementRequired()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();

			var shipment = Factory.New<ForwardingShipment>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 1), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			Factory.Save();
			Thread.Sleep(10);

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 2), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			Factory.Save();
			Thread.Sleep(10);

			documentData.Logs.CreateOrRecreateEventLog(Events.Held, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 3), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES={Constants.ACASActions.Code.DoNotLoadHold}");
			Factory.Save();

			var docDataParameters = new DummyDocDataObjectParameters
			{
				LogProvider = documentData
			};

			var builder = new AirCargoAdvanceScreeningBuilder(shipment, docDataParameters);
			var acas = builder.Build();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(acas);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new AcasMessagingExtensions(document.Object);
			var res = extensions.GetXmlNamespace();

			AssertEquals("xml namespace", "/AcknowledgementOfHold/1", res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestGetMessageStatus

		public void TestGetMessageStatus()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var shipment = Factory.New<ForwardingShipment>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 1), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			Factory.Save();

			var docDataParameters = new DummyDocDataObjectParameters
			{
				LogProvider = documentData
			};

			var builder = new AirCargoAdvanceScreeningBuilder(shipment, docDataParameters);
			var acas = builder.Build();

			AssertEquals("prerequisite; correct ACAS state has been set",
				AcasState.OriginalSent, acas.State);

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(acas);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new AcasMessagingExtensions(document.Object);
			var res = extensions.GetMessageStatus();

			const string expectedMessage =
@"The message previously sent has not yet received a response.
Please wait for a response before resending.";

			AssertEquals("message status", expectedMessage, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion
	}
}
