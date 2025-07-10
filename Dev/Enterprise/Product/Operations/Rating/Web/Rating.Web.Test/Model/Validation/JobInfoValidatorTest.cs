using System;
using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class JobInfoValidatorTest : TestCase
	{
		public void TestValidate_Containers()
		{
			var containerizedValidator = new JobInfoValidator(SourceEndpoint.JobCharges, isContainerized: true);
			var nonContainerizedValidator = new JobInfoValidator(SourceEndpoint.JobCharges, isContainerized: false);

			var jobInfo = new JobInfo();
			var error1 = containerizedValidator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.Containers)
				.Single();
			AssertEquals("Containers of JobInfo should contain at least one element for calculating job charges.", error1.ErrorMessage);

			var error2 = nonContainerizedValidator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.Containers)
				.Single();
			AssertEquals("Containers of JobInfo should contain at least one element for calculating job charges.", error2.ErrorMessage);

			jobInfo = new JobInfo()
			{
				Containers = new JobContainer[] { null }
			};

			var error3 = containerizedValidator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.Containers)
				.Single();
			AssertEquals("Provided Containers is not valid. It shouldn't contain null elements.", error3.ErrorMessage);

			var error4 = nonContainerizedValidator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.Containers)
				.Single();
			AssertEquals("Provided Containers is not valid. It shouldn't contain null elements.", error4.ErrorMessage);

			jobInfo = new JobInfo()
			{
				Containers = new JobContainer[] { new JobContainer() }
			};

			containerizedValidator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor("Containers[0].ContainerTypeCWCode");

			nonContainerizedValidator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor("Containers[0].ContainerTypeCWCode");
		}

		public void TestValidate_JobServices()
		{
			var jobInfo = new JobInfo();
			var validator = new JobInfoValidator(SourceEndpoint.JobCharges, true);

			jobInfo.JobServices = null;
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.JobServices);

			jobInfo.JobServices = new[]
			{
				new JobService() { Type = "FUM" },
				null
			};

			var error1 = validator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.JobServices)
				.Single();
			AssertEquals("Provided JobServices is not valid. It shouldn't contain null elements.", error1.ErrorMessage);

			jobInfo.JobServices = new[]
			{
				new JobService() { Type = "FUM" },
				new JobService() { Type = "QIN" },
			};
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.JobServices);

			jobInfo.JobServices = new[]
			{
				new JobService() { Type = null },
				new JobService() { Type = "QIN" },
			};

			var error2 = validator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor("JobServices[0].Type")
				.Single();
			AssertEquals("Type is Mandatory.", error2.ErrorMessage);
		}

		public void TestValidate_GoodsValueCurrency()
		{
			AssertValueAndCurrency(nameof(JobInfo.GoodsValue), nameof(JobInfo.GoodsValueCurrency));
		}

		public void TestValidate_InsuranceValueCurrency()
		{
			AssertValueAndCurrency(nameof(JobInfo.InsuranceValue), nameof(JobInfo.InsuranceValueCurrency));
		}

		void AssertValueAndCurrency(string valuePropertyName, string currencyPropertyName)
		{
			void AssertCurrencyValidationError(TestValidationResult<JobInfo> validationResults)
			{
				var error = validationResults
					.ShouldHaveValidationErrorFor(currencyPropertyName)
					.Single();
				AssertEquals($"{currencyPropertyName} is Mandatory when {valuePropertyName} is specified.", error.ErrorMessage);
			}

			var valueProperty = typeof(JobInfo).GetProperty(valuePropertyName);
			var currencyProperty = typeof(JobInfo).GetProperty(currencyPropertyName);

			var validator = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var jobInfo = new JobInfo();
			valueProperty.SetValue(jobInfo, 10.0M);
			currencyProperty.SetValue(jobInfo, null);

			AssertCurrencyValidationError(validator.TestValidate(jobInfo));

			valueProperty.SetValue(jobInfo, 10.0M);
			currencyProperty.SetValue(jobInfo, string.Empty);

			AssertCurrencyValidationError(validator.TestValidate(jobInfo));

			valueProperty.SetValue(jobInfo, 10.0M);
			currencyProperty.SetValue(jobInfo, "AUD");

			validator.TestValidate(jobInfo)
			.ShouldNotHaveValidationErrorFor(currencyPropertyName);

			valueProperty.SetValue(jobInfo, null);
			currencyProperty.SetValue(jobInfo, "AUD");

			validator.TestValidate(jobInfo)
			.ShouldNotHaveValidationErrorFor(currencyPropertyName);

			valueProperty.SetValue(jobInfo, null);
			currencyProperty.SetValue(jobInfo, null);

			validator.TestValidate(jobInfo)
			.ShouldNotHaveValidationErrorFor(currencyPropertyName);
		}

		public void TestValidate_PickupDropMode()
		{
			var validator = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var jobInfo = new JobInfo();

			jobInfo.PickupDropMode = null;

			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.PickupDropMode);

			jobInfo.PickupDropMode = string.Empty;
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.PickupDropMode);

			jobInfo.PickupDropMode = "SOME";
			var error = validator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.PickupDropMode)
				.Single();
			AssertEquals("Provided PickupDropMode ('SOME') is not valid. It can only be one of these values: 'ANY', 'ASK', 'HSL', 'HUL', 'HWL', 'LOF', 'PSL', 'SDL', 'TRL', 'WUP'.", error.ErrorMessage);

			jobInfo.PickupDropMode = "ANY";
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.PickupDropMode);
		}

		public void TestValidate_DeliveryDropMode()
		{
			var validator = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var jobInfo = new JobInfo();

			jobInfo.DeliveryDropMode = null;

			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.DeliveryDropMode);

			jobInfo.DeliveryDropMode = string.Empty;
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.DeliveryDropMode);

			jobInfo.DeliveryDropMode = "SOME";
			var error = validator.TestValidate(jobInfo)
				.ShouldHaveValidationErrorFor(i => i.DeliveryDropMode)
				.Single();
			AssertEquals("Provided DeliveryDropMode ('SOME') is not valid. It can only be one of these values: 'ANY', 'ASK', 'HSL', 'HUL', 'HWL', 'LOF', 'PSL', 'SDL', 'TRL', 'WUP'.", error.ErrorMessage);

			jobInfo.DeliveryDropMode = "ANY";
			validator.TestValidate(jobInfo)
				.ShouldNotHaveValidationErrorFor(i => i.DeliveryDropMode);
		}

		public void TestValidate_CustomFields_Populated()
		{
			var jobInfo = new JobInfo();
			jobInfo.CustomFields = new CustomField[]
			{
				new CustomField("key", "value")
			};

			var vv = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var result = vv.TestValidate(jobInfo);

			result.ShouldNotHaveValidationErrorFor(x => x.CustomFields);
			Assert(true);
		}

		public void TestValidate_CustomFields_Empty()
		{
			var jobInfo = new JobInfo();
			jobInfo.CustomFields = Array.Empty<CustomField>();

			var vv = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var result = vv.TestValidate(jobInfo);

			result.ShouldNotHaveValidationErrorFor(x => x.CustomFields);
			Assert(true);
		}

		public void TestValidate_CustomFields_Null()
		{
			var jobInfo = new JobInfo();
			jobInfo.CustomFields = null;

			var vv = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var result = vv.TestValidate(jobInfo);

			result.ShouldNotHaveValidationErrorFor(x => x.CustomFields);
			Assert(true);
		}

		public void TestValidate_CustomFields_DuplicateEntries()
		{
			var jobInfo = new JobInfo();
			jobInfo.CustomFields = new CustomField[]
			{
				new CustomField("Key", "value)"),
				new CustomField("KEY", "another")
			};

			var vv = new JobInfoValidator(SourceEndpoint.JobCharges, true);
			var result = vv.TestValidate(jobInfo);

			var error = result.ShouldHaveValidationErrorFor(x => x.CustomFields).Single();
			AssertEquals(string.Format("{0} contains duplicate entries for {1} field", nameof(JobInfo.CustomFields), nameof(CustomField.Name)), error.ErrorMessage);
		}
	}
}
