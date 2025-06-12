using System;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.Products.JPCustoms.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class JPCustomsGatewayMessageWrapperComponentTests : PipelineComponentBaseTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsGatewayMessageWrapperComponent_ReplaceMessageLength()
		{
			const string text = @"---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------This is a nice message 999999
or not";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				JPCustomsGatewayMessageWrapperComponent.ReplaceMessageLength(stream);
				stream.Position = 0;
				var actualText = new StreamReader(stream).ReadToEnd();
				Assert.AreEqual(text.Length, actualText.Length);
				Assert.AreEqual(@"---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------This is a nice message 000406
or not", actualText);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ApplicationException), "JP Customs message should be longer than 6")]
		public void JPCustomsGatewayMessageWrapperComponent_ReplaceMessageLength_ZeroLengthException()
		{
			using (var stream = new MemoryStream())
			{
				JPCustomsGatewayMessageWrapperComponent.ReplaceMessageLength(stream);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ApplicationException), "JP Customs message can't be longer than 500.000")]
		public void JPCustomsGatewayMessageWrapperComponent_ReplaceMessageLength_TooBigException()
		{
			using (var stream = new MemoryStream())
			{
				for (int i = 0; i < 5000001; i++) stream.WriteByte(48);
				JPCustomsGatewayMessageWrapperComponent.ReplaceMessageLength(stream);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsGatewayMessageWrapperComponent_WrapMessage()
		{
			const string text = "This is a nice message 999999";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				stream.Position = 0;
				using (var resultStream = JPCustomsGatewayMessageWrapperComponent.WrapMessage(stream, "192.58.3.1"))
				{
					resultStream.Position = 0;
					var actualText = new StreamReader(resultStream).ReadToEnd();
					Assert.AreEqual("<SendLodgement xmlns=\"http://cargowise.com/ehub/products/jpcustoms\"><message>This is a nice message 999999</message><audit>192.58.3.1</audit></SendLodgement>", actualText);
				}
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsGatewayMessageWrapperComponent_Properties()
		{
			var component = new JPCustomsGatewayMessageWrapperComponent();

			Assert.AreEqual("Wrap JPCustoms message in JPCustomsGateway envelop", component.Description);
			Assert.AreEqual("Wrap JPCustoms message in JPCustomsGateway envelop", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsGatewayMessageWrapperComponent_Enable()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsSendMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<JPCustomsGatewayMessageWrapperComponent>();

			MockRepository.ReplayAll();
			var originalStream = message.BodyPart.Data;

			component.Enabled = false;
			component.Execute(pipelineContext, message);

			Assert.AreEqual(originalStream, message.BodyPart.Data);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsGatewayMessageWrapperComponent_WrapperAndCompressed()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsSendMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<JPCustomsGatewayMessageWrapperComponent>();

			MockRepository.ReplayAll();

			message.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "192.168.1.1");

			component.Enabled = true;
			component.CompressAndEncode = true;
			component.Execute(pipelineContext, message);

			Assert.AreEqual(true, message.Context.Read("AckRequired", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			AssertXmlStream(GetEmbeddedResource("TestFiles.JPCustomsSendMessageWrapped.xml"), message.BodyPart.Data);

			MockRepository.VerifyAll();
		}


	}
}
