using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class XmlDisassemblerExtensionTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestXmlDisassemblerExtension()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.XmlDisassembleExtensionTestMsg.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var newMessage = MessageFactory.CreateMessage();
			newMessage.AddPart("newMessage", MessageFactory.CreateMessagePart(), true);
			newMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.XmlDisassembleExtensionNewMsg.xml");
			newMessage.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate () { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }));

			var component = MockRepository.PartialMock<XmlDisassemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper);

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage, component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAS2Enabled()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.XmlDisassembleExtensionTestMsg.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var mdnMessage = MessageFactory.CreateMessage();
			mdnMessage.AddPart("mdnMessage", MessageFactory.CreateMessagePart(), true);
			mdnMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.XmlDisassembleExtensionMDNMsg.mdn");
			mdnMessage.Context = MessageFactory.CreateMessageContext();

			var newMessage = MessageFactory.CreateMessage();
			newMessage.AddPart("newMessage", MessageFactory.CreateMessagePart(), true);
			newMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.XmlDisassembleExtensionNewMsg.xml");
			newMessage.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate () { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }));

			var AS2DasmHelper = MockRepository.StrictMock<IAS2DisassembleHelper>();
			Expect.Call(AS2DasmHelper.Disassemble(pipelineContext, message)).Return(mdnMessage);

			var component = MockRepository.PartialMock<XmlDisassemblerExtension>();
			Expect.Call(component.GetAS2DasmHelper()).Return(AS2DasmHelper);
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper);

			MockRepository.ReplayAll();

			component.AS2Enabled = true;
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(mdnMessage, component.GetNext(pipelineContext));
			Assert.AreEqual(newMessage, component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_XmlDisassemblerExtension()
		{
			var component = new XmlDisassemblerExtension();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("XML Disassembler Extension", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));

			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("C1E692C3-E69E-4CFE-8825-14685AC3C0F9"), guid);

			var propertyBag = new FakePropertyBag();
			component.AS2Enabled = false;
			component.Save(propertyBag, true, true);
			Assert.AreEqual(false, propertyBag.dict["AS2Enabled"]);

			propertyBag.dict["AS2Enabled"] = true;
			component.Load(propertyBag, 0);
			Assert.IsTrue(component.AS2Enabled);
		}

		/// <summary>
		/// Verify that properties set on component in designer are not reset when empty port 
		/// binding properties are loaded.
		/// </summary>
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void XmlDisassemblerExtension_DesignerPropertiesLoadTest()
		{
			string designerProperties =
@"<Properties>
	<AS2Enabled vt=""11"">1</AS2Enabled>
</Properties>";

			string bindingProperties =
@"<Properties/>";

			var xmlRdr = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(designerProperties)));
			xmlRdr.Read();
			IPropertyBag designerPropertyBag = new Winterdom.BizTalk.PipelineTesting.InstConfigPropertyBag(xmlRdr);

			var component = new XmlDisassemblerExtension();

			Assert.IsFalse(component.AS2Enabled);

			component.Load(designerPropertyBag, 0);

			Assert.IsTrue(component.AS2Enabled);

			xmlRdr = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(bindingProperties)));
			xmlRdr.Read();
			IPropertyBag bindingsPropertyBag = new Winterdom.BizTalk.PipelineTesting.InstConfigPropertyBag(xmlRdr);
			component.Load(bindingsPropertyBag, 0);

			Assert.IsTrue(component.AS2Enabled);
		}

		public class FakePropertyBag : IPropertyBag
		{
			public Dictionary<string, object> dict = new Dictionary<string, object>();

			public void Read(string propName, out object ptrVar, int errorLog)
			{
				dict.TryGetValue(propName, out ptrVar);
			}

			public void Write(string propName, ref object ptrVar)
			{
				dict.Add(propName, ptrVar);
			}
		}
	}
}

