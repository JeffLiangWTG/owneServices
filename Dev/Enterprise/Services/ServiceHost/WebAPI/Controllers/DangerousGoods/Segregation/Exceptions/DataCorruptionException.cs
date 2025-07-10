using System;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	[Serializable]
	public class DataCorruptionException : Exception
	{
		public DataCorruptionException(string message) : base(message)
		{ }

		public DataCorruptionException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected DataCorruptionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
