using System;
using CargoWise.eHub.Core.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class RenameSourceFileNameExtensionAndRemoveContentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPromoteStaticValue()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("newMessage", MessageFactory.CreateMessagePart(), true);
			message.Context = MessageFactory.CreateMessageContext();

			var component = new RenameSourceFileNameExtensionAndRemoveContent();
			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "PropertyType must be specified.");

			component.PropertyType = "Crap";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Extension must be specified.");

			component.Extension = "ok";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(FormatException),"Unexpected name namespace format (Crap).  Expected format is ^([^#]+)#([^#]+$)");

			message.Context.WriteProperty<FTP.ReceivedFileName>("");
			var property = new FTP.ReceivedFileName();
			component.PropertyType = string.Format("{0}#{1}", property.Name.Namespace, property.Name.Name);
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "provided PropertyType could not be found.");

			message.Context.WriteProperty<FTP.ReceivedFileName>("aaa.xml");
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("aaa.ok", message.Context.ReadPropertyString<FTP.ReceivedFileName>());
			Assert.AreEqual(ContextPropertyType.PropPromoted, message.Context.GetPropertyType<FTP.ReceivedFileName>());
			Assert.AreEqual(message, result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_RenameSourceFileNameExtensionAndRemoveContent()
		{
			var component = new RenameSourceFileNameExtensionAndRemoveContent();
			Assert.AreEqual("", component.Description);
			Assert.AreEqual("Rename Source File Extension and Remove Content", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("923BEFDE-9F41-467F-96DF-32DE86EE7719"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string enabledProp = null;
			string propertyTypeProp = null;
			string extensionProp = null;
			object enabledPtr = false;
			object propertyTypePtr = "PropType";
			object extensionPtr = "Extension";
			bool enabledValue = true;
			string propertyTypeValue = string.Empty;
			string extensionValue = string.Empty;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("PropertyType", ref propertyTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { propertyTypeProp = propName; propertyTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("Extension", ref extensionPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { extensionProp = propName; extensionValue = (string)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("PropertyType", out propertyTypePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "PropType_different"; }));
			Expect.Call(() => propertyBag.Read("Extension", out extensionPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Extension_different"; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.PropertyType = "PropType";
			component.Extension = "Extension";
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.AreEqual("PropertyType", propertyTypeProp);
			Assert.AreEqual("Extension", extensionProp);
			Assert.IsFalse(enabledValue);
			Assert.AreEqual("PropType", propertyTypeValue);
			Assert.AreEqual("Extension", extensionValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.AreEqual("PropType_different", component.PropertyType);
			Assert.AreEqual("Extension_different", component.Extension);

			MockRepository.VerifyAll();
		}
	}
}
