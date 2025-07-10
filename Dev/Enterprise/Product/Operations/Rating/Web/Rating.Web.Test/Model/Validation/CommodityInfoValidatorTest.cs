
using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class CommodityInfoValidatorTest : TestCase
	{
		public void TestValidate_MandatoryProperties()
		{
			void AssertMandatoryValidations(TestValidationResult<CommodityInfo> results)
			{
				var error = results
					.ShouldHaveValidationErrorFor(csl => csl.Type)
					.Single();
				AssertEquals("Type is Mandatory.", error.ErrorMessage);

				results
				.ShouldNotHaveValidationErrorFor(csl => csl.Value);
			}

			var commodityInfo = new CommodityInfo() { };
			var validator = new CommodityInfoValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(commodityInfo));

			commodityInfo = new CommodityInfo() { Type = "", Value = "" };
			validator = new CommodityInfoValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(commodityInfo));

			commodityInfo = new CommodityInfo() { Type = "", Value = null };
			validator = new CommodityInfoValidator(SourceEndpoint.Costing);
			AssertMandatoryValidations(validator.TestValidate(commodityInfo));
		}

		public void TestValidate_AcceptableTypes()
		{
			var commodityInfo = new CommodityInfo() { Type = "ZZZ", Value = "YYY" };
			var costingValidator = new CommodityInfoValidator(SourceEndpoint.Costing);
			var error = costingValidator
				.TestValidate(commodityInfo)
				.ShouldHaveValidationErrorFor(ci => ci.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'UCG'.", error.ErrorMessage);

			commodityInfo.Type = CommodityInfo.Types.CargoWise;
			costingValidator.TestValidate(commodityInfo)
			.ShouldNotHaveValidationErrorFor(ci => ci.Type);

			commodityInfo.Type = CommodityInfo.Types.UniversalCommodityGroup;
			costingValidator.TestValidate(commodityInfo)
			.ShouldNotHaveValidationErrorFor(ci => ci.Type);

			var nonCostingValidator = new CommodityInfoValidator(SourceEndpoint.IntercompanyTariffs);
			commodityInfo = new CommodityInfo() { Type = "ZZZ", Value = "YYY" };

			var error1 = nonCostingValidator
				.TestValidate(commodityInfo)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('ZZZ') is not valid. It can only be one of these values: 'CW', 'UCG'.", error1.ErrorMessage);

			commodityInfo.Type = CommodityInfo.Types.CargoWise;
			nonCostingValidator.TestValidate(commodityInfo)
				.ShouldNotHaveValidationErrorFor(na => na.Type);

			commodityInfo.Type = CommodityInfo.Types.UniversalCommodityGroup;
			var error2 = nonCostingValidator
				.TestValidate(commodityInfo)
				.ShouldHaveValidationErrorFor(na => na.Type)
				.Single();
			AssertEquals("Provided Type ('UCG') is not valid. It can only be used for querying Costings or calculating charges.", error2.ErrorMessage);
		}

		public void TestValidate_MaxLengthOfValues()
		{
			var commodityInfo = new CommodityInfo() { Type = CommodityInfo.Types.UniversalCommodityGroup, Value = "ABCDEFGHIJK" };
			var validator = new CommodityInfoValidator(SourceEndpoint.Costing);

			var error1 = validator.TestValidate(commodityInfo)
				.ShouldHaveValidationErrorFor(ci => ci.Value)
				.Single();
			AssertEquals("Provided universal commodity group ('ABCDEFGHIJK') is not valid. It can be a string of maximum 10 characters.", error1.ErrorMessage);

			commodityInfo.Value = "ABCDEFGHIJ";
			validator.TestValidate(commodityInfo)
				.ShouldNotHaveValidationErrorFor(ci => ci.Value);

			commodityInfo.Type = CommodityInfo.Types.CargoWise;
			commodityInfo.Value = "ABCD";
			validator.TestValidate(commodityInfo)
				.ShouldNotHaveValidationErrorFor(ci => ci.Value);

			commodityInfo.Value = "ABCDE";
			var error2 = validator.TestValidate(commodityInfo)
				.ShouldHaveValidationErrorFor(ci => ci.Value)
				.Single();
			AssertEquals("Provided commodity code ('ABCDE') is not valid. It can be a string of maximum 4 characters.", error2.ErrorMessage);
		}
	}
}
