using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class FileWrapperComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileWrapperComponent()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Input.pdf");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", "FileWrapperComponent_Input.pdf");

			var newMessage = MessageFactory.CreateMessage();
			newMessage.AddPart("newMessage", MessageFactory.CreateMessagePart(), true);
			newMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Output.xml");
			newMessage.Context = MessageFactory.CreateMessageContext();

			var component = new FileWrapperComponent();
			component.Enabled = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(new System.IO.StreamReader(newMessage.BodyPart.Data).ReadToEnd(), new System.IO.StreamReader(result.BodyPart.Data).ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileWrapperComponent_Disabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Input.pdf");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", "FileWrapperComponent_Input.pdf");

			var newMessage = MessageFactory.CreateMessage();
			newMessage.AddPart("newMessage", MessageFactory.CreateMessagePart(), true);
			newMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Output.xml");
			newMessage.Context = MessageFactory.CreateMessageContext();

			var component = new FileWrapperComponent();
			var result = component.Execute(pipelineContext, message);

			Assert.AreEqual(message, result);
		}
	}
}

