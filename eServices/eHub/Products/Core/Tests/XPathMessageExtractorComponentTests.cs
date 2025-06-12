using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Products.Core.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class XPathMessageExtractorComponentTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXPathMessageExtractor()
		{
			var messageContent = @"<CustomsMessage xmlns=""http://cargowise.com/ehub/products/customsmessage"">
<Content>VU5CK1VOT0E6MytJTkVUQ0VDUFQrSFlFVFNUVFNUKysrKysrKysrKysrMSsxMDQ0OTg1Jw==</Content>
</CustomsMessage>";
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageContent));
			message.Context = MessageFactory.CreateMessageContext();

			var component = new XPathMessageExtractorComponent();

			component.Enabled = true;
			component.XPath = "/*[local-name()='CustomsMessage' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/*[local-name()='Content' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/text()";
			component.Decode = true;
			component.Execute(pipelineContext, message);

			Assert.AreEqual("UNB+UNOA:3+INETCECPT+HYETSTTST++++++++++++1+1044985'", message.BodyPart.Data.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXPathMessageExtractor_ReturnRawMessage()
		{
			var messageContent = @"UNB+UNOA:3+INETCECPT+HYETSTTST++++++++++++1+1044985'";
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageContent));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<IsFlatFileSchema>("True");

			var component = new XPathMessageExtractorComponent();

			component.Enabled = true;
			component.XPath = "/*[local-name()='CustomsMessage' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/*[local-name()='Content' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/text()";
			component.Decode = true;
			component.SkipIfFlatFile = true;
			component.Execute(pipelineContext, message);

			Assert.AreEqual("UNB+UNOA:3+INETCECPT+HYETSTTST++++++++++++1+1044985'", message.BodyPart.Data.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXPathMessageExtractor_Exception()
		{
			var messageContent = @"UNB+UNOA:3+INETCECPT+HYETSTTST++++++++++++1+1044985'";
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageContent));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<IsFlatFileSchema>("True");

			var component = new XPathMessageExtractorComponent();

			component.Enabled = true;
			component.XPath = "/*[local-name()='CustomsMessage' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/*[local-name()='Content' and namespace-uri()='http://cargowise.com/ehub/products/customsmessage']/text()";
			component.Decode = true;
			component.SkipIfFlatFile = false;
			

			AssertException(() => component.Execute(pipelineContext, message), typeof(XmlException), "Data at the root level is invalid. Line 1, position 1.");
		}
	}
}
