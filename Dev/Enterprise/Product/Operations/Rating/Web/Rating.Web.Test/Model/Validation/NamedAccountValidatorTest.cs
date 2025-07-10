using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class NamedAccountValidatorTest : TestCase
	{
		public void TestValidate_MandatoryProperties()
		{
			void AssertMandatoryValidations(TestValidationResult<NamedAccount> results)
			{
				Assert(results.Errors.Count(e => e.PropertyName == "Type") == 1);
				AssertEquals("Type is Mandatory.", results.Errors.First(e => e.PropertyName == "Type").ErrorMessage);

				Assert(results.Errors.Count(e => e.PropertyName == "Value") == 1);
				AssertEquals("Value is Mandatory.", results.Errors.First(e => e.PropertyName == "Value").ErrorMessage);
			}

			var namedAccount = new NamedAccount() { };
			var validator = new NamedAccountValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(namedAccount));

			namedAccount = new NamedAccount() { Type = "", Value = "" };
			validator = new NamedAccountValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(namedAccount));
		}

		public void TestValidate_AcceptableTypes()
		{
			var namedAccount = new NamedAccount() { Type = "ZZZ", Value = "YYY" };
			var costingValidator = new NamedAccountValidator(SourceEndpoint.Costing);

			var result = costingValidator.TestValidate(namedAccount)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'NAC'.", result.ErrorMessage);

			namedAccount.Type = NamedAccount.Types.CargoWise;
			costingValidator.TestValidate(namedAccount)
				.ShouldNotHaveValidationErrorFor(na => na.Type);

			namedAccount.Type = NamedAccount.Types.NamedAccount;
			costingValidator.TestValidate(namedAccount)
				.ShouldNotHaveValidationErrorFor(na => na.Type);

			var nonCostingValidator = new NamedAccountValidator(SourceEndpoint.IntercompanyTariffs);

			namedAccount = new NamedAccount() { Type = "ZZZ", Value = "YYY" };
			result = nonCostingValidator.TestValidate(namedAccount)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'NAC'.", result.ErrorMessage);

			namedAccount.Type = NamedAccount.Types.CargoWise;
			nonCostingValidator.TestValidate(namedAccount)
				.ShouldNotHaveValidationErrorFor(na => na.Type);

			namedAccount.Type = NamedAccount.Types.NamedAccount;
			result = nonCostingValidator.TestValidate(namedAccount)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('NAC') is not valid. It can only be used for querying Costings or calculating charges.", result.ErrorMessage);
		}

		public void TestValidate_MaxLengthOfValues()
		{
			var namedAccount = new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "ABCDEFGHIJKLM" };
			var validator = new NamedAccountValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(namedAccount)
				.ShouldHaveValidationErrorFor(na => na.Value)
				.Single();
			AssertEquals("Provided named account ('ABCDEFGHIJKLM') is not valid. It can be a string of maximum 12 characters.", result.ErrorMessage);

			namedAccount.Value = "ABCDEFGHIJKL";
			validator.TestValidate(namedAccount)
				.ShouldNotHaveValidationErrorFor(na => na.Value);

			namedAccount.Type = NamedAccount.Types.NamedAccount;
			namedAccount.Value = "ABCDEFGHIJKLMNOPQ";
			validator.TestValidate(namedAccount)
				.ShouldNotHaveValidationErrorFor(na => na.Value); // no restrictions for value, when type is NamedAccount
		}
	}
}
