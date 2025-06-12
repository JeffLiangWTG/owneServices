using System;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class SmartDebatchDisassembleComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSmartDebatchDisassemble()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.ConsolsInternalWithTwoConsolsAndTwoContainersEach.xml");

			var pipelineContext = new PipelineContext();
			var component = new CloneDebatchDisassembleComponent();

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(message, component.GetNext(pipelineContext));

			component.Enabled = true;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "XPathToDebatch must be specified");

			component.XPathToDebatch = "/*[local-name()='ConsolsInternal' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Payload' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Consols' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Consol' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='ConsolDetail' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Containers' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Container' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']";
			component.Disassemble(pipelineContext, message);

			var result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithFirstConsolAndFirstContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithFirstConsolAndSecondContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithSecondConsolAndFirstContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithSecondConsolAndSecondContainer.xml"), result.BodyPart.Data);

			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_SmartDebatchDisassembleComponent()
		{
			var component = new CloneDebatchDisassembleComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("Clone Debatch Disassembler", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("9219D890-0C8B-4A9A-B31A-46160C582579"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string debatchXPathProp = null;
			string enabledProp = null;

			string debatchXPathValue = string.Empty;
			object enabledPtr = false;

			object debatchXPathPtr = "debatchXPath";
			bool enabledValue = true;

			Expect.Call(() => propertyBag.Write("XPathToDebatch", ref debatchXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { debatchXPathProp = propName; debatchXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("XPathToDebatch", out debatchXPathPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "debatchXPath_different"; }));
			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.XPathToDebatch = "debatchXPath";
			component.Enabled = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("XPathToDebatch", debatchXPathProp);
			Assert.AreEqual("Enabled", enabledProp);

			Assert.AreEqual("debatchXPath", debatchXPathValue);
			Assert.IsFalse(enabledValue);

			component.Load(propertyBag, 0);

			Assert.AreEqual("debatchXPath_different", component.XPathToDebatch);
			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}
	}
}
