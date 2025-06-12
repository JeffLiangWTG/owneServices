using System.IO;
using System.Text;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PipelineComponents;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests.PipelineComponents
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
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("<TypedPollingResultSet0 xmlns='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/SGC_MHAccess_SelectAccountsForPolling'><AccountNo>VWGT002</AccountNo><Password>User4WTG</Password><ProdFlag>0</ProdFlag></TypedPollingResultSet0>"));
			var pipelineContext = new PipelineContext();

			var component = new PromotePollingData();
			var outMsg = component.Execute(pipelineContext, message);

			Assert.AreEqual("VWGT002", message.Context.ReadPropertyString<CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PropertySchemas.AccountNo>());
			Assert.AreEqual("User4WTG", message.Context.ReadPropertyString<CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.PropertySchemas.Password>());
			Assert.AreEqual(0, outMsg.BodyPart.GetOriginalDataStream().Position);
		}
	}
}
