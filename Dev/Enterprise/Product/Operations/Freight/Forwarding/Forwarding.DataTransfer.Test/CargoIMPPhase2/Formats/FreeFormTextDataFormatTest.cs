using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class FreeFormTextDataFormatTest : DataFormatTestCase<FreeFormTextDataFormat, ZString>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample("as", "as", "Input data must consist of 'Free form text character including any character except for comma, semi colon or apostrophe' with length from 3 to 30"),
				new FormattingSample("asd", "asd"),
				new FormattingSample("aD9", "aD9"),
				new FormattingSample(@"`~!@#$%^&*()-_=+[{]}|:""<.>/? ", @"`~!@#$%^&*()-_=+[{]}|:""<.>/? "),
				new FormattingSample(",;'", "", "Input data must consist of 'Free form text character including any character except for comma, semi colon or apostrophe' with length from 3 to 30"),
				new FormattingSample("0123456789012345678901234567890", "012345678901234567890123456789", "Input data must consist of 'Free form text character including any character except for comma, semi colon or apostrophe' with length from 3 to 30"),
			};
		}

		protected override FreeFormTextDataFormat CreateDataFormat()
		{
			return new FreeFormTextDataFormat(3, 30);
		}
	}
}
