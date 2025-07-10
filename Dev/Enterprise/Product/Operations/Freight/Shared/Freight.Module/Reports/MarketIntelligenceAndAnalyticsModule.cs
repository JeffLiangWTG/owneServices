using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class MarketIntelligenceAndAnalyticsModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => ReportingUriHelper.GetPerformanceReportingUri();

		public override ModuleIdentifier ID => ModuleIDs.MarketIntelligenceAndAnalytics;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MarketIntelligenceAndAnalytics;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
	}
}
