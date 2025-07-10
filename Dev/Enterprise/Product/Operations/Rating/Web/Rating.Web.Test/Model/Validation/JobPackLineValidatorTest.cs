using System.Linq;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class JobPackLineValidatorTest : TestCase
	{
		public void TestValidate_WeightUnit()
		{
			var packLine = new JobPackLine();
			var validator = new JobPackLineValidator(SourceEndpoint.JobCharges);

			packLine.Weight = 10;
			var error1 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.WeightUnit)
				.Single();
			AssertEquals("WeightUnit is mandatory when Weight is specified.", error1.ErrorMessage);

			packLine.Weight = null;
			validator.TestValidate(packLine)
				.ShouldNotHaveValidationErrorFor(q => q.WeightUnit);

			packLine.Weight = 0;
			var error2 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.WeightUnit)
				.Single();
			AssertEquals("WeightUnit is mandatory when Weight is specified.", error2.ErrorMessage);

			packLine.Weight = 10;
			packLine.WeightUnit = "XX";
			var error3 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.WeightUnit)
				.Single();
			AssertEquals("Provided WeightUnit ('XX') is not valid. It can only be one of these values: 'MC', 'MG', 'G', 'HG', 'KG', 'LB', 'OZ', 'LT', 'OT', 'T', 'KT', 'TN', 'TL', 'DT'.", error3.ErrorMessage);

			packLine.WeightUnit = "KG";
			validator.TestValidate(packLine)
				.ShouldNotHaveValidationErrorFor(q => q.WeightUnit);
		}

		public void TestValidate_VolumeUnit()
		{
			var packLine = new JobPackLine();
			var validator = new JobPackLineValidator(SourceEndpoint.JobCharges);

			packLine.Volume = 10;
			var error1 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.VolumeUnit)
				.Single();
			AssertEquals("VolumeUnit is mandatory when Volume is specified.", error1.ErrorMessage);

			packLine.Volume = null;
			validator.TestValidate(packLine)
				.ShouldNotHaveValidationErrorFor(q => q.VolumeUnit);

			packLine.Volume = 0;
			var error2 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.VolumeUnit)
				.Single();
			AssertEquals("VolumeUnit is mandatory when Volume is specified.", error2.ErrorMessage);

			packLine.Volume = 10;
			packLine.VolumeUnit = "XX";
			var error3 = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.VolumeUnit)
				.Single();
			AssertEquals("Provided VolumeUnit ('XX') is not valid. It can only be one of these values: 'M3', 'D3', 'CF', 'CY', 'CI', 'L', 'ML', 'TE', 'CC', 'GA', 'GI'.", error3.ErrorMessage);

			packLine.VolumeUnit = "M3";
			validator.TestValidate(packLine)
				.ShouldNotHaveValidationErrorFor(q => q.VolumeUnit);
		}

		public void TestValidate_DGClassAndSubstance()
		{
			var packLine = new JobPackLine();
			var validator = new JobPackLineValidator(SourceEndpoint.JobCharges);

			packLine.DGClass = "1.4";
			packLine.DGSubstance = string.Empty;

			var validationResults = validator.TestValidate(packLine);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGClass);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGSubstance);

			packLine.DGClass = "1.4";
			packLine.DGSubstance = null;

			validationResults = validator.TestValidate(packLine);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGClass);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGSubstance);

			packLine.DGClass = string.Empty;
			packLine.DGSubstance = "1000S";

			validationResults = validator.TestValidate(packLine);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGClass);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGSubstance);

			packLine.DGClass = null;
			packLine.DGSubstance = "1000S";

			validationResults = validator.TestValidate(packLine);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGClass);
			validationResults.ShouldNotHaveValidationErrorFor(pl => pl.DGSubstance);

			packLine.DGClass = "1.4";
			packLine.DGSubstance = "1000S";

			var error = validator.TestValidate(packLine)
				.ShouldHaveValidationErrorFor(q => q.DGClass)
				.Single();
			AssertEquals("You should set either DGClass or DGSubstance.", error.ErrorMessage);
		}
	}
}
