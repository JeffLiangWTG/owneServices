using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingWebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "Enterprise.Tracking.Business";
			yield return "Enterprise.Tracking.Web";
			yield return "Enterprise.Tracking.Module";
			yield return "Enterprise.ZArchitecture.Web.GUI";
			yield return "Enterprise.ZArchitecture.Web.Business";
			yield return "Enterprise.ZArchitecture.Web.Shared";
			yield return "Enterprise.ZArchitecture.Web.GUI.Ajax";
			yield return "CargoWise.ComponentModel";
			yield return "Enterprise.ZArchitecture.GUI";
			yield return "Enterprise.ZArchitecture.Core";
			yield return "Enterprise.ZArchitecture.Business";
			yield return "Enterprise.ZArchitecture.Modules";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Tracking", "Tracking.Web", "Web.Base.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
