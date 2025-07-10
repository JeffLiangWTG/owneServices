using Enterprise.Security;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccPOSChargeCodeGroupControllerForTest : AccPOSChargeCodeGroupController
	{
		public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
		public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;
		public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;
		public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
	}
}
