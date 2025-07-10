using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util.Testing
{
	sealed class ByteArraySearchTest : TestCase
	{
		public void TestLocate()
		{
			var initialData = new byte[] { 1, 2, 3, 4, 5, 1, 2 };
			var locationsOf2 = initialData.Locate(new byte[] { 2 });
			AssertEquals(1, locationsOf2.First());
			AssertEquals(6, locationsOf2.Last());
			var locationsOf12 = initialData.Locate(new byte[] { 1, 2 });
			AssertEquals(0, locationsOf12.First());
			AssertEquals(5, locationsOf12.Last());
			var locationsOf6 = initialData.Locate(new byte[] { 6 });
			AssertEquals(false, locationsOf6.Any());
			var locationsOf51 = initialData.Locate(new byte[] { 5, 1 });
			AssertEquals(4, locationsOf51.First());
			AssertEquals(1, locationsOf51.Length);
		}
	}
}
