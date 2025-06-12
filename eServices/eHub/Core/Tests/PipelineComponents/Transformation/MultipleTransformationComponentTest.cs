using System;
using System.Collections.Generic;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Threading;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MultipleTransformationComponentTest : BaseComponentTest
	{
		protected delegate IBaseMessage PerformTransformationDelegate(List<TransformSet> transformList, IPipelineContext pipelineContext, IBaseMessage message, out string lastTargetMessageType);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRunMultipleTransformation()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var message1 = MessageFactory.CreateMessage();
			message1.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message1.BodyPart.Data = new VirtualStream();

			var message2 = MessageFactory.CreateMessage();
			message2.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message2.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml");
			message2.Context = MessageFactory.CreateMessageContext();

			var targetSchema = new Schema("name, namespace");

			string lastTargetMessageType = string.Empty;
			var transformaSetID = Guid.NewGuid();
			var transformSets = new List<TransformSet>() { new TransformSet(new List<TransformDetail>() { new TransformDetail(transformaSetID, targetSchema.SchemaName, targetSchema.TargetNamespace) }, null) };

			var docSpec = MockRepository.StrictMock<IDocumentSpec>();
			Expect.Call(docSpec.DocSpecStrongName).Return("SchemaStrongName").Repeat.Twice();
			Expect.Call(docSpec.DocType).Return("SchemaDocType").Repeat.Twice();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetDocumentSpecByType(targetSchema.ToString())).Return(docSpec);
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var performer = MockRepository.StrictMock<ITransformationPerformer>();
			Expect.Call(performer.PerformTransformation(new List<TransformSet>(), pipelineContext, message, out lastTargetMessageType)).
				Do(new PerformTransformationDelegate((List<TransformSet> transformList, IPipelineContext context, IBaseMessage messageToTransform, out string messageType) =>
			{
				messageType = string.Empty;
				return message1;
			}));
			Expect.Call(performer.PerformTransformation(transformSets, pipelineContext, message, out lastTargetMessageType)).
				Do(new PerformTransformationDelegate((List<TransformSet> transformList, IPipelineContext context, IBaseMessage messageToTransform, out string messageType) =>
				{
					messageType = targetSchema.ToString();
					MultipleTransformationComponent.context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TEST");
					return message2;
				}));

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			Expect.Call(accessor.SelectTransformsByPartiesMessage("sender1", "recipient1", "sourceMessageType1", null)).Return(new List<TransformSet>());
			Expect.Call(accessor.SelectTransformsByPartiesMessage("sender2", "recipient2", "sourceMessageType2", null)).Return(transformSets);

			var component = MockRepository.PartialMock<MultipleTransformationComponent>();
			Expect.Call(component.GetTransformationPerformer()).Return(performer).Repeat.Any();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor).Repeat.Any();

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);

			component.Enabled = true;
			message.Context.WriteProperty<BTS.SourceParty>("sender1");
			message.Context.WriteProperty<BTS.DestinationParty>("recipient1");
			message.Context.WriteProperty<BTS.MessageType>("sourceMessageType1");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message1, result);
			Assert.AreEqual(null, result.Context.ReadPropertyString<TransformSetID>());
			Assert.AreNotEqual("TEST", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			message.Context.WriteProperty<BTS.SourceParty>("sender2");
			message.Context.WriteProperty<BTS.DestinationParty>("recipient2");
			message.Context.WriteProperty<BTS.MessageType>("sourceMessageType2");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message2, result);
			Assert.AreEqual("SchemaStrongName", result.Context.ReadPropertyString<BTS.SchemaStrongName>());
			Assert.AreEqual(ContextPropertyType.PropWritten, result.Context.GetPropertyType<BTS.SchemaStrongName>());
			Assert.AreEqual("SchemaDocType", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, result.Context.GetPropertyType<BTS.MessageType>());
			Assert.AreEqual(transformaSetID.ToString().ToUpper(), result.Context.ReadPropertyString<TransformSetID>());
			Assert.AreEqual("TEST", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRunMultipleTransformation_WhenNoTransformationTypes_OnlySetTransformationSet()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var message1 = MessageFactory.CreateMessage();
			message1.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message1.BodyPart.Data = new VirtualStream();

			var message2 = MessageFactory.CreateMessage();
			message2.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message2.BodyPart.Data = GetEmbeddedResource("TestFiles.NoNamespaceSample_NamespaceAdded.xml");
			message2.Context = MessageFactory.CreateMessageContext();

			var targetSchema = new Schema("name, namespace");

			string lastTargetMessageType = string.Empty;
			var transformaSetID = Guid.NewGuid();
			var transformSets = new List<TransformSet>() { new TransformSet(new List<TransformDetail>() { new TransformDetail(transformaSetID, targetSchema.SchemaName, targetSchema.TargetNamespace) }, null) };

			var docSpec = MockRepository.StrictMock<IDocumentSpec>();
			Expect.Call(docSpec.DocSpecStrongName).Return("SchemaStrongName").Repeat.Twice();
			Expect.Call(docSpec.DocType).Return("SchemaDocType").Repeat.Twice();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetDocumentSpecByType(targetSchema.ToString())).Return(docSpec);
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var performer = MockRepository.StrictMock<ITransformationPerformer>();
			Expect.Call(performer.PerformTransformation(new List<TransformSet>(), pipelineContext, message, out lastTargetMessageType)).
				Do(new PerformTransformationDelegate((List<TransformSet> transformList, IPipelineContext context, IBaseMessage messageToTransform, out string messageType) =>
				{
					messageType = string.Empty;
					return message1;
				}));
			Expect.Call(performer.PerformTransformation(transformSets, pipelineContext, message, out lastTargetMessageType)).
				Do(new PerformTransformationDelegate((List<TransformSet> transformList, IPipelineContext context, IBaseMessage messageToTransform, out string messageType) =>
				{
					messageType = targetSchema.ToString();
					MultipleTransformationComponent.context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TEST");
					return message2;
				}));

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			Expect.Call(accessor.SelectTransformsByPartiesMessage("sender1", "recipient1", "sourceMessageType1", null)).Return(new List<TransformSet>() { new TransformSet(new List<TransformDetail>() { new TransformDetail(new Guid("BDB66614-4539-49C9-A348-2946EE5F46DB"), string.Empty, string.Empty) }, string.Empty) });
			Expect.Call(accessor.SelectTransformsByPartiesMessage("sender2", "recipient2", "sourceMessageType2", null)).Return(transformSets);

			var component = MockRepository.PartialMock<MultipleTransformationComponent>();
			Expect.Call(component.GetTransformationPerformer()).Return(performer).Repeat.Any();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor).Repeat.Any();

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);

			component.Enabled = true;
			message.Context.WriteProperty<BTS.SourceParty>("sender1");
			message.Context.WriteProperty<BTS.DestinationParty>("recipient1");
			message.Context.WriteProperty<BTS.MessageType>("sourceMessageType1");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message1, result);
			Assert.AreEqual("BDB66614-4539-49C9-A348-2946EE5F46DB", result.Context.ReadPropertyString<TransformSetID>());
			Assert.AreNotEqual("TEST", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			message.Context.WriteProperty<BTS.SourceParty>("sender2");
			message.Context.WriteProperty<BTS.DestinationParty>("recipient2");
			message.Context.WriteProperty<BTS.MessageType>("sourceMessageType2");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message2, result);
			Assert.AreEqual("SchemaStrongName", result.Context.ReadPropertyString<BTS.SchemaStrongName>());
			Assert.AreEqual(ContextPropertyType.PropWritten, result.Context.GetPropertyType<BTS.SchemaStrongName>());
			Assert.AreEqual("SchemaDocType", result.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, result.Context.GetPropertyType<BTS.MessageType>());
			Assert.AreEqual(transformaSetID.ToString().ToUpper(), result.Context.ReadPropertyString<TransformSetID>());
			Assert.AreEqual("TEST", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestRunMultipleTransformation_Error()
		{
			var testMessage = MessageFactory.CreateMessage();
			testMessage.Context = MessageFactory.CreateMessageContext();
			var mockTransformationPerformer = MockRepository.GenerateMock<ITransformationPerformer>();
			string lastTargetMessageType = String.Empty;
			mockTransformationPerformer.Stub(x => x.PerformTransformation(new List<TransformSet>(), null, testMessage, out lastTargetMessageType)).Throw(new InvalidOperationException());

			var testMultipleTransformationComponent = MockRepository.GeneratePartialMock<MultipleTransformationComponent>();
			testMultipleTransformationComponent.Enabled = true;
			testMultipleTransformationComponent.Stub(x => x.GetTransformationPerformer()).Return(mockTransformationPerformer);

			testMultipleTransformationComponent.Execute(null, testMessage);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(TimeoutException))]
		public void TestRunMultipleTransformation_Timeout()
		{
			var testMessage = MessageFactory.CreateMessage();
			testMessage.Context = MessageFactory.CreateMessageContext();
			var mockTransformationPerformer = MockRepository.GenerateMock<ITransformationPerformer>();
			string lastTargetMessageType = String.Empty;
			mockTransformationPerformer.Stub(x => x.PerformTransformation(new List<TransformSet>(), null, testMessage, out lastTargetMessageType)).Do(
				new PerformTransformationDelegate((List<TransformSet> transformList, IPipelineContext context, IBaseMessage messageToTransform, out string messageType) =>
				{
					messageType = string.Empty;
					Thread.Sleep(200);
					return messageToTransform;
				}));

			var testMultipleTransformationComponent = MockRepository.GeneratePartialMock<MultipleTransformationComponent>();
			testMultipleTransformationComponent.Enabled = true;
			testMultipleTransformationComponent.Timeout = 100;
			testMultipleTransformationComponent.Stub(x => x.GetTransformationPerformer()).Return(mockTransformationPerformer);

			testMultipleTransformationComponent.Execute(null, testMessage);
		}
			
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFindTransformTypes()
		{
			var message_1 = MessageFactory.CreateMessage();
			message_1.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message_1.BodyPart.Data = GetEmbeddedResource("TestFiles.850_00.xml");
			message_1.Context = MessageFactory.CreateMessageContext();
			message_1.Context.WriteProperty<BTS.SourceParty>("sender");
			message_1.Context.WriteProperty<BTS.DestinationParty>("recipient");
			message_1.Context.WriteProperty<BTS.MessageType>("sourceMessageType");

			var message_2 = MessageFactory.CreateMessage();
			message_2.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message_2.BodyPart.Data = GetEmbeddedResource("TestFiles.850_03.xml");
			message_2.Context = MessageFactory.CreateMessageContext();
			message_2.Context.WriteProperty<BTS.SourceParty>("sender");
			message_2.Context.WriteProperty<BTS.DestinationParty>("recipient");
			message_2.Context.WriteProperty<BTS.MessageType>("sourceMessageType");

			var message_3 = MessageFactory.CreateMessage();
			message_3.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message_3.BodyPart.Data = GetEmbeddedResource("TestFiles.850_05.xml");
			message_3.Context = MessageFactory.CreateMessageContext();
			message_3.Context.WriteProperty<BTS.SourceParty>("sender");
			message_3.Context.WriteProperty<BTS.DestinationParty>("recipient");
			message_3.Context.WriteProperty<BTS.MessageType>("sourceMessageType");

			var targetSchema_1 = new Schema("name_1, namespace_1");
			var targetSchema_2 = new Schema("name_2, namespace_2");

			string lastTargetMessageType = string.Empty;
			var transformaSetID1 = Guid.NewGuid();
			var transformaSetID2 = Guid.NewGuid();
			var transformSets = new List<TransformSet>() {
				new TransformSet(new List<TransformDetail>() { new TransformDetail(transformaSetID1, targetSchema_1.SchemaName,targetSchema_1.TargetNamespace)}, "//*[local-name()='BEG'][BEG01!='03']") ,
				new TransformSet(new List<TransformDetail>() { new TransformDetail(transformaSetID2, targetSchema_2.SchemaName,targetSchema_2.TargetNamespace)}, "//*[local-name()='BEG'][BEG01='03']") ,
			};

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			Expect.Call(accessor.SelectTransformsByPartiesMessage("sender", "recipient", "sourceMessageType", null)).Return(transformSets).Repeat.Times(3);

			var component = MockRepository.PartialMock<MultipleTransformationComponent>();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor).Repeat.Any();

			MockRepository.ReplayAll();

			List<TransformSet> result = null;
			result = component.FindTransformationTypes(message_1);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(result[0], transformSets[0]);

			result = component.FindTransformationTypes(message_2);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(result[0], transformSets[1]);

			result = component.FindTransformationTypes(message_3);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(result[0], transformSets[0]);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_MultipleTransformationComponent()
		{
			var component = new MultipleTransformationComponent();
			Assert.AreEqual("Perform multiple transformations to target format.", component.Description);
			Assert.AreEqual("Multiple Transformation", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("94B0793D-3BBE-42df-B13B-0C44A16E4E15"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			object enabledPtr = false;
			bool enabledValue = true;
			string timeoutProp = null;
			object timeoutPtr = System.Threading.Timeout.Infinite;
			int timeoutValue = 0;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Write("Timeout", ref timeoutPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { timeoutProp = propName; timeoutValue = (int)ptrVar; }));
			Expect.Call(() => propertyBag.Read("Timeout", out timeoutPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = 1000; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.Timeout = System.Threading.Timeout.Infinite;
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.IsFalse(enabledValue);
			Assert.AreEqual("Timeout", timeoutProp);
			Assert.AreEqual(System.Threading.Timeout.Infinite, timeoutValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.AreEqual(1000, component.Timeout);

			MockRepository.VerifyAll();
		}
	}
}
