using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	public class WebConfigTest : BaseWebConfigTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "MarketingManager", "WebVoting", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
