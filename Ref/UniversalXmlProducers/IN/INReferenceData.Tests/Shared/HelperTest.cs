using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class HelperTest
	{
		[Test]
		public void TestAssume()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() => Helper.Assume(true, "when condition passed"));
				Assert.Throws<UnhandledApplicationException>(() => Helper.Assume(false, "when condition failed"));
			});
		}
	}
}
