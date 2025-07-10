using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TWHValidationRequestValidatorTest : TestCase
	{
		public void TestIsValid()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = "FAC";
			validationRequest.OrgCode = "ORG";
			validationRequest.AddressCode = "ADR";
			validationRequest.ReferenceNumber = "REF";
			validationRequest.ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference);

			TestCaseIsValid(validationRequest, true, null);
		}

		public void TestIsValid_GivenReqHasFacilityCode_ThenSuccess()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = "FAC";
			validationRequest.OrgCode = null;
			validationRequest.AddressCode = null;
			validationRequest.ReferenceNumber = "REF";
			validationRequest.ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference);

			TestCaseIsValid(validationRequest, true, null);
		}

		public void TestIsValid_GivenReqHasOrgCodeAndAddressCode_ThenSuccess()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = null;
			validationRequest.OrgCode = "ORG";
			validationRequest.AddressCode = "ADR";
			validationRequest.ReferenceNumber = "REF";
			validationRequest.ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference);

			TestCaseIsValid(validationRequest, true, null);
		}

		public void TestIsValid_GivenReqHasNoOrgFacAndAddressCode_ThenFail()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = null;
			validationRequest.OrgCode = null;
			validationRequest.AddressCode = null;
			validationRequest.ReferenceNumber = "REF";
			validationRequest.ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference);

			TestCaseIsValid(validationRequest, false, "Either facility code or organization code with address code is required");
		}

		public void TestIsValid_GivenNoReferenceNumber_ThenFail()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = "FAC";
			validationRequest.OrgCode = "ORG";
			validationRequest.AddressCode = "ADR";
			validationRequest.ReferenceNumber = null;
			validationRequest.ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference);

			TestCaseIsValid(validationRequest, false, "Reference number is required in the request");
		}

		public void TestIsValid_GivenNoReferenceNumberType_ThenDoNotFail()
		{
			var validationRequest = new TWHValidationRequest();
			validationRequest.FacilityCode = "FAC";
			validationRequest.OrgCode = "ORG";
			validationRequest.AddressCode = "ADR";
			validationRequest.ReferenceNumber = "REF";

			TestCaseIsValid(validationRequest, true, null);
		}

		public void TestIsValid_NullInput_ShouldReturnFalseWithErrorMessage()
		{
			TestCaseIsValid(null, false, "Missing request details");
		}

		void TestCaseIsValid(TWHValidationRequest request, bool expectedResult, string expectedMessage)
		{
			var validator = new TWHValidationRequestValidator();
			var result = validator.IsValid(request, out var message);

			AssertEquals(expectedResult, result);
			AssertEquals(expectedMessage, message);
		}
	}
}
