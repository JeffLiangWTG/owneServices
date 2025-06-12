using Microsoft.AspNetCore.Identity;

namespace eServices.Dms.Core.ServiceDefaults.Authentication;

public class AppUser : IdentityUser
{
	public string? AccessKey { get; set; }
}
