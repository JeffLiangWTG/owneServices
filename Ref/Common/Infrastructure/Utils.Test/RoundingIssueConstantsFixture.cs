using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class RoundingIssueConstantsFixture
{
	[Test]
	public void ConstantsShouldNotChange()
	{
		Assert.That(RoundingIssueConstants.Flag, Is.EqualTo("X!"));
		Assert.That(RoundingIssueConstants.Value, Is.EqualTo(0.15m));
		Assert.That(RoundingIssueConstants.DummyDescription, Is.EqualTo("Dummy RefCusTaxOrFeeType"));
	}
}
