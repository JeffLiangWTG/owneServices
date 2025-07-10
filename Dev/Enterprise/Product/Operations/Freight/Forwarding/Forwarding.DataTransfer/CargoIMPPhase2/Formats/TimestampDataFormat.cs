namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class TimestampDataFormat : DateBaseDataFormat
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Format String.")]
		public TimestampDataFormat()
			: base("ddMMMyy HHmm")
		{
		}
	}
}
