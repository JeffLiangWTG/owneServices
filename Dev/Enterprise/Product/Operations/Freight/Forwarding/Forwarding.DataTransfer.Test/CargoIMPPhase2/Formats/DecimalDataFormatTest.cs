using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class DecimalDataFormatTest : DataFormatTestCase<DecimalDataFormat, ZDecimal>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample(5.468M, "5.47"),
				new FormattingSample(11M, "11"),
				new FormattingSample(0.468M, ".47"),
				new FormattingSample(10000.45M, "", "A decimal number with up to 6 digits and up to 2 decimal places"),
				new FormattingSample(1000000M, "", "A decimal number with up to 6 digits and up to 2 decimal places"),
			};
		}

		protected override DecimalDataFormat CreateDataFormat()
		{
			return new DecimalDataFormat(6, 2);
		}
	}
}
