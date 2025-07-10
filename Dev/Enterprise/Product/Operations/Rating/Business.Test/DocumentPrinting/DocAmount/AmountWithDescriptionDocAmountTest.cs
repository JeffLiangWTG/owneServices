namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount.Test
{
	internal class AmountWithDescriptionDocAmountTest : DocAmountBaseTest
	{
		public override void TestAmount()
			=> AssertDocAmountForCurrentCompanyCulture
			(
				docAmount: DocAmount.Create(amountAsDecimal: 1234.56781, decimals: 4, amountDescription: "{0} Flat"),
				culture1: "EN-AU",
				culture2: "IT-IT",
				expectedAmount1: "1,234.5678 Flat",
				expectedAmount2: "1.234,5678 Flat"
			);
	}
}
