using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing
{
	[TestedType(typeof(CommunicationContactRow))]
	sealed class CommunicationContactRowTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
			=> new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Candidate, "dummy@email.com"));

		public void TestConstructContactRowFromStaff_ConstructsCommunicationContact_WithStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";

			var contactrow = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Recruiter, staff));

			CombineAssertions("Contact rows that were constructed with a staff member should construct a contact with that staff member", () =>
			{
				AssertEquals(staff.GS_Code, contactrow.Contact.Staff);
				AssertEquals(staff.GS_EmailAddress, contactrow.Contact.Email);
			});
		}

		public void TestConstructContactRowFromEmail_ConstructsCommunicationContact_WithEmail()
		{
			var contactrow = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Candidate, "donald@quack.com"));

			CombineAssertions("Contact rows that were constructed with an email should construct a contact with that email", () =>
			{
				AssertEquals("donald@quack.com", contactrow.Contact.Email);
			});
		}

		public void TestConstructContactRowFromStaff_DoesNotSetRowToReadOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";

			var contactrow = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Recruiter, staff));

			AssertEquals("Contact rows that were constructed with a staff member should not cause the row to be set to readonly", false, contactrow.ReadOnly);
		}

		public void TestConstructContactRowFromEmail_DoesNotSetRowToReadOnly()
		{
			var contactrow = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Candidate, "donald@quack.com"));

			AssertEquals("Contact rows that were constructed with an email should not cause the row to be set to readonly", false, contactrow.ReadOnly);
		}

		public void TestCreateContactRow_HasSelectedTrueByDefault()
		{
			var contactrow = CommunicationContactRow.CreateUncommittedRow(Factory);

			AssertEquals("New contact rows should be 'Selected' by default", true, contactrow.Selected);
		}
	}
}
