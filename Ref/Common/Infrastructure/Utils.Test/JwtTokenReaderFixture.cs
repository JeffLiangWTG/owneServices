using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class JwtTokenReaderFixture
	{
		[Test]
		public void GetClaimValue()
		{
			var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
			var tokenReader = new JwtTokenReader(token);
			var claimValue = tokenReader.GetClaimValue("user");
			Assert.Null(claimValue);

			claimValue = tokenReader.GetClaimValue("name");
			Assert.AreEqual("John Doe", claimValue);
		}
	}
}
