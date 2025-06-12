using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests
{
	[TestClass]
	public class ContextAccessorTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSetContextProperty()
		{
			ContextAccessor contextAccessor = new ContextAccessor();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();

			Assert.AreEqual(0U, message.Context.CountProperties, "PRE: There is no message context property.");
			MultipleTransformationComponent.context = message.Context;

			contextAccessor.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TEST");
			var messageContext = MultipleTransformationComponent.context;
			Assert.IsNotNull(messageContext, "messageContext");
			Assert.AreEqual(1U, messageContext.CountProperties, "Should have one message context property.");
			Assert.AreEqual("TEST", messageContext.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"), "Message context should be created");

			contextAccessor.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "UPDATED TEST");
			messageContext = MultipleTransformationComponent.context;
			Assert.IsNotNull(messageContext, "messageContext");
			Assert.AreEqual(1U, messageContext.CountProperties, "Should have one message context property.");
			Assert.AreEqual("UPDATED TEST", messageContext.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"), "Message context should be updated");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetContextProperty()
		{
			ContextAccessor contextAccessor = new ContextAccessor();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("sender");

			Assert.AreEqual(1U, message.Context.CountProperties, "PRE: There is one message context property.");
			MultipleTransformationComponent.context = message.Context;

			string result = contextAccessor.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			Assert.AreEqual("sender", result, "Should get the context property value.");

			result = contextAccessor.GetContextProperty("WrongContextName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			Assert.AreEqual("", result, "Cannot find the context property.");

			result = contextAccessor.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/wrong-namespace", "DEFAULT");
			Assert.AreEqual("DEFAULT", result, "Cannot find the context property but should return a default value.");
		}
	}
}
