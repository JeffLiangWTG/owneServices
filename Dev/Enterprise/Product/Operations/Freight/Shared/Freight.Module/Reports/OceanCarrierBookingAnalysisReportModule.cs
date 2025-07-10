using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class OceanCarrierBookingAnalysisReportModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => ReportingUriHelper.GetPerformanceReportingUri();

		public override ModuleIdentifier ID => ModuleIDs.OceanCarrierBookingAnalysisReport;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OceanCarrierBookingAnalysisReport;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
	}
}

