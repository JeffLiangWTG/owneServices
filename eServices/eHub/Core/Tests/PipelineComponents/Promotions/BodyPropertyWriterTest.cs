using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class BodyPropertyWriterTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWritePropertyBody()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();

			var component = new BodyPropertyWriter();
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "BodyPropertyType cannot be null.");

			component.BodyPropertyType = "bodyPropertyType";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "ContextPropertyType cannot be null.");

			component.ContextPropertyType = "contextPropertyType";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Format cannot be null.");

			component.Format = "Destination Party: {0}";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException), "Unexpected name namespace format (bodyPropertyType).  Expected format is ^([^#]+)#([^#]+$)");

			var property = new MIME.FileName();
			component.BodyPropertyType = string.Format("{0}#{1}", property.Name.Namespace, property.Name.Name);
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException), "Unexpected name namespace format (contextPropertyType).  Expected format is ^([^#]+)#([^#]+$)");

			var contextProperty = new BTS.DestinationParty();
			component.ContextPropertyType = string.Format("{0}#{1}", contextProperty.Name.Namespace, contextProperty.Name.Name);
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Context Property 'http://schemas.microsoft.com/BizTalk/2003/system-properties#DestinationParty' missing in message context");

			message.Context.WriteProperty<BTS.DestinationParty>("11111");
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("Destination Party: 11111", message.BodyPart.PartProperties.ReadPropertyString<MIME.FileName>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_BodyPropertyWriter()
		{
			var component = new BodyPropertyWriter();
			Assert.AreEqual("Writes Message Body Property.", component.Description);
			Assert.AreEqual("Message Body Propery Writer", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("DBBA8578-5CC0-422e-81F2-6E89154B8FD7"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string enabledProp = null;
			string bodyPropertyTypeProp = null;
			string formatProp = null;
			string contextPropertyTypeProp = null;

			bool enabledValue = false;
			string bodyPropertyTypeValue = string.Empty;
			string formatValue = string.Empty;
			string contextPropertyTypeValue = string.Empty;

			object enabledPtr = false;
			object bodyPropertyTypePtr = "PropType";
			object formatPtr = "Format";
			object contextPropertyTypePtr = "ContextPropType";

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("BodyPropertyType", ref bodyPropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { bodyPropertyTypeProp = propName; bodyPropertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Format", ref formatPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { formatProp = propName; formatValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ContextPropertyType", ref contextPropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { contextPropertyTypeProp = propName; contextPropertyTypeValue = (string)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("BodyPropertyType", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("Format", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Format_different"; }));
			Expect.Call(() => propertyBag.Read("ContextPropertyType", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ContextPropType_different"; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.BodyPropertyType = "PropType";
			component.Format = "Format";
			component.ContextPropertyType = "ContextPropType";

			component.Save(propertyBag, true, true);

			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("BodyPropertyType", bodyPropertyTypeProp);
			Assert.AreEqual("Format", formatProp);
			Assert.AreEqual("ContextPropertyType", contextPropertyTypeProp);

			Assert.IsFalse(enabledValue);
			Assert.AreEqual("PropType", bodyPropertyTypeValue);
			Assert.AreEqual("Format", formatValue);
			Assert.AreEqual("ContextPropType", contextPropertyTypeValue);

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.Enabled);
			Assert.AreEqual("PropType_different", component.BodyPropertyType);
			Assert.AreEqual("Format_different", component.Format);
			Assert.AreEqual("ContextPropType_different", component.ContextPropertyType);

			MockRepository.VerifyAll();
		}
	}
}
