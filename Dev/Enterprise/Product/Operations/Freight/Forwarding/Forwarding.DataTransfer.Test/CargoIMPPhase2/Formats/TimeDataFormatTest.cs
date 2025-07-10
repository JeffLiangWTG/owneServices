using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class TimeDataFormatTest : DataFormatTestCase<TimeDataFormat, ZDateTime>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample(new ZDateTime(2009, 10, 11, 14, 18, 54), "1418"),
				new FormattingSample(new ZDateTime(2012, 8, 6, 3, 4, 54), "0304"),
			};
		}

		protected override TimeDataFormat CreateDataFormat()
		{
			return new TimeDataFormat();
		}
	}
}
