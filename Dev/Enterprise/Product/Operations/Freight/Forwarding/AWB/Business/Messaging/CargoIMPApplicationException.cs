using System;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	[Serializable]
	public class CargoIMPApplicationException : Exception
	{
		public CargoIMPApplicationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CargoIMPApplicationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
