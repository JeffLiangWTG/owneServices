using System.Collections.Specialized;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.XmlCredential.Testing
{
	public abstract class ConfigurationHandlerTestCase : TestCaseWithFactory
	{
		public static byte[] GetEServiceEncryptedPassword()
		{
			const string encryptedPasswordInBase64String = "AxbvhPkinnUsN9bJ+U0ufEJUrkQ7HJlHXQnsemNPraVoBT812RB4ieBbtTA1w8ezpUGc6gJbxgqfzbjCcDTdre5Tdrw51nkaR3lkarKT+bgG+OPqcHpaj8AZ7qiS8puWjiE3oMmhG9kML0L+Z6P1m11xPQ0/c8odux1KT5WnEaU=";
			return System.Convert.FromBase64String(encryptedPasswordInBase64String);
		}

		public const string ActualEServiceEncryptedPassword = "GOODBYE";

		protected void AssertEmail(EmailDef email, ZString subject, ZString body, ZString[] recipients)
		{
			CombineAssertions(() =>
			{
				AssertEquals("email.Subject", subject, email.Subject);
				AssertEquals("email.Body", body, email.Body);
				AssertEquals("email.Recipients.Count", recipients.Length, email.Recipients.Count);
				AssertContainsExactElementsInAnyOrder(recipients, email.Recipients.OfType<RecipientDef>().Select(x => x.Email));
			});
		}

		protected string GetUserLogStrings(StringCollection userLogStrings)
		{
			return new ZStringBuilder(userLogStrings.OfType<string>()).ToStringWithNewLineBetweenAppends().Trim();
		}
	}
}
