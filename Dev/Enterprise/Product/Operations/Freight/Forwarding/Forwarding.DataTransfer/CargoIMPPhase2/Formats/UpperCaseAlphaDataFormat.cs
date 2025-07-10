namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class UpperCaseAlphaDataFormat : UpperTextBaseDataFormat
	{
		const string ValidCharacters = "A-Z";
		public UpperCaseAlphaDataFormat(int min, int max) : base(min, max, ValidCharacters)
		{
		}

		public override string TypeErrorDescription
		{
			get { return Res.GetString("d13ce5f3-b60b-4c44-8937-458422d56cf5", "Upper case alphabetic characters"); }
		}
	}
}
