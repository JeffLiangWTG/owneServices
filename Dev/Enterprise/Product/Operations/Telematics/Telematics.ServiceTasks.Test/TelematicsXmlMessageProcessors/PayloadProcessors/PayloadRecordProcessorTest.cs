using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	abstract class PayloadRecordProcessorTest<TProcessor, TMessage> : TestCaseWithFactory
		where TProcessor : IPayloadRecordProcessor<TMessage>, new()
		where TMessage : IPayloadRecord, new()
	{
		protected override void SetUp()
		{
			base.SetUp();

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = Array.Empty<byte>();
			device.V3_HardwareIdentifier = string.Empty;
			device.V3_Model = string.Empty;

			processor = new TProcessor();
		}

		public void TestReturnsCount()
		{
			var result = processor.Process(Factory, device, new TMessage());
			AssertEquals(1, result);
		}

		public abstract void TestAddsRecord();

		protected GlbDevice device;
		protected TProcessor processor;
	}
}
