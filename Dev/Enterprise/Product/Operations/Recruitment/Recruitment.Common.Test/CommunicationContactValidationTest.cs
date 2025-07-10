using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;

namespace Enterprise.Recruitment.Testing
{
	sealed class CommunicationContactValidationTest : TestCaseWithFactory
	{
		public void TestInvalidStaffCode_AddsValidationError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = CommunicationContactPositions.Manager.Description;
			contactRow.Contact.Staff = "DUM";

			AssertHasError("Invalid staff code should add validation error", contactRow.Contact.StaffInfo, "Staff member does not exist");
		}

		public void TestEmptyStaffCode_WithInternalPositionSelected_AddsValidationError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = CommunicationContactPositions.Manager.Description;
			contactRow.Contact.Staff = string.Empty;

			AssertHasError("Empty staff code with internal position selected should add validation error", contactRow.Contact.StaffInfo, "You must select a staff member");
		}

		public void TestEmptyStaffCode_WithExternalPositionSelected_HasNoError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = CommunicationContactPositions.Manager.Description;
			contactRow.Contact.Staff = string.Empty;
			AssertHasError("PRE: Demonstrate staff can have errors to satisfy DAT", contactRow.Contact.StaffInfo, "You must select a staff member");

			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Staff = string.Empty;

			AssertNoError("Empty staff code with external position selected should have no errors", contactRow.Contact.StaffInfo, "You must select a staff member");
		}

		public void TestStaffCode_WithNoEmail_AddsValidationError()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = string.Empty;

			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);
			contactRow.Contact.Position = CommunicationContactPositions.Manager.Description;
			contactRow.Contact.Staff = "TST";

			AssertHasError("Staff with no email address should add validation error", contactRow.Contact.StaffInfo, "Staff member does not have an email address");
		}

		public void TestInvalidPosition_AddsValidationError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = "Dummy";

			AssertHasError("Invalid position should add validation error", contactRow.Contact.PositionInfo, "Invalid position");
		}

		public void TestEmptyEmail_WithExternalPosition_AddsValidationError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = string.Empty;

			AssertHasError("Empty email with external position selected should add validation error", contactRow.Contact.EmailInfo, "You must enter an email address");
		}

		public void TestEmptyEmail_WithInternalPosition_HasNoError()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = string.Empty;
			AssertHasError("PRE: Demonstrate email can have errors to satisfy DAT", contactRow.Contact.EmailInfo, "You must enter an email address");

			contactRow.Contact.Position = CommunicationContactPositions.Manager.Description;

			AssertNoError("Empty email with internal position selected should have no errors", contactRow.Contact.EmailInfo, "You must enter an email address");
		}

		public void TestReadOnlyContact_ShouldNotShowErrors()
		{
			var contactRow = CommunicationContactRow.CreateUncommittedRow(Factory);

			contactRow.Contact.Position = "Dummy";
			contactRow.Contact.Staff = "DUM";
			CombineAssertions("PRE: Demonstrate position & staff can have errors to satisfy DAT", () =>
			{
				AssertHasError(contactRow.Contact.PositionInfo, "Invalid position");
				AssertHasError(contactRow.Contact.StaffInfo, "Staff member does not exist");
			});

			contactRow.Contact.ReadOnly = true;
			contactRow.Contact.Position = "Dummy";
			contactRow.Contact.Staff = "DUM";

			CombineAssertions("Readonly communication contact should not show errors", () =>
			{
				AssertNoError("Invalid position should add validation error", contactRow.Contact.PositionInfo, "Invalid position");
				AssertNoError("Invalid staff code should add validation error", contactRow.Contact.StaffInfo, "Staff member does not exist");
			});
		}
	}
}
