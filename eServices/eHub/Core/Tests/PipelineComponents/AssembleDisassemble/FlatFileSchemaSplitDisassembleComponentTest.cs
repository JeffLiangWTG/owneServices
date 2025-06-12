using System;
using System.Collections;
using System.IO;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class FlatFileSchemaResolveDisassembleComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFlatFileSchemaResolveAndDisassemble()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.YASImportChargesAndCosts.xml");

			var ffMessage1 = MessageFactory.CreateMessage();
			var ffMessage2 = MessageFactory.CreateMessage();

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name, namespace"))).Return(ffMessage).Repeat.Twice();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(null);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(ffMessage1);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name2, namespace2"))).Return(ffMessage2);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var flatFileSplitter = MockRepository.StrictMock<IFlatFileSplitter>();
			Expect.Call(flatFileSplitter.Split(message, pipelineContext)).Return(new IBaseMessage[] { message }).Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileSchemaSplitDisassembleComponent>();
			Expect.Call(component.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			Expect.Call(component.GetClone(pipelineContext, message)).Return(message).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();
			Expect.Call(component.GetFlatFileSplitter()).Return(flatFileSplitter).Repeat.Any();

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(message, component.GetNext(pipelineContext));

			component.Enabled = true;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "InitialSchema must be specified");

			component.InitialSchema = new Schema("name, namespace");
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "SplitElementXPath must be specified");

			component.SplitElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "ValueList must be specified");

			component.ValueList = "AAA;BBB;CCC";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "SchemaNameList must be specified");

			component.SchemaNameList = "name1, namespace1;name2, namespace2";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "DebatchElementXPath must be specified");

			component.DebatchElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='CompanyID' and namespace-uri()='']";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "ChildElementName must be specified");

			component.ChildElementName = "ImportChargeOrCost";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Number of schemas must equals number of values");

			component.ValueList = "AAA;BBB";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Unable to disassemble message based on schema 'name1, namespace1'");

			ffMessage.BodyPart.Data.SeekBegin();
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(ffMessage1, component.GetNext(pipelineContext));
			Assert.AreEqual(ffMessage2, component.GetNext(pipelineContext));
			Assert.AreEqual(ffMessage, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFlatFileSchemaResolveAndDisassemble_InitialSchemaEntryNotFound()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.YASImportChargesAndCosts1.xml");

			var ffMessage1 = MessageFactory.CreateMessage();
			var ffMessage2 = MessageFactory.CreateMessage();

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name, namespace"))).Return(ffMessage);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(ffMessage1);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name2, namespace2"))).Return(ffMessage2);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var flatFileSplitter = MockRepository.StrictMock<IFlatFileSplitter>();
			Expect.Call(flatFileSplitter.Split(message, pipelineContext)).Return(new IBaseMessage[] { message }).Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileSchemaSplitDisassembleComponent>();
			Expect.Call(component.GetFFDisassembler()).Return(ffDisassembler);
			Expect.Call(component.GetClone(pipelineContext, message)).Return(message).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();
			Expect.Call(component.GetFlatFileSplitter()).Return(flatFileSplitter).Repeat.Any();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.InitialSchema = new Schema("name, namespace");
			component.SplitElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']";
			component.SchemaNameList = "name1, namespace1;name2, namespace2";
			component.ValueList = "AAA;BBB";
			component.DebatchElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='CompanyID' and namespace-uri()='']";
			component.ChildElementName = "ImportChargeOrCost";

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(ffMessage1, component.GetNext(pipelineContext));
			Assert.AreEqual(ffMessage2, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_FlatFileSchemaResolveDisassembpleComponent()
		{
			var component = new FlatFileSchemaSplitDisassembleComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("Flat File Schema Split Disassembler", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("C31EA9ED-988C-4CF6-A1BC-FDEA8C487315"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			string initialSchemaProp = null;
			string splitElementXPathProp = null;
			string valueListProp = null;
			string schemaNameListProp = null;
			string processSubscriptionsProp = null;
			string debatchElementXPathProp = null;
			string childElementNameProp = null;

			bool enabledValue = false;
			string initialSchemaValue = string.Empty;
			string splitElementXPathValue = string.Empty;
			string valueListValue = string.Empty;
			string schemaNameListValue = string.Empty;
			bool processSubscriptionsValue = false;
			string debatchElementXPathValue = string.Empty;
			string childElementNameValue = string.Empty;

			object enabledPtr = false;
			object initialSchemaPtr = "Mess, Schema";
			object splitElementXPathPtr = "XPath";
			object valueListPtr = "blah;blah";
			object schemaNameListPtr = "schm;schm";
			object processSubscriptionsPtr = false;
			object debatchElementXPathPtr = "debatchXPath";
			object childElementNamePtr = "ElementName";


			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("InitialSchema", ref initialSchemaPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { initialSchemaProp = propName; initialSchemaValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("SplitElementXPath", ref splitElementXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { splitElementXPathProp = propName; splitElementXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ValueList", ref valueListPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { valueListProp = propName; valueListValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("SchemaNameList", ref schemaNameListPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { schemaNameListProp = propName; schemaNameListValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ProcessSubscriptions", ref processSubscriptionsPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { processSubscriptionsProp = propName; processSubscriptionsValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DebatchElementXPath", ref debatchElementXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { debatchElementXPathProp = propName; debatchElementXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ChildElementName", ref childElementNamePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { childElementNameProp = propName; childElementNameValue = (string)ptrVar; }));


			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("InitialSchema", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Mess, Schema different"; }));
			Expect.Call(() => propertyBag.Read("SplitElementXPath", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "XPath_different"; }));
			Expect.Call(() => propertyBag.Read("ValueList", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "blah diff;blah diff"; }));
			Expect.Call(() => propertyBag.Read("SchemaNameList", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "schm diff;schm diff"; }));
			Expect.Call(() => propertyBag.Read("ProcessSubscriptions", out processSubscriptionsPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("DebatchElementXPath", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "debatchXPath diff"; }));
			Expect.Call(() => propertyBag.Read("ChildElementName", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "ElementName diff"; }));

			

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.InitialSchema = new Schema("Mess, Schema");
			component.SplitElementXPath = "XPath";
			component.ValueList = "blah;blah";
			component.SchemaNameList = "schm;schm";
			component.ProcessSubscriptions = false;
			component.DebatchElementXPath = "debatchXPath";
			component.ChildElementName = "ElementName";
			
			component.Save(propertyBag, true, true);

			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("InitialSchema", initialSchemaProp);
			Assert.AreEqual("SplitElementXPath", splitElementXPathProp);
			Assert.AreEqual("ValueList", valueListProp);
			Assert.AreEqual("SchemaNameList", schemaNameListProp);
			Assert.AreEqual("ProcessSubscriptions", processSubscriptionsProp);
			Assert.AreEqual("DebatchElementXPath", debatchElementXPathProp);
			Assert.AreEqual("ChildElementName", childElementNameProp);

			Assert.IsFalse(enabledValue);
			Assert.AreEqual("Mess, Schema", initialSchemaValue);
			Assert.AreEqual("XPath", splitElementXPathValue);
			Assert.AreEqual("blah;blah", valueListValue);
			Assert.AreEqual("schm;schm", schemaNameListValue);
			Assert.IsFalse(processSubscriptionsValue);
			Assert.AreEqual("debatchXPath", debatchElementXPathValue);
			Assert.AreEqual("ElementName", childElementNameValue);
			
			component.Load(propertyBag, 0);

			Assert.IsTrue(component.Enabled);
			Assert.AreEqual(new Schema("Mess, Schema different"), component.InitialSchema);
			Assert.AreEqual("XPath_different", component.SplitElementXPath);
			Assert.AreEqual("blah diff;blah diff", component.ValueList);
			Assert.AreEqual("schm diff;schm diff", component.SchemaNameList);
			Assert.IsTrue(component.ProcessSubscriptions);
			Assert.AreEqual("debatchXPath diff", component.DebatchElementXPath);
			Assert.AreEqual("ElementName diff", component.ChildElementName);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDuplicateSchemasInSchemaListDoesNotCauseDuplicateMessageDisassemble()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.YASImportChargesAndCosts.xml");

			var ffMessage1 = MessageFactory.CreateMessage();
			var ffMessage2 = MessageFactory.CreateMessage();

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name, namespace"))).Return(ffMessage).Repeat.Twice();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(null);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(ffMessage1);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name2, namespace2"))).Return(ffMessage2);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var flatFileSplitter = MockRepository.StrictMock<IFlatFileSplitter>();
			Expect.Call(flatFileSplitter.Split(message, pipelineContext)).Return(new IBaseMessage[] { message }).Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileSchemaSplitDisassembleComponent>();
			Expect.Call(component.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			Expect.Call(component.GetClone(pipelineContext, message)).Return(message).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();
			Expect.Call(component.GetFlatFileSplitter()).Return(flatFileSplitter).Repeat.Any();

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(message, component.GetNext(pipelineContext));

			component.Enabled = true;
			component.InitialSchema = new Schema("name, namespace");
			component.SplitElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']";
			component.ValueList = "AAA;BBB";
			component.SchemaNameList = "name1, namespace1;name1, namespace1";
			component.DebatchElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='CompanyID' and namespace-uri()='']";
			component.ChildElementName = "ImportChargeOrCost";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Unable to disassemble message based on schema 'name1, namespace1'");

			ffMessage.BodyPart.Data.SeekBegin();
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(ffMessage1, component.GetNext(pipelineContext));
			Assert.AreEqual(ffMessage, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDuplicateSchemasInSchemaListDoesNotCauseDuplicateMessageDisassemble_ForMoreThanTwoValues()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.YASImportChargesAndCosts_CSL.xml");

			var ffMessage1 = MessageFactory.CreateMessage();

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name, namespace"))).Return(ffMessage);
			Expect.Call(ffDisassembler.Disassemble(pipelineContext, message, new SchemaWithNone("name1, namespace1"))).Return(ffMessage1);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var flatFileSplitter = MockRepository.StrictMock<IFlatFileSplitter>();
			Expect.Call(flatFileSplitter.Split(message, pipelineContext)).Return(new IBaseMessage[] { message }).Repeat.Any();

			var component = MockRepository.PartialMock<FlatFileSchemaSplitDisassembleComponent>();
			Expect.Call(component.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			Expect.Call(component.GetClone(pipelineContext, message)).Return(message).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();
			Expect.Call(component.GetFlatFileSplitter()).Return(flatFileSplitter).Repeat.Any();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.InitialSchema = new Schema("name, namespace");
			component.SplitElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']";
			component.ValueList = "PAY;CST;CSL";
			component.SchemaNameList = "name1, namespace1;name1, namespace1;name1, namespace1";
			component.DebatchElementXPath = "/*[local-name()='ImportChargesAndCosts' and namespace-uri()='http://cargowise.com/ehub/clients/yas/2011/02']/*[local-name()='ImportChargeOrCost' and namespace-uri()='']/*[local-name()='CompanyID' and namespace-uri()='']";
			component.ChildElementName = "ImportChargeOrCost";

			ffMessage.BodyPart.Data.SeekBegin();
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(ffMessage1, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));
		}
	}
}
