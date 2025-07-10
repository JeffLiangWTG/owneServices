using System.Collections.Generic;

namespace Enterprise.Services.ServiceHost
{
	sealed class SendMessageResponse
	{
		public bool Success { get; set; }
		public ICollection<ValidationMessage> Messages { get; set; }
	}
}
