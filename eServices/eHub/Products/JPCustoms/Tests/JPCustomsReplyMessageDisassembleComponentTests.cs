using System;
using System.IO;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Products.JPCustoms.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class JPCustomsReplyMessageDisassembleComponentTests : PipelineComponentBaseTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsReplyMessageDisassembleComponent_Properties()
		{
			var component = new JPCustomsReplyMessageDisassembleComponent();

			Assert.AreEqual("Disassemble JPCustoms Reply message", component.Description);
			Assert.AreEqual("Disassemble JPCustoms Reply message", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsReplyMessageDisassembleComponent_Disabled()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<JPCustomsReplyMessageDisassembleComponent>();

			MockRepository.ReplayAll();
			var originalStream = message.BodyPart.Data;

			component.Enabled = false;
			component.Disassemble(pipelineContext, message);
			var outputMessage = component.GetNext(pipelineContext);

			using (var resultReader = new StreamReader(outputMessage.BodyPart.GetOriginalDataStream()))
			{
				Assert.AreEqual(GetEmbeddedResourceAsString("TestFiles.JPCustomsReplyMessage.txt"), resultReader.ReadToEnd());
			}
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsReplyMessageDisassembleComponent_Enabled()
		{
			var pipelineContext = new PipelineContext();

			#region input test files

			var SAHRInput = MessageFactory.CreateMessage();
			SAHRInput.Context = MessageFactory.CreateMessageContext();
			SAHRInput.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAHRInput.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.AHRResponseFlatFile.input_1.txt");

			var SAS111Input = MessageFactory.CreateMessage();
			SAS111Input.Context = MessageFactory.CreateMessageContext();
			SAS111Input.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS111Input.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SAS111.SAS111_DNL.txt");

			var SAS112Input = MessageFactory.CreateMessage();
			SAS112Input.Context = MessageFactory.CreateMessageContext();
			SAS112Input.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS112Input.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SAS111.SAS112_DNL.txt");

			var SAS108Input = MessageFactory.CreateMessage();
			SAS108Input.Context = MessageFactory.CreateMessageContext();
			SAS108Input.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS108Input.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SAS108.sample_sas108.txt");

			var SAS135Input = MessageFactory.CreateMessage();
			SAS135Input.Context = MessageFactory.CreateMessageContext();
			SAS135Input.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS135Input.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SAS135.mock_sas135_2.txt");

			var TCCOutputInput = MessageFactory.CreateMessage();
			TCCOutputInput.Context = MessageFactory.CreateMessageContext();
			TCCOutputInput.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			TCCOutputInput.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.TCC.TCCOutput.txt");

			var SystemCommonErrorInput = MessageFactory.CreateMessage();
			SystemCommonErrorInput.Context = MessageFactory.CreateMessageContext();
			SystemCommonErrorInput.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SystemCommonErrorInput.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SystemCommonErrorResponseFlatFile.input_1.txt");

			var SAS148Input = MessageFactory.CreateMessage();
			SAS148Input.Context = MessageFactory.CreateMessageContext();
			SAS148Input.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS148Input.BodyPart.Data = GetEmbeddedResource("SchemaTests_Input.SAS148.sample_sas148.txt");

			#endregion

			#region output test files

			var SAHRResult = MessageFactory.CreateMessage();
			SAHRResult.Context = MessageFactory.CreateMessageContext();
			SAHRResult.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAHRResult.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.AHRResponseFlatFile.output_1.xml");

			var SAS111Result = MessageFactory.CreateMessage();
			SAS111Result.Context = MessageFactory.CreateMessageContext();
			SAS111Result.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS111Result.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.SAS111.SAS111_DNLxml.xml");

			var SAS108Result = MessageFactory.CreateMessage();
			SAS108Result.Context = MessageFactory.CreateMessageContext();
			SAS108Result.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS108Result.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.SAS108.sample_sas108.xml");

			var SAS135Result = MessageFactory.CreateMessage();
			SAS135Result.Context = MessageFactory.CreateMessageContext();
			SAS135Result.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS135Result.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.SAS135.mock_sas135.xml");

			var TCCOutputResult = MessageFactory.CreateMessage();
			TCCOutputResult.Context = MessageFactory.CreateMessageContext();
			TCCOutputResult.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			TCCOutputResult.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.TCC.TCCOutput_XML.xml");

			var SystemCommonErrorResult = MessageFactory.CreateMessage();
			SystemCommonErrorResult.Context = MessageFactory.CreateMessageContext();
			SystemCommonErrorResult.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SystemCommonErrorResult.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.SystemCommonErrorResponseFlatFile.output_1.xml");

			var SAS148Result = MessageFactory.CreateMessage();
			SAS148Result.Context = MessageFactory.CreateMessageContext();
			SAS148Result.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			SAS148Result.BodyPart.Data = GetEmbeddedResource("SchemaTests_Output.SAS148.sample_sas148.xml");

			#endregion

			var mock = new Mock<IFFDisassembleHelper>();
			mock.Setup(a => a.Disassemble(It.IsAny<IPipelineContext>(), It.IsAny<IBaseMessage>(), It.IsAny<SchemaWithNone>())).Returns<IPipelineContext, IBaseMessage, SchemaWithNone>(
(ptx, msg, schema) =>
{
	if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.AHRResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SAHRResult;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.SAS111FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SAS111Result;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.SAS108FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SAS108Result;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.SAS135FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SAS135Result;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.TCCOutputFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return TCCOutputResult;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas.SystemCommonErrorResponseFlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SystemCommonErrorResult;
	else if (schema.SchemaName == "CargoWise.eHub.Products.JPCustoms.Schemas._2017.SAS148FlatFileSchema, CargoWise.eHub.Products.JPCustoms.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")
		return SAS148Result;
	else
		return null;
});

			var componentBase = new Mock<JPCustomsReplyMessageDisassembleComponent>();
			componentBase.CallBase = true;
			componentBase.Setup(a => a.GetFFDisassembler()).Returns(mock.Object);
			var component = componentBase.Object;
			IBaseMessage outputMessage = null;
			component.Enabled = true;

			component.Disassemble(pipelineContext, SAHRInput);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAHRResult, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SAS111Input);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAS111Result, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SAS112Input);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAS111Result, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SAS108Input);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAS108Result, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SAS135Input);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAS135Result, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, TCCOutputInput);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(TCCOutputResult, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SystemCommonErrorInput);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SystemCommonErrorResult, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));

			component.Disassemble(pipelineContext, SAS148Input);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(SAS148Result, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ApplicationException), "The message is not recognized as a proper JPCustoms reply message")]
		public void JPCustomsReplyMessageDisassembleComponent_UnrecognizedMessage()
		{
			var pipelineContext = new PipelineContext();

			var randomInput = MessageFactory.CreateMessage();
			randomInput.Context = MessageFactory.CreateMessageContext();
			randomInput.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			var randomStrem = new MemoryStream();
			randomStrem.Write(System.Text.Encoding.ASCII.GetBytes("TEST"), 0, 4);
			randomStrem.Seek(0, SeekOrigin.Begin);
			randomInput.BodyPart.Data = randomStrem;

			var testOutput = MessageFactory.CreateMessage();

			var mock = new Mock<IFFDisassembleHelper>();
			mock.Setup(a => a.Disassemble(It.IsAny<IPipelineContext>(), It.IsAny<IBaseMessage>(), It.IsAny<SchemaWithNone>())).Returns(testOutput);

			var componentBase = new Mock<JPCustomsReplyMessageDisassembleComponent>();
			componentBase.CallBase = true;
			componentBase.Setup(a => a.GetFFDisassembler()).Returns(mock.Object);
			var component = componentBase.Object;
			IBaseMessage outputMessage = null;
			component.Enabled = true;

			component.Disassemble(pipelineContext, randomInput);
			outputMessage = component.GetNext(pipelineContext);
			Assert.AreNotEqual(testOutput, outputMessage);
			Assert.IsNull(component.GetNext(pipelineContext));
		}
	}
}
