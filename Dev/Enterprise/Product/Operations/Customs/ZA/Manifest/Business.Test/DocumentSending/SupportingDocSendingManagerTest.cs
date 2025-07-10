using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class SupportingDocSendingManagerTests : TestCaseWithFactory
	{
		[CargoWise.Data.Testing.UseSnapshotProtection]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSupportingDocSendingManagerConcurrencyException_CloseStream()
		{
			using (Factory.AddDisposableService())
			using (RunNonTransactioned())
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifest.AMA_RN_NKCountry = CountryCodes.SouthAfrica;

				var imageBytes = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\MessageManagers\DocumentSending\TestDocs\Blank.pdf");
				var eDoc = manifest.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
				var eDoc2 = manifest.DocManagerInfo.AddFileOrDocument(imageBytes, "Worksheet.pdf", "WSH");

				CombineAssertions("Successful Send", () =>
				{
					var objectParent = new ManifestSupportingDocSendingObjectParent(manifest);
					var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject1.EDoc = eDoc.UniqueKey;
					sendingObject1.CaseNumber = "1234567";
					var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject2.EDoc = eDoc2.UniqueKey;
					sendingObject2.CaseNumber = "1234567";
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManager(objectParent, notification);
					notification.NextAnswer = true;
					// This will also resolve the concurrency issue.
					sendingManager.SendMessages();

					AssertNoExceptionThrown("Should not get a 'Stream Closed' inner exception.", () =>
					{
						Factory.Save();
					});
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendMessages()
		{
			using (Factory.AddDisposableService())
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var imageBytes = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\MessageManagers\DocumentSending\TestDocs\Blank.pdf");
				var eDoc = manifest.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
				var eDoc2 = manifest.DocManagerInfo.AddFileOrDocument(imageBytes, "Worksheet.pdf", "WSH");
				Factory.Save();

				CombineAssertions("Successful Send", () =>
				{
					var objectParent = new ManifestSupportingDocSendingObjectParent(manifest);
					var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject1.EDoc = eDoc.UniqueKey;
					sendingObject1.CaseNumber = "1234567";
					var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject2.EDoc = eDoc2.UniqueKey;
					sendingObject2.CaseNumber = "1234567";
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManager(objectParent, notification);
					notification.NextAnswer = true;
					sendingManager.SendMessages();
					AssertEquals(@"Documents queued for sending: Invoice.pdf, Worksheet.pdf", notification.LastMessage);
					AssertEquals("Supporting Document Sending Result", notification.LastCaption);
				});
			}
		}
	}
}
