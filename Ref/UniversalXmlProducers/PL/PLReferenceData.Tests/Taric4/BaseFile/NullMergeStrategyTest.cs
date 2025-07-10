using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class NullMergeStrategyTest
{
	[Test]
	public void TestMerge()
	{
		var mergeStrategy = new NullMergeStrategy();
		var mergeResult = mergeStrategy.Merge(new(), new());
		Assert.That(mergeResult, Is.Null);
	}
}
