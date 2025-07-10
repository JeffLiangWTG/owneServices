using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class ExtensionsTest
	{
		[Test]
		public void TestRemoveNonNumbers()
		{
			Assert.AreEqual("1123","112adwad3".RemoveNonDecimalValues());
			Assert.AreEqual("11,23", "<br>11,23/t/t</br>".RemoveNonDecimalValues());
			Assert.AreEqual("11.23", "<br>11awdawd.23/t/t</br>".RemoveNonDecimalValues());
		}
	}
}
