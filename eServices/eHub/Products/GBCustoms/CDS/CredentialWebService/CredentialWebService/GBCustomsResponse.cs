namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public class GBCustomsResponse
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public int expires_in { get; set; }
		public string refresh_token { get; set; }
		public string scope { get; set; }
		public string error { get; set; }
		public string error_description { get; set; }
		public bool IsSuccess
		{
			get
			{
				return string.IsNullOrWhiteSpace(error);
			}
		}

		public override string ToString()
		{
			return IsSuccess
				? $"AccessToken: [{access_token}] TokenType: [{token_type}] RefreshToken: [{refresh_token}] Scope: [{scope}] ExpiresIn: [{expires_in}]"
				: $"Error: [{error}] Description: [{error_description}]";
		}
	}
}