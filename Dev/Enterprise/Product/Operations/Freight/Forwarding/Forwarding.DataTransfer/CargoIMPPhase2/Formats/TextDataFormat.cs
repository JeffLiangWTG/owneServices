namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class TextDataFormat : UpperTextBaseDataFormat
	{
		const string ValidCharacters = @"A-Z0-9\-./ "; // Characters.

		public TextDataFormat(int min, int max)
			: base(min, max, ValidCharacters)
		{
		}

		public override string TypeErrorDescription
		{
			get { return Res.GetString("c6ab3948-beac-48ae-b8e7-c37df93e8249", "Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space"); }
		}
	}
}
