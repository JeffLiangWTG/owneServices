using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.ZArchitecture.Schema;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DE.ALPO
{
	sealed class AdvancedLogisticsPortOrderResetToOriginalTest : StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			AssertNull(null);
		}

		public void TestRecepient()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C00001298";
			consol.JK_MasterBillNum = "SYX0000001";
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "DEANR";

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

			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Services).Returns(services);

			var pivotPK = new ZGuid("A70B966D-3A74-44DE-99AA-CDFC73311B6B");
			var zDBOnlyQuery = new ZDBOnlyQuery(typeof(VisualizerMenuTemplatePivot));
			zDBOnlyQuery.AddToFilter(StmMenuTemplatePivotSchema.PK, pivotPK);
			var pivot = consol.Factory.Load<VisualizerMenuTemplatePivot>(zDBOnlyQuery).Single();
			pivot.DocType.RT_LogSystemCreatedDocsToEDocs = false;

			var descriptor = CreateDocumentDescriptor(consol, pivot);
			var expectedRecipient = "Terminal";

			AssertEquals(expectedRecipient, descriptor.MessageInstructions.Recipient);

			documentInfo.Setup(di => di.Descriptor).Returns(descriptor);
			document.SetupGet(di => di.Data).Returns(dynamicData.Object);
			document.SetupGet(di => di.Name).Returns(GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder);
			var command = new ResetToOriginalCommand();
			command.Invoke(null, documentInfo.Object);

			var expectedMsg = $@"WARNING: Using this option without checking with the {expectedRecipient} first might result in duplicate messages being processed by the {expectedRecipient}.
Resetting to Original should only be required when there is a serious messaging failure at the {expectedRecipient}'s end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the {expectedRecipient} to make sure they have not already processed the message.";
			var expectedCaption = "Warning";
			var expectedPrompt = "If you have done so, please type the following to confirm:";
			var expectedConformationStr = $"I have confirmed with the {expectedRecipient} that they did not process the Original message already sent.";

			notificationService.Verify(nf => nf.ShowConfirmation(expectedMsg, expectedCaption, expectedPrompt, expectedConformationStr), Times.Once);
		}
	}
}
