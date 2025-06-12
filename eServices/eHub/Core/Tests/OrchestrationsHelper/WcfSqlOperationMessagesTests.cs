using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.OrchestrationsHelper
{
	[TestClass]
	public class WcfSqlOperationMessagesTests : BaseComponentTest
	{
		[TestMethod]
		public void TestGetWcfSqlInsertInboxWcfDefault()
		{
			var guids = new Queue<Guid>(new[]
			{
				new Guid("00000000-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222")
			});
			var senderId = "ACAS_BR_FHL";
			var recipientId = "WTLEDINPN";
			var messageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
			var applicationCode = "UDM";
			var content = "INBOX CONTENT";

			WcfSqlOperationMessages.InternalNewGuid = guids.Dequeue;
			WcfSqlOperationMessages.GetCurrentUtcTime = () => new DateTime(2019, 11, 15);

			var wcfMessageXml = WcfSqlOperationMessages.CombineWcfSqlOps(
				WcfSqlOperationMessages.GetWcfSqlInsertInbox(senderId, recipientId, messageType, applicationCode,
					content));
			AssertXmlStream(GetEmbeddedResource("OrchestrationsHelper.TestFiles.Test_WCFMessageGenerator_Default.xml"), new MemoryStream(Encoding.UTF8.GetBytes(wcfMessageXml.OuterXml)));
		}

		[TestMethod]
		public void TestGetWcfSqlInsertInboxWcf()
		{
			var senderId = "ACAS_BR_FHL";
			var recipientId = "WTLEDINPN";
			var messageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
			var applicationCode = "UDM";
			var content = "INBOX CONTENT";
			var inboxPk = new Guid("00000000-0000-0000-0000-000000000000");
			var trackingId = new Guid("11111111-1111-1111-1111-111111111111");
			var envelopTrackingId = new Guid("22222222-2222-2222-2222-222222222222");
			var isFlatFile = true;
			var emailSubject = "Email";
			var fileName = "File";
			var status = 255;
			var dateTime = new DateTime(2019, 11, 15);

			var wcfMessageXml = 
				WcfSqlOperationMessages.CombineWcfSqlOps(
						WcfSqlOperationMessages.GetWcfSqlInsertInbox(senderId, recipientId, messageType, applicationCode, content, trackingId, inboxPk, envelopTrackingId, isFlatFile, emailSubject, fileName, dateTime, status));
			AssertXmlStream(GetEmbeddedResource("OrchestrationsHelper.TestFiles.Test_WCFMessageGenerator_Customs.xml"), new MemoryStream(Encoding.UTF8.GetBytes(wcfMessageXml.OuterXml)));
		}

		[TestMethod]
		public void TestGetWcfSqlInsertOutboxWcfDefault()
		{
			var guids = new Queue<Guid>(new[]
			{
				new Guid("00000000-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222")
			});
			var senderId = "ACAS_BR_FHL";
			var recipientId = "WTLEDINPN";
			var messageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
			var content = "<Test/>";
			var inboxPK = new Guid("33333333-3333-3333-3333-333333333333");

			WcfSqlOperationMessages.InternalNewGuid = guids.Dequeue;
			WcfSqlOperationMessages.GetCurrentUtcTime = () => new DateTime(2019, 11, 15);

			var wcfMessageXml = WcfSqlOperationMessages.CombineWcfSqlOps(
				WcfSqlOperationMessages.GetWcfSqlInsertOutbox(senderId, recipientId, messageType, content, inboxPK));
			AssertXmlStream(GetEmbeddedResource("OrchestrationsHelper.TestFiles.Test_WCFMessageGenerator_Outbox_Default.xml"), new MemoryStream(Encoding.UTF8.GetBytes(wcfMessageXml.OuterXml)));
		}

		[TestMethod]
		public void TestGetWcfSqlInsertOutboxWcf()
		{
			var senderId = "ACAS_BR_FHL";
			var recipientId = "WTLEDINPN";
			var messageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
			var content = "<Test/>";
			var inboxPk = new Guid("00000000-0000-0000-0000-000000000000");
			var trackingId = new Guid("11111111-1111-1111-1111-111111111111");
			var envelopTrackingId = new Guid("22222222-2222-2222-2222-222222222222");
			var outboxPk = new Guid("33333333-3333-3333-3333-333333333333");
			var emailSubject = "Email";
			var fileName = "File";
			var status = 255;
			var dateTime = new DateTime(2019, 11, 15);

			var wcfMessageXml =
				WcfSqlOperationMessages.CombineWcfSqlOps(
					WcfSqlOperationMessages.GetWcfSqlInsertOutbox(senderId, recipientId, messageType, content, trackingId, inboxPk, outboxPk, envelopTrackingId, emailSubject, fileName, null, dateTime, status));
			AssertXmlStream(GetEmbeddedResource("OrchestrationsHelper.TestFiles.Test_WCFMessageGenerator_Outbox_Customs.xml"), new MemoryStream(Encoding.UTF8.GetBytes(wcfMessageXml.OuterXml)));
		}

		[TestMethod]
		public void TestGetWcfSqlCompositeOperationsMultipleOperations()
		{
			var guids = new Queue<Guid>(new[]
			{
				new Guid("00000000-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"),
				new Guid("44444444-4444-4444-4444-444444444444"), new Guid("55555555-5555-5555-5555-555555555555")
			});
			var senderId = "ACAS_BR_FHL";
			var recipientId = "WTLEDINPN";
			var messageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
			var applicationCode = "UDM";
			var content = "<Test/>";
			

			WcfSqlOperationMessages.InternalNewGuid = guids.Dequeue;
			WcfSqlOperationMessages.GetCurrentUtcTime = () => new DateTime(2019, 11, 15);

			var inboxPk = WcfSqlOperationMessages.InternalNewGuid();
			var trackingPk = WcfSqlOperationMessages.InternalNewGuid();

			var wcfMessageXml = WcfSqlOperationMessages.CombineWcfSqlOps(
				WcfSqlOperationMessages.GetWcfSqlInsertInbox(senderId, recipientId, messageType, applicationCode, content, trackingPk, inboxPk),
				WcfSqlOperationMessages.GetWcfSqlInsertOutbox(senderId, recipientId, messageType, content, inboxPk, trackingPk));

			AssertXmlStream(GetEmbeddedResource("OrchestrationsHelper.TestFiles.Test_WCFMessageGenerator_MultipleOperations.xml"), new MemoryStream(Encoding.UTF8.GetBytes(wcfMessageXml.OuterXml)));
		}
	}
}