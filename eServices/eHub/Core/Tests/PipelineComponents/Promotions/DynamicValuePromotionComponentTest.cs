using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using System.Collections.Generic;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class DynamicValuePromotionComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPromoteDynamicValue()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();

			var component = new DynamicValuePromotionComponent();
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "DynamicPropertyType must be specified.");

			component.DynamicPropertyType = "Crap";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "DynamicPropertyValueFormat must be specified.");

			component.DynamicPropertyValueFormat = "{0},{1}";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "ExistingValuePropertyTypes must be specified.");

			component.ExistingValuePropertyTypes = "Crap types";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException), "Invalid property promotion parameter:Crap types");

			var property1 = new FFSchemaType();
			var property2 = new MessageTrackingID();
			var property3 = new InternalTrackingID();

			message.Context.WriteProperty<FFSchemaType>("FFSchema");
			message.Context.WriteProperty<MessageTrackingID>("TrackingID");
			component.DynamicPropertyType = string.Format("{0}#{1}", property3.Name.Namespace, property3.Name.Name);
			component.ExistingValuePropertyTypes = string.Format("{0}#{1},{2}#{3}", property1.Name.Namespace, property1.Name.Name, property2.Name.Namespace, property2.Name.Name);
			component.Execute(pipelineContext, message);
			Assert.AreEqual("FFSchema,TrackingID", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropWritten, message.Context.GetPropertyType<InternalTrackingID>());

			component.PromoteValue = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("FFSchema,TrackingID", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<InternalTrackingID>());
			Assert.AreEqual(message, result);

			message.Context.WriteProperty<FFSchemaType>("FFSchema");
			message.Context.WriteProperty<MessageTrackingID>("TrackingID");
			component.DynamicPropertyType = string.Format("{0}#{1}", property3.Name.Namespace, property3.Name.Name);
			component.DynamicPropertyValueFormat = "{0},{1}_{2}";
			component.ExistingValuePropertyTypes = string.Format("{0}#{1},{2}#{3},MethodCall:substring@{0}#{1}@1@5", property1.Name.Namespace, property1.Name.Name, property2.Name.Namespace, property2.Name.Name);
			component.Execute(pipelineContext, message);
			Assert.AreEqual("FFSchema,TrackingID_FSche", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<InternalTrackingID>());

			#region setup read counter context
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.ActionProcedures.Add(new ActionProcedure { OutputParm = null, Procedure = "ReadCounterValue", InputParms = new List<string>(new string[] { "@name", "TESTCOUNTER", "@padlength", "5" }), Result = "00123" });
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
			#endregion

			message.Context.WriteProperty<FFSchemaType>("FFSchema");
			message.Context.WriteProperty<MessageTrackingID>("TrackingID");
			component.DynamicPropertyType = string.Format("{0}#{1}", property3.Name.Namespace, property3.Name.Name);
			component.DynamicPropertyValueFormat = "{0},{1}_{2}";
			component.ExistingValuePropertyTypes = string.Format("{0}#{1},{2}#{3},MethodCall:ReadCounterValue@TESTCOUNTER@5", property1.Name.Namespace, property1.Name.Name, property2.Name.Namespace, property2.Name.Name);
			component.Execute(pipelineContext, message);
			Assert.AreEqual("FFSchema,TrackingID_00123", message.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<InternalTrackingID>());

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_DynamicValuePromotionComponent()
		{
			var component = new DynamicValuePromotionComponent();
			Assert.AreEqual("Promote or simply write a composed value into the context from existing context property values.", component.Description);
			Assert.AreEqual("Dynamic Value Promoter", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("C51D6E0F-4892-45CB-A3A6-81705A716017"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string promoteValueProp = null;
			string dynamicPropertyTypeProp = null;
			string dynamicPropertyValueFormatProp = null;
			string existingValuePropertyTypesProp = null;
			string enabledProp = null;

			bool promoteValueValue = false;
			string dynamicPropertyTypeValue = string.Empty;
			string dynamicPropertyValueFormatValue = string.Empty;
			string existingValuePropertyTypesValue = string.Empty;
			bool enabledValue = false;

			object promoteValuePtr = false;
			object dynamicPropertyTypePtr = "PropType";
			object dynamicPropertyValueFormaPtr = "ValueFormat";
			object existingValuePropertyTypesPtr = "ExistingPropTypes";
			object enabledPtr = false;

			Expect.Call(() => propertyBag.Write("PromoteValue", ref promoteValuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { promoteValueProp = propName; promoteValueValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DynamicPropertyType", ref dynamicPropertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { dynamicPropertyTypeProp = propName; dynamicPropertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DynamicPropertyValueFormat", ref dynamicPropertyValueFormaPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { dynamicPropertyValueFormatProp = propName; dynamicPropertyValueFormatValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ExistingValuePropertyTypes", ref existingValuePropertyTypesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { existingValuePropertyTypesProp = propName; existingValuePropertyTypesValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("PromoteValue", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("DynamicPropertyType", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("DynamicPropertyValueFormat", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ValueFormat_different"; }));
			Expect.Call(() => propertyBag.Read("ExistingValuePropertyTypes", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ExistingPropTypes_different"; }));
			Expect.Call(() => propertyBag.Read("Enabled", out promoteValuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.PromoteValue = false;
			component.DynamicPropertyType = "PropType";
			component.DynamicPropertyValueFormat = "ValueFormat";
			component.ExistingValuePropertyTypes = "ExistingPropTypes";
			component.Enabled = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("PromoteValue", promoteValueProp);
			Assert.AreEqual("DynamicPropertyType", dynamicPropertyTypeProp);
			Assert.AreEqual("DynamicPropertyValueFormat", dynamicPropertyValueFormatProp);
			Assert.AreEqual("ExistingValuePropertyTypes", existingValuePropertyTypesProp);
			Assert.AreEqual("Enabled", enabledProp);

			Assert.IsFalse(promoteValueValue);
			Assert.AreEqual("PropType", dynamicPropertyTypeValue);
			Assert.AreEqual("ValueFormat", dynamicPropertyValueFormatValue);
			Assert.AreEqual("ExistingPropTypes", existingValuePropertyTypesValue);
			Assert.IsFalse(enabledValue);

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.PromoteValue);
			Assert.AreEqual("PropType_different", component.DynamicPropertyType);
			Assert.AreEqual("ValueFormat_different", component.DynamicPropertyValueFormat);
			Assert.AreEqual("ExistingPropTypes_different", component.ExistingValuePropertyTypes);
			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}
	}
}
