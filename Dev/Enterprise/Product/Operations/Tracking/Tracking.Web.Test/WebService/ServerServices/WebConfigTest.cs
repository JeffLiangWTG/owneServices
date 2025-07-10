using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WebConfigTest : BaseWebConfigTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Tracking", "Tracking.Web", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
