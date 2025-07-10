using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class UserManagementException : ZException
	{
		public UserManagementException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected UserManagementException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public UserManagementException(string message)
			: base(message)
		{
		}
	}
}
