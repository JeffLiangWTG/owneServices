using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class LegacyVGMMessageEventProcessorTest : TestCaseWithFactory
	{
		#region TestOnMessageSent

		const string verifiedGrossContainerWeight = "Verified Gross Container Weight";

		public void TestOnMessageSent()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);

			var @event = Events.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has MSN event", msn);

			var documentDataSource = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, documentDataSource);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMSN = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MSN event found on container", containerMSN);
				AssertEquals("consol MSN event SL_IsEstimate matches container MSN SL_IsEstimate",
					msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
				AssertEquals("consol MSN event SL_EventTime matches container MSN SL_EventTime",
					msn.SL_EventTime, containerMSN.SL_EventTime);
				AssertEquals("consol MSN event SL_Reference matches container MSN SL_Reference",
					msn.SL_Reference, containerMSN.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region TestOnMessageWithdrawalSent

		public void TestOnMessageWithdrawalSent()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);

			var @event = Events.MessageWithdrawCancelRequest;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var mwr = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has MWR event", mwr);

			var documentDataSource = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, documentDataSource);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageWithdrawalSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMWR = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MWR event found on container", containerMWR);
				AssertEquals("consol MWR event SL_IsEstimate matches container MWR SL_IsEstimate",
					mwr.SL_IsEstimate, containerMWR.SL_IsEstimate);
				AssertEquals("consol MWR event SL_EventTime matches container MWR SL_EventTime",
					mwr.SL_EventTime, containerMWR.SL_EventTime);
				AssertEquals("consol MWR event SL_Reference matches container MWR SL_Reference",
					mwr.SL_Reference, containerMWR.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region TestOnResetToOriginal

		public void TestOnResetToOriginal()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);

			var @event = Events.StatusUpdated;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var stu = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has STU event", stu);

			var documentDataSource = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, documentDataSource);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnResetToOriginal();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerSTU = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("STU event found on container", containerSTU);
				AssertEquals("consol STU event SL_IsEstimate matches container STU SL_IsEstimate",
					stu.SL_IsEstimate, containerSTU.SL_IsEstimate);
				AssertEquals("consol STU event SL_EventTime matches container STU SL_EventTime",
					stu.SL_EventTime, containerSTU.SL_EventTime);
				AssertEquals("consol STU event SL_Reference matches container STU SL_Reference",
					stu.SL_Reference, containerSTU.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region Implementation

		IDynamicData CreateDynamicData(ForwardingConsol consol, UniversalShipment shipment)
		{
			var metaDataProvider = new MetaDataProviderForTest(consol);
			var dynamicData = shipment.MakeDynamic(metaDataProvider);
			return dynamicData;
		}

		UniversalShipment CreateShipment(ForwardingConsol consol)
		{
			var uxmlContainers = consol
				.Containers
				.Cast<ForwardingContainer>()
				.Select(container => new UniversalDataBuss.DataObjects.Universal.Container
				{
					ContainerNumber = container.JC_ContainerNum
				});

			var uxmlShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uxmlShipment.SetContainerCollection(() => new UniversalDataBuss.DataObjects.Core.DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>(uxmlContainers));

			return uxmlShipment;
		}

		ForwardingConsol CreateConsol(ZString containerGrossWeightVerificationStatus)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.AgentConsol;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var mainTransport = consol.Transports[0];
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_FK;
			mainTransport.JW_VoyageFlight = "F9999";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;

			Factory.Save();

			return consol;
		}

		sealed class MetaDataProviderForTest : IMetaDataProvider
		{
			public MetaDataProviderForTest(ForwardingConsol consol)
			{
				this.consol = consol;
			}

			readonly ForwardingConsol consol;

			object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
			{
				if (metaDataType == MetaDataType.Identifier
					&& dynamicData.Value is UniversalDataBuss.DataObjects.Universal.Container container)
				{
					var containerBizObj = consol
						.Containers
						.OfType<ForwardingContainer>()
						.FirstOrDefault(c => string.CompareOrdinal(c.JC_ContainerNum, container.ContainerNumber) == 0);

					if (containerBizObj != null)
					{
						return containerBizObj.PK;
					}
				}

				return null;
			}
		}

		#endregion
	}
}
