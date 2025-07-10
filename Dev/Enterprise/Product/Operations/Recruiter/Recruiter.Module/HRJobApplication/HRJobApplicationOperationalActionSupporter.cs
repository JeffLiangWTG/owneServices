using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicationOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(HRJobApplication);

		public override BusinessContext BusinessContext => BusinessContext.HRJobApplication;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.HRJobApplication;
	}
}
