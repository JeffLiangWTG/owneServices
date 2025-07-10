using System;
using CargoWise.Definitions;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(Trip);

		public override BusinessContext BusinessContext => BusinessContext.eManifest;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.USeManifest;
	}
}
