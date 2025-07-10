using System;

namespace Enterprise.ProductionRules.Integration
{
	[Serializable]
	public class FactLoadingException : Exception
	{
		public FactLoadingException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected FactLoadingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
