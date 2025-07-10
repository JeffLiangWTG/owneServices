using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Services.Scim.Api.Test
{
	public class WebConfigAssemblyTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override string DebugCompilationAssembliesXPath => "system.web/compilation/assemblies";

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Services", "Scim", "Enterprise.Services.Scim.Api", "Web.config");

		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "Microsoft.Owin.Host.SystemWeb";
			yield return "System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089";
			yield return "Enterprise.Services.Scim.Api";
			yield return "Microsoft.Owin";
			yield return "Microsoft.IdentityModel.Tokens";
			yield return "System.IdentityModel.Tokens.Jwt";
			yield return "Microsoft.Identity.Client";
			yield return "Microsoft.IdentityModel.JsonWebTokens";
			yield return "IdentityModel";
			yield return "Microsoft.IdentityModel.Protocols.OpenIdConnect";
			yield return "Microsoft.IdentityModel.Protocols";
		}
	}
}
