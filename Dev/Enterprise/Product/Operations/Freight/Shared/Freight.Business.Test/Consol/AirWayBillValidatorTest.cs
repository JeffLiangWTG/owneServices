using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public class AirWayBillValidatorTest : TestCaseWithFactory
	{
		public void TestValidAWB_ShouldNotHaveWarningsOrErrors()
		{
			Class.TestProperty = "08199999992";
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertNoWarningsOrErrors("Valid AWB");
		}

		public void TestTooShortAWB_ShouldHaveWarning()
		{
			Class.TestProperty = "0819999";
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertWarningOnly("Too short AWB", "should contain 11 digits");
		}

		public void TestTooLongAWB_ShouldHaveWarning()
		{
			Class.TestProperty = "081900000000092";
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertWarningOnly("Too long AWB", "should contain 11 digits");
		}

		public void TestAWBWithIncorrectCheckDigit_ShouldHaveWarning()
		{
			Class.TestProperty = "08199999990";
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertWarningOnly("AWB with incorrect check digit", "Invalid check digit");
		}

		public void TestAWBWithNonDigitsCharacters_ShouldHaveWarning()
		{
			Class.TestProperty = "O8199000092"; // O instead of 0
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertWarningOnly("AWB with non-digits", "can only contain numbers");
		}

		public void TestAWBWithNonAsciiDigits_ShouldHaveWarning()
		{
			Class.TestProperty = "081990000۹٣"; // Arabic digits instead of ASCII
			Validator.Validate(Class.TestPropertyInfo);
			Class.AssertWarningOnly("AWB with non-ASCII digits", "can only contain numbers");
		}

		public void TestValidateAndAddMessageErrorWithInvalidInput_ShouldHaveMessageError()
		{
			Class.TestProperty = "08199999990";
			Validator.ValidateAndAddMessageError(Class.TestPropertyInfo);
			Class.AssertMessageErrorOnly("AWB with incorrect check digit", "Invalid check digit");
		}

		#region Implementation

		TestClass Class;
		AirWayBillValidator Validator;
		protected override void SetUp()
		{
			base.SetUp();
			Class = new TestClass();
			Validator = new AirWayBillValidator();
		}

		class TestClass : NonPersistentBusinessObject, IObsoleteValidation
		{
			public TestClass()
			{
			}

			public ZString fTestProperty;
			public ZString TestProperty
			{
				get { return fTestProperty; }
				set
				{
					fTestProperty = value;
					ValidateTestProperty();
				}
			}

			public ZPropertyInfo TestPropertyInfo
			{
				get { return GetZPropertyInfo(nameof(TestProperty)); }
			}

			public void ValidateTestProperty()
			{
				TestPropertyInfo.ClearAllNotifications();
			}

			void AssertNoErrors(string testCase) => TestCaseWithFactory.AssertNoErrors(testCase, TestPropertyInfo);
			void AssertNoMessageErrors(string testCase) => TestCaseWithFactory.AssertNoMessageErrors(testCase, TestPropertyInfo);
			void AssertNoWarnings(string testCase) => TestCaseWithFactory.AssertNoWarnings(testCase, TestPropertyInfo);
			void AssertWarning(string testCase, string messagePart) => TestCaseWithFactory.AssertHasWarningContaining(testCase, TestPropertyInfo, messagePart);
			void AssertMessageError(string testCase, string messagePart) => TestCaseWithFactory.AssertHasMessageErrorContaining(testCase, TestPropertyInfo, messagePart);

			public void AssertNoWarningsOrErrors(string testCase) =>
				CombineAssertions(() =>
				{
					AssertNoErrors($"{testCase} - no errors");
					AssertNoMessageErrors($"{testCase} - no message errors");
					AssertNoWarnings($"{testCase} - no warnings");
				});

			public void AssertWarningOnly(string testCase, string expectedWarning) =>
				CombineAssertions(() =>
				{
					AssertNoErrors($"{testCase} - no errors");
					AssertNoMessageErrors($"{testCase} - no message errors");
					AssertWarning($"{testCase} - warning", expectedWarning);
				});
			public void AssertMessageErrorOnly(string testCase, string expectedWarning) =>
				CombineAssertions(() =>
				{
					AssertNoErrors($"{testCase} - no errors");
					AssertMessageError($"{testCase} - message error", expectedWarning);
					AssertNoWarnings($"{testCase} - no warnings");
				});
		}

		#endregion
	}
}
