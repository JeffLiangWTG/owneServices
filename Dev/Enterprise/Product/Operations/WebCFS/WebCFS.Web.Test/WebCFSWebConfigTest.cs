using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.WebCFS.Web.Testing
{
	public class WebCFSWebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "CargoWise.ComponentModel";
			yield return "Enterprise.ZArchitecture.Business";
			yield return "Enterprise.ZArchitecture.Modules";
			yield return "Enterprise.WebCFS.Web";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "WebCFS", "WebCFS.Web", "Web.Base.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
