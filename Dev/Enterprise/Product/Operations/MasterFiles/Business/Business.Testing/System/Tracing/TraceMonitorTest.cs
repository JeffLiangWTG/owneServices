using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TraceMonitor))]
	sealed class TraceMonitorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBizoProperties()
		{
			var msgWriter = new DummyMessageWriter();
			var tracer = new TraceMonitor();
			tracer.LogCallStack = true;
			tracer.LogDateTime = false;
			tracer.LogThreadId = true;
			tracer.LogProcessId = false;
			tracer.MessageWriter = msgWriter;

			AssertEquals(nameof(tracer.LogCallStack), true, tracer.LogCallStack);
			AssertEquals(nameof(tracer.LogDateTime), false, tracer.LogDateTime);
			AssertEquals(nameof(tracer.LogThreadId), true, tracer.LogThreadId);
			AssertEquals(nameof(tracer.LogProcessId), false, tracer.LogProcessId);
			AssertEquals(nameof(tracer.MessageWriter), msgWriter, tracer.MessageWriter);
		}

		public void TestIntitializeAndClearTraceSources()
		{
			var traceMonitor = new TraceMonitor();
			traceMonitor.MessageWriter = new DummyMessageWriter();
			traceMonitor.IntitializeTraceSources();
			Assert("Trace sources successfully initialized", traceMonitor.IsTracing);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TraceMonitor();
		}
	}
}
