using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class OptionalLocationValidatorTest : LocationValidatorTest
	{
		public override void TestValidate_MandatoryProperties()
		{
			var location = new Location() { };
			var validator = GetValidator(SourceEndpoint.Costing, Location.Types.All);
			var results = validator.TestValidate(location);
			Assert(results.Errors.All(e => e.PropertyName != "Type"));
			Assert(results.Errors.All(e => e.PropertyName != "Value"));

			location = new Location() { Type = "", Value = "" };
			results = validator.TestValidate(location);
			Assert(results.Errors.All(e => e.PropertyName != "Type"));
			Assert(results.Errors.All(e => e.PropertyName != "Value"));

			location = new Location() { Type = Location.Types.Country, Value = "" };
			results = validator.TestValidate(location);
			Assert(results.Errors.Count == 1);
			AssertEquals("Value is mandatory when the Type is specified.", results.Errors[0].ErrorMessage);
			AssertEquals("Value", results.Errors[0].PropertyName);

			location = new Location() { Type = "", Value = "AU" };
			results = validator.TestValidate(location);
			Assert(results.Errors.Count == 1);
			AssertEquals("Type is mandatory when the Value is specified.", results.Errors[0].ErrorMessage);
			AssertEquals("Type", results.Errors[0].PropertyName);
		}

		protected override LocationValidator GetValidator(SourceEndpoint source, IEnumerable<string> validLocationTypes)
			=> new OptionalLocationValidator(source, validLocationTypes);
	}
}
