using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class VGMMessageEventProcessorTest : TestCaseWithFactory
	{
		#region TestOnMessageSent

		const string verifiedGrossContainerWeight = "Verified Gross Container Weight";

		public void TestOnMessageSent_MSNEmailAction()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			shippingLine.RSL_IsNVO = false;

			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsSeaWholesaler = false;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			Factory.Save();

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";

			var @event = Events.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has MSN event", msn);

			var documentDataSource = CreateVGM(consol);
			var dynamicData = CreateDynamicData(documentDataSource);
			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(VerifiedGrossMass.Containers));

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.VerifiedGrossMass);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull(msn);
			AssertEquals("|ACT=Email|CMP=9001|DEP=Carrier|MST=Verified Gross Container Weight", msn.SL_Reference);
		}

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

			var documentDataSource = CreateVGM(consol);
			var dynamicData = CreateDynamicData(documentDataSource);
			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(VerifiedGrossMass.Containers));

			var dynamicVerifiedStatuses = new List<IDynamicData>();

			foreach (var dynamicContainer in dynamicDataContainers)
			{
				var status = dynamicContainer.GetDynamicProperty("VerifiedStatus.Code");
				AssertNotNull("VerifiedStatus.Code was found on container", status);
				dynamicVerifiedStatuses.Add(status);
			}

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.VerifiedGrossMass);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMSN = containerBizObj.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MSN event found on container", containerMSN);
				AssertEquals("consol MSN event SL_IsEstimate matches container MSN SL_IsEstimate",
					msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
				AssertEquals("consol MSN event SL_EventTime matches container MSN SL_EventTime",
					msn.SL_EventTime, containerMSN.SL_EventTime);
				AssertEquals("consol MSN event SL_Reference matches container MSN SL_Reference",
					msn.SL_Reference, containerMSN.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated", Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, containerBizObj.JC_GrossWeightVerificationStatus);
			}

			AssertEquals("dynamicdata is marked as not having changes",
				false, dynamicData.HasChanges);
			AssertEquals("dynamicdata is marked as not overridden",
				false, dynamicData.IsOverriddenIncludingChildren);
		}

		[TestDate(2019, 5, 1)]
		public void TestOnMessageSent_ContainersEvents()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);
			var @event = AutoEvents.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now.AddDays(-1),
				$"Obsolete MSN|MST={verifiedGrossContainerWeight}");

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"Expected MSN|MST={verifiedGrossContainerWeight}");

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now.AddDays(1),
				$"Newest but with-wrong-MST MSN|MST=What Ever");

			var documentDataSource = CreateVGM(consol);
			var dynamicData = CreateDynamicData(documentDataSource);
			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(VerifiedGrossMass.Containers));

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.VerifiedGrossMass);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMSN = containerBizObj.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MSN event found on container.", containerMSN);
				Assert("Container MSN SL_IsEstimate is set as expected.", containerMSN.SL_IsEstimate);
				AssertEquals("Container MSN SL_EventTime is set as expected.",
					ZDateTime.Now, containerMSN.SL_EventTime);
				AssertEquals("Container MSN SL_Reference is set as expected.",
					$"Expected MSN|MST={verifiedGrossContainerWeight}", containerMSN.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated.", Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, containerBizObj.JC_GrossWeightVerificationStatus);
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

			var documentDataSource = CreateVGM(consol);
			var dynamicData = CreateDynamicData(documentDataSource);
			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(VerifiedGrossMass.Containers));

			var verifiedStatuses = new List<IDynamicData>();

			foreach (var dynamicContainer in dynamicDataContainers)
			{
				var status = dynamicContainer.GetDynamicProperty("VerifiedStatus.Code");
				AssertNotNull("VerifiedStatus.Code was found on container", status);
				verifiedStatuses.Add(status);
			}

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.VerifiedGrossMass);
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

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated", Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent, containerBizObj.JC_GrossWeightVerificationStatus);
			}

			AssertEquals("dynamicdata is marked as not having changes",
				false, dynamicData.HasChanges);
			AssertEquals("dynamicdata is marked as not overridden",
				false, dynamicData.IsOverriddenIncludingChildren);
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

			var documentDataSource = CreateVGM(consol);
			var dynamicData = CreateDynamicData(documentDataSource);
			var dynamicDataContainers = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(VerifiedGrossMass.Containers));

			var dynamicVerifiedStatuses = new List<IDynamicData>();

			foreach (var dynamicContainer in dynamicDataContainers)
			{
				var status = dynamicContainer.GetDynamicProperty("VerifiedStatus.Code");
				AssertNotNull("VerifiedStatus.Code was found on container", status);
				dynamicVerifiedStatuses.Add(status);
			}

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.VerifiedGrossMass);
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

			AssertEquals("dynamicdata is marked as not having changes",
				false, dynamicData.HasChanges);
			AssertEquals("dynamicdata is marked as not overridden",
				false, dynamicData.IsOverriddenIncludingChildren);
		}

		#endregion

		#region Implementation

		IDynamicData CreateDynamicData(VerifiedGrossMass vgm)
		{
			var metaDataProvider = new MetaDataProviderForTest();
			var factory = new DocumentVisualizer.DocDataObjects.DocDataObjectDynamicDataFactory();
			var dynamicData = vgm.MakeDynamic(metaDataProvider, dynamicDataFactory: factory);
			return dynamicData;
		}

		VerifiedGrossMass CreateVGM(ForwardingConsol consol)
		{
			var containers = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();

			var parameters = new DummyDocDataObjectParameters
			{
				Data = containers
			};

			var builder = new VGMBuilder(consol, parameters);
			return builder.Build();
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
			object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
			{
				if (metaDataType == MetaDataType.Identifier
					&& dynamicData.Value is Container container)
				{
					return container.Identifier;
				}

				return null;
			}
		}

		#endregion
	}
}
