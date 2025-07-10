using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccChargeCodeControllerForTest : AccChargeCodeController
	{
		public new SecurityCheckpoint GetCheckPointForCopy(BusinessObject bizo)
		{
			return base.GetCheckPointForCopy(bizo);
		}
	}
}
