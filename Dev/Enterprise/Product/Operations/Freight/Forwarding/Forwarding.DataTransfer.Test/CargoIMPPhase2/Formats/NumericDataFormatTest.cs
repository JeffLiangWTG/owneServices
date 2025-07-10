using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class NumericDataFormatTest : DataFormatTestCase<NumericDataFormat, ZInt>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample(5, "05"),
				new FormattingSample(12, "12"),
				new FormattingSample(0, "00"),
				new FormattingSample(9999, "9999"),
				new FormattingSample(10000, "", "A number with from 2 to 4 digits"),
			};
		}

		protected override NumericDataFormat CreateDataFormat()
		{
			return new NumericDataFormat(2, 4);
		}
	}
}
