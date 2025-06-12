using System;
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
	public class ReplaceMessageBodyComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDynamicDebatch()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.SelectOutboxToDeliverResult.xml");
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = new PipelineContext();

			var component = new ReplaceMessageBodyComponent();
			component.Enabled = true;

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.eBond.xml"), result.BodyPart.GetOriginalDataStream());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_DynamicDebatchComponent()
		{
			var component = new ReplaceMessageBodyComponent();
			Assert.AreEqual("Replace SelectOutboxMessageToDeliver result message Body", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("BA6E91C7-3AC8-4525-8B7D-776E8C026117"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;

			bool enabledValue = false;

			object enabledPtr = false;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;

			component.Save(propertyBag, true, true);
			
			Assert.AreEqual("Enabled", enabledProp);

			Assert.IsFalse(enabledValue);			

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}
	}
}
