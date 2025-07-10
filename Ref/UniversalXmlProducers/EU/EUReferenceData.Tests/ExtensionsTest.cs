using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	class ExtensionsTest
	{
		[Test]
		public void TestSubstringSafe()
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, (null as string).SubstringSafe(1, 4));

				Assert.AreEqual("", "".SubstringSafe(2, 4));
				Assert.AreEqual("abc", "abcdef".SubstringSafe(0, 3));
				Assert.AreEqual("ef", "abcdef".SubstringSafe(4, 4));
			});
		}

		[Test]
		public void TestLeft()
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, (null as string).Left(4));

				Assert.AreEqual("", "".Left(4));
				Assert.AreEqual("abc", "abcdef".Left(3));
				Assert.AreEqual("abcdef", "abcdef".Left(7));
			});
		}

		[Test]
		public void TestTrimEnd()
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(null, (null as string).TrimEnd("AA"));

				Assert.AreEqual("", "".TrimEnd("AA"));
				Assert.AreEqual("ABCA", "ABCA".TrimEnd("AA"));
				Assert.AreEqual("28392A", "28392AAAAA".TrimEnd("AA"));
			});
		}
	}
}
