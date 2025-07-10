using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class CSFilterValidatorTest : TestCase
	{
		public void TestValidate_RateTypes()
		{
			var csFilter = new CSFilter { };
			csFilter.RateTypes = null;

			var validator = new CSFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes);

			csFilter.RateTypes = System.Array.Empty<string>();
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes);

			csFilter.RateTypes = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes);

			csFilter.RateTypes = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.RateTypes)
				.Single();
			AssertEquals("Provided RateTypes is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			csFilter.RateTypes = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.RateTypes)
				.Single();
			AssertEquals("Provided RateTypes is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}

		public void TestValidate_RateTypes2()
		{
			var csFilter = new CSFilter { };
			csFilter.RateTypes2 = null;

			var validator = new CSFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes2);

			csFilter.RateTypes2 = System.Array.Empty<string>();
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes2);

			csFilter.RateTypes2 = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.RateTypes2);

			csFilter.RateTypes2 = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.RateTypes2)
				.Single();
			AssertEquals("Provided RateTypes2 is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			csFilter.RateTypes2 = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.RateTypes2)
				.Single();
			AssertEquals("Provided RateTypes2 is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}

		public void TestValidate_ServiceStrings()
		{
			var csFilter = new CSFilter { };
			csFilter.ServiceStrings = null;

			var validator = new CSFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.ServiceStrings);

			csFilter.ServiceStrings = System.Array.Empty<string>();
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.ServiceStrings);

			csFilter.ServiceStrings = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(csFilter)
			.ShouldNotHaveValidationErrorFor(f => f.ServiceStrings);

			csFilter.ServiceStrings = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.ServiceStrings)
				.Single();
			AssertEquals("Provided ServiceStrings is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			csFilter.ServiceStrings = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(csFilter)
				.ShouldHaveValidationErrorFor(f => f.ServiceStrings)
				.Single();
			AssertEquals("Provided ServiceStrings is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}
	}
}
