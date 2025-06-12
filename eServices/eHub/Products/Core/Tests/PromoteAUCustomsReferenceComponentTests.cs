using System;
using System.Transactions;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class PromoteAUCustomsReferenceComponentTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAUCustomsEnabled()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Text.AUCustomsReply.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = new PromoteAUCustomsReferenceComponent();

			component.Enabled = false;
			component.Execute(pipelineContext, message);

			Assert.AreEqual(message.BodyPart.Data, message.BodyPart.Data);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAUCustomsExtractMessageReference()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Text.AUCustomsReply.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.GenerateStrictMock<PromoteAUCustomsReferenceComponent>();
			component.Expect(x => x.GetTransactionScope(Arg<IPipelineContext>.Is.Anything)).Return(new TransactionScope());

			component.Enabled = true;
			component.Execute(pipelineContext, message);

			Assert.AreEqual("AAL364P", message.Context.ReadPropertyString<Reference>());

			component.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_PromoteAUCustomsReferenceComponent()
		{
			var component = new PromoteAUCustomsReferenceComponent();

			Assert.AreEqual("Product: Promote AUCustoms reference component", component.Description);
			Assert.AreEqual("Product: Promote AUCustoms reference component", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}
	}
}


