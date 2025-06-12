using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class StreamWrapperComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWrapStream()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var component = new StreamWrapperComponent();
			message.BodyPart.Data = new MemoryStream();
			component.DiscardEmptyMessages = true;
			var result = component.Execute(pipelineContext, message);
			Assert.IsNull(result);

			message.BodyPart.Data = new MemoryStream();
			component.DiscardEmptyMessages = false;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Zero-byte file received with name ''.");
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWrapStream_ShouldRemoveBOMFromFileWithBOM_WhenPreserveBOMEnabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.InterchangeXML_WithBOM.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new StreamWrapperComponent();
			component.RemoveBOMEnabled = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(typeof(ReadOnlySeekableStream), message.BodyPart.GetOriginalDataStream().GetType());
			Assert.AreEqual(message, result);
			Assert.IsTrue(CompareByteResults(message.BodyPart.GetOriginalDataStream(), "TestFiles.InterchangeXML_WithoutBOM.xml"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWrapStream_ShouldNotRemoveBOMFromFileWithoutBOM_WhenPreserveBOMEnabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.InterchangeXML_WithoutBOM.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new StreamWrapperComponent();
			component.RemoveBOMEnabled = true;
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(typeof(ReadOnlySeekableStream), message.BodyPart.GetOriginalDataStream().GetType());
			Assert.AreEqual(message, result);
			Assert.IsTrue(CompareByteResults(message.BodyPart.GetOriginalDataStream(), "TestFiles.InterchangeXML_WithoutBOM.xml"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMaxMessageSize()
		{
			var pipelineContext = new PipelineContext();
			var message = MockRepository.GenerateMock<IBaseMessage>();
			message.Stub(x => x.GetSize(out Arg<ulong>.Out(1100000).Dummy, out Arg<bool>.Out(true).Dummy));
			var component = new StreamWrapperComponent() { MaxMessageSizeMB = 1 };

			try
			{
				var result = component.Execute(pipelineContext, message);
				Assert.Fail("Expected exception not received.");
			}
			catch (InvalidDataException ex) when (ex.Message == "Message exceeds maximum allowed size of 1 MB.")
			{
			}
			catch (Exception ex)
			{
				Assert.Fail($"Unexpected exception:\r\n{ex}");
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_StreamWrapperComponent()
		{
			var component = new StreamWrapperComponent();
			Assert.AreEqual("Used to wrap message streams so they become manageable when large.", component.Description);
			Assert.AreEqual("Stream Wrapper", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("04CA3170-C99B-4E24-B010-31FE118CD5D6"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string decodeValuesProp = null;
			string decodeEnabledProp = null;
			string removeBOMEnabledProp = null;
			string replacementEnabledProp = null;
			string replacementPositionProp = null;
			string replacementTextProp = null;
			string discardEmptyMessagesProp = null;
			string prependDataProp = null;
			string appendDataProp = null;
			string dakosyReaderProp = null;
			string removeNamespaceProp = null;
			string maxMessageSizeMBProp = null;

			string decodeValuesValue = string.Empty;
			bool decodeEnabledValue = false;
			bool removeBOMEnabledValue = false;
			bool replacementEnabledValue = false;
			int replacementPositionValue = 0;
			string replacementTextValue = string.Empty;
			bool discardEmptyMessagesValue = false;
			string prependDataValue = string.Empty;
			string appendDataValue = string.Empty;
			bool dakosyReaderValue = false;
			bool removeNamespaceValue = false;
			int maxMessageSizeMBValue = 0;

			object decodeValuesPtr = "decodeValues";
			object decodeEnabledPtr = false;
			object removeBOMEnabledPtr = false;
			object replacementEnabledPtr = false;
			object replacementPositionPtr = 0;
			object replacementTextPtr = "ReplacementText";
			object discardEmptyMessagesPtr = false;
			object prependDataPtr = "PrependData";
			object appendDataPtr = "AppendData";
			object dakosyReaderPtr = false;
			object removeNamespacePtr = false;
			object maxMessageSizeMBPtr = 0;

			Expect.Call(() => propertyBag.Write("DecodeValues", ref decodeValuesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { decodeValuesProp = propName; decodeValuesValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DecodeEnabled", ref decodeEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { decodeEnabledProp = propName; decodeEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("RemoveBOMEnabled", ref removeBOMEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { removeBOMEnabledProp = propName; removeBOMEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ReplacementEnabled", ref replacementEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { replacementEnabledProp = propName; replacementEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ReplacementPosition", ref replacementPositionPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { replacementPositionProp = propName; replacementPositionValue = (int)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ReplacementText", ref replacementTextPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { replacementTextProp = propName; replacementTextValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DiscardEmptyMessages", ref discardEmptyMessagesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { discardEmptyMessagesProp = propName; discardEmptyMessagesValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("PrependData", ref prependDataPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { prependDataProp = propName; prependDataValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("AppendData", ref appendDataPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { appendDataProp = propName; appendDataValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DakosyReaderEnabled", ref dakosyReaderPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { dakosyReaderProp = propName; dakosyReaderValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("RemoveNamespaceEnabled", ref removeNamespacePtr))
							.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { removeNamespaceProp = propName; removeNamespaceValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("MaxMessageSizeMB", ref maxMessageSizeMBPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { maxMessageSizeMBProp = propName; maxMessageSizeMBValue = (int)ptrVar; }));

			Expect.Call(() => propertyBag.Read("DecodeValues", out decodeValuesPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "sender_different"; }));
			Expect.Call(() => propertyBag.Read("DecodeEnabled", out decodeEnabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("RemoveBOMEnabled", out removeBOMEnabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("ReplacementEnabled", out replacementEnabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("ReplacementPosition", out replacementPositionPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = 1; }));
			Expect.Call(() => propertyBag.Read("ReplacementText", out replacementTextPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ReplacementText_2"; }));
			Expect.Call(() => propertyBag.Read("DiscardEmptyMessages", out discardEmptyMessagesPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("PrependData", out prependDataPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PrependData_2"; }));
			Expect.Call(() => propertyBag.Read("AppendData", out appendDataPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "AppendData_2"; }));
			Expect.Call(() => propertyBag.Read("DakosyReaderEnabled", out dakosyReaderPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("RemoveNamespaceEnabled", out removeNamespacePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("MaxMessageSizeMB", out maxMessageSizeMBPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = 1; }));

			MockRepository.ReplayAll();

			component.DecodeValues = "decodeValues";
			component.DecodeEnabled = false;
			component.RemoveBOMEnabled = false;
			component.ReplacementEnabled = false;
			component.ReplacementPosition = 0;
			component.ReplacementText = "ReplacementText";
			component.DiscardEmptyMessages = false;
			component.PrependData = "PrependData";
			component.AppendData = "AppendData";
			component.DakosyReaderEnabled = false;
			component.RemoveNamespaceEnabled = false;
			component.MaxMessageSizeMB = 0;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("DecodeValues", decodeValuesProp);
			Assert.AreEqual("DecodeEnabled", decodeEnabledProp);
			Assert.AreEqual("RemoveBOMEnabled", removeBOMEnabledProp);
			Assert.AreEqual("ReplacementEnabled", replacementEnabledProp);
			Assert.AreEqual("ReplacementPosition", replacementPositionProp);
			Assert.AreEqual("ReplacementText", replacementTextProp);
			Assert.AreEqual("DiscardEmptyMessages", discardEmptyMessagesProp);
			Assert.AreEqual("PrependData", prependDataProp);
			Assert.AreEqual("AppendData", appendDataProp);
			Assert.AreEqual("DakosyReaderEnabled", dakosyReaderProp);
			Assert.AreEqual("RemoveNamespaceEnabled", removeNamespaceProp);
			Assert.AreEqual("MaxMessageSizeMB", maxMessageSizeMBProp);

			Assert.AreEqual("decodeValues", decodeValuesValue);
			Assert.IsFalse(decodeEnabledValue);
			Assert.IsFalse(removeBOMEnabledValue);
			Assert.IsFalse(replacementEnabledValue);
			Assert.AreEqual(0, replacementPositionValue);
			Assert.AreEqual("ReplacementText",replacementTextValue);
			Assert.IsFalse(discardEmptyMessagesValue);
			Assert.AreEqual("PrependData", prependDataValue);
			Assert.AreEqual("AppendData", appendDataValue);
			Assert.IsFalse(dakosyReaderValue);
			Assert.IsFalse(removeNamespaceValue);
			Assert.AreEqual(0, maxMessageSizeMBValue);

			component.Load(propertyBag, 0);

			Assert.AreEqual("sender_different", component.DecodeValues);
			Assert.IsTrue(component.DecodeEnabled);
			Assert.IsTrue(component.RemoveBOMEnabled);
			Assert.IsTrue(component.ReplacementEnabled);
			Assert.AreEqual(1,component.ReplacementPosition);
			Assert.AreEqual("ReplacementText_2", component.ReplacementText);
			Assert.IsTrue(component.DiscardEmptyMessages);
			Assert.AreEqual("PrependData_2", component.PrependData);
			Assert.AreEqual("AppendData_2", component.AppendData);
			Assert.IsTrue(component.DakosyReaderEnabled);
			Assert.IsTrue(component.RemoveNamespaceEnabled);
			Assert.AreEqual(1,component.MaxMessageSizeMB);

			MockRepository.VerifyAll();
		}


		private bool CompareByteResults(Stream originalStream, string outputFile)
		{
			var originalStreamBuffer = new byte[originalStream.Length];
			originalStream.Read(originalStreamBuffer, 0, originalStreamBuffer.Length);

			var outputStream = GetEmbeddedResource(outputFile);
			var outputBuffer = new byte[outputStream.Length];
			outputStream.Read(outputBuffer, 0, outputBuffer.Length);

			return originalStreamBuffer.SequenceEqual(outputBuffer);
		}
	}
}
