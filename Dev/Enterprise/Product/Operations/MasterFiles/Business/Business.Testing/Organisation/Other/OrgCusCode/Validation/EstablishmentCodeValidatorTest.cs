using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EstablishmentCodeValidatorTest : TestCase
	{
		public void TestIsNNNNA()
		{
			AssertEquals(true, Validator.CodeIsNNNNA("1234D"));
			AssertEquals(false, Validator.CodeIsNNNNA("V031Q"));
		}

		public void TestIsANNNA()
		{
			AssertEquals(true, Validator.CodeIsANNNA("B123A"));
			AssertEquals(false, Validator.CodeIsANNNA("3031P"));
		}

		public void TestIsAANNA()
		{
			AssertEquals(true, Validator.CodeIsAANNA("CB12A"));
			AssertEquals(false, Validator.CodeIsAANNA("V031P"));
		}

		public void TestEstablishmentCodeValidNNNNA()
		{
			char checkSum;
			AssertEquals(true, Validator.CodeIsNNNNA("1234D"));
			AssertEquals(true, Validator.EstablishmentCodeValidNNNNA("1234D", out checkSum));
			AssertEquals(false, Validator.EstablishmentCodeValidNNNNA("1234F", out checkSum));
			AssertEquals('D', checkSum);
		}

		public void TestEstablishmentCodeValidANNNA()
		{
			char checkSum;
			AssertEquals(true, Validator.CodeIsANNNA("B123A"));
			AssertEquals(true, Validator.EstablishmentCodeValidANNNA("B123B", out checkSum));
			AssertEquals(false, Validator.EstablishmentCodeValidANNNA("B123A", out checkSum));
			AssertEquals('B', checkSum);
		}

		public void TestEstablishmentCodeValidAANNA()
		{
			char checkSum;
			AssertEquals(true, Validator.CodeIsAANNA("CB12A"));
			AssertEquals(true, Validator.EstablishmentCodeValidAANNA("CB12E", out checkSum));
			AssertEquals(false, Validator.EstablishmentCodeValidAANNA("CB12A", out checkSum));
			AssertEquals('E', checkSum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Validator = new EstablishmentCodeValidator();
		}
		EstablishmentCodeValidator Validator;
	}
}
