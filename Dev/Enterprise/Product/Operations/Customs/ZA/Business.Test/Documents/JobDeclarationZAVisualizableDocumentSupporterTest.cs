using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Customs.ZA.Business.DeclarationDocumentConstants;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects.Testing
{
	sealed class JobDeclarationZAVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = new JobDeclarationZAVisualizableDocumentSupporter(declaration);

			AssertEquals("CustomizeFormCheckpoint", Env.Security.MaintainJobDeclarationCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetDocDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = new JobDeclarationZAVisualizableDocumentSupporter(declaration);
			var docDataObject = supporter.GetDocDataObject(declaration, DataContext.CargoDuesBrokerage, null);
			AssertNotNull(docDataObject.Right);
			AssertType(typeof(CargoDues), docDataObject.Right);
		}

		public void TestGetUniversalXmlDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var builder = new CargoDuesBrokerageDocDataBuilder(declaration, new DocDataObjectParameters(DeclarationDocumentConstants.DocumentNames.LoadCoastwise, "data-store"));
			var cargoDues = builder.Build();
			var supporter = new JobDeclarationZAVisualizableDocumentSupporter(declaration);

			var data = cargoDues.MakeDynamic();
			var document = new Mock<DocumentVisualizer.Core.IDocument>();
			document.Setup(d => d.Data).Returns(data);
			document.Setup(d => d.DataContext).Returns(DataContext.CargoDuesBrokerage);

			var docDataObject = supporter.GetUniversalXmlDataObject(DefaultDataObjectWriterStrategy.TestInstance, document.Object, MessageType.Unspecified);

			AssertNotNull(docDataObject.Right);
			AssertType<UniversalDataBuss.DataObjects.Universal.Shipment>(docDataObject.Right);
		}

		public void TestGetEventParent_Export()
		{
			AssertGetEventParent("Cargo Dues - Export", "Cargo Dues Brokerage - Export");
		}

		public void TestGetEventParent_Import()
		{
			AssertGetEventParent("Cargo Dues - Import", "Cargo Dues Brokerage - Import");
		}

		public void TestContinueWithSendingMessageAmendment_IsAwaitingResponse()
		{
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(DocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(DocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport, true);
		}

		public void TestContinueWithSendingMessageWithdrawal_IsAwaitingResponse()
		{
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(DocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(DocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport, false);
		}

		public void TestContinueWithResetToOriginal_IsTNPAOrderNumberNotEmpty()
		{
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(DocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(DocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport);
		}

		void AssertGetEventParent(string documentName, string documentDataStoreName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00001119";

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.Constants.JDD_ParentID] = declaration.PK;
			documentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = "JE";
			documentData[JobDocumentDataSchema.Constants.JDD_Name] = documentDataStoreName;

			Factory.Save();

			string incomingEvent = $@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>{documentName}</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001119</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-01-15T14:42:38</EventTime>
		<EventType>ISN</EventType>
		<EventParameters>
		    <Department>WiseTechGlobal</Department>
		    <MessageType>{documentName}</MessageType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>MessageReference</Type>
				<Value>BCPTDZASYDAUSYDHYEDCPTZACPT00002600</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);

			var supporter = new JobDeclarationZAVisualizableDocumentSupporter(declaration);
			AssertEquals(documentData.PK, ((BusinessObject)supporter.GetEventParent(xmlEvent)).PK);
		}

		void AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(string dataStoreName, string documentName, bool isMessageAmendment)
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentData = PrepareDocumentData(declaration, dataStoreName);
			var document = new Mock<DocumentVisualizer.Core.IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var cargoDuesBrokerage = new CargoDues(nameof(DataContextType.CustomsDeclaration), declaration.JE_DeclarationReference, DataContext.CargoDuesBrokerage)
			{
				IsQuotationDocument = false,
				TNPAOrderNumber = ZString.Empty
			};

			document.Setup(d => d.Data.Value).Returns(cargoDuesBrokerage);
			document.Setup(d => d.Name).Returns(documentName);

			var parameters = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			CreateEvent(documentData, Events.MessageSent, new ZDateTime(2022, 04, 01), parameters);

			var extensions = new CargoDuesMessagingExtensions(document.Object, declaration);

			var message = "A message has previously been sent and is awaiting reply from TNPA. Once TNPA reply, the TNPA Order Number will be populated here and then you can send further messages - either resend (as an amendment) or Withdraw/Cancel.";
			var comfirmation = "Confirmation";

			AssertEquals(false, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);

			CreateEvent(documentData, Events.MessageAccepted, new ZDateTime(2022, 04, 02), parameters);
			CreateEvent(documentData, Events.MessageWithdrawCancelRequest, new ZDateTime(2022, 04, 03), parameters);

			notifications = new Mock<IUserNotifications>();

			AssertEquals(false, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);
		}

		void AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(string dataStoreName, string documentName)
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentData = PrepareDocumentData(declaration, dataStoreName);
			var document = new Mock<DocumentVisualizer.Core.IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var cargoDuesBrokerage = new CargoDues(nameof(DataContextType.CustomsDeclaration), declaration.JE_DeclarationReference, DataContext.CargoDuesBrokerage)
			{
				IsQuotationDocument = false,
				TNPAOrderNumber = ZString.Empty
			};

			document.Setup(d => d.Data.Value).Returns(cargoDuesBrokerage);
			document.Setup(d => d.Name).Returns(documentName);

			CreateEvent(documentData, Events.MessageSent, new ZDateTime(2022, 04, 15), new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			var extensions = new CargoDuesMessagingExtensions(document.Object, declaration);

			var message = "This function is not possible because TNPA have already responded to your original message and provided their Order No.";
			var comfirmation = "Confirmation";

			AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);

			notifications = new Mock<IUserNotifications>();

			cargoDuesBrokerage.TNPAOrderNumber = "TNPANO";

			AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);
		}

		void CreateEvent(IVisualizerDocumentData documentData, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			(documentData as IStmALogParent).Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);

			Factory.Save();
			Thread.Sleep(1);
		}

		IVisualizerDocumentData PrepareDocumentData(JobDeclaration declaration, string documentDataStoreName)
		{
			var documentData = declaration.Factory.New<VisualizerDocumentData>();

			using (documentData.SuspendSettingHasChanges())
			{
				documentData.Parent = declaration;
				documentData.JDD_Name = documentDataStoreName;
			}

			return documentData;
		}
	}
}
