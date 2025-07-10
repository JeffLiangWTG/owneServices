using System;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AllJobsConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestCode()
		{
			AssertEquals("ALL", ConsumerType.Code);
		}

		public void TestDescription()
		{
			AssertEquals("Any Job Type", ConsumerType.Description);
		}

		public override void TestControllerID()
		{
			AssertExceptionThrown<NotImplementedException>(() =>
			{
				var controllerID = ConsumerType.ControllerID;
			});
		}

		public void TestBizoType()
		{
			AssertExceptionThrown<NotImplementedException>(() =>
			{
				var bizoType = ConsumerType.BizoType;
			});
		}

		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return new AllJobsConsumerType();
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
