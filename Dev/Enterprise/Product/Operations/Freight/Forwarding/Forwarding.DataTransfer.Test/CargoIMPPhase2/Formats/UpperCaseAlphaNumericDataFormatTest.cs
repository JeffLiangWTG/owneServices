using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class UpperCaseAlphaNumericDataFormatTest : DataFormatTestCase<UpperCaseAlphaNumericDataFormat, ZString>
	{
		protected override FormattingSample[] GetSamples()
		{
			ZString errorMessage = "Input data must consist of 'Mixed - a single upper case character alpha or numeric' with length from 3 to 10";
			return new FormattingSample[]
			{
				new FormattingSample("as", "AS", errorMessage),
				new FormattingSample("asd", "ASD"),
				new FormattingSample("aD9", "AD9"),
				new FormattingSample(@",;'`~!@#$%^&*()-_=+[{]}|:""<.>/? ", "", errorMessage),
				new FormattingSample("01234567890", "0123456789", errorMessage),
			};
		}

		protected override UpperCaseAlphaNumericDataFormat CreateDataFormat()
		{
			return new UpperCaseAlphaNumericDataFormat(3, 10);
		}
	}
}
