using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ProcessQueueParentForTest))]
	sealed class ProcessQueueUserControlBasherTest_ActiveQueueIsCommercialQueue : ProcessQueueUserControlBasherTestCase
	{
		protected override ProcessQueueType.Enum QueueType
		{
			get { return ProcessQueueType.Enum.Customs; }
		}
	}
}
