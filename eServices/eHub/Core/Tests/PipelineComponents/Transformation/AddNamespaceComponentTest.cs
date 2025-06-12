using System;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class AddNamespaceComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAddNamespace()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var message1 = MessageFactory.CreateMessage();
			message1.Context = MessageFactory.CreateMessageContext();
			message1.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message1.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml");
			message1.Context = MessageFactory.CreateMessageContext();

			var message2 = MessageFactory.CreateMessage();
			message2.Context = MessageFactory.CreateMessageContext();
			message2.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message2.BodyPart.Data = GetEmbeddedResource("TestFiles.VerySimpleXMLWithoutNamespace.xml");
			message2.Context = MessageFactory.CreateMessageContext();

			var message3 = MessageFactory.CreateMessage();
			message3.Context = MessageFactory.CreateMessageContext();
			message3.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message3.BodyPart.Data = GetEmbeddedResource("TestFiles.VerySimpleXMLWithoutNamespace.xml");
			message3.Context = MessageFactory.CreateMessageContext();

			var message4 = MessageFactory.CreateMessage();
			message4.Context = MessageFactory.CreateMessageContext();
			message4.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message4.BodyPart.Data = GetEmbeddedResource("TestFiles.NonUTF8WithoutNamespace.xml");
			message4.Context = MessageFactory.CreateMessageContext();

			var component = new AddNamespaceComponent();
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Namespace must be specified");

			component.Namespace = "http://cargowise.com/ehub/clients/tpe/2010/07";
			component.NsPrefix = "ns0";
			result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml"), result.BodyPart.GetOriginalDataStream());

			component.Namespace = "http://differentNamespace";
			component.NsPrefix = "ns1";
			result = component.Execute(pipelineContext, message1);
			AssertXmlStream(GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml"), result.BodyPart.GetOriginalDataStream());

			component.Namespace = "http://cargowise.com/ehub/clients/tpe/2010/07";
			component.NsPrefix = "ns0";
			result = component.Execute(pipelineContext, message2);
			AssertXmlStream(GetEmbeddedResource("TestFiles.VerySimpleXMLWithNamespace.xml"), result.BodyPart.GetOriginalDataStream());

			result = component.Execute(pipelineContext, message3);
			AssertXmlStream(GetEmbeddedResource("TestFiles.VerySimpleXMLWithNamespace.xml"), result.BodyPart.GetOriginalDataStream());

			component.Namespace = "http://www.cargowise.com/Schemas/TEST";
			component.NsPrefix = "ns0";
			component.ReplaceInvalidChars = true;
			result = component.Execute(pipelineContext, message4);
			AssertXmlStream(GetEmbeddedResource("TestFiles.NonUTF8WithNamespace.xml"), result.BodyPart.GetOriginalDataStream());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_AddNamespaceComponent()
		{
			var component = new AddNamespaceComponent();
			Assert.AreEqual("Adds a primary namespace to an Xml document updating the root node prefix if one is specified.", component.Description);
			Assert.AreEqual("Add Namespace", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("D0C4DBE1-AAD5-4800-A504-E6C86EFC00FD"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string enabledProp = null;
			string namespaceProp = null;
			string nsPrefixProp = null;
			string codePageEncodingProp = null;
			string replaceInvalidCharsProp = null;

			bool enabledValue = false;
			string namespaceValue = string.Empty;
			string nsPrefixValue = string.Empty;
			string codePageEncodingValue = string.Empty;
			bool replaceInvalidCharsValue = false;

			object enabledPtr = false;
			object namespacePtr = "namespace";
			object nsPrefixPtr = "nsPrefix";
			object codePageEncodingPtr = "codePageEncoding";
			object replaceInvalidCharsPtr = false;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Namespace", ref namespacePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { namespaceProp = propName; namespaceValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("NsPrefix", ref nsPrefixPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { nsPrefixProp = propName; nsPrefixValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("CodePageEncoding", ref codePageEncodingPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { codePageEncodingProp = propName; codePageEncodingValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ReplaceInvalidChars", ref replaceInvalidCharsPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { replaceInvalidCharsProp = propName; replaceInvalidCharsValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("Namespace", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "namespace_different"; }));
			Expect.Call(() => propertyBag.Read("NsPrefix", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "nsPrefix_different"; }));
			Expect.Call(() => propertyBag.Read("CodePageEncoding", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "codePageEncoding_different"; }));
			Expect.Call(() => propertyBag.Read("ReplaceInvalidChars", out replaceInvalidCharsPtr, 0)).
				Do(new ReadPropertyBagDelegate((string replaceInvalidCharsName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.Namespace = "namespace";
			component.NsPrefix = "nsPrefix";
			component.CodePageEncoding = "codePageEncoding";
			component.ReplaceInvalidChars = false;

			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("Namespace", namespaceProp);
			Assert.AreEqual("NsPrefix", nsPrefixProp);
			Assert.AreEqual("CodePageEncoding", codePageEncodingProp);
			Assert.AreEqual("ReplaceInvalidChars", replaceInvalidCharsProp);

			Assert.IsFalse(enabledValue);
			Assert.AreEqual("namespace", component.Namespace);
			Assert.AreEqual("nsPrefix", component.NsPrefix);
			Assert.AreEqual("codePageEncoding", component.CodePageEncoding);
			Assert.IsFalse(replaceInvalidCharsValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.AreEqual("namespace_different", component.Namespace);
			Assert.AreEqual("nsPrefix_different", component.NsPrefix);
			Assert.AreEqual("codePageEncoding_different", component.CodePageEncoding);
			Assert.IsTrue(component.ReplaceInvalidChars);

			MockRepository.VerifyAll();
		}
	}
}
