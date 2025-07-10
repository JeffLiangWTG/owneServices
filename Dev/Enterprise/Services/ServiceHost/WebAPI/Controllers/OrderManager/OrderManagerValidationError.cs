using System;

namespace Enterprise.Services.ServiceHost
{
	[Serializable]
	public class OrderManagerValidationError : Exception
	{
		public OrderManagerValidationError(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected OrderManagerValidationError(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
