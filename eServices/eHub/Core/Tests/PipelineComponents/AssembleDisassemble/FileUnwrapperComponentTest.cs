using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class FileUnwrapperComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileUnwrapperComponent()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Output.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new FileUnwrapperComponent();
			component.Enabled = true;
			var result = component.Execute(pipelineContext, message);

			var expected = GetEmbeddedResource("TestFiles.FileWrapperComponent_Input.pdf");
			var actual = result.BodyPart.Data;

			Assert.AreEqual("FileWrapperComponent_Input.pdf", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			int file1byte;
			int file2byte;
			do
			{
				file1byte = expected.ReadByte();
				file2byte = actual.ReadByte();
			}
			while ((file1byte == file2byte) && (file1byte != -1));

			Assert.IsTrue(file1byte == file2byte);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileUnwrapperComponent_Disabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FileWrapperComponent_Output.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new FileUnwrapperComponent();
			var result = component.Execute(pipelineContext, message);

			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFileUnwrapperComponent_InvalidInput()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FileUnwrapperComponent_InvalidIntput.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new FileUnwrapperComponent();
			component.Enabled = true;
			var result = component.Execute(pipelineContext, message);

			var expected = GetEmbeddedResource("TestFiles.FileWrapperComponent_Input.pdf");
			var actual = result.BodyPart.Data;

			Assert.AreEqual(message, result);
		}
	}
}

