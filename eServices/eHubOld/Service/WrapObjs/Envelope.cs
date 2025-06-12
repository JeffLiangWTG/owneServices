namespace CargoWise.eServices.eHub.Service
{
	public struct Envelope
	{
		public InterchangeDetails InterchangeDetails;
		public Document Document;
	}

	public struct InterchangeDetails
	{
		public string InterchangeID;
		public string SenderID;
		public string RecipientID;
		public string InterchangeVersion;
		public string SenderApplicaitonVersion;
	}

	public struct Document
	{
		public string DocumentType;
		public string DocumentContent;
	}
}
