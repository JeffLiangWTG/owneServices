using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	abstract class ProcessQueueUserControlBasherTestCase : BasherTest
	{
		public override Form GetFormToBash()
		{
			ProcessQueueParentForTest parent = new ProcessQueueParentForTest(Factory);
			parent.ActiveProcessQueueForBinding[0].QueueType = QueueType;
			return new FormForProcessQueueUserControlBasherTest(parent);
		}

		protected abstract ProcessQueueType.Enum QueueType { get; }
	}
}
