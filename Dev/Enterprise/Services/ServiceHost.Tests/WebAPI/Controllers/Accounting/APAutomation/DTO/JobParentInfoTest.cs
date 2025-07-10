using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.ServiceHost.Tests
{
	class JobParentInfoTest : TestCaseWithFactory
	{
		public void TestJobParentInfo_ShouldPassValidation_WhenValid()
		{
			var info = new JobParentInfo
			{
				ParentId = Guid.NewGuid(),
				ParentTableCode = "JS"
			};
			var results = new List<ValidationResult>();
			var context = new ValidationContext(info);
			var isValid = Validator.TryValidateObject(info, context, results, validateAllProperties: true);
			AssertEquals(expected: true, isValid);
			AssertEquals(expected: 0, results.Count);
		}

		public void TestJobParentInfo_ShouldFailValidation_WhenParentTableCodeIsNull()
		{
			var info = new JobParentInfo
			{
				ParentId = Guid.NewGuid(),
				ParentTableCode = null
			};
			var results = new List<ValidationResult>();
			var context = new ValidationContext(info);
			var isValid = Validator.TryValidateObject(info, context, results, validateAllProperties: true);
			AssertEquals(expected: false, isValid);

			var hasExpectedMessage = results.Exists(r => r.ErrorMessage.Equals("ParentTableCode is required."));
			AssertEquals(expected: true, hasExpectedMessage);
		}

		public void TestJobParentInfo_ShouldFailValidation_WhenParentTableCodeIsEmpty()
		{
			var info = new JobParentInfo
			{
				ParentId = Guid.NewGuid(),
				ParentTableCode = string.Empty
			};
			var results = new List<ValidationResult>();
			var context = new ValidationContext(info);
			var isValid = Validator.TryValidateObject(info, context, results, validateAllProperties: true);
			AssertEquals(expected: false, isValid);

			var hasExpectedMessage = results.Exists(r => r.ErrorMessage.Equals("ParentTableCode is required."));
			AssertEquals(expected: true, hasExpectedMessage);
		}

		public void TestJobParentInfo_ShouldFailValidation_WhenParentTableCodeIsTooShort()
		{
			var info = new JobParentInfo
			{
				ParentId = Guid.NewGuid(),
				ParentTableCode = "A"
			};
			var results = new List<ValidationResult>();
			var context = new ValidationContext(info);
			var isValid = Validator.TryValidateObject(info, context, results, validateAllProperties: true);
			AssertEquals(expected: false, isValid);

			var hasExpectedMessage = results.Exists(r => r.ErrorMessage.Equals("ParentTableCode must be at least 2 characters long."));
			AssertEquals(expected: true, hasExpectedMessage);
		}
	}
}
