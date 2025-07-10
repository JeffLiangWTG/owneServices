using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWiseNext.Infrastructure.Authentication;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.Testing.Common
{
	public static class OIDCConfigSetupHelper
	{
		public static IOIDCConfig SetOidcConfigTest(string url, string clientId, bool isOIDCEnabled = true)
		{
			var mockOidcClaimMapping = new Mock<IOIDCClaimsMapping>();
			mockOidcClaimMapping.Setup(e => e.ClaimName).Returns(OidcConstants.UniqueName);
			mockOidcClaimMapping.Setup(e => e.Identifier).Returns("GlbStaff.GS_LoginName");

			var oidcClaimItems = new List<IOIDCClaimsMapping>
				{
					mockOidcClaimMapping.Object
				};

			var mockClaimMapping = new Mock<EntityFramework.IBusinessObjectCollection<IOIDCClaimsMapping>>();
			mockClaimMapping.Setup(m => m.Count).Returns(() => oidcClaimItems.Count);
			mockClaimMapping.Setup(m => m[It.IsAny<int>()]).Returns<int>(i => oidcClaimItems[i]);
			mockClaimMapping.Setup(m => m.GetEnumerator()).Returns(() => oidcClaimItems.GetEnumerator());
			var mockClaimMappingObject = mockClaimMapping.Object;

			Assert.That(mockClaimMappingObject.Count, Is.EqualTo(1));
			Assert.That(mockClaimMappingObject[0].ClaimName.ToString(), Is.EqualTo(OidcConstants.UniqueName));
			Assert.That(mockClaimMappingObject[0].Identifier.ToString(), Is.EqualTo("GlbStaff.GS_LoginName"));

			var oidcScopeItems = new List<IOIDCScope>();

			var mockOidcScopes = new Mock<IOIDCScope>();
			mockOidcScopes.Setup(e => e.ScopeName).Returns("profile");
			oidcScopeItems.Add(mockOidcScopes.Object);
			mockOidcScopes = new Mock<IOIDCScope>();
			mockOidcScopes.Setup(e => e.ScopeName).Returns("offline_access");
			oidcScopeItems.Add(mockOidcScopes.Object);
			mockOidcScopes = new Mock<IOIDCScope>();
			mockOidcScopes.Setup(e => e.ScopeName).Returns(clientId);
			oidcScopeItems.Add(mockOidcScopes.Object);

			var mockScopes = new Mock<EntityFramework.IBusinessObjectCollection<IOIDCScope>>();
			mockScopes.Setup(m => m.Count).Returns(() => oidcScopeItems.Count);
			mockScopes.Setup(m => m[It.IsAny<int>()]).Returns<int>(i => oidcScopeItems[i]);
			mockScopes.Setup(m => m.GetEnumerator()).Returns(() => oidcScopeItems.GetEnumerator());
			var mockScopesObject = mockScopes.Object;

			Assert.That(mockScopesObject.Count, Is.EqualTo(3));
			Assert.That(mockScopesObject[0].ScopeName.ToString(), Is.EqualTo("profile"));
			Assert.That(mockScopesObject[1].ScopeName.ToString(), Is.EqualTo("offline_access"));
			Assert.That(mockScopesObject[2].ScopeName.ToString(), Is.EqualTo(clientId));

			var mockOIDCConfig = new Mock<IOIDCConfig>();
			mockOIDCConfig.Setup(config => config.IsOIDCEnabled).Returns(isOIDCEnabled);
			mockOIDCConfig.Setup(config => config.AuthorityURL).Returns(url);
			mockOIDCConfig.Setup(config => config.ClientIdentifier).Returns(clientId);
			mockOIDCConfig.Setup(config => config.OIDCServerType).Returns(OIDCServerTypes.Azure);
			mockOIDCConfig.Setup(config => config.Scopes).Returns(mockScopesObject);
			mockOIDCConfig.Setup(config => config.ClaimsMappings).Returns(mockClaimMappingObject);

			return mockOIDCConfig.Object;
		}
	}
}
