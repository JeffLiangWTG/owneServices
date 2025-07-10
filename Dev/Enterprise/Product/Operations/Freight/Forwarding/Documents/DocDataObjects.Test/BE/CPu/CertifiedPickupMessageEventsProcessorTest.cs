using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Freight.Forwarding.Business.CertifiedPickupConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	sealed class CertifiedPickupMessageEventsProcessorTest : TestCaseWithFactory
	{
		#region TestOnMessageSent

		public void TestOnMessageSent()
		{
			var consol = CreateConsol();
			var @event = Events.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"|DEP=Terminal|MST={ParameterMessageTypes.Transfer}");

			var msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("Consol has MSN event", msn);

			var certifiedPickup = CreateCertifiedPickup(consol);
			var containers = certifiedPickup.SelectedContainers.ToArray();
			containers[0].Action.IsTransferToForwarder = true;
			containers[0].CurrentStatus = Status.Accepted;
			containers[1].Action.IsTransferToTransporter = true;
			containers[1].CurrentStatus = Status.DeclinedByNextPartyForTransferRevoke;

			var document = CreateMockDocument(consol, certifiedPickup);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			var containerMSN = consol.Containers[0].Logs.MostRecentLogByEventTime(@event);

			AssertNotNull(containerMSN);
			AssertEquals(msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
			AssertEquals(msn.SL_EventTime, containerMSN.SL_EventTime);
			AssertEquals("|DEP=Terminal|EQN=CONT1111111|MSB=TransferSentAwaitingResponse|MST=Certified Pickup - Transfer", containerMSN.SL_Reference);

			containerMSN = consol.Containers[1].Logs.MostRecentLogByEventTime(@event);

			AssertNotNull(containerMSN);
			AssertEquals(msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
			AssertEquals(msn.SL_EventTime, containerMSN.SL_EventTime);
			AssertEquals("|DEP=Terminal|EQN=CONT2222222|MSB=TransferSentAwaitingResponse|MST=Certified Pickup - Transfer", containerMSN.SL_Reference);
		}

		public void TestOnMessageSent_IgnoreTransferSentAwaitingResponse()
		{
			var consol = CreateConsol();
			var @event = Events.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"|DEP=Terminal|MST={ParameterMessageTypes.Transfer}");

			var msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("Consol has MSN event", msn);

			var certifiedPickup = CreateCertifiedPickup(consol);
			var containers = certifiedPickup.SelectedContainers.ToArray();
			containers[0].Action.IsTransferToForwarder = true;
			containers[0].CurrentStatus = Status.Revoked;
			containers[1].Action.IsTransferToTransporter = true;
			containers[1].CurrentStatus = Status.TransferSentAwaitingResponse;

			var document = CreateMockDocument(consol, certifiedPickup);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			var containerMSN = consol.Containers[0].Logs.MostRecentLogByEventTime(@event);

			AssertNotNull(containerMSN);
			AssertEquals(msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
			AssertEquals(msn.SL_EventTime, containerMSN.SL_EventTime);
			AssertEquals("|DEP=Terminal|EQN=CONT1111111|MSB=TransferSentAwaitingResponse|MST=Certified Pickup - Transfer", containerMSN.SL_Reference);

			containerMSN = consol.Containers[1].Logs.MostRecentLogByEventTime(@event);

			AssertNull(containerMSN);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Factory.Save();

			return consol;
		}

		Mock<IDocument> CreateMockDocument(ForwardingConsol consol, CertifiedPickup certifiedPickup)
		{
			var metaDataProvider = new MetaDataProviderForTest();
			var factory = new DocumentVisualizer.DocDataObjects.DocDataObjectDynamicDataFactory();
			var dynamicData = certifiedPickup.MakeDynamic(metaDataProvider, dynamicDataFactory: factory);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns("Transfer");
			document.SetupGet(d => d.DataContext).Returns(DataContext.BECertifiedPickup);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			return document;
		}

		CertifiedPickup CreateCertifiedPickup(ForwardingConsol consol)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = DocDataObjects.BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new CertifiedPickupBuilder(consol, parameters);
			var data = builder.Build();

			return data;
		}

		sealed class MetaDataProviderForTest : IMetaDataProvider
		{
			public MetaDataProviderForTest()
			{
			}

			object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType) => null;
		}

		#endregion
	}
}
