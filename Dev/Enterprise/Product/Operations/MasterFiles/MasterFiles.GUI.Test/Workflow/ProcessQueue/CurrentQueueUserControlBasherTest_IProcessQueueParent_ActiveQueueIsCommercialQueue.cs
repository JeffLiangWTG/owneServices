using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(FormForCurrentQueueUserControlBasherTest))]
	sealed class CurrentQueueUserControlBasherTest_IProcessQueueParent_ActiveQueueIsCommercialQueue : CurrentQueueUserControlBasherTestCase
	{
		public override Form GetFormToBash()
		{
			return new FormForIProcessQueueParentUserControlBasherTest(BusinessEntity);
		}

		protected override BusinessObject BusinessEntity
		{
			get
			{
				ProcessQueueParentForTest result = new ProcessQueueParentForTest(Factory);
				result.ActiveProcessQueueForBinding[0].QueueType = ProcessQueueType.Enum.Commercial;
				return result;
			}
		}

		protected override string BindToPrefix
		{
			get { return "ActiveProcessQueueForBinding."; }
		}
	}
}
