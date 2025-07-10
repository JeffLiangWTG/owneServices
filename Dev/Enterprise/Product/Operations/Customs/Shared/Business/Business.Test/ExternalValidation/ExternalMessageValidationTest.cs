using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ExternalMessageValidationTest : TestCaseWithFactory
	{
		public void TestGetAdviceHowToFixNoValidExchangeRates()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclarationForTesting>();
			var validation = new ExternalMessageValidation(declarationMock.Object);

			AssertContains("Please report missing exchange rate data to your system administrator.", validation.GetAdviceHowToFixNoValidExchangeRates());
		}
	}
}
