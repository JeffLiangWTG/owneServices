using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class ContainerTypeValidatorTest : TestCase
	{
		public void TestValidate_MandatoryProperties()
		{
			void AssertMandatoryValidations(TestValidationResult<ContainerType> results)
			{
				var error1 = results.ShouldHaveValidationErrorFor(csl => csl.Type).Single();
				AssertEquals("Type is Mandatory.", error1.ErrorMessage);

				var error2 = results.ShouldHaveValidationErrorFor(csl => csl.Value).Single();
				AssertEquals("Value is Mandatory.", error2.ErrorMessage);
			}

			var containerType = new ContainerType() { };
			var validator = new ContainerTypeValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(containerType));

			containerType = new ContainerType() { Type = "", Value = "" };
			validator = new ContainerTypeValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(containerType));
		}

		public void TestValidate_AcceptableTypes()
		{
			var containerType = new ContainerType() { Type = "ZZZ", Value = "YYY" };
			var validator = new ContainerTypeValidator(SourceEndpoint.Costing);
			var error1 = validator.TestValidate(containerType)
				.ShouldHaveValidationErrorFor(ct => ct.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'ISO'.", error1.ErrorMessage);

			containerType.Type = ContainerType.Types.CargoWise;
			validator.TestValidate(containerType)
			.ShouldNotHaveValidationErrorFor(ct => ct.Type);

			containerType.Type = ContainerType.Types.ISO;
			validator.TestValidate(containerType)
			.ShouldNotHaveValidationErrorFor(ct => ct.Type);
		}

		public void TestValidate_MaxLengthOfValues()
		{
			var containerType = new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "ABCDEFGHIJK" };
			var validator = new ContainerTypeValidator(SourceEndpoint.Costing);
			var error1 = validator.TestValidate(containerType)
				.ShouldHaveValidationErrorFor(ct => ct.Value)
				.Single();
			AssertEquals("Provided CargoWise code ('ABCDEFGHIJK') is not valid. It can be a string of maximum 10 characters.", error1.ErrorMessage);

			containerType.Value = "ABCDEFGHIJ";
			validator.TestValidate(containerType)
			.ShouldNotHaveValidationErrorFor(ct => ct.Value);

			containerType.Type = ContainerType.Types.ISO;
			containerType.Value = "ABCD";
			validator.TestValidate(containerType)
			.ShouldNotHaveValidationErrorFor(ct => ct.Value);

			containerType.Value = "ABCDE";
			var error2 = validator.TestValidate(containerType)
				.ShouldHaveValidationErrorFor(ct => ct.Value)
				.Single();
			AssertEquals("Provided ISO code ('ABCDE') is not valid. It can be a string of maximum 4 characters.", error2.ErrorMessage);
		}
	}
}
