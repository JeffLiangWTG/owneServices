using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util.Testing
{
	sealed class SecureRandomStringGeneratorTest : TestCase
	{
		public void TestGetRandomString()
		{
			var random = SecureRandomStringGenerator.GetRandomString();
			AssertEquals("Length of generated strings", 15, random.Length);
			Assert("Format of generated strings", Regex.IsMatch(random, "[0-9A-Za-z_]{15}"));

			var randomStrings = new HashSet<string> { random };
			for (int i = 0; i < 5; i++)
			{
				Assert("Generated strings should be unique", randomStrings.Add(SecureRandomStringGenerator.GetRandomString()));
			}
		}
	}
}
