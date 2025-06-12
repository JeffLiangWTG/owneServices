using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.ServiceDefaults.Authentication
{
	public partial class AppIdentityContext : IdentityDbContext<AppUser>
	{
		public AppIdentityContext(DbContextOptions<AppIdentityContext> options)
			: base(options)
		{
		}
	}
}
