using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	class SecureRandomStringGeneratorFixture
	{
		[Test]
		public void RandomString_ShouldBeOfExpectedLength()
		{
			const int expectedLength = 16;
			string randomString = SecureRandomStringGenerator.RandomString;
			Assert.That(randomString.Length, Is.EqualTo(expectedLength), "The length of the random string is not as expected.");
		}

		[Test]
		public void RandomString_ShouldContainOnlyValidCharacters()
		{
			const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+[]{};:,.<>?";
			string randomString = SecureRandomStringGenerator.RandomString;
			Assert.That(randomString.All(c => validChars.Contains(c)), Is.True, "The random string contains invalid characters.");
		}

		[Test]
		public void RandomString_ShouldBeSameBetweenCalls()
		{
			string firstRandomString = SecureRandomStringGenerator.RandomString;
			string secondRandomString = SecureRandomStringGenerator.RandomString;
			Assert.That(firstRandomString, Is.EqualTo(secondRandomString), "The random strings should be equal.");
		}
	}
}
