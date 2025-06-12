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
	public class StaticValuePromotionComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPromoteStaticValue()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();

			var component = new StaticValuePromotionComponent();
			component.Enabled = false;
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "PropertyType must be specified.");

			component.PropertyType = "Crap";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Value must be specified.");

			component.Value = "crap Value";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException),"Unexpected name namespace format (Crap).  Expected format is ^([^#]+)#([^#]+$)");

			var property1 = new FFSchemaType();
			component.PropertyType = string.Format("{0}#{1}", property1.Name.Namespace, property1.Name.Name);
			component.Execute(pipelineContext, message);
			Assert.AreEqual("crap Value", message.Context.ReadPropertyString<FFSchemaType>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<FFSchemaType>());


			var property2 = new MessageTrackingID();
			component.PromoteValue = true;
			component.Value = "another crap Value";
			component.PropertyType = string.Format("{0}#{1}", property2.Name.Namespace, property2.Name.Name);
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("crap Value", message.Context.ReadPropertyString<FFSchemaType>());
			Assert.AreEqual("another crap Value", message.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<FFSchemaType>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<MessageTrackingID>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_StaticValuePromotionComponent()
		{
			var component = new StaticValuePromotionComponent();
			Assert.AreEqual("Promote or simply write a static value into the context", component.Description);
			Assert.AreEqual("Static Value Promoter", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("0BAB4B30-AD94-4A43-ABCF-0E922D4BFA68"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			string promoteValueProp = null;
			string propertyTypeProp = null;
			string valueProp = null;

			object enabledPtr = false;
			object promoteValuePtr = false;
			object propertyTypePtr = "PropType";
			object valuePtr = "Value";

			bool enabledValue = true;
			bool promoteValueValue = true;
			string propertyTypeValue = string.Empty;
			string valueValue = string.Empty;

			Expect.Call(() => propertyBag.Write("PromoteValue", ref promoteValuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { promoteValueProp = propName; promoteValueValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Enabled", ref promoteValuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; promoteValueValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("PropertyType", ref propertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { propertyTypeProp = propName; propertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Value", ref valuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { valueProp = propName; valueValue = (string)ptrVar; }));

			Expect.Call(() => propertyBag.Read("PromoteValue", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("Enabled", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = false; }));
			Expect.Call(() => propertyBag.Read("PropertyType", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("Value", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Value_different"; }));
			
			MockRepository.ReplayAll();

			component.Enabled = false;
			component.PromoteValue = false;
			component.PropertyType = "PropType";
			component.Value = "Value";
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("PromoteValue", promoteValueProp);
			Assert.AreEqual("PropertyType", propertyTypeProp);
			Assert.AreEqual("Value", valueProp);
			Assert.IsTrue(enabledValue);
			Assert.IsFalse(promoteValueValue);
			Assert.AreEqual("PropType", propertyTypeValue);
			Assert.AreEqual("Value", valueValue);

			component.Load(propertyBag, 0);
			Assert.IsFalse(component.Enabled);
			Assert.IsTrue(component.PromoteValue);
			Assert.AreEqual("PropType_different", component.PropertyType);
			Assert.AreEqual("Value_different", component.Value);

			MockRepository.VerifyAll();
		}
	}
}
