using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class FormForIProcessQueueParentUserControlBasherTest : FormForCurrentQueueUserControlBasherTest
	{
		public FormForIProcessQueueParentUserControlBasherTest(IBusiness businessEntity) : base(businessEntity)
		{
		}

		public override string BindToPrefix
		{
			get { return "ActiveProcessQueueForBinding."; }
		}
	}
}
