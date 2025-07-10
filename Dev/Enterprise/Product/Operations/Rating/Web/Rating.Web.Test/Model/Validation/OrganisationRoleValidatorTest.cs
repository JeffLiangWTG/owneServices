using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation.TestHelper;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class OrganisationValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_Role()
		{
			var validator = new OrganisationRoleValidator(SourceEndpoint.JobCharges);
			var org = new OrganisationRole()
			{
				Code = "XXX"
			};

			var result1 = validator.TestValidate(org)
				.ShouldHaveValidationErrorFor(o => o.Role)
				.Single();
			AssertEquals("Role is Mandatory.", result1.ErrorMessage);

			org.Role = "XX";
			var result2 = validator.TestValidate(org)
				.ShouldHaveValidationErrorFor(o => o.Role)
				.Single();
			AssertEquals("Provided Role ('XX') is not valid. It can only be one of these values: 'LC', 'AG', 'CNE', 'CNR', 'SAG', 'RAG', 'CCUS', 'IB', 'EB', 'DA', 'PA', 'DTC', 'PTC', 'ICFS', 'ECFS', 'CA', 'CCR', 'COR', 'CAR', 'DCTO', 'DCTR', 'DCFS', 'DCFT', 'ACTO', 'ACTR', 'ACFS', 'ACFT'.", result2.ErrorMessage);

			org.Role = "SAG";
			validator.TestValidate(org)
				.ShouldNotHaveValidationErrorFor(o => o.Role);
		}
		public void TestValidate_Code()
		{
			var validator = new OrganisationRoleValidator(SourceEndpoint.JobCharges);
			var org = new OrganisationRole()
			{
				Role = "SAG"
			};

			var result1 = validator.TestValidate(org)
				.ShouldHaveValidationErrorFor(o => o.Code)
				.Single();
			AssertEquals("Code is Mandatory.", result1.ErrorMessage);

			org.Code = "XXXX";
			validator.TestValidate(org)
				.ShouldNotHaveValidationErrorFor(o => o.Code);
		}
	}
}
