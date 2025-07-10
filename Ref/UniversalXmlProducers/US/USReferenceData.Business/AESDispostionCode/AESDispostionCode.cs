namespace CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode
{
	public class AESDispostionCode
	{
		public string ResponseCode
		{
			get => responseCode;
			set => responseCode = value;
		}
		string responseCode;

		public string NarrativeText
		{
			get => narrativeText;
			set
			{
				if (narrativeText == null)
				{
					narrativeText = value;
				}
				else
				{
					narrativeText += value;
				}
			}
		}
		string narrativeText;

		public string Severity
		{
			get => severity;
			set => severity = value;
		}
		string severity;

		public string Reason
		{
			get => reason;
			set
			{
				if (reason == null)
				{
					reason = value;
				}
				else
				{
					reason += value;
				}
			}
		}
		string reason;

		public string Resolution
		{
			get => resolution;
			set
			{
				if (resolution == null)
				{
					resolution = value;
				}
				else
				{
					resolution += value;
				}
			}
		}
		string resolution;
	}
}
