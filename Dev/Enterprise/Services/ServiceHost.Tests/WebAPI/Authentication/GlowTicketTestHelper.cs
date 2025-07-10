using System;
using System.Security.Principal;
using System.Web.Http;
using CargoWise.Authentication.Glow.Ticketing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests
{
	static class GlowTicketTestHelper
	{
		public static GlowAuthenticationTicketIdentity CreateStaffIdentity(GlbStaff staff, Guid branchKey, Guid departmentKey)
		{
			return new GlowAuthenticationTicketIdentity(new AuthenticationTicket
			{
				ProviderType = GlbStaffSchema.Constants.Prefix,
				ProviderKey = staff.PK.ToGuid(),
				Username = staff.GS_LoginName,
				InteropContextBranchKey = branchKey,
				InteropContextDepartmentKey = departmentKey,
			});
		}

		public static GlowAuthenticationTicketIdentity CreateContactIdentity(OrgContact contact, Guid branchKey, Guid departmentKey)
		{
			return new GlowAuthenticationTicketIdentity(new AuthenticationTicket
			{
				ProviderType = OrgContactSchema.Constants.Prefix,
				ProviderKey = contact.PK.ToGuid(),
				Username = $"{contact.OrgCode.TrimEnd()}/{contact.OC_Email}",
				InteropContextBranchKey = branchKey,
				InteropContextDepartmentKey = departmentKey,
			});
		}

		public static void SetUpStaffPrincipal(ApiController controller, GlbStaff staff, Guid branchKey = default, Guid departmentKey = default)
		{
			var identity = CreateStaffIdentity(staff, branchKey, departmentKey);
			controller.User = new GenericPrincipal(identity, null);
		}

		public static void SetUpContactPrincipal(ApiController controller, OrgContact contact, Guid branchKey = default, Guid departmentKey = default)
		{
			var identity = CreateContactIdentity(contact, branchKey, departmentKey);
			controller.User = new GenericPrincipal(identity, null);
		}
	}
}
