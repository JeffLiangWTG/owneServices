using System;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class UserExistsInMultipleDomainsException : Exception
	{
		public UserExistsInMultipleDomainsException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected UserExistsInMultipleDomainsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
