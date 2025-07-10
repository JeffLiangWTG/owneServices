namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount.Test
{
	internal class QuotationLineTypeDocAmountTest : DocAmountBaseTest
	{
		public override void TestAmount()
			=> AssertDocAmountForCurrentCulture
			(
				docAmount: DocAmount.Create(amountAsDecimal: 1234.56781, type: QuotationLineType.f4),
				culture1: "EN-AU",
				culture2: "IT-IT",
				expectedAmount1: "1234.5678",
				expectedAmount2: "1234,5678"
			);
	}
}
