using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers
{
	public class SendInstructionEmailRequest
	{
		public Guid UserKey { get; set; }
		public string LandingPageUri { get; set; }
	}

	public class SendConfirmationEmailRequest
	{
		public Guid UserKey { get; set; }
		public PasswordInstructionType InstructionType { get; set; }
	}
}
