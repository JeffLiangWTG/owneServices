using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class BodyPropertyReaderTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWritePropertyBody()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();

			var component = new BodyPropertyReader();
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "BodyPropertyType cannot be null.");

			component.BodyPropertyType = "bodyPropertyType";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "ContextPropertyType cannot be null.");

			component.ContextPropertyType = "contextPropertyType";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Format cannot be null.");

			component.Format = "Attachment File Name: {0}";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException), "Unexpected name namespace format (bodyPropertyType).  Expected format is ^([^#]+)#([^#]+$)");

			var property = new MIME.FileName();
			component.BodyPropertyType = string.Format("{0}#{1}", property.Name.Namespace, property.Name.Name);
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException), "Unexpected name namespace format (contextPropertyType).  Expected format is ^([^#]+)#([^#]+$)");

			var contextProperty = new OverrideFilename();
			component.ContextPropertyType = string.Format("{0}#{1}", contextProperty.Name.Namespace, contextProperty.Name.Name);
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Message Body Property 'http://schemas.microsoft.com/BizTalk/2003/mime-properties#FileName' missing in message body.");

			message.BodyPart.PartProperties.WriteProperty<MIME.FileName>("BLAH.xml");
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("Attachment File Name: BLAH.xml", message.Context.ReadPropertyString<OverrideFilename>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<OverrideFilename>());
			Assert.AreEqual(message, result);

			component.Promote = true;
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual("Attachment File Name: BLAH.xml", message.Context.ReadPropertyString<OverrideFilename>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<OverrideFilename>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_BodyPropertyReader()
		{
			var component = new BodyPropertyReader();
			Assert.AreEqual("Reads Message Body Property and writes into message context property.", component.Description);
			Assert.AreEqual("Message Body Propery Reader", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("84538DBE-153F-43df-96F6-DFBBAC1CC444"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string enabledProp = null;
			string bodyPropertyTypeProp = null;
			string formatProp = null;
			string contextPropertyTypeProp = null;
			string promoteProp = null;

			bool enabledValue = false;
			string bodyPropertyTypeValue = string.Empty;
			string formatValue = string.Empty;
			string contextPropertyTypeValue = string.Empty;
			bool promoteValue = false;

			object enabledPtr = false;
			object bodyPropertyTypePtr = "PropType";
			object formatPtr = "Format";
			object contextPropertyTypePtr = "ContextPropType";
			object promotePtr = false;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("BodyPropertyType", ref bodyPropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { bodyPropertyTypeProp = propName; bodyPropertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Format", ref formatPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { formatProp = propName; formatValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ContextPropertyType", ref contextPropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { contextPropertyTypeProp = propName; contextPropertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Promote", ref promotePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { promoteProp = propName; promoteValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("BodyPropertyType", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("Format", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Format_different"; }));
			Expect.Call(() => propertyBag.Read("ContextPropertyType", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ContextPropType_different"; }));
			Expect.Call(() => propertyBag.Read("Promote", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.BodyPropertyType = "PropType";
			component.Format = "Format";
			component.ContextPropertyType = "ContextPropType";
			component.Promote = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("BodyPropertyType", bodyPropertyTypeProp);
			Assert.AreEqual("Format", formatProp);
			Assert.AreEqual("ContextPropertyType", contextPropertyTypeProp);
			Assert.AreEqual("Promote", promoteProp);

			Assert.IsFalse(enabledValue);
			Assert.AreEqual("PropType", bodyPropertyTypeValue);
			Assert.AreEqual("Format", formatValue);
			Assert.AreEqual("ContextPropType", contextPropertyTypeValue);
			Assert.IsFalse(promoteValue);

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.Enabled);
			Assert.AreEqual("PropType_different", component.BodyPropertyType);
			Assert.AreEqual("Format_different", component.Format);
			Assert.AreEqual("ContextPropType_different", component.ContextPropertyType);
			Assert.IsTrue(component.Promote);

			MockRepository.VerifyAll();
		}
	}
}
