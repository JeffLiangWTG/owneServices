using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	[TestFixture]
	class BatchCalculatorTest
	{
		[TestCase(1, 0, 499)]
		[TestCase(2, 500, 999)]
		[TestCase(17, 8000, 8499)]
		public void CalculateIndexes(int batchNumber, int expectedIndexFrom, int expectedIndexTo)
		{
			Assert.AreEqual((expectedIndexFrom, expectedIndexTo), BatchCalculator.CalculateFromToIndexForBatch(batchNumber, 20));
		}
	}
}
