using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor.Test
{
	[TestFixture]
	public class EHubMessageBuilderFixture
	{
		[Test]
		public void GenericMessageBuilder()
		{
			var message = string.Format(CultureInfo.InvariantCulture, MessageHelper.GetMessageLayout, MessageHelper.GetDataSetLayout("Testing", "TstTable", new DateTime(2020, 8, 25)));

			var eHubSubscriberConfiguration = new EHubMessageBuilder(message, "TST");
			var eHubMessage = eHubSubscriberConfiguration.Build();

			Assert.That(eHubMessage.ApplicationCode, Is.EqualTo("GMD"));
			Assert.That(eHubMessage.EmailSubject, Is.EqualTo(""));
			Assert.That(eHubMessage.Filename, Is.EqualTo(""));
			Assert.That(eHubMessage.RecipientID, Is.EqualTo("TST"));
			Assert.That(eHubMessage.SchemaName, Is.EqualTo(@"http://cargowise.com/ehub/core/genericmessagedelivery#GenericMessageInterchange"));
			Assert.That(eHubMessage.SchemaType, Is.EqualTo(MessageSchemaType.Xml));
			Assert.That(eHubMessage.SenderID, Is.EqualTo("RefDbRepoDataPublishing"));
			Assert.That(eHubMessage.TrackingID, Is.TypeOf<Guid>());

			var messageXDoc = XDocument.Load(eHubMessage.MessageStream);
			Assert.That(messageXDoc.ToString(), Does.Contain($"<InterchangeType>{ApplicationConfig.Messaging.InterchangeType}</InterchangeType>"));
			Assert.That(messageXDoc.ToString(), Does.Contain($"<SenderID>{ApplicationConfig.Messaging.From}</SenderID>"));

			Assert.That(messageXDoc.ToString(), Does.Contain($@"<RefDbRepoMessage>
      <DataSets>
        <DataSet>
          <DataSetId>Testing</DataSetId>
          <TimeStamp>2020-08-25T00:00:00</TimeStamp>
          <TableName>TstTable</TableName>
        </DataSet>
      </DataSets>
    </RefDbRepoMessage>"));
		}
	}
}
