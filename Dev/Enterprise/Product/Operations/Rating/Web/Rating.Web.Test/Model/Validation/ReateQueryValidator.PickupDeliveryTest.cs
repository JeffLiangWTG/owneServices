using System;
using System.Linq;
using System.Linq.Expressions;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class RateQueryValidatorPickupDeliveryTest : TestCase
	{
		#region Non-JobCharges source endpoints

		public void TestValidate_Costing_PickupDelivery_PropertiesNotSpecified() =>
			TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint.Costing);

		public void TestValidate_ClientRates_PickupDelivery_PropertiesNotSpecified() =>
			TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint.ClientRates);

		public void TestValidate_CompanyTariffs_PickupDelivery_PropertiesNotSpecified() =>
			TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint.CompanyTariffs);

		public void TestValidate_IntercompanyTariffs_PickupDelivery_NotSpecified() =>
			TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint.IntercompanyTariffs);

		static void TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint source)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(source);
			rateQuery.PickupOrg = null;
			rateQuery.PickupAddrCode = null;
			rateQuery.PickupCity = null;
			rateQuery.PickupPostcode = null;
			rateQuery.DeliveryOrg = null;
			rateQuery.DeliveryAddrCode = null;
			rateQuery.DeliveryCity = null;
			rateQuery.DeliveryPostcode = null;

			var validator = new RateQueryValidator(source);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(x => x.PickupOrg);
			result.ShouldNotHaveValidationErrorFor(x => x.PickupAddrCode);
			result.ShouldNotHaveValidationErrorFor(x => x.PickupCity);
			result.ShouldNotHaveValidationErrorFor(x => x.PickupPostcode);
			result.ShouldNotHaveValidationErrorFor(x => x.DeliveryOrg);
			result.ShouldNotHaveValidationErrorFor(x => x.DeliveryAddrCode);
			result.ShouldNotHaveValidationErrorFor(x => x.DeliveryCity);
			result.ShouldNotHaveValidationErrorFor(x => x.DeliveryPostcode);
			Assert(true);
		}

		public void TestValidate_PickupDelivery_NonJobCharges_Costing_Specified() =>
			TestValidate_PickupDelivery_NonJobCharges_Specified(SourceEndpoint.Costing);

		public void TestValidate_PickupDelivery_NonJobCharges_ClientRates_Specified() =>
			TestValidate_PickupDelivery_NonJobCharges_Specified(SourceEndpoint.ClientRates);

		public void TestValidate_PickupDelivery_NonJobCharges_CompanyTariffs_Specified() =>
			TestValidate_PickupDelivery_NonJobCharges_Specified(SourceEndpoint.CompanyTariffs);

		public void TestValidate_PickupDelivery_NonJobCharges_IntercompanyTariffs_Specified() =>
			TestValidate_PickupDelivery_NonJobCharges_Specified(SourceEndpoint.IntercompanyTariffs);

		static void TestValidate_PickupDelivery_NonJobCharges_Specified(SourceEndpoint source)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(source);
			rateQuery.PickupOrg = "PICORG";
			rateQuery.PickupAddrCode = "PICADDR";
			rateQuery.PickupCity = "PICCITY";
			rateQuery.PickupPostcode = "PICPOSTCODE";
			rateQuery.DeliveryOrg = "DELORG";
			rateQuery.DeliveryAddrCode = "DELADDR";
			rateQuery.DeliveryCity = "DELCITY";
			rateQuery.DeliveryPostcode = "DELPOSTCODE";

			var validator = new RateQueryValidator(source);
			var result = validator.TestValidate(rateQuery);

			AssertSingleValidationErrorFor(x => x.PickupOrg, "PickupOrg is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.PickupAddrCode, "PickupAddrCode is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.PickupCity, "PickupCity is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.PickupPostcode, "PickupPostcode is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.DeliveryOrg, "DeliveryOrg is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.DeliveryAddrCode, "DeliveryAddrCode is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.DeliveryCity, "DeliveryCity is only applicable for job charges calculation endpoint");
			AssertSingleValidationErrorFor(x => x.DeliveryPostcode, "DeliveryPostcode is only applicable for job charges calculation endpoint");
			return;

			void AssertSingleValidationErrorFor(Expression<Func<RateQuery, string>> testMemberAccessor, string expectedMessage)
			{
				AssertEquals(expectedMessage, result.ShouldHaveValidationErrorFor(testMemberAccessor).Single().ErrorMessage);
			}
		}

		#endregion

		#region JobCharges source endpoint

		public void TestValidate_JobCharges_PickupDelivery_NotSpecified() =>
			TestValidate_PickupDelivery_PropertiesNotSpecified(SourceEndpoint.JobCharges);

		public void TestValidate_JobCharges_Pickup_OrganisationCode_Empty_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(
				rateQuery,
				x => x.PickupOrg,
				"Please provide a value for PickupOrg. It is mandatory when both PickupCity and PickupPostcode are not specified or empty.");
		}

		public void TestValidate_JobCharges_Delivery_OrganisationCode_Empty_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(
				rateQuery,
				x => x.DeliveryOrg,
				"Please provide a value for DeliveryOrg. It is mandatory when both DeliveryCity and DeliveryPostcode are not specified or empty.");
		}

		public void TestValidate_JobCharges_Pickup_OrganisationCode_NotEmpty_HavingAddressShortCode_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupOrg = "ORG";
			TestValidateFor(rateQuery, x => x.PickupOrg);

			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupOrg);
		}

		public void TestValidate_JobCharges_Delivery_OrganisationCode_NotEmpty_HavingAddressShortCode_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryOrg = "ORG";
			TestValidateFor(rateQuery, x => x.DeliveryOrg);

			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryOrg);
		}

		public void TestValidate_JobCharges_Pickup_OrganisationCode_NotEmpty_HavingEitherCityAndPostcode()
		{
			const string expectedMessage = "The field PickupOrg must be empty if either PickupCity or PickupPostcode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = null;
			TestValidateFor(rateQuery, x => x.PickupOrg, expectedMessage);

			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupCity = null;
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupOrg, expectedMessage);

			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupOrg, expectedMessage);
		}

		public void TestValidate_JobCharges_Delivery_OrganisationCode_NotEmpty_HavingEitherCityAndPostcode()
		{
			const string expectedMessage = "The field DeliveryOrg must be empty if either DeliveryCity or DeliveryPostcode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = null;
			TestValidateFor(rateQuery, x => x.DeliveryOrg, expectedMessage);

			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryCity = null;
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryOrg, expectedMessage);

			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryOrg, expectedMessage);
		}

		public void TestValidate_JobCharges_Pickup_AddressCode_Empty_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupOrg = "ORG";
			TestValidateFor(
				rateQuery,
				x => x.PickupAddrCode,
				"Please provide a value for PickupAddrCode. It is mandatory when both PickupCity and PickupPostcode are not specified or empty.");
		}

		public void TestValidate_JobCharges_Delivery_AddressCode_Empty_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryOrg = "ORG";
			TestValidateFor(
				rateQuery,
				x => x.DeliveryAddrCode,
				"Please provide a value for DeliveryAddrCode. It is mandatory when both DeliveryCity and DeliveryPostcode are not specified or empty.");
		}

		public void TestValidate_JobCharges_Pickup_AddressCode_NotEmpty_HavingOrganisationCode_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupAddrCode);

			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupAddrCode);
		}

		public void TestValidate_JobCharges_Delivery_AddressCode_NotEmpty_HavingOrganisationCode_NoCityNoPostcode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryAddrCode);

			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryAddrCode);
		}

		public void TestValidate_JobCharges_Pickup_AddressCode_NotEmpty_HavingEitherCityAndPostcode()
		{
			const string expectedMessage = "The field PickupAddrCode must be empty if either PickupCity or PickupPostcode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupAddrCode = "ADDR";
			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = null;
			TestValidateFor(rateQuery, x => x.PickupAddrCode, expectedMessage);

			rateQuery.PickupAddrCode = "ADDR";
			rateQuery.PickupCity = null;
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupAddrCode, expectedMessage);

			rateQuery.PickupAddrCode = "ADDR";
			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupAddrCode, expectedMessage);
		}

		public void TestValidate_JobCharges_Delivery_AddressCode_NotEmpty_HavingEitherCityAndPostcode()
		{
			const string expectedMessage = "The field DeliveryAddrCode must be empty if either DeliveryCity or DeliveryPostcode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryAddrCode = "ADDR";
			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = null;
			TestValidateFor(rateQuery, x => x.DeliveryAddrCode, expectedMessage);

			rateQuery.DeliveryAddrCode = "ADDR";
			rateQuery.DeliveryCity = null;
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryAddrCode, expectedMessage);

			rateQuery.DeliveryAddrCode = "ADDR";
			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryAddrCode, expectedMessage);
		}

		public void TestValidate_JobCharges_Pickup_City_NotEmpty_NoOrganisationCodeNoAddressShortCode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.PickupCity = "Sydney";
			TestValidateFor(rateQuery, x => x.PickupCity);

			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupCity);
		}

		public void TestValidate_JobCharges_Delivery_City_NotEmpty_NoOrganisationCodeNoAddressShortCode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.DeliveryCity = "Sydney";
			TestValidateFor(rateQuery, x => x.DeliveryCity);

			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryCity);
		}

		public void TestValidate_JobCharges_Pickup_City_NotEmpty_HavingEitherOrganisationCodeAndAddressShortCode()
		{
			const string expectedMessage = "The field PickupCity must be empty if either PickupOrg or PickupAddrCode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupOrg = "ORG";
			TestValidateFor(rateQuery, x => x.PickupCity, expectedMessage);

			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupOrg = null;
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupCity, expectedMessage);

			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupCity, expectedMessage);
		}

		public void TestValidate_JobCharges_Delivery_City_NotEmpty_HavingEitherOrganisationCodeAndAddressShortCode()
		{
			const string expectedMessage = "The field DeliveryCity must be empty if either DeliveryOrg or DeliveryAddrCode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryOrg = "ORG";
			TestValidateFor(rateQuery, x => x.DeliveryCity, expectedMessage);

			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryOrg = null;
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryCity, expectedMessage);

			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryCity, expectedMessage);
		}

		public void TestValidate_JobCharges_Pickup_Postcode_NotEmpty_NoOrganisationCodeNoAddressShortCode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupPostcode);

			rateQuery.PickupCity = "Sydney";
			rateQuery.PickupPostcode = "2015";
			TestValidateFor(rateQuery, x => x.PickupPostcode);
		}

		public void TestValidate_JobCharges_Delivery_Postcode_NotEmpty_NoOrganisationCodeNoAddressShortCode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryPostcode);

			rateQuery.DeliveryCity = "Sydney";
			rateQuery.DeliveryPostcode = "2015";
			TestValidateFor(rateQuery, x => x.DeliveryPostcode);
		}

		public void TestValidate_JobCharges_Pickup_Postcode_NotEmpty_HavingEitherOrganisationCodeAndAddressShortCode()
		{
			const string expectedError = "The field PickupPostcode must be empty if either PickupOrg or PickupAddrCode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PickupPostcode = "2015";
			rateQuery.PickupOrg = "ORG";
			TestValidateFor(rateQuery, x => x.PickupPostcode, expectedError);

			rateQuery.PickupPostcode = "2015";
			rateQuery.PickupOrg = null;
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupPostcode, expectedError);

			rateQuery.PickupPostcode = "2015";
			rateQuery.PickupOrg = "ORG";
			rateQuery.PickupAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.PickupPostcode, expectedError);
		}

		public void TestValidate_JobCharges_Delivery_Postcode_NotEmpty_HavingEitherOrganisationCodeAndAddressShortCode()
		{
			const string expectedError = "The field DeliveryPostcode must be empty if either DeliveryOrg or DeliveryAddrCode, or both, are specified.";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.DeliveryPostcode = "2015";
			rateQuery.DeliveryOrg = "ORG";
			TestValidateFor(rateQuery, x => x.DeliveryPostcode, expectedError);

			rateQuery.DeliveryPostcode = "2015";
			rateQuery.DeliveryOrg = null;
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryPostcode, expectedError);

			rateQuery.DeliveryPostcode = "2015";
			rateQuery.DeliveryOrg = "ORG";
			rateQuery.DeliveryAddrCode = "ADDR";
			TestValidateFor(rateQuery, x => x.DeliveryPostcode, expectedError);
		}

		static void TestValidateFor(RateQuery rateQuery, Expression<Func<RateQuery, string>> testMemberAccessor, string expectedMessage = null)
		{
			var validator = new RateQueryValidator(SourceEndpoint.JobCharges);

			var result = validator.TestValidate(rateQuery);
			if (expectedMessage != null)
			{
				AssertEquals(expectedMessage, result.ShouldHaveValidationErrorFor(testMemberAccessor).Single().ErrorMessage);
			}
			else
			{
				result.ShouldNotHaveValidationErrorFor(testMemberAccessor);
			}

			Assert("This test uses FluentAssertions", condition: true);
		}

		#endregion
	}
}
