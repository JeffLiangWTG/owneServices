using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.Tracking.Business.Testing
{
	public class GlowTrackingUrlGeneratorTest : TestCaseWithFactory
	{
		public void TestPortalsNotEnabled()
		{
			using (SetGlowPortalsUriRegistry(null))
			{
				var contactPK = new ZGuid(Guid.NewGuid());
				var accessToken = "someToken";
				var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
				singleSignOnHelperMock.Setup(h => h.CreateLimitedToken(contactPK.ToGuid(), "OC", It.IsAny<GlowSingleSignOnTokenOptions>())).Returns(accessToken);
				ObjectFactory.Substitute(singleSignOnHelperMock.Object);

				var generator = new GlowTrackingUrlGenerator();

				AssertNull(generator.GenerateURL(contactPK));
			}
		}

		public void TestPortalsEnabled()
		{
			using (SetGlowPortalsUriRegistry("https://glow"))
			{
				var contactPK = new ZGuid(Guid.NewGuid());
				var accessToken = "someToken";
				var singleSignOnHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
				singleSignOnHelperMock.Setup(h => h.CreateLimitedToken(contactPK.ToGuid(), "OC", It.IsAny<GlowSingleSignOnTokenOptions>())).Returns(accessToken);
				ObjectFactory.Substitute(singleSignOnHelperMock.Object);

				var generator = new GlowTrackingUrlGenerator();

				var url = generator.GenerateURL(contactPK);
				AssertEquals("https://glow/TRK?sso_otp=someToken", url.AbsoluteUri);
			}
		}

		IDisposable SetGlowPortalsUriRegistry(string uri)
		{
			var originalValue = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, uri);

			return new DisposableAction(() => { GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue); });
		}
	}
}
