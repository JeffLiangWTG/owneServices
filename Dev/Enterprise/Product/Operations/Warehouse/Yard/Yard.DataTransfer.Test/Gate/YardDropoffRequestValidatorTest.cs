using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	class YardDropoffRequestValidatorTest : TestCase
	{
		public void TestIsValid_Successful()
		{
			TestCaseIsValid(("FAC", "ORG", "ADR", "CON", true), true, null);
		}

		public void TestIsValid_WithFacilityCode_Successful()
		{
			TestCaseIsValid(("FAC", null, null, "CON", true), true, null);
		}

		public void TestIsValid_WithOrgCodeAndAddressCode_Successful()
		{
			TestCaseIsValid((null, "ORG", "ADR", "CON", true), true, null);
		}

		public void TestIsValid_NullInput_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(null, false, "Missing request details");
		}

		public void TestIsValid_MissingAllFacilityCodeAndAddressCodeAndOrgCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, null, null, "CON", true), false, "Either facility code or organization code with address code is required");
		}

		public void TestIsValid_MissingBothFacilityCodeAndOrgCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, null, "ADR", "CON", true), false, "Either facility code or organization code with address code is required");
		}

		public void TestIsValid_MissingBothFacilityCodeAndAddressCode_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid((null, "ORG", null, "CON", true), false, "Either facility code or organization code with address code is required");
		}
		public void TestIsValid_MissingReferenceNumber_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(("FAC", "ORG", "ADR", null, false), false, "Reference number is missing");
		}

		public void TestIsValid_MissingLaden_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(("FAC", "ORG", "ADR", "CON", null), false, "Laden is missing");
		}

		void TestCaseIsValid((string facilityCode, string orgCode, string addressCode, string reference, bool? isLaden)? testInput, bool expectedResult, string expectedMessage)
		{
			YardDropoffRequest input = null;
			if (testInput.HasValue)
			{
				input = new YardDropoffRequest
				{
					FacilityCode = testInput.Value.facilityCode,
					OrgCode = testInput.Value.orgCode,
					AddressCode = testInput.Value.addressCode,
					ReferenceNumber = testInput.Value.reference,
					IsLaden = testInput.Value.isLaden,
				};
			}
			var validator = new YardDropoffRequestValidator();
			var result = validator.IsValid(input, out var message);
			AssertEquals(expectedResult, result);
			AssertEquals(expectedMessage, message);
		}

		public void TestIsValid_InvalidRequestType_ShouldThrowException()
		{
			var validator = new YardDropoffRequestValidator();
			AssertExceptionThrown<ArgumentException>(() => validator.IsValid(new YardPickupRequest(), out _));
		}
	}
}
