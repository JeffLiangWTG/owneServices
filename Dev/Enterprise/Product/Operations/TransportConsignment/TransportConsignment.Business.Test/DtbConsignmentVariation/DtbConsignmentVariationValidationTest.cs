using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentVariationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckStatus()
		{
			var variation = Factory.New<DtbConsignmentVariation>();

			variation.LTV_Status = "";
			AssertMandatoryValidationError(variation.LTV_StatusInfo, true);

			variation.LTV_Status = "ZZZ";
			AssertListValidationInvalidCodeError(variation.LTV_StatusInfo, true);

			variation.LTV_Status = DtbConsignmentVariationStatuses.Codes.Open;
			AssertNoErrors(variation.LTV_StatusInfo);
		}

		public void TestCheckType()
		{
			var variation = Factory.New<DtbConsignmentVariation>();

			variation.LTV_Type = "";
			AssertMandatoryValidationError(variation.LTV_TypeInfo, true);

			variation.LTV_Type = "ZZZ";
			AssertListValidationInvalidCodeError(variation.LTV_TypeInfo, true);

			variation.LTV_Type = DtbConsignmentVariationTypes.Codes.QuantityWeightVolume;
			AssertNoErrors(variation.LTV_TypeInfo);
		}
	}
}
