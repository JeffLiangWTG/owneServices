using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Serializable]
	public class PutawayAllocateLocationConcurrencyException : WhsException
	{
		public PutawayAllocateLocationConcurrencyException()
			: base((NoResString)"Putaway allocate location concurrency Exception")
		{
		}

		public PutawayAllocateLocationConcurrencyException(string desc)
			: base(desc)
		{
		}

#if NETFRAMEWORK
		protected PutawayAllocateLocationConcurrencyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
