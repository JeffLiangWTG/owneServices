using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class CGFilterValidatorTest : TestCase
	{
		public void TestValidate_Products()
		{
			var cgFilter = new CGFilter { };
			cgFilter.Products = null;

			var validator = new CGFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Products);

			cgFilter.Products = System.Array.Empty<string>();
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Products);

			cgFilter.Products = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Products);

			cgFilter.Products = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.Products)
				.Single();
			AssertEquals("Provided Products is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			cgFilter.Products = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.Products)
				.Single();
			AssertEquals("Provided Products is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}

		public void TestValidate_References()
		{
			var cgFilter = new CGFilter { };
			cgFilter.References = null;

			var validator = new CGFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.References);

			cgFilter.References = System.Array.Empty<string>();
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.References);

			cgFilter.References = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.References);

			cgFilter.References = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.References)
				.Single();
			AssertEquals("Provided References is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			cgFilter.References = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.References)
				.Single();
			AssertEquals("Provided References is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}

		public void TestValidate_Vias()
		{
			var cgFilter = new CGFilter { };
			cgFilter.Vias = null;

			var validator = new CGFilterValidator(SourceEndpoint.Costing);
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Vias);

			cgFilter.Vias = System.Array.Empty<string>();
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Vias);

			cgFilter.Vias = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(cgFilter)
			.ShouldNotHaveValidationErrorFor(f => f.Vias);

			cgFilter.Vias = new string[] { "ABC", "DEF", "ENG", null };
			var error1 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.Vias)
				.Single();
			AssertEquals("Provided Vias is not valid. It shouldn't contain null or empty elements.", error1.ErrorMessage);

			cgFilter.Vias = new string[] { "ABC", "DEF", "ENG", "" };
			var error2 = validator.TestValidate(cgFilter)
				.ShouldHaveValidationErrorFor(f => f.Vias)
				.Single();
			AssertEquals("Provided Vias is not valid. It shouldn't contain null or empty elements.", error2.ErrorMessage);
		}
	}
}
