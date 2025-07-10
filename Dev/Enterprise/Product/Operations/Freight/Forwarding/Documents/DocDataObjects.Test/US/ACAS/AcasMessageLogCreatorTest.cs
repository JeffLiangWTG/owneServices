using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
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
	sealed class AcasMessageLogCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog_ShipmentReport

		public void TestCreateMessageSentLog_ShipmentReport()
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

			var logCreator = new AcasMessageLogCreator();
			var res = logCreator.CreateMessageSentLog(documentData, dynamicData.Object, "zzz", "recipient");

			AssertEquals("log creator indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"MSN |DEP=recipient|LOC=US|MST=Advanced Cargo Report"
				},
				logs);
		}

		#endregion

		#region TestCreateMessageSentLog_HoldAcknowledgement

		public void TestCreateMessageSentLog_HoldAcknowledgement()
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

			documentData.Logs.CreateOrRecreateEventLog(Events.InterchangeSent, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 2), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			Factory.Save();
			Thread.Sleep(10);

			documentData.Logs.CreateOrRecreateEventLog(Events.Held, EstimateActual.Actual, new ZDateTimeOffset(2019, 2, 3), $"|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES={Constants.ACASActions.Code.DoNotLoadHold}");
			Factory.Save();
			Thread.Sleep(10);

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

			var logCreator = new AcasMessageLogCreator();
			var res = logCreator.CreateMessageSentLog(documentData, dynamicData.Object, "zzz", "recipient");

			AssertEquals("log creator indicated that sent log has been created", true, res);

			var logs = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					"MSN |MST=Advanced Cargo Report|LOC=US",
					"ISN |MST=Advanced Cargo Report|LOC=US",
					"SHL |MST=Advanced Cargo Report|LOC=US|RES=6H",
					"MSN |DEP=recipient|LOC=US|MSB=Hold Acknowledgement|MST=Advanced Cargo Report"
				},
				logs);
		}

		#endregion
	}
}
