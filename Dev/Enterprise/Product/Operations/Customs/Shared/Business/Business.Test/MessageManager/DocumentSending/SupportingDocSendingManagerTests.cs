using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class SupportingDocSendingManagerTests : TestCaseWithFactory
	{
		public void TestAvoidNullReferenceException_NotifyQueuedForSending()
		{
			using (Factory.AddDisposableService())
			{
				entryHeader.MovementReferenceNumberSetter("1234", ZDateTime.UtcNow);
				var objectParent = new JobDeclarationSupportingDocSendingObjectParent(declaration);
				var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
				sendingObject1.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
				var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
				sendingObject2.EDoc = eDoc2.UniqueKey;
				sendingObject2.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
				var notification = new MessageNotificationCollector_ForTest();
				var sendingManager = new SupportingDocSendingManagerForTest(objectParent, notification);
				notification.NextAnswer = true;
				AssertNoExceptionThrown(() =>
				{
					sendingManager.SendMessages();
				});
			}
		}

		public void TestSendMessages()
		{
			using (Factory.AddDisposableService())
			{
				CombineAssertions("Successful Send", () =>
				{
					entryHeader.MovementReferenceNumberSetter("1234", ZDateTime.UtcNow);
					var objectParent = new JobDeclarationSupportingDocSendingObjectParent(declaration);
					var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject1.EDoc = eDoc.UniqueKey;
					sendingObject1.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
					var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject2.EDoc = eDoc2.UniqueKey;
					sendingObject2.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManagerForTest(objectParent, notification);
					notification.NextAnswer = true;
					sendingManager.SendMessages();
					AssertEquals(@"Documents queued for sending: Invoice.pdf, Worksheet.pdf", notification.LastMessage);
					AssertEquals("Supporting Document Sending Result", notification.LastCaption);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();

			var imageBytes = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\MessageManagers\DocumentSending\TestDocs\Blank.pdf");
			eDoc = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
			eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Worksheet.pdf", "WSH");

			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("12341234", ZDateTime.UtcNow);

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		BaseJobDeclaration declaration;
		CusEntryHeader entryHeader;
		IeDoc eDoc;
		IeDoc eDoc2;
	}
}
