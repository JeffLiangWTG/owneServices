using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbStaffWrapperProviderForTesting : GlbStaffWrapperProvider
	{
		protected override StaffCredentialsUserControl GetNewUserControlCore()
		{
			throw new NotImplementedException();
		}

		protected override GlbStaffWrapper GetWrapperCore(GlbStaff staff)
		{
			return staff.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapperForTesting(staff));
		}

		public override MenuItem GetNewTopLevelMenu(GlbStaffWrapper wrapper)
		{
			throw new NotImplementedException();
		}
	}
}
