using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GlbStaff; }
		}
		public override Type RootType
		{
			get { return typeof(GlbStaff); }
		}
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Staff;
	}
}
