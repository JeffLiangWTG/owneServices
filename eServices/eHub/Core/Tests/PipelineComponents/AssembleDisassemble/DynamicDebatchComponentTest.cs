using System;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class DynamicDebatchComponentTest : BaseComponentTest
	{
		delegate IBaseMessage DisassembleDelegate(IPipelineContext pipelineContext, IBaseMessage message, Schema envelopeSchema, Schema documentSchema);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDynamicDebatch()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.OutboxMessage.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = new PipelineContext();

			var disassembler = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(disassembler.Disassemble(pipelineContext, message)).Return(message);

			var component = MockRepository.PartialMock<DynamicDebatchComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(disassembler);

			MockRepository.ReplayAll();

			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "MessageTypeXpath must be specified.");

			component.MessageTypeXPath = "/*[local-name()='OutboxMessage']/@MessageType";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "IsFlatFileXPath must be specified.");

			component.IsFlatFileXPath = "/*[local-name()='OutboxMessage']/@IsFlatFileSchema";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "ContentXPath must be specified.");

			component.ContentXPath = "/*[local-name()='OutboxMessage' and namespace-uri()='http://cargowise.com/ehub/core/2010/06']/*[local-name()='Content' and namespace-uri()='']";
			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			AssertXmlStream(GetEmbeddedResource("TestFiles.XmlInterchange.xml"), result.BodyPart.GetOriginalDataStream());
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_DynamicDebatchComponent()
		{
			var component = new DynamicDebatchComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("Dynamic Debatch Component", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("A5C0B158-C89F-4A63-9665-2A0D68793038"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string messageTypeXPathProp = null;
			string isFlatFileXPathProp = null;
			string contentXPathProp = null;

			string messageTypeXPathValue = string.Empty;
			string isFlatFileXPathValue = string.Empty;
			string contentXPathValue = string.Empty;

			object messageTypeXPathPtr = "messageTypeXPath";
			object isFlatFileXPathPtr = "isFlatFileXPath";
			object contentXPathPtr = "contentXPath";

			Expect.Call(() => propertyBag.Write("MessageTypeXPath", ref messageTypeXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { messageTypeXPathProp = propName; messageTypeXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("IsFlatFileXPath", ref isFlatFileXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { isFlatFileXPathProp = propName; isFlatFileXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ContentXPath", ref contentXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { contentXPathProp = propName; contentXPathValue = (string)ptrVar; }));

			Expect.Call(() => propertyBag.Read("MessageTypeXPath", out messageTypeXPathPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "messagaTypeXPath_different"; }));
			Expect.Call(() => propertyBag.Read("IsFlatFileXPath", out isFlatFileXPathPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "isFlatFileXPath_different"; }));
			Expect.Call(() => propertyBag.Read("ContentXPath", out contentXPathPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "contentXPath_different"; }));

			MockRepository.ReplayAll();

			component.MessageTypeXPath = "messageTypeXPath";
			component.IsFlatFileXPath = "isFlatFileXPath";
			component.ContentXPath = "contentXPath";

			component.Save(propertyBag, true, true);

			Assert.AreEqual("MessageTypeXPath", messageTypeXPathProp);
			Assert.AreEqual("IsFlatFileXPath", isFlatFileXPathProp);
			Assert.AreEqual("ContentXPath", contentXPathProp);

			Assert.AreEqual("messageTypeXPath", messageTypeXPathValue);
			Assert.AreEqual("isFlatFileXPath", isFlatFileXPathValue);
			Assert.AreEqual("contentXPath", contentXPathValue);

			component.Load(propertyBag, 0);

			Assert.AreEqual("messagaTypeXPath_different", component.MessageTypeXPath);
			Assert.AreEqual("isFlatFileXPath_different", component.IsFlatFileXPath);
			Assert.AreEqual("contentXPath_different", component.ContentXPath);

			MockRepository.VerifyAll();
		}
	}
}
