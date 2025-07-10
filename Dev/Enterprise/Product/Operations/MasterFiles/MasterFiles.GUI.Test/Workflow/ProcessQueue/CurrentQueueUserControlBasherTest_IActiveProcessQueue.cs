using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(FormForCurrentQueueUserControlBasherTest))]
	sealed class CurrentQueueUserControlBasherTest_IActiveProcessQueue : CurrentQueueUserControlBasherTestCase
	{
		public override Form GetFormToBash()
		{
			return new FormForCurrentQueueUserControlBasherTest(BusinessEntity);
		}

		protected override BusinessObject BusinessEntity
		{
			get
			{
				ProcessQueue processQueue = Factory.New<ProcessQueue>();
				return ActiveProcessQueue.New(processQueue);
			}
		}
	}
}
