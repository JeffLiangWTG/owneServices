using Enterprise.Freight.Agency.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	[TestsSubclassesOf(typeof(CMMMessageProcessor))]
	public abstract class CMMMessageProcessorTest : BaseAgencyTest
	{
		public abstract void TestProcessCODECOMessage_Ack();
		public abstract void TestProcessCODECOMessage_Warning();
		public abstract void TestProcessCODECOMessage_Fail();
	}
}
