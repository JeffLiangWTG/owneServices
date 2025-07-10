using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	public class WebVotingWebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "CargoWise.ComponentModel";
			yield return "Enterprise.MarketingManager.WebVoting";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "MarketingManager", "WebVoting", "Web.Base.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
