using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class JobContainerValidatorTest : TestCase
	{
		public void TestValidate_ContainerTypeCWCode()
		{
			void AssertMandatoryValidations(TestValidationResult<JobContainer> results)
			{
				var error = results.ShouldHaveValidationErrorFor(q => q.ContainerTypeCWCode).Single();
				AssertEquals("When job is containerized, ContainerTypeCWCode is Mandatory.", error.ErrorMessage);
			}

			void AssertMaxLengthValidations(string providedValue, TestValidationResult<JobContainer> results)
			{
				var error = results.ShouldHaveValidationErrorFor(q => q.ContainerTypeCWCode).Single();
				AssertEquals($"Provided code ('{providedValue}') is not valid. It can be a string of maximum 10 characters.", error.ErrorMessage);
			}

			var jobContainer = new JobContainer();
			var validator = new JobContainerValidator(SourceEndpoint.JobCharges, true);

			jobContainer.ContainerTypeCWCode = null;
			AssertMandatoryValidations(validator.TestValidate(jobContainer));

			jobContainer.ContainerTypeCWCode = "";
			AssertMandatoryValidations(validator.TestValidate(jobContainer));

			jobContainer.ContainerTypeCWCode = "ABCDEFGHIJK";
			AssertMaxLengthValidations("ABCDEFGHIJK", validator.TestValidate(jobContainer));

			jobContainer.ContainerTypeCWCode = "ABCDEFGHIJ";
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.ContainerTypeCWCode);

			validator = new JobContainerValidator(SourceEndpoint.JobCharges, false);
			jobContainer.ContainerTypeCWCode = null;
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.ContainerTypeCWCode);

			jobContainer.ContainerTypeCWCode = "";
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.ContainerTypeCWCode);

			jobContainer.ContainerTypeCWCode = "ABCDEFGHIJK";
			AssertMaxLengthValidations("ABCDEFGHIJK", validator.TestValidate(jobContainer));

			jobContainer.ContainerTypeCWCode = "ABCDEFGHIJ";
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.ContainerTypeCWCode);
		}

		public void TestValidate_Unit()
		{
			var jobContainer = new JobContainer();
			var validator = new JobContainerValidator(SourceEndpoint.JobCharges, true);

			jobContainer.Unit = null;
			var error = validator.TestValidate(jobContainer)
				.ShouldHaveValidationErrorFor(q => q.Unit)
				.Single();
			AssertEquals("When job is containerized, Unit cannot be null.", error.ErrorMessage);

			jobContainer.Unit = 0;
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.Unit);

			validator = new JobContainerValidator(SourceEndpoint.JobCharges, false);
			jobContainer.Unit = null;
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.ContainerTypeCWCode);
		}

		public void TestValidate_Ownership()
		{
			var jobContainer = new JobContainer();
			var validator = new JobContainerValidator(SourceEndpoint.JobCharges, true);

			jobContainer.Ownership = "XXX";
			var error = validator.TestValidate(jobContainer)
				.ShouldHaveValidationErrorFor(q => q.Ownership)
				.Single();
			AssertEquals("Provided Ownership ('XXX') is not valid. It can only be one of these values: 'CAR', 'SHP'.", error.ErrorMessage);

			jobContainer.Ownership = "CAR";
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.Ownership);

			jobContainer.Ownership = "SHP";
			validator.TestValidate(jobContainer)
			.ShouldNotHaveValidationErrorFor(q => q.Ownership);
		}
	}
}
