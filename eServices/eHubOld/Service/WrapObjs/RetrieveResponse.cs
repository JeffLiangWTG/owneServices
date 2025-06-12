namespace CargoWise.eServices.eHub.Service
{
	public class RetrieveResponse
	{
		public string RequestID { get; set; }
		public bool HasError { get; set; }
		public string ErrorDescription { get; set; }

		public Envelope[] Envelopes { get; set; }
	}
}
