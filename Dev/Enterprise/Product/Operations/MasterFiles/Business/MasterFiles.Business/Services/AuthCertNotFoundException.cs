using System;
using System.Security.Authentication;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class AuthCertNotFoundException : AuthenticationException
	{
		public AuthCertNotFoundException() : base((NoResString)"System to system trust certificate not found.")
		{ }

#if NETFRAMEWORK
		protected AuthCertNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
