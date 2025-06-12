using System;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class AUCustomsMessageXmlWrapperComponentTests : BaseComponentTest
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAUCustomsMessageXmlWrapperComponentEnabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Text.AUCustomsReply.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new AUCustomsMessageXmlWrapperComponent();
			component.Enabled = false;
			component.Execute(pipelineContext, message);

			Assert.AreEqual(message.BodyPart.Data, message.BodyPart.Data);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAUCustomsMessageXmlWrapperComponent()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Text.AUCustomsReply.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new AUCustomsMessageXmlWrapperComponent();

			message.Context.PromoteProperty<Reference>("AAL364P");
			component.Enabled = true;
			component.Encode = false;
			component.Execute(pipelineContext, message);

			AssertXmlStream(GetEmbeddedResource("TestFiles.AUCustomsReply.xml"), message.BodyPart.Data);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAUCustomsMessageXmlWrapperEncodedComponent()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Text.AUCustomsReply.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new AUCustomsMessageXmlWrapperComponent();

			message.Context.PromoteProperty<Reference>("AAL364P");
			component.Enabled = true;
			component.Encode = true;
			component.Execute(pipelineContext, message);

			AssertXmlStream(GetEmbeddedResource("TestFiles.AUCustomsReplyEncoded.xml"), message.BodyPart.Data);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_AUCustomsMessageXmlWrapperComponent()
		{
			var component = new AUCustomsMessageXmlWrapperComponent();

			Assert.AreEqual("Product: Wrap AUCustoms reply message in xml envelop", component.Description);
			Assert.AreEqual("Product: AUCustoms message xml wrapper component", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}
	}
}


