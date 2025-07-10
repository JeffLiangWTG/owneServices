using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.WebCFS.Web.Testing
{
	public class TrackingGlobalConfigTest : ZGlobalConfigTest
	{
		protected override ZGlobalConfig GetNewConfig()
		{
			return new CFSGlobalConfig();
		}

		protected new CFSGlobalConfig TestConfig
		{
			get { return (CFSGlobalConfig)base.TestConfig; }
		}
	}
}
