using System;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class AssignMessageTrackingIDAndApplicationCodeProductComponentTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignMessageTrackingID()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			var component = new AssignMessageTrackingIDAndApplicationCodeProductComponent();
			component.Enabled = false;
			component.OverrideExisting = false;
			component.ApplicationCode = "NZC";
			component.Execute(pipelineContext, message);
			Assert.IsNull(message.Context.ReadPropertyString<MessageTrackingID>());

			message.Context.WriteProperty<MessageTrackingID>("FC0E921C-A404-48ED-801C-A26863712C34");
			component.Enabled = true;
			component.OverrideExisting = false;
			component.Execute(pipelineContext, message);
			Assert.AreEqual("NZC", message.Context.ReadPropertyString<ApplicationCode>());
			Assert.AreEqual("FC0E921C-A404-48ED-801C-A26863712C34", message.Context.ReadPropertyString<MessageTrackingID>());

			component.Enabled = true;
			component.OverrideExisting = true;
			component.Execute(pipelineContext, message);
			Assert.AreNotEqual("FC0E921C-A404-48ED-801C-A26863712C34", message.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual("NZC", message.Context.ReadPropertyString<ApplicationCode>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_AssignMessageTrackingIDAndApplicationCodeComponent()
		{
			var component = new AssignMessageTrackingIDAndApplicationCodeProductComponent();

			Assert.AreEqual("Product: Assign MessageTrackingID and Application Code", component.Description);
			Assert.AreEqual("Product: Assign MessageTrackingID and Application Code", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}
	}
}
