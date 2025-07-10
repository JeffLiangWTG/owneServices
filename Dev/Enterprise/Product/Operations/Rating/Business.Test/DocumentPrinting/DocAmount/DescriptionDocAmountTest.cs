using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount.Test
{
	internal class DescriptionDocAmountTest : DocAmountBaseTest
	{
		public override void TestAmount()
			=> AssertDocAmountForCurrentCulture
			(
				docAmount: DocAmount.Create(amountAsString: (NoResString)"1234.5678"),
				culture1: "EN-AU",
				culture2: "IT-IT",
				expectedAmount1: "1234.5678",
				expectedAmount2: "1234.5678" // IT-IT doesn't apply because we passed in amountAsString instead of decimal
			);
	}
}
