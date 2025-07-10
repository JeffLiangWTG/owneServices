using System;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation
{
	[Serializable]
	public class DataNotFoundException : Exception
	{
		public DataNotFoundException(string message) : base(message)
		{ }

		public DataNotFoundException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected DataNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
