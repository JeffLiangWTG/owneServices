using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	class YardPickupRequestValidatorTest : TestCase
	{
		public void TestIsValid_Successful()
		{
			TestCaseIsValid(("FAC", "ORG", "ADR", "REF"), true, null);
		}

		public void TestIsValid_WithFacilityCode_Successful()
		{
			TestCaseIsValid(("FAC", null, null, "REF"), true, null);
		}

		public void TestIsValid_WithOrgCodeAndAddressCode_Successful()
		{
			TestCaseIsValid((null, "ORG", "ADR", "REF"), true, null);
		}

		public void TestIsValid_NullInput_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(null, false, "Missing request details");
		}

		public void TestIsValid_MissingAllFacilityCodeAndAddressCodeAndOrgCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, null, null, "REF"), false, "Either facility code or organization code with address code is required");
		}

		public void TestIsValid_MissingBothFacilityCodeAndOrgCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, null, "ADR", "REF"), false, "Either facility code or organization code with address code is required");
		}

		public void TestIsValid_MissingBothFacilityCodeAndAddressCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, "ORG", null, "REF"), false, "Either facility code or organization code with address code is required");
		}
		public void TestIsValid_MissingReferenceNumber_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(("FAC", "ORG", "ADR", null), false, "Reference number is missing");
		}

		void TestCaseIsValid((string facilityCode, string orgCode, string addressCode, string reference)? testInput, bool expectedResult, string expectedMessage)
		{
			YardPickupRequest input = null;
			if (testInput.HasValue)
			{
				input = new YardPickupRequest
				{
					FacilityCode = testInput.Value.facilityCode,
					OrgCode = testInput.Value.orgCode,
					AddressCode = testInput.Value.addressCode,
					ReferenceNumber = testInput.Value.reference,
				};
			}
			var validator = new YardPickupRequestValidator();
			var result = validator.IsValid(input, out var message);
			AssertEquals(expectedResult, result);
			AssertEquals(expectedMessage, message);
		}

		public void TestIsValid_InvalidRequestType_ShouldThrowException()
		{
			var validator = new YardPickupRequestValidator();
			AssertExceptionThrown<ArgumentException>(() => validator.IsValid(new YardDropoffRequest(), out _));
		}
	}
}
