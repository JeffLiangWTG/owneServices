namespace Enterprise.MasterFiles.Business
{
	public class CreatedUserInfo
	{
		public string LoginName { get; set; }
		public string Email { get; set; }
#nullable enable
		public string? ErrorMessage { get; set; }
	}
}
