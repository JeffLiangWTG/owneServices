using System;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class NotAllowedException : NotSupportedException
	{
		public NotAllowedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public NotAllowedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
