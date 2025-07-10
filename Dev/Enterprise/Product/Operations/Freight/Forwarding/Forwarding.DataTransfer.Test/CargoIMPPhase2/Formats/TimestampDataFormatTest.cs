using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class TimestampDataFormatTest : DataFormatTestCase<TimestampDataFormat, ZDateTime>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample(new ZDateTime(2009, 10, 11, 14, 18, 54), "11OCT09 1418"),
				new FormattingSample(new ZDateTime(2012, 8, 6, 3, 4, 54), "06AUG12 0304"),
			};
		}

		protected override TimestampDataFormat CreateDataFormat()
		{
			return new TimestampDataFormat();
		}
	}
}
