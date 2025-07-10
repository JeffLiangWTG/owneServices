using System.IO;
using CargoWise.Definitions;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Newtonsoft.Json;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class DelayAlertDocumentDeliveryJobTest : AutoDocumentDeliveryJobTest
	{
		public const string ImportDelayAlert = "Delay Alert";
		public const string ExportDelayAlert = "Export Delay Alert";

		public void TestHasRecipients()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;
			AssertEquals(false, ((DelayAlertDocumentDeliveryJob)DocumentDelivery).HasRecipients);

			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			Recipient.OC_Fax = "5555 5555";
			AssertEquals(true, ((DelayAlertDocumentDeliveryJob)DocumentDelivery).HasRecipients);
		}

		public void TestCanSerializeDeserializeAndDeliver()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";
			Factory.Save();

			DelayAlertDocumentDeliveryJob jobToSerialize = (DelayAlertDocumentDeliveryJob)DocumentDelivery;
			var jsonSerializer = new JsonSerializer();

			byte[] blob;
			using (MemoryStream serializeStream = new MemoryStream())
			{
				var jsonWriter = new JsonTextWriter(new StreamWriter(serializeStream));
				jsonSerializer.Serialize(jsonWriter, jobToSerialize);
				jsonWriter.Flush();

				serializeStream.Seek(0, SeekOrigin.Begin);
				blob = serializeStream.ToArray();
			}

			using (Stream deSerializeStream = new MemoryStream(blob))
			{
				var jsonReader = new JsonTextReader(new StreamReader(deSerializeStream));
				DelayAlertDocumentDeliveryJob deSerializedJob = jsonSerializer.Deserialize<DelayAlertDocumentDeliveryJob>(jsonReader);

				NotificationBuffer notifications = new NotificationBuffer();
				deSerializedJob.Deliver(notifications);
				AssertDocumentDelivered("clinton@edi.com.au", Core.Constants.ContactNotifyModes.Email);
			}
		}

		#region Implementation

		protected sealed override AutoDocumentDeliveryJob NewDocumentDeliveryJob(bool sendToDocManager)
		{
			return new DelayAlertDocumentDeliveryJob(BusinessObjectToDeliver, IsExportDA);
		}

		protected sealed override DocumentCommand DocumentCommand
		{
			get { return LoadDocumentCommand(Context, IsExportDA ? ExportDelayAlert : ImportDelayAlert); }
		}

		protected abstract bool IsExportDA { get; }
		protected abstract BusinessContext Context { get; }

		#endregion
	}
}
