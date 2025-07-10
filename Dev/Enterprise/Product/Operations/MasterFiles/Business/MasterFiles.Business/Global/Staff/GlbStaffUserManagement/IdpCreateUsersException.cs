using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class IdpCreateUsersException : ZException
	{
		public IdpCreateUsersException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected IdpCreateUsersException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public IdpCreateUsersException(string message)
			: base(message)
		{
		}
	}
}
