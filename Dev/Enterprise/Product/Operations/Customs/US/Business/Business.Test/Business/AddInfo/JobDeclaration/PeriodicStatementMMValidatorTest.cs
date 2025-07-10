using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PeriodicStatementMMValidatorTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestInvalidOrEmptyDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_PeriodicStatementMM = "2";
			var validator = new PeriodicStatementMMValidator();
			validator.ValidateAgainstReleaseDate(declaration.US_PeriodicStatementMMInfo, ZDateTime.Empty);
			validator.ValidateAgainstReleaseDate(declaration.US_PeriodicStatementMMInfo, ZDateTime.Invalid);
		}
	}
}
