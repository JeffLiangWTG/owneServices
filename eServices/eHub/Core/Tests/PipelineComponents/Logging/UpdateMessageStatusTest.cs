using System;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class UpdateMessageStatusTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUpdateMessageStatus()
		{
			var pipelineContext = new PipelineContext();
			string messageTrackingID = Guid.NewGuid().ToString();
			var message = MessageFactory.CreateMessage();
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");

			var outboxAccessor = MockRepository.StrictMock<IOutboxAccessor>();
			Expect.Call(() => { outboxAccessor.UpdateMessageStatus(null, null, null, null, null); }).IgnoreArguments()
				.Do(new Action<string, string, string, int?, int?>((m, ip, ok, i, o) =>
				{
					Assert.IsNull(i);
					Assert.AreEqual(1, o);
				}));

			var component = MockRepository.PartialMock<UpdateMessageStatus>();
			Expect.Call(component.GetOutboxAccessor()).Return(outboxAccessor);
			component.Enabled = true;
			component.InboxStatus = null;
			component.OutboxStatus = "1";

			MockRepository.ReplayAll();

			Assert.AreEqual(message, component.Execute(pipelineContext, message));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_UpdateMessageStatus()
		{
			var component = new UpdateMessageStatus();
			Assert.AreEqual("Update inbox and/or outbox message to specified statuses.", component.Description);
			Assert.AreEqual("Update Message Status", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			Guid guid;
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("04B6FDC9-4CE1-4656-9411-7AEA388AD780"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			object enabledPtr = false;
			bool enabledValue = true;
			string inboxStatusProp = null;
			object inboxStatusPtr = "3";
			string inboxStatusValue = null;
			string outboxStatusProp = null;
			object outboxStatusPtr = null;
			string outboxStatusValue = null;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			Expect.Call(() => propertyBag.Write("InboxStatus", ref inboxStatusPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { inboxStatusProp = propName; inboxStatusValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Read("InboxStatus", out inboxStatusPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = string.Empty; }));

			Expect.Call(() => propertyBag.Write("OutboxStatus", ref outboxStatusPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { outboxStatusProp = propName; outboxStatusValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Read("OutboxStatus", out outboxStatusPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "1"; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.InboxStatus = "3";
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.IsFalse(enabledValue);
			Assert.AreEqual("InboxStatus", inboxStatusProp);
			Assert.AreEqual("3", inboxStatusValue);
			Assert.AreEqual("OutboxStatus", outboxStatusProp);
			Assert.IsNull(outboxStatusValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);
			Assert.AreEqual(string.Empty, component.InboxStatus);
			Assert.AreEqual("1", component.OutboxStatus);

			MockRepository.VerifyAll();
		}
	}
}
