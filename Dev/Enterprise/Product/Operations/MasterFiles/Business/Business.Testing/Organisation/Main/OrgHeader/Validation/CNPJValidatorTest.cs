using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CNPJValidatorTest : TestCase
	{
		public void TestValidCNPJ()
		{
			//Formula 1 == 1
			AssertEquals("Valid CNPJ", true, CNPJValidator.ValidateCNPJ("66.714.155/0001-06"));

			//Formula 2 == 1
			AssertEquals("Valid CNPJ", true, CNPJValidator.ValidateCNPJ("90.117.749/7654-80"));

			//Formula 1 & 2 == 1
			AssertEquals("Valid CNPJ", true, CNPJValidator.ValidateCNPJ("16.045.345/0001-00"));

			//Ignore formatting
			AssertEquals("Valid CNPJ", true, CNPJValidator.ValidateCNPJ("27383104000189"));
			AssertEquals("Valid CNPJ - Formatted", true, CNPJValidator.ValidateCNPJ("27.383.104/0001-89"));
		}

		public void TestEmptyCNPJ()
		{
			AssertEquals("Invalid CNPJ - Empty", false, CNPJValidator.ValidateCNPJ(""));
		}

		public void TestInvalidLengthCNPJ()
		{
			AssertEquals("Invalid CNPJ - Too short", false, CNPJValidator.ValidateCNPJ("3335339800010"));
			AssertEquals("Invalid CNPJ - Too long", false, CNPJValidator.ValidateCNPJ("333533980001088"));
		}

		public void TestInvalidCNPJ()
		{
			//Formula 1 == 1
			AssertEquals("Invalid CNPJ", false, CNPJValidator.ValidateCNPJ("66.714.155/0001-66"));

			//Formula 2 == 1
			AssertEquals("Invalid CNPJ", false, CNPJValidator.ValidateCNPJ("90.117.749/7654-88"));

			//Formula 1 & 2 == 1
			AssertEquals("Invalid CNPJ", false, CNPJValidator.ValidateCNPJ("16.045.345/0001-99"));

			//Ignore formatting
			AssertEquals("Invalid CNPJ", false, CNPJValidator.ValidateCNPJ("45116822000190"));
			AssertEquals("Invalid CNPJ - Formatted", false, CNPJValidator.ValidateCNPJ("45.116.822/0001-90"));

			//with letters
			AssertEquals("Invalid CNPJ", false, CNPJValidator.ValidateCNPJ("45116822000190AAA"));
		}
	}
}
