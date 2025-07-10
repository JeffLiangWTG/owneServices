using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class AdvancedLogisticsPortOrderMessagingHelperTest : TestCaseWithFactory
	{
		public void TestResetToOriginalEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "BEANR";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var alpo = builder.Build();

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder);

			dynamicData.SetupGet(di => di.Value).Returns(alpo);

			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.ResetToOriginalDisabled(consol));

			var eventParams = new List<KeyValuePair<string, string>>();
			eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder));

			documentData.Logs.CreateOrRecreateEventLog(Events.InterchangeSent, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();

			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.ResetToOriginalDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.InterchangeReceiptAcknowledged, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();

			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.ResetToOriginalDisabled(consol));
		}

		public void TestMessageSendEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "BEANR";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var alpo = builder.Build();

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder);

			dynamicData.SetupGet(di => di.Value).Returns(alpo);

			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			var eventParams = new List<KeyValuePair<string, string>>();
			eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder));

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageWithdrawCancelRequest, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.InterchangeReceiptAcknowledged, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();

			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.InterchangeSent, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			var resetToOriginal = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "Reset To Original");
			eventParams.Add(resetToOriginal);
			documentData.Logs.CreateOrRecreateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));
			eventParams.Remove(resetToOriginal);

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			Factory.Save();
			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			documentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParams.ToArray());
			var szbNumber = consol.Numbers.AddNew();
			szbNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			szbNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			szbNumber.CE_EntryNum = string.Empty;
			Factory.Save();
			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));

			szbNumber.CE_EntryNum = "XX123";
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageSendDisabled(consol));
		}

		public void TestMessageWithdrawEnabled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "BEANR";

			var szbNumber = consol.Numbers.AddNew();
			szbNumber.CE_EntryType = "SZB";
			szbNumber.CE_RN_NKCountryCode = "DE";
			szbNumber.CE_EntryNum = "SZB123";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notificationService.Object);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = DocDataObjects.ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var alpo = builder.Build();

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder);

			dynamicData.SetupGet(di => di.Value).Returns(alpo);

			AssertEquals("disabled", false, AdvancedLogisticsPortOrderMessagingHelper.MessageWithdrawDisabled(consol));

			szbNumber.CE_EntryNum = string.Empty;
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageWithdrawDisabled(consol));

			consol.Numbers.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("disabled", true, AdvancedLogisticsPortOrderMessagingHelper.MessageWithdrawDisabled(consol));
		}
	}
}
