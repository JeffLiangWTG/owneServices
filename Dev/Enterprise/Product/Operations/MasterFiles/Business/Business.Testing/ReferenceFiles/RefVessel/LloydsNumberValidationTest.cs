namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LloydsNumberValidationTest : NUnit.Framework.TestCase
	{
		public void TestEmptyVessel()
		{
			Validation.Validate("");
			AssertEquals("Should have no error", true, Validation.IsValid);
		}

		public void TestShortVessel()
		{
			Validation.Validate("123456");
			CombineAssertions(() =>
			{
				AssertEquals("Should have an error", false, Validation.IsValid);
				AssertEquals("ErrorText", "Lloyds number must be 7 characters in length", Validation.ErrorText);
			});
		}

		public void TestInvalidNumericVessel()
		{
			Validation.Validate("4837292");
			CombineAssertions(() =>
			{
				AssertEquals("Should have an error", false, Validation.IsValid);
				AssertEquals("ErrorText", "Invalid check-digit in Lloyds number", Validation.ErrorText);
			});
		}

		public void TestInvalidNonNumericVessel()
		{
			Validation.Validate("1111X11");
			CombineAssertions(() =>
			{
				AssertEquals("Should have an error", false, Validation.IsValid);
				AssertEquals("ErrorText", "Lloyds number must be completely numeric (nothing but numbers)", Validation.ErrorText);
			});
		}

		public void TestValidVessel()
		{
			Validation.Validate("4837293");
			AssertEquals("Should have no error", true, Validation.IsValid);
		}

		public void TesthumanReadableName()
		{
			var humanReadableName = "[some kind of name]";
			CombineAssertions(() =>
			{
				Validation.Validate("123456", humanReadableName);
				AssertEquals("< 7 character long", "[some kind of name] must be 7 characters in length", Validation.ErrorText);

				Validation.Validate("4837292", humanReadableName);
				AssertEquals("invalid number", "Invalid check-digit in [some kind of name]", Validation.ErrorText);

				Validation.Validate("1111X11", humanReadableName);
				AssertEquals("not numeric only", "[some kind of name] must be completely numeric (nothing but numbers)", Validation.ErrorText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			Validation = new LloydsNumberValidation();
		}
		LloydsNumberValidation Validation;
	}
}
