using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class HtmlEmailWithAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBcc()
		{
			AssertNoErrors("Precondition: Bcc should not have errors.", Email.BccInfo);

			Email.Bcc = "!@#";
			AssertHasError(Email.BccInfo, @"The email address ""!@#"" is invalid.");

			Email.Bcc = "test@test.com";
			AssertNoErrors(Email.BccInfo);

			Email.Bcc = "";
			AssertNoErrors(Email.BccInfo);

			Email.Bcc = "Test@test.com;test@test2.cvom@.com";
			AssertHasError(Email.BccInfo, @"The email address ""test@test2.cvom@.com"" is invalid.");

			Email.Bcc = "Test@test.com;test@test2.cvom";
			AssertNoErrors(Email.BccInfo);
		}

		public void TestValidateBody()
		{
			Email.FilledBodyIsMandatory = false;
			Email.Body = "";
			Email.Validation.ValidateBody();
			AssertNoErrors("Body should not have errors because it is not mandatory to be filled", Email.BodyInfo);

			Email.FilledBodyIsMandatory = true;
			Email.Validation.ValidateBody();
			AssertHasErrors("Body should have errors because it is now mandatory to be filled", Email.BodyInfo);

			Email.Body = "Entered something";
			Email.Validation.ValidateBody();
			AssertNoErrors("Body should not have errors becaues it is filled", Email.BodyInfo);

			Email.Body = "(*ABC*";
			Email.Validation.ValidateBody();
			AssertNoErrors("Body should not have errors because ShouldCheckEmailBodyWellForm is false ", Email.BodyInfo);
			Email.ShouldCheckFormatOfEmailBody = true;
			Email.Validation.ValidateBody();
			AssertHasError(Email.BodyInfo, "The attached document is malformed, a start tag (* should always be followed by an end tag *)");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ISendEmailSource source = Factory.New<OrgHeader>();
			Email = new HtmlEmailWithAttachment(source);
		}
		HtmlEmailWithAttachment Email;

		#endregion
	}
}
