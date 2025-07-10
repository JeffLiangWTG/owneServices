using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Serializable]
	public class AbortFinalizationException : Exception
	{
		public AbortFinalizationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected AbortFinalizationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
