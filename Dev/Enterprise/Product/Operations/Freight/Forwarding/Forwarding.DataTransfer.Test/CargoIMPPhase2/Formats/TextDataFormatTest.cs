using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class TextDataFormatTest : DataFormatTestCase<TextDataFormat, ZString>
	{
		protected override FormattingSample[] GetSamples()
		{
			return new FormattingSample[]
			{
				new FormattingSample("as", "AS", "Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 3 to 10"),
				new FormattingSample("asd", "ASD"),
				new FormattingSample("aD9", "AD9"),
				new FormattingSample(@"-. /", @"-. /"),
				new FormattingSample(",;'@#$%^&*()_+=[{]}<>?", "", "Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 3 to 10"),
				new FormattingSample("01234567890", "0123456789", "Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 3 to 10"),
			};
		}

		protected override TextDataFormat CreateDataFormat()
		{
			return new TextDataFormat(3, 10);
		}
	}
}
