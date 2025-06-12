using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Bam.EventObservation;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
    [TestClass]
    public class UpdateDistributionStatusComponentTest : BaseComponentTest
    {
        delegate void UpdateStatusEventDelegate(IPersistQueryable statusEvent);

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUpdateDistributionStatus()
        {
            var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
            SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
            var eventStream = MockRepository.StrictMock<EventStream>();
            IPersistQueryable updateStatusEvent = null;
            Expect.Call(pipelineContext.GetEventStream()).Return(eventStream);
            Expect.Call(() => { eventStream.StoreCustomEvent(new UpdateStatusEvent(null, null, null, null)); }).IgnoreArguments()
                .Do(new UpdateStatusEventDelegate((IPersistQueryable statusEvent) => { updateStatusEvent = statusEvent; }));
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();

            var component = MockRepository.PartialMock<UpdateDistributionStatusComponent>();
            Expect.Call(component.GetOutboxAccessor()).Return(null).Repeat.Any();
            
            MockRepository.ReplayAll();

            component.Execute(pipelineContext, message);

            component.Enabled = true;
            message.Context.WriteProperty<MessageTrackingID>("11111");
            message.Context.WriteProperty<BTS.SourceParty>("sender");
            message.Context.WriteProperty<BTS.DestinationParty>("recipient");
            var result = component.Execute(pipelineContext, message);

            Assert.IsNotNull(updateStatusEvent);
            Assert.AreEqual(typeof(UpdateStatusEvent), updateStatusEvent.GetType());
            Assert.AreEqual(message, result);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEnableDeliveryNotification()
        {
            var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
            SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
            var eventStream = MockRepository.StrictMock<EventStream>();
            IPersistQueryable updateStatusEvent = null;
            Expect.Call(pipelineContext.GetEventStream()).Return(eventStream);
            Expect.Call(() => { eventStream.StoreCustomEvent(new UpdateStatusEvent(null, null, null, null)); }).IgnoreArguments()
                .Do(new UpdateStatusEventDelegate((IPersistQueryable statusEvent) => { updateStatusEvent = statusEvent; }));
            var message = MessageFactory.CreateMessage();
            message.Context = MessageFactory.CreateMessageContext();

            var component = MockRepository.PartialMock<UpdateDistributionStatusComponent>();
            Expect.Call(component.GetOutboxAccessor()).Return(null).Repeat.Any();

            MockRepository.ReplayAll();

            component.Enabled = true;
            component.UseDeliveryNotification = true;
            message.Context.WriteProperty<MessageTrackingID>("11111");
            message.Context.WriteProperty<BTS.SourceParty>("sender");
            message.Context.WriteProperty<BTS.DestinationParty>("recipient");
            var result = component.Execute(pipelineContext, message);
            Assert.AreEqual("UseDeliveryNotificationUpdateStatus", message.Context.Read("CorrelationToken", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
            Assert.AreEqual(true, message.Context.Read("AckRequired", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
            Assert.IsNull(updateStatusEvent);
            Assert.AreEqual(message, result);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCommonInterfacesImplementation_UpdateDistributionStatusComponent()
        {
            var component = new UpdateDistributionStatusComponent();
            Assert.AreEqual(string.Empty, component.Description);
            Assert.AreEqual("Update Distribution Status", component.Name);
            Assert.AreEqual("1.0", component.Version);
            Assert.AreEqual(IntPtr.Zero, component.Icon);
            Assert.IsNull(component.Validate(null));
            var guid = new Guid();
            component.GetClassID(out guid);
            Assert.AreEqual(new Guid("02C1C425-6D89-44CF-A01D-3997B54A53BF"), guid);

            var propertyBag = MockRepository.StrictMock<IPropertyBag>();
            string enabledProp = null;
            bool enabledValue = false;
            object enabledPtr = false;

            string deliveryProp = null;
            bool deliveryValue = true;
            object deliveryPtr = true;

            Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
                .Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

            Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
                Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

            Expect.Call(() => propertyBag.Write("UseDeliveryNotification", ref deliveryPtr))
                .Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { deliveryProp = propName; deliveryValue = (bool)ptrVar; }));

            Expect.Call(() => propertyBag.Read("UseDeliveryNotification", out deliveryPtr, 0)).
                Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

            MockRepository.ReplayAll();

            component.Enabled = false;
            component.UseDeliveryNotification = true;
            component.Save(propertyBag, true, true);

            Assert.AreEqual("Enabled", enabledProp);
            Assert.AreEqual("UseDeliveryNotification", deliveryProp);
            Assert.IsFalse(enabledValue);
            Assert.IsTrue(deliveryValue);
            component.Load(propertyBag, 0);

            Assert.IsTrue(component.Enabled);
            Assert.IsTrue(component.UseDeliveryNotification);

            MockRepository.VerifyAll();
        }
    }
}
