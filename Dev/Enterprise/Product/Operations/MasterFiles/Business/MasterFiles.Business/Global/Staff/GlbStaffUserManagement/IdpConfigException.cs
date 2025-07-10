using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class IdpConfigException : ZException
	{
		public IdpConfigException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected IdpConfigException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public IdpConfigException(string message)
			: base(message)
		{
		}
	}
}
