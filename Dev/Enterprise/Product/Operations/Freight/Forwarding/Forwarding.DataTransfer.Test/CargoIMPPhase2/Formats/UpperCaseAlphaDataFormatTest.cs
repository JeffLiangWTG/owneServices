using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class UpperCaseAlphaDataFormatTest : DataFormatTestCase<UpperCaseAlphaDataFormat, ZString>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample("as", "AS", "Input data must consist of 'Upper case alphabetic characters' with length from 3 to 10"),
				new FormattingSample("asd", "ASD"),
				new FormattingSample("aD9", "AD", "Input data must consist of 'Upper case alphabetic characters' with length from 3 to 10"),
				new FormattingSample("-. /,;'@#$%^&*()_+=[{]}<>/?", "", "Input data must consist of 'Upper case alphabetic characters' with length from 3 to 10"),
				new FormattingSample("asdfghjklqw", "ASDFGHJKLQ", "Input data must consist of 'Upper case alphabetic characters' with length from 3 to 10"),
			};
		}

		protected override UpperCaseAlphaDataFormat CreateDataFormat()
		{
			return new UpperCaseAlphaDataFormat(3, 10);
		}
	}
}
