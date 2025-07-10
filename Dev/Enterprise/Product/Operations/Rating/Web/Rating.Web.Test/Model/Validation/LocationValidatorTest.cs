using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class LocationValidatorTest : TestCase
	{
		public virtual void TestValidate_MandatoryProperties()
		{
			void AssertMandatoryValidations(TestValidationResult<Location> results)
			{
				var error1 = results
					.ShouldHaveValidationErrorFor(csl => csl.Type)
					.Single()
					.ErrorMessage;
				AssertEquals("Type is Mandatory.", error1);

				var error2 = results
					.ShouldHaveValidationErrorFor(csl => csl.Value)
					.Single()
					.ErrorMessage;
				AssertEquals("Value is Mandatory.", error2);
			}

			var location = new Location() { };
			var validator = GetValidator(SourceEndpoint.Costing, Location.Types.All);
			AssertMandatoryValidations(validator.TestValidate(location));

			location = new Location() { Type = "", Value = "" };
			validator = GetValidator(SourceEndpoint.Costing, Location.Types.All);
			AssertMandatoryValidations(validator.TestValidate(location));
		}

		public void TestValidate_ValidateTypeThroughValidProvidedTypes()
		{
			var validLocationTypes = new[] { "AAA", "BBB" };

			var location = new Location() { Type = "ZZZ" };

			var validator = GetValidator(SourceEndpoint.Costing, validLocationTypes);
			var error = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Type)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'AAA', 'BBB'.", error);

			location.Type = "AAA";
			validator.TestValidate(location).ShouldNotHaveValidationErrorFor(csl => csl.Type);
		}

		public void TestValidate_MaxLengthOfValues()
		{
			var location = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var validator = GetValidator(SourceEndpoint.Costing, Location.Types.All);
			var validationResults = validator.TestValidate(location);

			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			location.Value = "AUSYD1";
			var error1 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided UNLOCO ('AUSYD1') is not valid. It should be a string of 5 characters.", error1);

			location.Value = "AUSY";
			var error2 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided UNLOCO ('AUSY') is not valid. It should be a string of 5 characters.", error2);

			location.Type = Location.Types.Zone;
			location.Value = "AUSY";
			validationResults = validator.TestValidate(location);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			location.Value = "AUSYD";
			var error3 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided International Zone ('AUSYD') is not valid. It should be a string of 4 characters.", error3);

			location.Value = "SYD";
			var error4 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided International Zone ('SYD') is not valid. It should be a string of 4 characters.", error4);

			location.Type = Location.Types.IATACity;
			location.Value = "SYD";
			validationResults = validator.TestValidate(location);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			location.Value = "SYD1";
			var error5 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided IATA City ('SYD1') is not valid. It should be a string of 3 characters.", error5);

			location.Value = "AU";
			var error6 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided IATA City ('AU') is not valid. It should be a string of 3 characters.", error6);

			location.Type = Location.Types.Country;
			location.Value = "AU";
			validationResults = validator.TestValidate(location);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			location.Value = "SYD";
			var error7 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided Country/Region ('SYD') is not valid. It should be a string of 2 characters.", error7);

			location.Value = "Z";
			var error8 = validator.TestValidate(location)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided Country/Region ('Z') is not valid. It should be a string of 2 characters.", error8);

			location.Type = Location.Types.City;
			location.Value = "WHATEVER";
			validationResults = validator.TestValidate(location);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			location.Type = Location.Types.Postcode;
			location.Value = "WHATEVER";
			validationResults = validator.TestValidate(location);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Type);
			validationResults.ShouldNotHaveValidationErrorFor(csl => csl.Value);
		}

		protected virtual LocationValidator GetValidator(SourceEndpoint source, IEnumerable<string> validLocationTypes)
			=> new LocationValidator(source, validLocationTypes);
	}
}
