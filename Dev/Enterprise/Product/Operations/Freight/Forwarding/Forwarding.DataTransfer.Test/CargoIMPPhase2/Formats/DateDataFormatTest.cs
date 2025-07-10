using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class DateDataFormatTest : DataFormatTestCase<DateDataFormat, ZDateTime>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample(new ZDateTime(2009, 10, 11, 14, 18, 54), "11OCT09"),
				new FormattingSample(new ZDateTime(2012, 8, 6, 14, 18, 54), "06AUG12"),
			};
		}

		protected override DateDataFormat CreateDataFormat()
		{
			return new DateDataFormat();
		}
	}
}
