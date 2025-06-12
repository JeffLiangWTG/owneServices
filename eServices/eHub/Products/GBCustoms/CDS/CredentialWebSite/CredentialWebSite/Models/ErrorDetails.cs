namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Models
{
	public class ErrorDetails
	{
		public ErrorDetails(string error, string errorDescription, string errorCode, string category)
		{
			Error = error;
			ErrorDescription = errorDescription;
			ErrorCode = errorCode;
			Category = category;
		}

		public string Error { get; set; }

		public string ErrorDescription { get; set; }

		public string ErrorCode { get; set; }

		public string Category { get; private set; }
	}
}