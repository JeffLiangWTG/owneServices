using System;
using System.Security.Principal;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	public interface IGlowAuthenticationTicketIdentity : IIdentity
	{
		Guid ProviderKey { get; }
		string ProviderType { get; }
		Guid BranchKey { get; set; }
		Guid DepartmentKey { get; set; }
	}

	public static class GlowAuthenticationTicketIdentityExtensions
	{
		public static Guid? GetContactPK(this IGlowAuthenticationTicketIdentity identity)
		{
			if (identity != null && identity.IsAuthenticated && identity.ProviderType == OrgContactSchema.Constants.Prefix)
			{
				return identity.ProviderKey;
			}

			return null;
		}

		public static OrgContact GetContact(this IGlowAuthenticationTicketIdentity identity, BusinessObjectFactory factory)
		{
			var contactPK = identity?.GetContactPK();
			if (contactPK != null)
			{
				return factory.Load<OrgContact>(contactPK.Value);
			}

			return null;
		}

		public static bool IsStaff(this IGlowAuthenticationTicketIdentity identity) => identity != null && identity.IsAuthenticated && identity.ProviderType == GlbStaffSchema.Constants.Prefix;
	}
}
