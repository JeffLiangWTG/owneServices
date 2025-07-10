namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class FreeFormTextDataFormat : TextBaseDataFormat
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Characters.")]
		const string ValidCharacters = @"a-zA-Z0-9`~!@#$%^&*()\-_=+\[{\]}\|:""<.>/? ";

		public FreeFormTextDataFormat(int min, int max)
			: base(min, max, ValidCharacters)
		{
		}

		public override string TypeErrorDescription
		{
			get { return Res.GetString("a9e92bd5-7cb5-4525-9b92-7383da11aa58", "Free form text character including any character except for comma, semi colon or apostrophe"); }
		}
	}
}
