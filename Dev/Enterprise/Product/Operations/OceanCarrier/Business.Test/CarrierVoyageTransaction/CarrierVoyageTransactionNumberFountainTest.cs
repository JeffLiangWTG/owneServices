using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.OceanCarrier.Business.Testing
{
	sealed class CarrierVoyageTransactionNumberFountainTest : TestCaseWithFactory
	{
		public void TestCarrierVoyageTransactionNumberFountainShouldGenerateCorrectlyFormattedNumber()
		{
			var numberFountain = Env.NumberFountains.CarrierVoyageTransactionId;

			var number = numberFountain.GetNextFormatted(Factory);
			AssertNotNullOrEmpty("TransactionId", number);
			AssertStartsWith("TransactionId should start with ID", "ID", number);
			AssertEquals("TransactionId should have length 10", 10, number.Length);
		}
	}
}
