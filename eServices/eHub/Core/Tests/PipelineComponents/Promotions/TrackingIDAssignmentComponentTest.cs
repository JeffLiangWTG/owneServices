using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class TrackingIDAssignmentComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssignTrackingID()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			var component = new TrackingIDAssignmentComponent();

			component.AssignSenderTrackingID = false;
			component.AssignNewInternalTrackingID = false;
			var BTSTrackngID = Guid.NewGuid().ToString().ToUpper();
			message.Context.WriteProperty<BTS.InterchangeID>(BTSTrackngID);
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual(BTSTrackngID, result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.IsNull(result.Context.ReadPropertyString<MessageTrackingID>());

			message.Context.WriteProperty<InternalTrackingID>("1111");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual("1111", result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.IsNull(result.Context.ReadPropertyString<MessageTrackingID>());

			component.AssignSenderTrackingID = true;
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual("1111", result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual("1111", result.Context.ReadPropertyString<MessageTrackingID>());
			Assert.AreEqual(message, result);

			//InternalTrackingID read from message data
			message.Context.WriteProperty<InternalTrackingID>(null);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.TypedPolling.xml");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual("e8d5c18a-5094-4437-a5b6-5c373dc659f6", result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(0, message.BodyPart.GetOriginalDataStream().Position);

			message.Context.WriteProperty<InternalTrackingID>(null);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FSUAssembleResult.txt");
			message.BodyPart.GetOriginalDataStream().Position = 3;
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual(BTSTrackngID, result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual(3, message.BodyPart.GetOriginalDataStream().Position);

			message.Context.WriteProperty<InternalTrackingID>(null);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FSUAssembleResult.txt");
			message.BodyPart.GetOriginalDataStream().Position = 3;
			component.AssignNewInternalTrackingID = true;
			component.InternalTrackingID = () => new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
			result = component.Execute(pipelineContext, message);
			Assert.AreEqual("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", result.Context.ReadPropertyString<InternalTrackingID>());
			Assert.AreEqual("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", result.Context.ReadPropertyString<MessageTrackingID>());
		}
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_TrackingIDAssignmentComponent()
		{
			var component = new TrackingIDAssignmentComponent();
			Assert.AreEqual("Assigns an internal tracking ID + an optional sender tracking ID if not supplied in the message", component.Description);
			Assert.AreEqual("Tracking ID Assignment", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("9C077BC9-B0FF-47fc-81F4-75952844483D"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string assignSenderProp = string.Empty;
			string assignNewTrackingProp = string.Empty;
            string logMessageDetailsProp = string.Empty;

            object valuePtr = false;
			bool valueAssignSenderProp = true;
			bool valueAssignNewTrackingProp = true;
            bool valueLogMessageDetailsProp = true;

            Expect.Call(() => propertyBag.Write("AssignSenderTrackingID", ref valuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { assignSenderProp = propName; valueAssignSenderProp = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("AssignSenderTrackingID", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			Expect.Call(() => propertyBag.Write("AssignNewInternalTrackingID", ref valuePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { assignNewTrackingProp = propName; valueAssignNewTrackingProp = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("AssignNewInternalTrackingID", out valuePtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

            Expect.Call(() => propertyBag.Write("LogMessageDetails", ref valuePtr))
                .Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { logMessageDetailsProp = propName; valueLogMessageDetailsProp = (bool)ptrVar; }));

            Expect.Call(() => propertyBag.Read("LogMessageDetails", out valuePtr, 0)).
                Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

            MockRepository.ReplayAll();
			component.Save(propertyBag, true, true);

            Assert.AreEqual("AssignSenderTrackingID", assignSenderProp);
			Assert.AreEqual("AssignNewInternalTrackingID", assignNewTrackingProp);
            Assert.AreEqual("LogMessageDetails", logMessageDetailsProp);

            Assert.IsFalse(valueAssignSenderProp);
			Assert.IsFalse(valueAssignNewTrackingProp);
            Assert.IsFalse(valueLogMessageDetailsProp);

            component.Load(propertyBag, 0);
			Assert.IsTrue(component.AssignSenderTrackingID);
			Assert.IsTrue(component.AssignNewInternalTrackingID);
            Assert.IsTrue(component.LogMessageDetails);

            MockRepository.VerifyAll();
		}
	}
}
