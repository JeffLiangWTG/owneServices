using System.IO;
using System.Text;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.BizTalk.PipelineComponents;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests.CPOINT.PipelineComponents
{
	[TestClass]
	public class PromotePollingDataTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void PromotePollingData_Execute()
		{
			var messageFactory = new MessageFactory();
			var message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("<TypedPollingResultSet0 xmlns='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/ConnectionDetails'><MessageType>ccc</MessageType><UserName>ABC</UserName><Password>SSSSS</Password><Checkpoint>1234</Checkpoint><IsProd>0</IsProd></TypedPollingResultSet0>"));
			var pipelineContext = new PipelineContext();

			var component = new PromotePollingData();
			var outMsg = component.Execute(pipelineContext, message);

			Assert.AreEqual("0", message.Context.ReadPropertyString<BizTalk.PropertySchemas.IsProd>());
			Assert.AreEqual("ccc", message.Context.ReadPropertyString<BizTalk.PropertySchemas.MessageType>());
			Assert.AreEqual("ABC", message.Context.ReadPropertyString<BizTalk.PropertySchemas.UserName>());
			Assert.AreEqual("FPM_BE_CPOINT_POLLING_ABC", message.Context.Read("TriggerMutex", "http://cargowise.com/ehub/core/mutex").ToString());
			Assert.AreEqual(0, outMsg.BodyPart.GetOriginalDataStream().Position);
		}
	}
}
