using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Products.JPCustoms.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class DecodeAndDecompressMessageComponentTests : PipelineComponentBaseTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecodeAndDecompressMessageComponent_Properties()
		{
			var component = new DecodeAndDecompressMessageComponent();

			Assert.AreEqual("Decode and Decompress Message Component", component.Description);
			Assert.AreEqual("DecodeAndDecompressMessageComponent", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecodeAndDecompressMessageComponent_Enable()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyEncodedMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<DecodeAndDecompressMessageComponent>();

			MockRepository.ReplayAll();

			var originalStream = message.BodyPart.Data;

			component.Enabled = false;
			component.Execute(pipelineContext, message);

			Assert.AreEqual(originalStream, message.BodyPart.Data);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DecodeAndDecompressMessageComponent_DecodeAndDecompress()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Il6/fnX87Vdp+q3X/GNvZ3dvd2/v/u6DnZ003T0+fnKyu3cv1Wf3eO/e/hPz19d9dvBs23/S3/v3+andbz95Tv3s37/x5Rufnfuf7t3b29nZfyDY8oh2dmhEuwf73PXuPfrn9FX6rRthfbPPT+xt+JJQun9//zdOusT5gOc3Tl4cn5wwWT99cPBwZ6DRLcj/Gyd7v3Hy/wATGV1JKgIAAA=="));
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<DecodeAndDecompressMessageComponent>();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.Execute(pipelineContext, message);

			string actualText = new StreamReader(message.BodyPart.Data).ReadToEnd();
			string expectedText = GetEmbeddedResourceAsString("TestFiles.JPCustomsReplyMessage.txt");
			Assert.AreEqual(expectedText, actualText);

			MockRepository.VerifyAll();
		}
	}
}
