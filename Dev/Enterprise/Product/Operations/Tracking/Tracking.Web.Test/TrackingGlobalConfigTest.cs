using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingGlobalConfigTest : ZGlobalConfigTest
	{
		protected override ZGlobalConfig GetNewConfig()
		{
			return new TrackingGlobalConfig();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Not sure if used through reflection")]
		new TrackingGlobalConfig TestConfig
		{
			get { return (TrackingGlobalConfig)base.TestConfig; }
		}
	}
}
