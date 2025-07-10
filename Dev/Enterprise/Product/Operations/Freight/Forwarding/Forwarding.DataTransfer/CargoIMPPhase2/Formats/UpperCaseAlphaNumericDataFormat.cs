namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class UpperCaseAlphaNumericDataFormat : UpperTextBaseDataFormat
	{
		const string ValidCharacters = "A-Z0-9";

		public UpperCaseAlphaNumericDataFormat(int min, int max)
			: base(min, max, ValidCharacters)
		{
		}

		public override string TypeErrorDescription
		{
			get { return Res.GetString("71d3aa5a-53c8-43ca-b50d-c9b0dbf98866", "Mixed - a single upper case character alpha or numeric"); }
		}
	}
}
