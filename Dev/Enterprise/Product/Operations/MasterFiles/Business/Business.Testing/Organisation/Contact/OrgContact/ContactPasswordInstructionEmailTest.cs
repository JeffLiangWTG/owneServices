using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContactPasswordInstructionEmail))]
	sealed class ContactPasswordInstructionEmailTest : ContactPasswordInstructionEmailTestCase<ContactPasswordInstructionEmail>
	{
		protected override BusinessObject GetNewBusinessObjectSendingEmail()
		{
			return Factory.NewWithValidTestData<OrgContact>();
		}
		protected override void SetSenderForLogs(IPasswordInstructionEmailSource source)
		{
			var contactSource = (OrgContact)source;
			contactSource.Person.SetHashedPassword("hello");
		}
	}
}
