using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class SingleTransfomationComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRunSingleTransformation()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var message1 = MessageFactory.CreateMessage();
			message1.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message1.BodyPart.Data = new VirtualStream();

			var targetSchema = new Schema("SchemaName, SchemaNamespace");

			var docSpec = MockRepository.StrictMock<IDocumentSpec>();
			Expect.Call(docSpec.DocSpecStrongName).Return("SchemaStrongName").Repeat.Twice();
			Expect.Call(docSpec.DocType).Return("SchemaDocType").Repeat.Twice();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetDocumentSpecByName(targetSchema.ToString())).Return(docSpec);
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var performer = MockRepository.StrictMock<ITransformationPerformer>();
			Expect.Call(performer.PerformTransformation("TransformationType", pipelineContext, message)).Return(message1).Repeat.Times(3);

			var component = MockRepository.PartialMock<SingleTransfomationComponent>();
			Expect.Call(component.GetTransformationPerformer()).Return(performer).Repeat.Any();

			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Transformation Type property must be specified.");

			component.TransformationType = "TransformationType";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Target Schema property must be specified.");

			component.TargetSchema = new Schema("SchemaName, SchemaNamespace");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Execution of transform TransformationType produced 0 length stream.");

			component.ConsumeZeroLengthResult = true;
			var result = component.Execute(pipelineContext, message);
			Assert.IsNull(result);

			component.ConsumeZeroLengthResult = false;
			message1.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml");
			message1.Context = MessageFactory.CreateMessageContext();
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message1, result);
			Assert.AreEqual("SchemaStrongName", result.Context.ReadPropertyString<BTS.SchemaStrongName>());
			Assert.AreEqual(ContextPropertyType.PropWritten, result.Context.GetPropertyType<BTS.SchemaStrongName>());
			Assert.AreEqual("SchemaDocType", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, result.Context.GetPropertyType<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_SingleTransfomationComponent()
		{
			var component = new SingleTransfomationComponent();
			Assert.AreEqual("Perform single transformation to target format.", component.Description);
			Assert.AreEqual("Single Transformation", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("44482978-1ACF-4AA8-A7FB-F9678CD1CAB9"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			string transformationTypeProp = null;
			string targetSchemaProp = null;
			string consumeZeroLengthResultProp = null;

			object enabledPtr = false;
			object transformationTypePtr = "TransType";
			object targetSchemaPtr = "Schema";
			object consumeZeroLengthResultPtr = false;

			bool enabledValue = true;
			string transformationTypeValue = string.Empty;
			string targetSchemaValue = string.Empty;
			bool consumeZeroLengthResultValue = true;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("TransformationType", ref transformationTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { transformationTypeProp = propName; transformationTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("TargetSchema", ref targetSchemaPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { targetSchemaProp = propName; targetSchemaValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ConsumeZeroLengthResult", ref consumeZeroLengthResultPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { consumeZeroLengthResultProp = propName; consumeZeroLengthResultValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("TransformationType", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "TransType_different"; }));
			Expect.Call(() => propertyBag.Read("TargetSchema", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Schema_different"; }));
			Expect.Call(() => propertyBag.Read("ConsumeZeroLengthResult", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.TransformationType = "TransType";
			component.TargetSchema = new Schema("Schema");
			component.ConsumeZeroLengthResult = false;
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("TransformationType", transformationTypeProp);
			Assert.AreEqual("TargetSchema", targetSchemaProp);
			Assert.AreEqual("ConsumeZeroLengthResult", consumeZeroLengthResultProp);

			Assert.IsFalse(enabledValue);
			Assert.AreEqual("TransType", transformationTypeValue);
			Assert.AreEqual("Schema", targetSchemaValue);
			Assert.IsFalse(consumeZeroLengthResultValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.AreEqual(new Schema("Schema_different"), component.TargetSchema);
			Assert.AreEqual("TransType_different", component.TransformationType);
			Assert.IsTrue(component.ConsumeZeroLengthResult);

			MockRepository.VerifyAll();
		}
	}
}
