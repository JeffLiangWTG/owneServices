using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Warehouse.Web.Testing
{
	class WebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Warehouse", "Web", "Warehouse.Web", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";

		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "Enterprise.Warehouse.Web";
		}
	}
}
