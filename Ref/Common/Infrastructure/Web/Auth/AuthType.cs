namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public static class AuthType
	{
		public const string BasicAuth = "BasicAuth";
		public const string TokenAuth = "TokenAuth";
		public const string CombinedAuth = BasicAuth + "," + TokenAuth;
	}
}
