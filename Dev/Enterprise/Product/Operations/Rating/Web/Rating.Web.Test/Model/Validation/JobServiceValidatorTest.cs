using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class JobServiceValidatorTest : TestCase
	{
		public void TestValidate_MandatoryProperties()
		{
			var jobService = new JobService() { };
			var validator = new JobServiceValidator(SourceEndpoint.JobCharges);
			var error1 = validator.TestValidate(jobService)
				.ShouldHaveValidationErrorFor(js => js.Type)
				.Single()
				.ErrorMessage;
			AssertEquals("Type is Mandatory.", error1);

			jobService.Type = "";
			var error2 = validator.TestValidate(jobService)
				.ShouldHaveValidationErrorFor(js => js.Type)
				.Single()
				.ErrorMessage;
			AssertEquals("Type is Mandatory.", error2);

			jobService.Type = "FUM";
			validator.TestValidate(jobService)
				.ShouldNotHaveValidationErrorFor(js => js.Type);
		}

		public void TestValidate_ServiceLocation()
		{
			var jobService = new JobService() { Location = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" } };
			var validator = new JobServiceValidator(SourceEndpoint.JobCharges);

			validator.TestValidate(jobService)
				.ShouldNotHaveValidationErrorFor(js => js.Location.Type);

			jobService.Location.Type = Location.Types.Country;
			jobService.Location.Value = "AU";

			validator.TestValidate(jobService)
				.ShouldNotHaveValidationErrorFor(js => js.Location.Type);

			jobService.Location.Type = Location.Types.IATACity;
			jobService.Location.Value = "SYD";

			var error1 = validator.TestValidate(jobService)
				.ShouldHaveValidationErrorFor(js => js.Location.Type)
				.Single();
			AssertEquals("Provided Type ('IATACITY') is not valid. It can only be one of these values: 'COUNTRY', 'UNLOCO'.", error1.ErrorMessage);
		}
		public void TestValidate_MeasurementBasis()
		{
			var validator = new JobServiceValidator(SourceEndpoint.JobCharges);
			var jobService = new JobService();

			validator.TestValidate(jobService)
				.ShouldNotHaveValidationErrorFor(js => js.MeasurementBasis);

			jobService.Rate = 5m;
			var error1 = validator.TestValidate(jobService)
				.ShouldHaveValidationErrorFor(js => js.MeasurementBasis)
				.Single();
			AssertEquals("MeasurementBasis should be provided when Rate is specified.", error1.ErrorMessage);

			jobService.Rate = 5m;
			jobService.MeasurementBasis = "ABC";
			var error2 = validator.TestValidate(jobService)
				.ShouldHaveValidationErrorFor(js => js.MeasurementBasis)
				.Single();
			AssertEquals("Provided MeasurementBasis ('ABC') is not valid. It can only be one of these values: 'HR', 'DY', 'SV', 'FR', 'CN', 'PD', 'DD', 'CH'.", error2.ErrorMessage);
		}
	}
}
