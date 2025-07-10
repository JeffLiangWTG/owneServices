using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	public class EmailValidatorFixture
	{
		[Test]
		public void ValidatingEmails()
		{
			var wrongEmail = "anything";
			var wrongEmail2 = "anything@@a.com";
			var wrongEmail3 = "anything@email";
			var acceptedEmail = "anything@email.com";

			Assert.That(EmailValidator.IsValidEmail(wrongEmail), Is.EqualTo(false));
			Assert.That(EmailValidator.IsValidEmail(wrongEmail2), Is.EqualTo(false));
			Assert.That(EmailValidator.IsValidEmail(wrongEmail3), Is.EqualTo(false));
			Assert.That(EmailValidator.IsValidEmail(acceptedEmail), Is.EqualTo(true));
		}
	}
}
