using Enterprise.ZArchitecture.Core.Lists;
using NUnit.Framework;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	sealed class EmailReaderTest : TransactionedTestCase
	{
		public void TestEmailReader()
		{
			using (var reader = new EmailReaderForTest(MailRetrievalProtocols.POP3, "pop.wisetech.com", 110, "user1", "password1", "TLC"))
			{
				AssertEquals("pop.wisetech.com", reader.MailHost);
				AssertEquals("user1", reader.MailUserName);
				AssertNotNull(reader.Protocol_Expoxed);
			}
		}
	}
}
