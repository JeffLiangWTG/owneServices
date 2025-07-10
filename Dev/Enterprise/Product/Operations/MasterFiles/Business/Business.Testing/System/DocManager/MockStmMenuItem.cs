using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MockStmMenuItem : StmMenuItem
	{
		public MockStmMenuItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
		}
	}
}
