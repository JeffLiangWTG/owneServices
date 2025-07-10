using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Rating.Web.Testing
{
	public class WebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "Microsoft.Owin.Host.SystemWeb";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Rating", "Web", "Rating.Web", "Web.Base.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
