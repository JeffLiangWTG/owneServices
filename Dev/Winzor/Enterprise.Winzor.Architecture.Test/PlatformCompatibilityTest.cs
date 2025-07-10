using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class PlatformCompatibilityTest
{
	[Test]
	public void CanReadReleaseInfo()
	{
		Assert.That(ReleaseInfo.Instance.ReleaseRing, Is.Not.Null.Or.Empty);
	}
}
