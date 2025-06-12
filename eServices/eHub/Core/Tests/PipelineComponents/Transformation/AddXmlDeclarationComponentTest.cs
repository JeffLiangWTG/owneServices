using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class AddXmlDeclarationComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAddXmlDeclaration()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			var message1 = MessageFactory.CreateMessage();

			var assempleHelper = MockRepository.StrictMock<XmlAssembleHelper>();
			Expect.Call(assempleHelper.Assemble(pipelineContext, message)).Return(message1);

			var component = MockRepository.StrictMock<AddXmlDeclarationComponent>();
			Expect.Call(component.GetAssembleHelper()).Return(assempleHelper);

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message, result);

			component.Enabled = true;
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(message1, result);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_AddXmlDeclarationComponent()
		{
			var component = new AddXmlDeclarationComponent();
			Assert.AreEqual("Adds an xml declaration header to xml document.", component.Description);
			Assert.AreEqual("Add XML Declaration", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("25184F9C-BD9D-42d0-B2DC-6C1F38C73A44"), guid);

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
