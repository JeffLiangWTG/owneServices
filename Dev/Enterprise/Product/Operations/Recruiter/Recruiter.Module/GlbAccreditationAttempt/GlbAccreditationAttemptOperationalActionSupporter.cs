using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationAttemptOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(GlbAccreditationAttempt);

		public override BusinessContext BusinessContext => BusinessContext.AccreditationAttempt;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.GlbAccreditationAttempt;
	}
}
