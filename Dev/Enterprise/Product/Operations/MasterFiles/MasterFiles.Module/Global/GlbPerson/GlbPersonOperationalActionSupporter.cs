using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class GlbPersonOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.GlbPerson; }
		}
		public override Type RootType
		{
			get { return typeof(GlbPerson); }
		}
		public override SecurityCheckpoint CustomizationSecurityCheckpoint
		{
			get { return Env.Security.PersonIntelligenceCustomiseActions; }
		}
		public override SecurityCheckpoint RunSecurityCheckpoint
		{
			get { return Env.Security.PersonIntelligenceRunActions; }
		}
	}
}
