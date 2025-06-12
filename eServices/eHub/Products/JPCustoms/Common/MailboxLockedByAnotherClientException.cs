using System;

namespace CargoWise.eHub.Products.JPCustoms.Common
{
	[Serializable]
	public class MailboxLockedByAnotherClientException : Exception
	{
		public MailboxLockedByAnotherClientException(string message)
			: base(message)
		{

		}
	}
}
