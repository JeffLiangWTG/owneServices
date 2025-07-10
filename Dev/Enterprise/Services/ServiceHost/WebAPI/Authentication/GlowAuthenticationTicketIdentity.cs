using System;
using CargoWise.Authentication.Glow.Ticketing;

namespace Enterprise.Services.ServiceHost
{
	public sealed class GlowAuthenticationTicketIdentity : IGlowAuthenticationTicketIdentity
	{
		public GlowAuthenticationTicketIdentity(AuthenticationTicket ticket)
		{
			Name = ticket.Username;
			ProviderKey = ticket.ProviderKey;
			ProviderType = ticket.ProviderType;
			BranchKey = ticket.InteropContextBranchKey;
			DepartmentKey = ticket.InteropContextDepartmentKey;
		}

		public Guid ProviderKey { get; }
		public string Name { get; }
		public string ProviderType { get; }
		public string AuthenticationType => "GlowTicket";
		public bool IsAuthenticated => true;
		public Guid BranchKey { get; set; }
		public Guid DepartmentKey { get; set; }
	}
}
