using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class CustomFieldValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_NameValue()
		{
			var validator = new CustomFieldValidator(SourceEndpoint.JobCharges);
			var cf = new CustomField("Name", "value");

			var result = validator.TestValidate(cf);

			result.ShouldNotHaveAnyValidationErrors();
			Assert(true);
		}

		public void TestValidate_Value_Null()
		{
			var validator = new CustomFieldValidator(SourceEndpoint.JobCharges);
			var cf = new CustomField("Name", null);

			var result = validator.TestValidate(cf);

			var error = result.ShouldHaveValidationErrorFor(x => x.Value).Single();
			AssertEquals(string.Format("{0} is mandatory", nameof(CustomField.Value)), error.ErrorMessage);
		}

		public void TestValidate_Name_Null()
		{
			var validator = new CustomFieldValidator(SourceEndpoint.JobCharges);
			var cf = new CustomField(null, "value");

			var result = validator.TestValidate(cf);

			var error = result.ShouldHaveValidationErrorFor(x => x.Name).Single();
			AssertEquals(string.Format("{0} is mandatory", nameof(CustomField.Name)), error.ErrorMessage);
		}
	}
}
