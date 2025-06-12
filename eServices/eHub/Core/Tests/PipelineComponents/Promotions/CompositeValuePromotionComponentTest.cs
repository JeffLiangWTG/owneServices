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
	public class CompositeValuePromotionComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPromoteCompositeValue()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();

			var component = new CompositeValuePromotionComponent();
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "CompositePropertyType must be specified.");

			component.CompositePropertyType = "Crap";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "PropertyTypes must be specified.");

			component.PropertyTypes = "typ1,type2";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "CompositeValueDelimiter must be specified.");

			var property1 = new FFSchemaType();
			var property2 = new MessageTrackingID();
			var property3 = new InternalTrackingID();
			component.CompositeValueDelimiter = ",";
			component.CompositePropertyType = string.Format("{0}#{1}", property1.Name.Namespace, property1.Name.Name);
			message.Context.WriteProperty<FFSchemaType>("BLAH");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Composite value and property type part counts do not match.  Please review the pipeline configuration.");

			message.Context.WriteProperty<FFSchemaType>("1111,2222");
			component.PropertyTypes = string.Format("{0}#{1},{2}#{3}", property2.Name.Namespace, property2.Name.Name, property3.Name.Namespace, property3.Name.Name);
			component.Execute(pipelineContext, message);
			Assert.AreEqual("1111", message.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("2222", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<MessageTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<InternalTrackingID>());

			component.PromoteValue = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("1111", message.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("2222", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<MessageTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<InternalTrackingID>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_CompositeValuePromotionComponent()
		{
			var component = new CompositeValuePromotionComponent();
			Assert.AreEqual("Promote or simply write one or more values into the context from an existing composite (delimited) context property value.", component.Description);
			Assert.AreEqual("Composite Value Promoter", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("244468D1-6088-4A6E-A408-A230B0C5AC28"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string promoteValueProp = null;
			string compositePropertyTypeProp = null;
			string compositeValueDelimiteProp = null;
			string propertyTypesProp = null;
			string enabledProp = null;

			bool promoteValueValue = false;
			string compositePropertyTypeValue = string.Empty;
			string compositeValueDelimiteValue = string.Empty;
			string propertyTypesValue = string.Empty;
			bool enabledValue = false;

			object promoteValuePtr = false;
			object compositePropertyTypePtr = "PropType";
			object compositeValueDelimitePtr = "Delimiter";
			object propertyTypesPtr = "PropTypes";
			object enabledPtr = false;

			Expect.Call(() => propertyBag.Write("PromoteValue", ref promoteValuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { promoteValueProp = propName; promoteValueValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("CompositePropertyType", ref compositePropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { compositePropertyTypeProp = propName; compositePropertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("CompositeValueDelimiter", ref compositeValueDelimitePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { compositeValueDelimiteProp = propName; compositeValueDelimiteValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("PropertyTypes", ref propertyTypesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { propertyTypesProp = propName; propertyTypesValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("PromoteValue", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("CompositePropertyType", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("CompositeValueDelimiter", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Delimiter_different"; }));
			Expect.Call(() => propertyBag.Read("PropertyTypes", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropTypes_different"; }));
			Expect.Call(() => propertyBag.Read("Enabled", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.PromoteValue = false;
			component.CompositePropertyType = "PropType";
			component.CompositeValueDelimiter = "Delimiter";
			component.PropertyTypes = "PropTypes";
			component.Enabled = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("PromoteValue", promoteValueProp);
			Assert.AreEqual("CompositePropertyType", compositePropertyTypeProp);
			Assert.AreEqual("CompositeValueDelimiter", compositeValueDelimiteProp);
			Assert.AreEqual("PropertyTypes", propertyTypesProp);
			Assert.AreEqual("Enabled", enabledProp);

			Assert.IsFalse(promoteValueValue);
			Assert.AreEqual("PropType", compositePropertyTypeValue);
			Assert.AreEqual("Delimiter", compositeValueDelimiteValue);
			Assert.AreEqual("PropTypes", propertyTypesValue);
			Assert.IsFalse(enabledValue);

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.PromoteValue);
			Assert.AreEqual("PropType_different", component.CompositePropertyType);
			Assert.AreEqual("Delimiter_different", component.CompositeValueDelimiter);
			Assert.AreEqual("PropTypes_different", component.PropertyTypes);
			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}
	}
}
