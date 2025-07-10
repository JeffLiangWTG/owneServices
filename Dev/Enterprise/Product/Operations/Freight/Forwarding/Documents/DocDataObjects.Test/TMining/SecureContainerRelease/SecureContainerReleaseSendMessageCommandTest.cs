using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using TMiningConstants = Enterprise.Freight.Forwarding.Business.TMiningConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class SecureContainerReleaseSendMessageCommandTest : TestCaseWithFactory
	{
		public void TestSendMessage()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var secureContainerRelease = CreateSecureContainerReleaseDocDataObject(SecureContainerRelease.FormModeTransfer);
			var documentInfo = CreateDocumentInfoForSecureContainerRelease(secureContainerRelease, documentData);

			var command = new SecureContainerReleaseSendMessageCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize SecureContainerReleaseSendMessageCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert("expected command to execute successfully", command.Invoke(commandParameters, documentInfo));

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("message was created", message);
			AssertContains("message should have ORG purpose", "<Purpose>ORG</Purpose>", message.EM_MessageText);
		}

		public void TestSendWithdrawal()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			var documentData = CreateDocumentDataAttachedToConsol();
			Factory.Save();

			var secureContainerRelease = CreateSecureContainerReleaseDocDataObject(SecureContainerRelease.FormModeRevoke);
			var documentInfo = CreateDocumentInfoForSecureContainerRelease(secureContainerRelease, documentData);

			var command = new SecureContainerReleaseSendMessageCommand();

			if (!InitializeCommand(command, documentInfo))
			{
				Fail("Failed to initialize SecureContainerReleaseSendMessageCommand");
			}

			var commandParameters = new MacroMap(new Dictionary<string, object>());
			Assert("expected command to execute successfully", command.Invoke(commandParameters, documentInfo));

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertNotNull("message was created", message);
			AssertContains("message should have WTH purpose", "<Purpose>WTH</Purpose>", message.EM_MessageText);
		}

		// Creates Forwarding Consol from which the message is initiated and document related storage
		VisualizerDocumentData CreateDocumentDataAttachedToConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var documentData = Factory.New<VisualizerDocumentData>();

			documentData.Parent = consol;
			documentData.JDD_Name = DataContext.TMiningSecureContainerRelease;

			return documentData;
		}

		// Create document data
		SecureContainerRelease CreateSecureContainerReleaseDocDataObject(string formMode)
		{
			var context = new CommonContext(Factory);

			var secureContainerRelease = new SecureContainerRelease("ForwardingConsol", "C20210512");
			secureContainerRelease.FormMode = formMode;
			secureContainerRelease.ContainerMode = new DummyCodeDescription() { Code = Core.Constants.ContainerModes.FCL, Description = "Full Container Load" };
			secureContainerRelease.BillOfLading = "1122334499";

			secureContainerRelease.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};

			if (formMode == SecureContainerRelease.FormModeRevoke)
			{
				secureContainerRelease.Containers = new[]
				{
					CreateContainerTestData("MSCU1247858",
						"REL210426_3",
						formMode,
						TMiningConstants.SecureContainerReleaseStatus.TransferSent,
						true,
						"no reason for revoke."),

					CreateContainerTestData("MSCU1247859",
						"REL210426_4",
						formMode,
						TMiningConstants.SecureContainerReleaseStatus.TransferSent,
						false,
						"no reason for revoke.")
				};
			}
			else
			{
				secureContainerRelease.Containers = new[]
				{
					CreateContainerTestData("MSCU1247856",
						"REL210426_1",
						formMode,
						TMiningConstants.SecureContainerReleaseStatus.Assigned,
						true,
						"no reason for accept."),

					CreateContainerTestData("MSCU1247857",
						"REL210426_2",
						formMode,
						TMiningConstants.SecureContainerReleaseStatus.Assigned,
						false,
						"no reason for decline."),
				};
			}

			return secureContainerRelease;
		}

		// Create document data containers
		SecureContainerReleaseContainer CreateContainerTestData(ZString containerNumber, ZString releaseIdentification, string formMode, ZString currentStatus, bool isTranferToForwarder, ZString reason)
		{
			var container = new SecureContainerReleaseContainer(DefaultDataObjectWriterStrategy.TestInstance, formMode);

			container.Number = containerNumber;
			container.ReleaseIdentification = releaseIdentification;
			container.CurrentStatus = currentStatus;
			container.IsTranferToForwarder = isTranferToForwarder;
			container.IsNonOperativeReefer = false;

			return container;
		}

		// Create document and all other related objects needed for sending
		IDocumentInfo CreateDocumentInfoForSecureContainerRelease(SecureContainerRelease secureContainerRelease, VisualizerDocumentData documentData)
		{
			var mockData = new Mock<IDynamicData>();
			mockData.SetupGet(d => d.Value).Returns(secureContainerRelease);

			var mockDocument = new Mock<IDocument>();
			mockDocument.SetupGet(d => d.Data).Returns(mockData.Object);
			mockDocument.SetupGet(d => d.DataContext).Returns(DataContext.TMiningSecureContainerRelease);

			var mockMessageInstructions = new Mock<IMessageInstructions>();

			var mockEDocsInstructions = new Mock<IEDocsInstructions>();
			mockEDocsInstructions.SetupGet(i => i.SaveCopyToEDocs).Returns(false);

			var mockDocumentDescriptor = new Mock<IDocumentDescriptor>();
			mockDocumentDescriptor.SetupGet(d => d.MessageInstructions).Returns(mockMessageInstructions.Object);
			mockDocumentDescriptor.SetupGet(d => d.EDocsInstructions).Returns(mockEDocsInstructions.Object);

			var mockSecurityService = new Mock<IDocumentSecurityService>();
			mockSecurityService.SetupGet(s => s.CanSendMessage).Returns(true);

			var mockDeliveryService = new Mock<IDocumentDeliveryService>();
			mockDeliveryService
				.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.IsAny<INotifications>()))
				.Returns(CreateEdiCommunicationSettings());

			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IDocumentSecurityService>(mockSecurityService.Object);
			services.Register<IDocumentDeliveryService>(mockDeliveryService.Object);

			var mockDocumentInfo = new Mock<IDocumentInfo>();
			mockDocumentInfo.SetupGet(d => d.Descriptor).Returns(mockDocumentDescriptor.Object);
			mockDocumentInfo.SetupGet(d => d.Document).Returns(mockDocument.Object);
			mockDocumentInfo.SetupGet(d => d.DocumentData).Returns(documentData);
			mockDocumentInfo.SetupGet(d => d.Services).Returns(services);

			return mockDocumentInfo.Object;
		}

		IEDICommunicationSettings CreateEdiCommunicationSettings()
		{
			var mockEDICommunicationsMode = new Mock<IEDICommunicationsMode>();
			mockEDICommunicationsMode.SetupGet(s => s.EK_CommunicationsTransport).Returns("HUB");
			mockEDICommunicationsMode.SetupGet(s => s.EK_Destination).Returns("Valhalla");

			var mockEDICommunicationSettings = new Mock<IEDICommunicationSettings>();
			mockEDICommunicationSettings
				.SetupGet(s => s.Recipient)
				.Returns(new ZArchitecture.Core.CodeDescriptionPair("foor", "baar"));

			mockEDICommunicationSettings
				.SetupGet(s => s.Purpose)
				.Returns(new ZArchitecture.Core.CodeDescriptionPair("for", "bar"));

			mockEDICommunicationSettings
				.SetupGet(s => s.CommunicationsModes)
				.Returns(new [] { mockEDICommunicationsMode.Object });

			return mockEDICommunicationSettings.Object;
		}

		// Each command need to be initiated before usage
		bool InitializeCommand(SecureContainerReleaseSendMessageCommand command, IDocumentInfo documentInfo)
		{
			if (command is INotifiableDocumentInfoCreated notifiableDocumentInfoCreated)
			{
				notifiableDocumentInfoCreated.NotifyDocumentInfoCreated(documentInfo);
				return true;
			}

			return false;
		}
	}
}
