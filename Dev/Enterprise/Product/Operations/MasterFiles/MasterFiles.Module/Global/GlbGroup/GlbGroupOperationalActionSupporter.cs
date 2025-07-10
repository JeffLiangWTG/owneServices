using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	class GlbGroupOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(GlbGroup);

		public override BusinessContext BusinessContext => BusinessContext.GlbGroup;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Groups;
	}
}
