using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing
{
	[TestedType(typeof(CommunicationContact))]
	sealed class CommunicationContactTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CommunicationContact(Factory, CommunicationContactPositions.Candidate, "dummy@email.com");

		public void TestConstructContact_FromStaff_PopulatesNameAndEmail()
		{
			var staff = CreateStaff();
			var contact = new CommunicationContact(Factory, CommunicationContactPositions.Recruiter, staff);

			AssertEquals("Contacts that were constructed with a staff member should have code populated", staff.GS_Code, contact.Staff);
			AssertEquals("Contacts that were constructed with a staff member should have email populated", staff.GS_EmailAddress, contact.Email);
		}

		public void TestConstructContact_FromStaff_IsReadOnly()
		{
			var staff = CreateStaff();
			var contact = new CommunicationContact(Factory, CommunicationContactPositions.Recruiter, staff);

			Assert("Contacts that were constructed with a staff member should be read only", contact.ReadOnly);
		}

		public void TestConstructContact_FromEmailAddress_PopulatesEmail()
		{
			var contact = new CommunicationContact(Factory, CommunicationContactPositions.Manager, "Tom.Jerry@cartoon.co");

			AssertEquals("Contacts that were constructed with an email should have email populated", "Tom.Jerry@cartoon.co", contact.Email);
		}

		public void TestConstructContact_FromEmailAddress_IsReadOnly()
		{
			var contact = new CommunicationContact(Factory, CommunicationContactPositions.Manager, "Tom.Jerry@cartoon.co");

			Assert("Contacts that were constructed with an email should be read only", contact.ReadOnly);
		}

		public void TestCreateContact_ChangingBetweenTwoInternalPositions_KeepsSelectedStaff()
		{
			var contact = CommunicationContact.CreateUncommittedRow(Factory);
			var staff = CreateStaff();

			contact.Position = CommunicationContactPositions.Manager.Description;
			contact.Staff = staff.GS_Code;
			contact.Position = CommunicationContactPositions.InterestedParty.Description;

			AssertEquals("Changing between two internal positions should keep previously selected staff", staff.GS_Code, contact.Staff);
		}

		public void TestCreateContact_ChangingFromInternalPositionToReference_ClearsSelectedStaff()
		{
			var contact = CommunicationContact.CreateUncommittedRow(Factory);
			var staff = CreateStaff();

			contact.Position = CommunicationContactPositions.Manager.Description;
			contact.Staff = staff.GS_Code;
			contact.Position = CommunicationContactPositions.Reference.Description;

			AssertNullOrEmpty("Changing from internal position to reference should clear selected staff", contact.Staff);
		}

		public void TestCreateContact_ChangingFromReferenceToInternalPosition_ClearsSelectedStaff()
		{
			var contact = CommunicationContact.CreateUncommittedRow(Factory);

			contact.Position = CommunicationContactPositions.Reference.Description;
			contact.Email = "donald@quack.com";
			contact.Position = CommunicationContactPositions.TeamLead.Description;

			AssertNullOrEmpty("Changing from reference to internal position should clear email", contact.Email);
		}

		public void TestCreateContact_WithoutAnyPositionSelected_HasAllOtherColumnsSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: string.Empty,
				shouldStaffBeReadonly: true,
				shouldEmailBeReadonly: true);

		public void TestCreateContact_WithInterestedPartyPositionSelected_HasOnlyEmailSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: CommunicationContactPositions.InterestedParty.Description,
				shouldStaffBeReadonly: false,
				shouldEmailBeReadonly: true);

		public void TestCreateContact_WithSeniorDeveloperPositionSelected_HasOnlyEmailSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: CommunicationContactPositions.SeniorDeveloper.Description,
				shouldStaffBeReadonly: false,
				shouldEmailBeReadonly: true);

		public void TestCreateContact_WithTeamLeadPositionSelected_HasOnlyEmailSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: CommunicationContactPositions.TeamLead.Description,
				shouldStaffBeReadonly: false,
				shouldEmailBeReadonly: true);

		public void TestCreateContact_WithOtherPositionSelected_HasOnlyEmailSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: CommunicationContactPositions.OtherPosition.Description,
				shouldStaffBeReadonly: false,
				shouldEmailBeReadonly: true);

		public void TestCreateContact_WithReferencePositionSelected_HasOnlyStaffSetToReadOnly()
			=> CheckReadonlyProperties(
				positionToUse: CommunicationContactPositions.Reference.Description,
				shouldStaffBeReadonly: true,
				shouldEmailBeReadonly: false);

		void CheckReadonlyProperties(string positionToUse, bool shouldStaffBeReadonly, bool shouldEmailBeReadonly)
		{
			var contact = CommunicationContact.CreateUncommittedRow(Factory);

			contact.Position = positionToUse;

			CombineAssertions($"Readonly status of properties with position '{positionToUse}' selected", () =>
			{
				AssertEquals($"Readonly status of 'Staff'", shouldStaffBeReadonly, contact.StaffInfo.ReadOnly);
				AssertEquals($"Readonly status of 'Email'", shouldEmailBeReadonly, contact.EmailInfo.ReadOnly);
			});
		}

		GlbStaff CreateStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";
			return staff;
		}
	}
}
