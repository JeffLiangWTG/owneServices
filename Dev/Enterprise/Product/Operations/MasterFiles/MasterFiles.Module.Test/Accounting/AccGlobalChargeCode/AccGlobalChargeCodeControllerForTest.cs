using Enterprise.Security;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccGlobalChargeCodeControllerForTest : AccGlobalChargeCodeController
	{
		public new SecurityCheckpoint CheckPointForNew
		{
			get { return base.CheckPointForNew; }
		}

		public new SecurityCheckpoint CheckPointForView
		{
			get { return base.CheckPointForView; }
		}

		public new SecurityCheckpoint CheckPointForEdit
		{
			get { return base.CheckPointForEdit; }
		}

		public new SecurityCheckpoint CheckPointForDelete
		{
			get { return base.CheckPointForDelete; }
		}
	}
}
