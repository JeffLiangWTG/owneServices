using System;
using System.Transactions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class InsertMessageReferenceProductComponentTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInsertMessageReferenceComponentEnabledFalse()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			var component = new InsertMessageReferenceProductComponent();

			component.Enabled = false;
			component.Execute(pipelineContext, message);
			Assert.IsNull(message.Context.ReadPropertyString<MessageTrackingID>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInsertMessageReferenceComponentApplicationCodeAndReferenceDefineAsProperies()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.NZCustoms.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var registryAccessor = MockRepository.GenerateStrictMock<IRegistryAccessor>();
			registryAccessor.Expect(x => x.InsertMessageReference("HEYDAUIKB", "NZC", "56DAD5B4-F797-499B-AFE7-9A020C07A7F7"));
			registryAccessor.Expect(x => x.InsertMessageReference("HEYDAUIKB", "ABC", "56DAD5B4-F797-499B-AFE7-9A020C07A7F8"));

			var component = MockRepository.GenerateStrictMock<InsertMessageReferenceProductComponent>();
			component.Expect(x => x.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			component.Expect(x => x.GetTransactionScope(Arg<IPipelineContext>.Is.Anything))
				.Do((Func<IPipelineContext, TransactionScope>)delegate(IPipelineContext dummy) { return new TransactionScope(); }); // need to recreate TransactionScope each call

			message.Context.WriteProperty<SenderID>("HEYDAUIKB");

			component.Enabled = false;
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			component.Reference = "56DAD5B4-F797-499B-AFE7-9A020C07A7F7";
			component.ApplicationCode = "NZC";
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			component.Reference = null;
			component.ApplicationCode = null;
			message.Context.WriteProperty<Reference>("56DAD5B4-F797-499B-AFE7-9A020C07A7F8");
			message.Context.WriteProperty<ApplicationCode>("ABC");
			component.Execute(pipelineContext, message);

			component.VerifyAllExpectations();
			registryAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_InsertMessageReferenceComponent()
		{
			var component = new InsertMessageReferenceProductComponent();

			Assert.AreEqual("", component.Description);
			Assert.AreEqual("Product: Insert Message Reference", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}
	}
}


