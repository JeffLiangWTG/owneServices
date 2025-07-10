using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class CarrierServiceLevelValidatorTest : TestCase
	{
		public void TestValidate_MandatoryProperties()
		{
			void AssertMandatoryValidations(TestValidationResult<CarrierServiceLevel> results)
			{
				var error = results
					.ShouldHaveValidationErrorFor(csl => csl.Type)
					.Single();
				AssertEquals("Type is Mandatory.", error.ErrorMessage);

				results
				.ShouldNotHaveValidationErrorFor(csl => csl.Value);
			}

			var carrierServiceLevel = new CarrierServiceLevel() { };
			var validator = new CarrierServiceLevelValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(carrierServiceLevel));

			carrierServiceLevel = new CarrierServiceLevel() { Value = "", Type = "" };
			validator = new CarrierServiceLevelValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(carrierServiceLevel));

			carrierServiceLevel = new CarrierServiceLevel() { Value = null, Type = "" };
			validator = new CarrierServiceLevelValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(carrierServiceLevel));
		}

		public void TestValidate_AcceptableTypes()
		{
			var carrierServiceLevel = new CarrierServiceLevel() { Type = "ZZZ", Value = "YYY" };
			var costingValidator = new CarrierServiceLevelValidator(SourceEndpoint.Costing);

			var error1 = costingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldHaveValidationErrorFor(csl => csl.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'UC'.", error1.ErrorMessage);

			carrierServiceLevel.Type = CarrierServiceLevel.Types.CargoWise;
			costingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldNotHaveValidationErrorFor(csl => csl.Type);

			carrierServiceLevel.Type = CarrierServiceLevel.Types.Universal;
			costingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldNotHaveValidationErrorFor(csl => csl.Type);

			var nonCostingValidator = new CarrierServiceLevelValidator(SourceEndpoint.IntercompanyTariffs);

			carrierServiceLevel = new CarrierServiceLevel() { Type = "ZZZ", Value = "YYY" };
			var error2 = nonCostingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'UC'.", error2.ErrorMessage);

			carrierServiceLevel.Type = CarrierServiceLevel.Types.CargoWise;
			nonCostingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldNotHaveValidationErrorFor(na => na.Type);

			carrierServiceLevel.Type = CarrierServiceLevel.Types.Universal;
			var error3 = nonCostingValidator
				.TestValidate(carrierServiceLevel)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('UC') is not valid. It can only be used for querying Costings or calculating charges.", error3.ErrorMessage);
		}

		public void TestValidate_MaxLengthOfValues()
		{
			var carrierServiceLevel = new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "ABCD" };
			var validator = new CarrierServiceLevelValidator(SourceEndpoint.Costing);
			var error1 = validator
				.TestValidate(carrierServiceLevel)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single();
			AssertEquals("Provided Value as CargoWise code ('ABCD') is not valid. It can be a string of maximum 3 characters.", error1.ErrorMessage);

			carrierServiceLevel.Value = "ABC";

			validator
			.TestValidate(carrierServiceLevel)
			.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			carrierServiceLevel.Type = CarrierServiceLevel.Types.Universal;
			carrierServiceLevel.Value = "ABCD";

			validator
			.TestValidate(carrierServiceLevel)
			.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			carrierServiceLevel.Value = "ABCDE";

			validator
			.TestValidate(carrierServiceLevel)
			.ShouldNotHaveValidationErrorFor(csl => csl.Value);

			carrierServiceLevel.Value = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLM";

			var error2 = validator
				.TestValidate(carrierServiceLevel)
				.ShouldHaveValidationErrorFor(csl => csl.Value)
				.Single();
			AssertEquals("Provided Value as Universal code ('ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLM') is not valid. It can be a string of maximum 64 characters.", error2.ErrorMessage);
		}
	}
}
